using System;
using System.Collections.Generic;
using System.Diagnostics;
using Library.Lab;
using Library.Unmanaged;
using Modbus.Device;

namespace Library.LabEquipment.Devices
{
    public class PowerMeter
    {
        #region Constants
        private const string STR_ClassName = "PowerMeter";
        protected const Logfile.Level logLevel = Logfile.Level.Finer;
        /*
         * String constants for logfile messages
         */
        protected const string STRLOG_Success_arg = "Success: {0:s}";
        protected const string STRLOG_SuccessVoltage_arg2 = "Success: {0:s}  Voltage: {1:f}";
        protected const string STRLOG_SuccessCurrent_arg2 = "Success: {0:s}  Current: {1:f}";
        protected const string STRLOG_SuccessPowerFactor_arg2 = "Success: {0:s}  PowerFactor: {1:f}";
        #endregion

        #region Variables
        protected static Dictionary<int, RegisterDescription> RegisterMap;
        //
        private ModbusIpMaster modbusIpMaster;
        private int slaveId;
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

        #region Types
        protected enum ModbusRegs_RO
        {
            Phase1ToNeutralVoltageTHD = 0, // THD = Total Harmonic Distortion
            Phase2ToNeutralVoltageTHD = 2,
            Phase3ToNeutralVoltageTHD = 4,
            Phase1ToPhase2VoltageTHD = 6,
            Phase2ToPhase3VoltageTHD = 8,
            Phase3ToPhase1VoltageTHD = 10,
            Phase1CurrentTHD = 12,
            Phase2CurrentTHD = 14,
            Phase3CurrentTHD = 16,
            FrequencyPhase1 = 18,
            Phase1ToNeutralVoltageRMS = 20, // RMS = Root Mean Square
            Phase2ToNeutralVoltageRMS = 22,
            Phase3ToNeutralVoltageRMS = 24,
            Phase1ToPhase2VoltageRMS = 26,
            Phase2ToPhase3VoltageRMS = 28,
            Phase3ToPhase1VoltageRMS = 30,
            Phase1CurrentRMS = 32,
            Phase2CurrentRMS = 34,
            Phase3CurrentRMS = 36,
            NeutralCurrentRMS = 38,
            Phase1ActivePower = 40,
            Phase2ActivePower = 42,
            Phase3ActivePower = 44,
            Phase1ReactivePower = 46,
            Phase2ReactivePower = 48,
            Phase3ReactivePower = 50,
            Phase1ApparentPower = 52,
            Phase2ApparentPower = 54,
            Phase3ApparentPower = 56,
            Phase1PowerFactor = 58,
            Phase2PowerFactor = 60,
            Phase3PowerFactor = 62,
            PhaseToNeutralVoltageMeanTHD = 64,
            PhaseToPhaseVoltageMeanTHD = 66,
            PhaseCurrentMeanTHD = 68,
            PhaseToNeutralVoltageMeanRMS = 70,
            PhaseToPhaseVoltageMeanRMS = 72,
            ThreePhaseCurrentRMS = 74,
            TotalActivePower = 76,
            TotalReactivePower = 78,
            TotalApparentPower = 80,
            TotalPowerFactor = 82,
        }

        protected struct RegisterDescription
        {
            public string name;
            public string units;
            public string format;
            public string comment;

            public RegisterDescription(string name, string units, string format, string comment)
            {
                this.name = name;
                this.units = units;
                this.format = format;
                this.comment = comment;
            }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="modbusIpMaster"></param>
        /// <param name="slaveId"></param>
        public PowerMeter(ModbusIpMaster modbusIpMaster, int slaveId)
        {
            const string methodName = "PowerMeter";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            this.modbusIpMaster = modbusIpMaster;
            this.slaveId = slaveId;

            /*
             * Create register map
             */
            if (RegisterMap == null)
            {
                RegisterMap = new Dictionary<int, RegisterDescription>();
                RegisterMap.Add((int)ModbusRegs_RO.PhaseToPhaseVoltageMeanRMS, new RegisterDescription("Ph-Ph Voltage", "Volts", "f01", "Phase to Phase Voltage, Mean RMS Amplitude"));
                RegisterMap.Add((int)ModbusRegs_RO.ThreePhaseCurrentRMS, new RegisterDescription("Phase Current", "Amps", "f03", "Three Phase Current, RMS Amplitude"));
                RegisterMap.Add((int)ModbusRegs_RO.TotalPowerFactor, new RegisterDescription("Power Factor", "", "f01", "Total Power Factor (Imp/Exp)"));
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-----------------------------------------------------------------------------------------------------------//

        public virtual bool Initialise()
        {
            const string methodName = "Initialise";
            Logfile.WriteCalled(STR_ClassName, methodName);

            bool success = false;

            try
            {
                /*
                 * Nothing to do here
                 */

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

        public bool GetDescriptionVoltage(out string name, out string units, out string format)
        {
            return this.GetDescription(ModbusRegs_RO.PhaseToPhaseVoltageMeanRMS, out name, out units, out format);
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool GetDescriptionCurrent(out string name, out string units, out string format)
        {
            return this.GetDescription(ModbusRegs_RO.ThreePhaseCurrentRMS, out name, out units, out format);
        }

        //-----------------------------------------------------------------------------------------------------------//

        public bool GetDescriptionPowerFactor(out string name, out string units, out string format)
        {
            return this.GetDescription(ModbusRegs_RO.TotalPowerFactor, out name, out units, out format);
        }

        //-------------------------------------------------------------------------------------------------//

        public bool GetVoltage(int baseAddress, out float value)
        {
            const string methodName = "GetVoltage";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            int register = (int)ModbusRegs_RO.PhaseToPhaseVoltageMeanRMS + baseAddress;
            bool success = this.ReadModbusValue(register, out value);

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_SuccessVoltage_arg2, success, value));

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool GetCurrent(int baseAddress, out float value)
        {
            const string methodName = "GetCurrent";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            int register = (int)ModbusRegs_RO.ThreePhaseCurrentRMS + baseAddress;
            bool success = this.ReadModbusValue(register, out value);

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_SuccessCurrent_arg2, success, value));

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool GetPowerFactor(int baseAddress, out float value)
        {
            const string methodName = "GetPowerFactor";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            int register = (int)ModbusRegs_RO.TotalPowerFactor + baseAddress;
            bool success = this.ReadModbusValue(register, out value);

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_SuccessPowerFactor_arg2, success, value));

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="register"></param>
        /// <param name="value"></param>
        /// <returns>bool</returns>
        protected bool ReadModbusValue(int register, out float value)
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
                value = Conversion.ToFloat((inregs[1] << 16) | inregs[0]);

                Trace.WriteLine(String.Format("ReadModbusValue: register={0:d} value={1:f04}", register, value));

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

    }
}
