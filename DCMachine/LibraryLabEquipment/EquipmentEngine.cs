using System;
using Library.Lab;
using Library.LabEquipment.Engine;
using Library.LabEquipment.Engine.Drivers;
using Library.LabEquipment.Devices;
using Library.LabEquipment.Drivers;

namespace Library.LabEquipment
{
    public class EquipmentEngine : LabEquipmentEngine
    {
        #region Constants
        private const string STR_ClassName = "EquipmentEngine";
        private const Logfile.Level logLevel = Logfile.Level.Info;
        #endregion

        #region Variables
        private DeviceRedLion deviceRedLion;
        #endregion

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="labEquipmentConfiguration"></param>
        public EquipmentEngine(LabEquipmentConfiguration labEquipmentConfiguration)
            : base(labEquipmentConfiguration)
        {
            const string methodName = "EquipmentEngine";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            try
            {
                /*
                 * Create instances of the equipment devices
                 */
                this.deviceRedLion = new DeviceRedLion(this.labEquipmentConfiguration);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="setupId"></param>
        /// <returns>DriverGeneric</returns>
        protected override DriverGeneric GetDriver(string setupId)
        {
            const string methodName = "GetDriver";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_SetupId_arg, setupId));

            DriverGeneric driverGeneric = null;

            /*
             * Create an instance of the driver for the specified setup Id
             */
            switch (setupId)
            {
                case Consts.STRXML_SetupId_VoltageVsSpeed:
                    driverGeneric = new DriverVoltageVsSpeed(this.labEquipmentConfiguration);
                    break;

                case Consts.STRXML_SetupId_VoltageVsField:
                    driverGeneric = new DriverVoltageVsField(this.labEquipmentConfiguration);
                    break;

                case Consts.STRXML_SetupId_VoltageVsLoad:
                    driverGeneric = new DriverVoltageVsLoad(this.labEquipmentConfiguration);
                    break;

                case Consts.STRXML_SetupId_SpeedVsVoltage:
                    driverGeneric = new DriverSpeedVsVoltage(this.labEquipmentConfiguration);
                    break;

                case Consts.STRXML_SetupId_SpeedVsField:
                    driverGeneric = new DriverSpeedVsField(this.labEquipmentConfiguration);
                    break;

                default:
                    driverGeneric = base.GetDriver(setupId);
                    break;
            }

            /*
             * If a driver instance was created, set the devices in the driver instance
             */
            if (driverGeneric != null)
            {
                ((DriverEquipment)driverGeneric).SetDeviceRedLion(this.deviceRedLion);
            }
            else
            {
                driverGeneric = base.GetDriver(setupId);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    driverGeneric.DriverName);

            return driverGeneric;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>bool</returns>
        protected override bool PowerupEquipment()
        {
            const string methodName = "PowerupEquipment";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

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
                Logfile.WriteError(ex.ToString());
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>bool</returns>
        protected override bool InitialiseEquipment()
        {
            const string methodName = "InitialiseEquipment";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;

            try
            {
                /*
                 * Initialise the equipment devices
                 */
                success = this.deviceRedLion.Initialise();
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>bool</returns>
        protected override bool PowerdownEquipment()
        {
            const string methodName = "PowerdownEquipment";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

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
                Logfile.WriteError(ex.ToString());
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }
    }
}
