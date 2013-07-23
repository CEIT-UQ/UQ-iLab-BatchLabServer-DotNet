using System;
using System.Collections.Generic;
using System.Diagnostics;
using Library.Lab;
using Library.Lab.Utilities;
using Modbus.Device;

namespace Library.LabEquipment.Devices
{
    public class DCDrive
    {
        #region Constants
        private const string STR_ClassName = "DCDrive";
        private const Logfile.Level logLevel = Logfile.Level.Finer;
        /*
         * String constants for logfile messages
         */
        private const string STRLOG_Speed_arg = "Speed: {0:d} RPM";
        private const string STRLOG_Torque_arg = "Torque: {0:d} %";
        private const string STRLOG_Field_arg = "Field: {0:d} %";
        private const string STRLOG_Time_arg = "Time: {0:d} seconds";
        private const string STRLOG_Success_arg = "Success: {0:s}";
        private const string STRLOG_SuccessSpeed_arg2 = "Success: {0:s}  Speed: {1:f}";
        private const string STRLOG_SuccessTorque_arg2 = "Success: {0:s}  Torque: {1:f}";
        private const string STRLOG_SuccessArmatureVoltage_arg2 = "Success: {0:s}  Armature Voltage: {1:f}";
        private const string STRLOG_SuccessFieldCurrent_arg2 = "Success: {0:s}  Field Current: {1:f}";
        private const string STRLOG_SuccessActiveFault_arg2 = "Success: {0:s}  ActiveFault: {1:d}";
        /*
         * String constants for exception messages
         */
        private const string STRERR_FailedActiveFaultReset_arg1 = "Failed to reset active fault - Fault code: {0:d}";
        private const string STRERR_RegisterWriteReadMismatch_arg3 = "Register write/read mismatch: {0:d} {1:d} {2:d}";
        #endregion

        #region Variables
        private static Dictionary<int, RegisterDescription> RegisterMap;
        private ModbusIpMaster modbusIpMaster;
        private int slaveId;
        private DeviceRedLion.KeepAliveCallback keepAliveCallback;
        #endregion

        #region Properties
        public static string ClassName
        {
            get { return STR_ClassName; }
        }

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

        public int ChangeSpeedTime
        {
            get { return DELAY_ChangeSpeed; }
        }

        public int ChangeTorqueTime
        {
            get { return DELAY_ChangeTorque; }
        }

        public int ChangeFieldTime
        {
            get { return DELAY_ChangeField; }
        }
        #endregion

        #region Registers
        /// <summary>
        /// Modbus registers ReadOnly
        /// </summary>
        private enum ModbusRegs_RO
        {
            SpeedEncoder = 4002,
            Torque = 4007,
            ArmatureVoltage = 4013,
            FieldCurrent = 4024,
        };

        /// <summary>
        /// Modbus registers Read/Write
        /// </summary>
        private enum ModbusRegs_RW
        {
            ControlWord = 4200,
            ControlSpeed = 4201,
            ControlTorque = 4202,
            MinSpeedLimit = 4203,
            MaxSpeedLimit = 4204,
            MaxTorqueLimit = 4205,
            MinTorqueLimit = 4206,
            SpeedRampTime = 4207,
            FieldLimit = 4208,
            FieldTrim = 4209,
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

            public int[] raw;
            public int[] eng;
            public string name;
            public string units;
            public string format;
            public string comment;

            public RegisterDescription(int raw_zero, int raw_full, int raw_offset, int eng_zero, int eng_full, string name, string units, string format, string comment)
            {
                this.raw = new int[RAW_LENGTH] { raw_zero, raw_full, raw_offset };
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
        public const int DEFAULT_MinSpeedLimit = -1500;
        public const int DEFAULT_MaxSpeedLimit = 1500;
        public const int DEFAULT_MinTorqueLimit = -100;
        public const int DEFAULT_MaxTorqueLimit = 100;
        public const int DEFAULT_SpeedRampTime = 5;
        public const int DEFAULT_FieldLimit = 100;
        public const int DEFAULT_FieldTrim = 0;

        /*
         * Minimum/maximum values for control registers
         */
        public const int SPEED_Error = 15;
        public const int MINIMUM_Speed = -1500;
        public const int MAXIMUM_Speed = 1500;
        public const int MAXIMUM_MaximumCurrent = 10000;
        public const int MINIMUM_MinimumTorque = -50;
        public const int MINIMUM_MaximumTorque = 50;

        /*
         * Control register settings
         */
        private const int CW_Default = 0x0000;
        private const int CW_ResetDriveFault = 0x04F6;
        private const int CW_ResetDrive = 0x0476;
        private const int CW_MainContactorOn = 0x0477;
        private const int CW_StartDrive = 0x047F;
        private const int CW_StartDriveTorque = 0x147F;

        /*
         * Command execution delays (seconds)
         */
        private const int DELAY_ResetDriveFault = 5;
        private const int DELAY_ResetDrive = 5;
        private const int DELAY_SetMainContactorOn = 5;
        private const int DELAY_StartDrive = 5;
        private const int DELAY_StartDriveTorque = 5;
        private const int DELAY_ChangeSpeed = DEFAULT_SpeedRampTime + 2;
        private const int DELAY_ChangeTorque = 5;
        private const int DELAY_ChangeField = 5;

        #endregion

        //-----------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modbusIpMaster"></param>
        /// <param name="slaveId"></param>
        public DCDrive(ModbusIpMaster modbusIpMaster, int slaveId, DeviceRedLion.KeepAliveCallback keepAliveCallback)
        {
            const string methodName = "DCDrive";
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
                RegisterMap.Add((int)ModbusRegs_RO.SpeedEncoder, new RegisterDescription(0, 2000, 0, 0, 2000, "Motor Speed", "RPM", "f0", "Actual speed measured with pulse encoder"));
                RegisterMap.Add((int)ModbusRegs_RO.Torque, new RegisterDescription(0, 10000, 0, 0, 100, "Motor Torque", "%", "f01", "Motor torque"));
                RegisterMap.Add((int)ModbusRegs_RO.ArmatureVoltage, new RegisterDescription(-400, 400, 0, -400, 400, "Armature Voltage", "Volts", "", "Actual armature voltage"));
                RegisterMap.Add((int)ModbusRegs_RO.FieldCurrent, new RegisterDescription(0, 200, 0, 0, 2, "Field Current", "Amps", "f02", "Actual field current"));

                RegisterMap.Add((int)ModbusRegs_RW.ControlSpeed, new RegisterDescription(-20000, 20000, 0, -1500, 1500, "", "RPM", "f01", "Control Speed"));
                RegisterMap.Add((int)ModbusRegs_RW.ControlTorque, new RegisterDescription(-32700, 32700, 0, -327, 327, "", "%", "f01", "Control Torque"));
                RegisterMap.Add((int)ModbusRegs_RW.MinSpeedLimit, new RegisterDescription(-10000, 0, 0, -10000, 0, "", "RPM", "", "Minimum Speed Limit"));
                RegisterMap.Add((int)ModbusRegs_RW.MaxSpeedLimit, new RegisterDescription(0, 10000, 0, 0, 10000, "", "RPM", "", "Maximum Speed Limit"));
                RegisterMap.Add((int)ModbusRegs_RW.MaxTorqueLimit, new RegisterDescription(0, 32500, 0, 0, 325, "", "%", "", "Maximum Torque Limit"));
                RegisterMap.Add((int)ModbusRegs_RW.MinTorqueLimit, new RegisterDescription(-32500, 0, 0, -325, 0, "", "%", "", "Minimum Torque Limit"));
                RegisterMap.Add((int)ModbusRegs_RW.SpeedRampTime, new RegisterDescription(0, 30000, 0, 0, 300, "", "Secs", "", "Speed Ramp Time"));
                RegisterMap.Add((int)ModbusRegs_RW.FieldLimit, new RegisterDescription(0, 10000, 0, 0, 100, "", "%", "", "Field Limit"));
                RegisterMap.Add((int)ModbusRegs_RW.FieldTrim, new RegisterDescription(-20, 20, 0, -20, 20, "", "%", "", "Field Trim"));
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool Initialise()
        {
            const string methodName = "Initialise";
            Logfile.WriteCalled(STR_ClassName, methodName);

            bool success = false;

            try
            {
                /*
                 * Reset drive
                 */
                if (this.ResetDrive() == false)
                {
                    throw new ApplicationException(this.lastError);
                }

                /*
                 * Configure registers
                 */
                if (this.SetControlSpeed(0) &&
                    this.SetControlTorque(0) &&
                    this.SetMinimumSpeedLimit(DEFAULT_MinSpeedLimit) &&
                    this.SetMaximumSpeedLimit(DEFAULT_MaxSpeedLimit) &&
                    this.SetMaximumTorqueLimit(DEFAULT_MaxTorqueLimit) &&
                    this.SetMinimumTorqueLimit(DEFAULT_MinTorqueLimit) &&
                    this.SetSpeedRampTime(DEFAULT_SpeedRampTime) &&
                    this.SetFieldLimit(DEFAULT_FieldLimit) == false)
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

        public bool SetMinimumSpeedLimit(int speed)
        {
            const string methodName = "SetMinimumSpeedLimit";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                String.Format(STRLOG_Speed_arg, speed));

            bool success = SetEngValue(ModbusRegs_RW.MinSpeedLimit, speed, 0);

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool SetMaximumSpeedLimit(int speed)
        {
            const string methodName = "SetMaximumSpeedLimit";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                String.Format(STRLOG_Speed_arg, speed));

            bool success = SetEngValue(ModbusRegs_RW.MaxSpeedLimit, speed, 0);

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool SetMaximumTorqueLimit(int percent)
        {
            const string methodName = "SetMaximumTorqueLimit";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                String.Format(STRLOG_Torque_arg, percent));

            bool success = SetEngValue(ModbusRegs_RW.MaxTorqueLimit, percent, 0);

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool SetMinimumTorqueLimit(int percent)
        {
            const string methodName = "SetMinimumTorqueLimit";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                String.Format(STRLOG_Torque_arg, percent));

            bool success = SetEngValue(ModbusRegs_RW.MinTorqueLimit, percent, 0);

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

        public bool SetFieldLimit(int percent)
        {
            const string methodName = "SetFieldLimit";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                String.Format(STRLOG_Field_arg, percent));

            bool success = SetEngValue(ModbusRegs_RW.FieldLimit, percent, 0);

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
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
                if (this.SetControlWord(CW_ResetDriveFault, DELAY_ResetDriveFault) == false ||
                    this.SetControlWord(CW_ResetDrive, DELAY_ResetDrive) == false)
                {
                    throw new ApplicationException();
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
            return this.GetDescription(ModbusRegs_RO.SpeedEncoder, out name, out units, out format);
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool GetDescriptionTorque(out string name, out string units, out string format)
        {
            return this.GetDescription(ModbusRegs_RO.Torque, out name, out units, out format);
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool GetDescriptionArmatureVoltage(out string name, out string units, out string format)
        {
            return this.GetDescription(ModbusRegs_RO.ArmatureVoltage, out name, out units, out format);
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool GetDescriptionFieldCurrent(out string name, out string units, out string format)
        {
            return this.GetDescription(ModbusRegs_RO.FieldCurrent, out name, out units, out format);
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool GetSpeed(out float value)
        {
            const string methodName = "GetSpeed";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = this.GetEngValue(ModbusRegs_RO.SpeedEncoder, out value);

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_SuccessSpeed_arg2, success, value));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool GetTorque(out float value)
        {
            const string methodName = "GetTorque";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = this.GetEngValue(ModbusRegs_RO.Torque, out value);

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_SuccessTorque_arg2, success, value));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool GetArmatureVoltage(out float value)
        {
            const string methodName = "GetSpeed";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = this.GetEngValue(ModbusRegs_RO.ArmatureVoltage, out value);

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_SuccessArmatureVoltage_arg2, success, value));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool GetFieldCurrent(out float value)
        {
            const string methodName = "GetFieldCurrent";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = this.GetEngValue(ModbusRegs_RO.FieldCurrent, out value);

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_SuccessFieldCurrent_arg2, success, value));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool EnableDrive()
        {
            const string methodName = "EnableDrive";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = this.SetControlWord(CW_MainContactorOn, DELAY_StartDrive);

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool StartDrive()
        {
            const string methodName = "StartDrive";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = this.SetControlWord(CW_StartDrive, DELAY_StartDrive);

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool StartDriveTorque()
        {
            const string methodName = "StartDriveTorque";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = this.SetControlWord(CW_StartDriveTorque, DELAY_StartDriveTorque);

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool StopDrive()
        {
            const string methodName = "StopDrive";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = this.SetControlWord(CW_ResetDrive, DELAY_ResetDrive);

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool ChangeSpeed(int speed)
        {
            const string methodName = "ChangeSpeed";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                String.Format(STRLOG_Speed_arg, speed));

            bool success = this.SetEngValue(ModbusRegs_RW.ControlSpeed, speed, DELAY_ChangeSpeed);

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool ChangeTorque(int percent)
        {
            const string methodName = "ChangeTorque";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                String.Format(STRLOG_Torque_arg, percent));

            bool success = this.SetEngValue(ModbusRegs_RW.ControlTorque, percent, DELAY_ChangeTorque);

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool ChangeField(int percent)
        {
            const string methodName = "ChangeField";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                String.Format(STRLOG_Field_arg, percent));

            bool success = this.SetEngValue(ModbusRegs_RW.FieldLimit, percent, DELAY_ChangeField);

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
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

        private bool GetEngValue(ModbusRegs_RO register, out float engValue)
        {
            return this.GetEngValue((int)register, out engValue);
        }

        //-----------------------------------------------------------------------------------------------------------//

        private bool GetEngValue(ModbusRegs_RW register, out float engValue)
        {
            return this.GetEngValue((int)register, out engValue);
        }

        //-----------------------------------------------------------------------------------------------------------//

        private bool GetEngValue(int register, out float engValue)
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
                if (this.ReadModbusHoldingRegister(register, out rawValue) == false)
                {
                    throw new ApplicationException();
                }

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
                int rawValue = this.ConvertEngToRaw(engValue, registerDescription.eng, registerDescription.raw);

                Trace.WriteLine(String.Format("SetEngValue: register={0:d} engValue={1:d} {2:s}", register, engValue, registerDescription.units));

                /*
                 * Write the register
                 */
                if (this.WriteModbusHoldingRegister(register, rawValue, delay) == false)
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
        private float ConvertRawToEng(int rawValue, int[] raw, int[] eng)
        {
            float engValue = rawValue;

            /*
             * Add the offset to the value and scale
             */
            if (eng[RegisterDescription.ENG_FULL] - eng[RegisterDescription.ENG_ZERO] > 0)
            {
                engValue += raw[RegisterDescription.RAW_OFFSET];
                engValue *= eng[RegisterDescription.ENG_FULL] - eng[RegisterDescription.ENG_ZERO];
                engValue /= raw[RegisterDescription.RAW_FULL] - raw[RegisterDescription.RAW_ZERO];
            }

            return engValue;
        }

        //-----------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="engValue"></param>
        /// <param name="eng"></param>
        /// <param name="raw"></param>
        /// <returns>int</returns>
        private int ConvertEngToRaw(int engValue, int[] eng, int[] raw)
        {
            int rawValue = engValue;

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

        private bool SetControlWord(int value, int seconds)
        {
            return this.WriteModbusHoldingRegister((int)ModbusRegs_RW.ControlWord, value, seconds);
        }

        //-----------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="register"></param>
        /// <param name="value"></param>
        /// <param name="seconds"></param>
        /// <returns>bool</returns>
        private bool WriteModbusHoldingRegister(int register, int value, int seconds)
        {
            bool success = false;

            Trace.WriteLine(String.Format("WriteModbusHoldingRegister: register={0:d}  value=0x{1:X04} ({2:d})  delay={3:d}secs", register, (ushort)value, (ushort)value, seconds));

            try
            {
                this.lastError = null;

                /*
                 * Convert value to array of ushort
                 */
                ushort[] outregs = new ushort[1] { (ushort)value };

                /*
                 * Write the value to the register
                 */
                this.modbusIpMaster.WriteMultipleRegisters((byte)this.slaveId, (ushort)register, outregs);

                /*
                 * Read the value back and compare it with the value written
                 */
                ushort[] inregs = this.modbusIpMaster.ReadHoldingRegisters((byte)this.slaveId, (ushort)register, (ushort)1);
                int invalue = inregs[0];

                Trace.WriteLine(String.Format("  ReadBack: value=0x{0:X04} ({1:d})", invalue, invalue));

                if (invalue != (ushort)value)
                {
                    throw new ApplicationException(String.Format(STRERR_RegisterWriteReadMismatch_arg3, register, (ushort)value, invalue));
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
        private bool ReadModbusHoldingRegister(int register, out int value)
        {
            bool success = false;

            value = 0;

            try
            {
                this.lastError = null;

                /*
                 * Read the value from the registers
                 */
                ushort[] inregs = this.modbusIpMaster.ReadHoldingRegisters((byte)this.slaveId, (ushort)register, (ushort)1);
                value = (short)inregs[0];

                Trace.WriteLine(String.Format("ReadModbusHoldingRegister: register={0:d}  value=0x{1:X04} ({2:d})", register, value, value));

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
