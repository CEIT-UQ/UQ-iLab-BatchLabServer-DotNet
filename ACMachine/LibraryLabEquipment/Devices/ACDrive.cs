using System;
using System.Collections.Generic;
using System.Diagnostics;
using Library.Lab;
using Library.Lab.Utilities;
using Modbus.Device;

namespace Library.LabEquipment.Devices
{
    public class ACDrive
    {
        #region Constants
        private const string STR_ClassName = "ACDrive";
        private const Logfile.Level logLevel = Logfile.Level.Finer;
        /*
         * String constants for logfile messages
         */
        private const string STRLOG_Speed_arg = "Speed: {0:d} RPM";
        private const string STRLOG_Torque_arg = "Torque: {0:d} %";
        private const string STRLOG_Time_arg = "Time: {0:d} seconds";
        private const string STRLOG_Current_arg = "Current: {0:d} milliamps";
        private const string STRLOG_Success_arg = "Success: {0:s}";
        private const string STRLOG_SuccessSpeed_arg2 = STRLOG_Success_arg + "  Speed: {1:d}";
        private const string STRLOG_SuccessTorque_arg2 = STRLOG_Success_arg + "  Torque: {1:d}";
        private const string STRLOG_SuccessActiveFault_arg2 = STRLOG_Success_arg + "  ActiveFault: {1:d}";
        /*
         * String constants for exception messages
         */
        private const string STRERR_FailedActiveFaultReset_arg1 = "Failed to reset active fault - Fault code: {0:d}";
        private const string STRERR_RegisterWriteReadMismatch_arg3 = "Register write/read mismatch: ";
        #endregion

        #region Variables
        private static Dictionary<int, RegisterDescription> RegisterMap;
        //
        private ModbusIpMaster modbusIpMaster;
        private int slaveId;
        private DeviceRedLion.KeepAliveCallback keepAliveCallback;
        #endregion

        #region Properties
        private string lastError;

        public string LastError
        {
            get
            {
                string error = lastError;
                lastError = null;
                return error;
            }
        }
        #endregion

        #region Registers
        /// <summary>
        /// Modbus registers ReadOnly
        /// </summary>
        private enum ModbusRegs_RO
        {
            DriveSpeed = 3000,
            DriveTorque = 3006,
            ActiveFault = 3050,
            LoadTemperature = 3138,
        };

        /// <summary>
        /// Modbus registers Read/Write
        /// </summary>
        private enum ModbusRegs_RW
        {
            ControlWord = 3200,
            ControlSpeed = 3202,
            ControlTorque = 3204,
            SpeedRampTime = 3206,
            MaximumCurrent = 3208,
            MaximumTorque = 3210,
            MinimumTorque = 3212,
        };

        private struct RegisterDescription
        {
            public const int RAW_ZERO = 0;
            public const int RAW_FULL = 1;
            public const int RAW_OFFSET = 2;
            public const int RAW_LENGTH = 3;
            public const int ENG_ZERO = 0;
            public const int ENG_FULL = 1;
            public const int ENG_LENGTH = 2;

            public long[] raw;
            public int[] eng;
            public string name;
            public string units;
            public string format;
            public string comment;

            public RegisterDescription(long raw_zero, long raw_full, long raw_offset, int eng_zero, int eng_full, string name, string units, string format, string comment)
            {
                this.raw = new long[RAW_LENGTH] { raw_zero, raw_full, raw_offset };
                this.eng = new int[ENG_LENGTH] { eng_zero, eng_full };
                this.name = name;
                this.units = units;
                this.format = format;
                this.comment = comment;
            }
        }

        /*
         * Default values for control registers
         */
        public const int DEFAULT_SpeedRampTime = 3;
        public const int DEFAULT_MaximumCurrent = 5500;
        public const int DEFAULT_MinimumTorque = -100;
        public const int DEFAULT_MaximumTorque = 100;
        /*
         * Maximum values for control registers
         */
        public const int SPEED_NoLoad = 1500;
        public const int SPEED_FullLoad = 1480;
        public const int SPEED_LockedRotor = 0;
        public const int SPEED_Synchronous = 1500;
        public const int SPEED_Error = 20;
        public const int MAXIMUM_MaximumCurrent = 10200;

        /*
         * Control register settings
         */
        private const long CW_Default = 0x00000000;
        private const long CW_EnableDrivePower = 0x40000000;
        private const long CW_DisableDrivePower = CW_Default;
        private const long CW_ResetDrive = 0x400009A1;
        private const long CW_StartDriveNoLoad = 0x600008A1;
        private const long CW_StartDriveFullLoad = 0x600008A2;
        private const long CW_StartDriveLockedRotor = 0x500008A2;
        private const long CW_StartDriveSyncSpeed = 0x400008A2;
        private const long CW_StopDriveFullLoad = 0x60000821;
        private const long CW_StopDrive = 0x400008A1;

        /*
         * Command execution delays (seconds)
         */
        private const int DELAY_Reset = 3;
        private const int DELAY_EnableDrivePower = 5;
        private const int DELAY_DisableDrivePower = 5;
        private const int DELAY_ResetDrive = 5;
        private const int DELAY_StartDrive = 5;
        private const int DELAY_StopDrive = 10;
        private const int DELAY_StartDriveSyncSpeed = DELAY_StartDrive * 2;
        private const int DELAY_StartDriveFullLoad = DELAY_StartDrive * 2;
        private const int DELAY_StopDriveFullLoad = DELAY_StopDrive * 2;
        private const int DELAY_SetSpeed = DEFAULT_SpeedRampTime + 2;

        #endregion

        //-----------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modbusIpMaster"></param>
        /// <param name="slaveId"></param>
        public ACDrive(ModbusIpMaster modbusIpMaster, int slaveId, DeviceRedLion.KeepAliveCallback keepAliveCallback)
        {
            const string methodName = "ACDrive";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            this.modbusIpMaster = modbusIpMaster;
            this.slaveId = slaveId;
            this.keepAliveCallback = keepAliveCallback;

            /*
             * Create register map
             */
            if (RegisterMap == null)
            {
                RegisterMap = new Dictionary<int, RegisterDescription>();
                RegisterMap.Add((int)ModbusRegs_RO.DriveSpeed, new RegisterDescription(-3000000, 3000000, 0, -30000, 30000, "Motor Speed", "RPM", "f0", "Drive Speed"));
                RegisterMap.Add((int)ModbusRegs_RO.DriveTorque, new RegisterDescription(0, 16000, 0, 0, 1600, "Motor Torque", "%", "f0", "Drive Torque"));
                RegisterMap.Add((int)ModbusRegs_RO.LoadTemperature, new RegisterDescription(-400, 1600, 0, -40, 160, "Load Temp", "deg-C", "f01", "Load Temperature"));

                RegisterMap.Add((int)ModbusRegs_RW.ControlSpeed, new RegisterDescription(-200000000, 200000000, 0, -3000, 3000, "", "RPM", "", "Control Speed"));
                RegisterMap.Add((int)ModbusRegs_RW.ControlTorque, new RegisterDescription(-200000000, 200000000, 0, -2000, 2000, "", "%", "", "Control Torque"));
                RegisterMap.Add((int)ModbusRegs_RW.SpeedRampTime, new RegisterDescription(0, 1800000, 0, 0, 1800, "", "Secs", "", "Speed Ramp Time"));
                RegisterMap.Add((int)ModbusRegs_RW.MaximumCurrent, new RegisterDescription(0, 3000000, 0, 0, 30000000, "", "Milliamps", "", "Maximum Current"));
                RegisterMap.Add((int)ModbusRegs_RW.MaximumTorque, new RegisterDescription(0, 3000000, 0, 0, 300000, "", "%", "", "Maximum Torque"));
                RegisterMap.Add((int)ModbusRegs_RW.MinimumTorque, new RegisterDescription(49536, 65536, -65535, -1600, 0, "", "%", "", "Minimum Torque"));
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-----------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>bool</returns>
        public bool Initialise()
        {
            const string methodName = "Initialise";
            Logfile.WriteCalled(STR_ClassName, methodName);

            bool success = false;

            try
            {
                /*
                 * Configure registers
                 */
                if (
                    this.SetControlWord(CW_Default, 0) == false ||
                    this.SetControlSpeed(0) == false ||
                    this.SetControlTorque(0) == false ||
                    this.SetSpeedRampTime(DEFAULT_SpeedRampTime) == false ||
                    this.SetMaximumCurrent(DEFAULT_MaximumCurrent) == false ||
                    this.SetMaximumTorque(DEFAULT_MaximumTorque) == false ||
                    this.SetMinimumTorque(DEFAULT_MinimumTorque) == false
                    )
                {
                    throw new ApplicationException(this.lastError);
                }

                /*
                 * Reset drive
                 */
                if (this.ResetDrive() == false)
                {
                    throw new ApplicationException(this.lastError);
                }

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool SetControlSpeed(int speed)
        {
            const string methodName = "SetControlSpeed";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                String.Format(STRLOG_Speed_arg, speed));

            bool success = SetEngValue(ModbusRegs_RW.ControlSpeed, speed, 0);

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool SetControlTorque(int percent)
        {
            const string methodName = "SetControlTorque";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                String.Format(STRLOG_Torque_arg, percent));

            bool success = SetEngValue(ModbusRegs_RW.ControlTorque, percent, 0);

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool SetSpeedRampTime(int seconds)
        {
            const string methodName = "SetSpeedRampTime";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                String.Format(STRLOG_Time_arg, seconds));

            bool success = SetEngValue(ModbusRegs_RW.SpeedRampTime, seconds, 0);

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool SetMaximumCurrent(int current)
        {
            const string methodName = "SetMaximumCurrent";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                String.Format(STRLOG_Current_arg, current));

            bool success = SetEngValue(ModbusRegs_RW.MaximumCurrent, current, 0);

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool SetMinimumTorque(int percent)
        {
            const string methodName = "SetMinimumTorque";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                String.Format(STRLOG_Torque_arg, percent));

            bool success = SetEngValue(ModbusRegs_RW.MinimumTorque, percent, 0);

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool SetMaximumTorque(int percent)
        {
            const string methodName = "SetMaximumTorque";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                String.Format(STRLOG_Torque_arg, percent));

            bool success = SetEngValue(ModbusRegs_RW.MaximumTorque, percent, 0);

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>bool</returns>
        public bool EnableDrivePower()
        {
            const string methodName = "EnableDrivePower";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = this.SetControlWord(CW_EnableDrivePower, DELAY_EnableDrivePower);

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>bool</returns>
        public bool DisableDrivePower()
        {
            const string methodName = "DisableDrivePower";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = this.SetControlWord(CW_DisableDrivePower, DELAY_DisableDrivePower);

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>bool</returns>
        public bool ResetDrive()
        {
            const string methodName = "ResetDrive";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;

            try
            {
                if (this.EnableDrivePower() == false)
                {
                    throw new ApplicationException(this.lastError);
                }

                try
                {
                    int faultCode;
                    if (this.GetActiveFault(out faultCode) == false ||
                        this.SetControlWord(CW_ResetDrive, DELAY_Reset) == false ||
                        this.GetActiveFault(out faultCode) == false)
                    {
                        throw new ApplicationException(this.lastError);
                    }

                    /*
                     * Check that the active fault has been reset
                     */
                    if (faultCode != 0)
                    {
                        throw new ApplicationException(String.Format(STRERR_FailedActiveFaultReset_arg1, faultCode));
                    }
                }
                finally
                {
                    if (this.DisableDrivePower() == false)
                    {
                        throw new ApplicationException(this.lastError);
                    }
                }

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                Trace.WriteLine(ex.Message);
                this.lastError = ex.Message;
            }

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool GetDescriptionSpeed(out string name, out string units, out string format)
        {
            return this.GetDescription(ModbusRegs_RO.DriveSpeed, out name, out units, out format);
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool GetDescriptionTorque(out string name, out string units, out string format)
        {
            return this.GetDescription(ModbusRegs_RO.DriveTorque, out name, out units, out format);
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool GetDescriptionLoadTemperature(out string name, out string units, out string format)
        {
            return this.GetDescription(ModbusRegs_RO.LoadTemperature, out name, out units, out format);
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool GetSpeed(out int value)
        {
            const string methodName = "GetSpeed";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = this.GetEngValue(ModbusRegs_RO.DriveSpeed, out value);

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_SuccessSpeed_arg2, success, value));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool GetTorque(out int value)
        {
            const string methodName = "GetTorque";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = this.GetEngValue(ModbusRegs_RO.DriveTorque, out value);

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_SuccessTorque_arg2, success, value));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool GetLoadTemperature(out int value)
        {
            const string methodName = "GetLoadTemperature";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = this.GetEngValue(ModbusRegs_RO.LoadTemperature, out value);

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_SuccessTorque_arg2, success, value));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool GetActiveFault(out int value)
        {
            const string methodName = "GetActiveFault";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = this.ReadModbusValue((int)ModbusRegs_RO.ActiveFault, out value);

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_SuccessActiveFault_arg2, success, value));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool StartDriveNoLoad()
        {
            const string methodName = "StartDriveNoLoad";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = this.SetControlWord(CW_StartDriveNoLoad, DELAY_StartDrive);

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool StartDriveFullLoad()
        {
            const string methodName = "StartDriveFullLoad";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            /*
             * Start the drive with no load before applying full load
             */
            bool success = this.SetControlWord(CW_StartDriveNoLoad, DELAY_StartDrive);
            if (success == true)
            {
                success = this.SetControlWord(CW_StartDriveFullLoad, DELAY_StartDrive);
            }

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool StartDriveLockedRotor()
        {
            const string methodName = "StartDriveLockedRotor";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = this.SetControlWord(CW_StartDriveLockedRotor, DELAY_StartDrive);

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool StartDriveSyncSpeed()
        {
            const string methodName = "StartDriveSyncSpeed";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = this.SetControlWord(CW_StartDriveSyncSpeed, DELAY_StartDriveSyncSpeed);

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool StopDriveNoLoad()
        {
            const string methodName = "StopDriveNoLoad";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = this.SetControlWord(CW_StopDrive, DELAY_StopDrive);

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool StopDriveFullLoad()
        {
            const string methodName = "StopDriveFullLoad";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            /*
             * Take load off AC drive before stopping drive
             */
            bool success = this.SetControlWord(CW_StopDriveFullLoad, DELAY_StopDrive);
            if (success == true)
            {
                success = this.SetControlWord(CW_StopDrive, DELAY_StopDrive);
            }

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool StopDriveLockedRotor()
        {
            const string methodName = "StopDriveLockedRotor";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = this.SetControlWord(CW_StopDrive, DELAY_StopDrive);

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool StopDriveSyncSpeed()
        {
            const string methodName = "StopDriveSyncSpeed";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = this.SetControlWord(CW_StopDrive, DELAY_StopDrive);

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public void KeepAlive()
        {
            int value;
            this.ReadModbusValue((int)ModbusRegs_RW.ControlWord, out value);
        }

        //===========================================================================================================//

        private bool GetDescription(ModbusRegs_RO register, out string name, out string units, out string format)
        {
            bool success = false;

            try
            {
                /*
                 * Find the register description
                 */
                RegisterDescription registerDescription;
                if (RegisterMap.TryGetValue((int)register, out registerDescription) == false)
                {
                    throw new ApplicationException();
                }

                name = registerDescription.name;
                units = registerDescription.units;
                format = registerDescription.format;

                success = true;
            }
            catch
            {
                name = units = format = null;
            }

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        private bool GetEngValue(ModbusRegs_RO register, out int engValue)
        {
            return this.GetEngValue((int)register, out engValue);
        }

        //-----------------------------------------------------------------------------------------------------------//

        private bool GetEngValue(ModbusRegs_RW register, out int engValue)
        {
            return this.GetEngValue((int)register, out engValue);
        }

        //-----------------------------------------------------------------------------------------------------------//

        private bool GetEngValue(int register, out int engValue)
        {
            bool success = false;

            try
            {
                /*
                 * Find the register description
                 */
                RegisterDescription registerDescription;
                if (RegisterMap.TryGetValue(register, out registerDescription) == false)
                {
                    throw new ApplicationException();
                }

                /*
                 * Read the register
                 */
                int rawValue;
                if (this.ReadModbusValue(register, out rawValue) == false)
                {
                    throw new ApplicationException();
                }

                Trace.WriteLine(String.Format("ReadModbusValue: register={0:d} value={1:d}", register, rawValue));

                /*
                 * Convert the register value to eng. units
                 */
                engValue = this.ConvertRawToEng(rawValue, registerDescription.raw, registerDescription.eng);

                success = true;
            }
            catch
            {
                engValue = 0;
            }

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        private bool SetEngValue(ModbusRegs_RW register, int engValue, int delay)
        {
            return this.SetEngValue((int)register, engValue, delay);
        }

        //-----------------------------------------------------------------------------------------------------------//

        private bool SetEngValue(int register, int engValue, int delay)
        {
            bool success = false;

            try
            {
                /*
                 * Find the register description
                 */
                RegisterDescription registerDescription;
                if (RegisterMap.TryGetValue(register, out registerDescription) == false)
                {
                    throw new ApplicationException();
                }

                /*
                 * Convert the eng. units to a raw register value
                 */
                long rawValue = this.ConvertEngToRaw(engValue, registerDescription.eng, registerDescription.raw);

                Trace.WriteLine(String.Format("SetEngValue: register={0:d} engValue={1:d} {2:s}", register, engValue, registerDescription.units));

                /*
                 * Write the register
                 */
                if (this.WriteModbusValue(register, rawValue, delay) == false)
                {
                    throw new ApplicationException();
                }

                success = true;
            }
            catch
            {
            }

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rawValue"></param>
        /// <param name="raw"></param>
        /// <param name="eng"></param>
        /// <returns>int</returns>
        private int ConvertRawToEng(long rawValue, long[] raw, int[] eng)
        {
            long engValue = rawValue;

            /*
             * Add the offset to the value and scale
             */
            if (eng[RegisterDescription.ENG_FULL] - eng[RegisterDescription.ENG_ZERO] > 0)
            {
                engValue += raw[RegisterDescription.RAW_OFFSET];
                engValue *= eng[RegisterDescription.ENG_FULL] - eng[RegisterDescription.ENG_ZERO];
                engValue /= raw[RegisterDescription.RAW_FULL] - raw[RegisterDescription.RAW_ZERO];
            }

            return (int)engValue;
        }

        //-----------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="engValue"></param>
        /// <param name="eng"></param>
        /// <param name="raw"></param>
        /// <returns>long</returns>
        private long ConvertEngToRaw(int engValue, int[] eng, long[] raw)
        {
            long rawValue = engValue;

            /*
             * Unscale the value and subtract the offset
             */
            if (eng[RegisterDescription.ENG_FULL] - eng[RegisterDescription.ENG_ZERO] > 0)
            {
                rawValue *= raw[RegisterDescription.RAW_FULL] - raw[RegisterDescription.RAW_ZERO];
                rawValue /= eng[RegisterDescription.ENG_FULL] - eng[RegisterDescription.ENG_ZERO];
                rawValue -= raw[RegisterDescription.RAW_OFFSET];
            }

            return rawValue;
        }

        //-----------------------------------------------------------------------------------------------------------//

        private bool SetControlWord(long value, int seconds)
        {
            return this.WriteModbusValue((int)ModbusRegs_RW.ControlWord, value, seconds);
        }

        //-----------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="register"></param>
        /// <param name="value"></param>
        /// <param name="seconds"></param>
        /// <returns>bool</returns>
        private bool WriteModbusValue(int register, long value, int seconds)
        {
            bool success = false;

            Trace.Write(String.Format("WriteModbusValue: register={0:d}  value=0x{1:X08} ({2:d})  delay={3:d}secs", register, value, value, seconds));

            try
            {
                this.lastError = null;

                /*
                 * Convert value to array of ushort
                 */
                ushort[] outregs = new ushort[2] { (ushort)value, (ushort)(value >> 16) };

                /*
                 * Write the value to the registers
                 */
                this.modbusIpMaster.WriteMultipleRegisters((byte)this.slaveId, (ushort)register, outregs);

                /*
                 * Read the value back and compare it with the value written
                 */
                ushort[] inregs = this.modbusIpMaster.ReadHoldingRegisters((byte)this.slaveId, (ushort)register, (ushort)2);
                long invalue = (inregs[1] << 16) | inregs[0];

                Trace.WriteLine(String.Format("  ReadBack: value=0x{0:X08} ({1:d})", invalue, invalue, seconds));

                if (invalue != value)
                {
                    throw new ApplicationException(String.Format(STRERR_RegisterWriteReadMismatch_arg3, (int)register, value, invalue));
                }

                /*
                 * Wait for action to occur
                 */
                this.WaitDelay(seconds);

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                Trace.WriteLine(ex.Message);
                this.lastError = ex.Message;
            }

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="register"></param>
        /// <param name="value"></param>
        /// <returns>bool</returns>
        private bool ReadModbusValue(int register, out int value)
        {
            bool success = false;

            value = 0;

            try
            {
                this.lastError = null;

                /*
                 * Read the value from the registers
                 */
                ushort[] inregs = this.modbusIpMaster.ReadHoldingRegisters((byte)this.slaveId, (ushort)register, (ushort)2);
                value = (inregs[1] << 16) | inregs[0];

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                Trace.WriteLine(ex.Message);
                this.lastError = ex.Message;
            }

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seconds"></param>
        /// <returns>bool</returns>
        private bool WaitDelay(int seconds)
        {
            bool success = true;

            try
            {
                for (int i = 0; i < seconds; i++)
                {
                    Trace.Write(".");
                    Delay.MilliSeconds(1000);

                    if (this.keepAliveCallback != null)
                    {
                        this.keepAliveCallback();
                    }
                }

                success = true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

            return success;
        }

    }
}
