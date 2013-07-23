using System;
using Library.Lab;
using Modbus.Device;

namespace Library.LabEquipment.Devices
{
    public class PowerMeterMut : PowerMeter
    {
        #region Constants
        private const string STR_ClassName = "PowerMeterMut";
        /*
         * Constants
         */
        private const int INT_ModbusRegs_RO_BaseAddress = 2000;
        #endregion

        //-------------------------------------------------------------------------------------------------//

        public PowerMeterMut(ModbusIpMaster modbusIpMaster, int slaveId)
            : base(modbusIpMaster, slaveId)
        {
            const string methodName = "PowerMeterMut";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            /*
             * Nothing to do here
             */

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-----------------------------------------------------------------------------------------------------------//

        public override bool Initialise()
        {
            const string methodName = "Initialise";
            Logfile.WriteCalled(STR_ClassName, methodName);

            bool success = false;

            try
            {
                float value;
                success =
                    this.GetVoltage(out value) &&
                    this.GetCurrent(out value) &&
                    this.GetPowerFactor(out value);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool GetVoltage(out float value)
        {
            return this.GetVoltage(INT_ModbusRegs_RO_BaseAddress, out value);
        }

        //-------------------------------------------------------------------------------------------------//

        public bool GetCurrent(out float value)
        {
            return this.GetCurrent(INT_ModbusRegs_RO_BaseAddress, out value);
        }

        //-------------------------------------------------------------------------------------------------//

        public bool GetPowerFactor(out float value)
        {
            return this.GetPowerFactor(INT_ModbusRegs_RO_BaseAddress, out value);
        }

    }
}
