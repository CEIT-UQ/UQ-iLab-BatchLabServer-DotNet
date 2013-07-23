using System;
using Library.Lab;
using Library.Lab.Types;
using Library.LabEquipment.Devices;
using Library.LabEquipment.Engine;

namespace Library.LabEquipment.Drivers
{
    public class DriverSimActivityNoDelay : DriverRadioactivity
    {
        #region Constants
        private const String STR_ClassName = "DriverSimActivityNoDelay";
        private const Logfile.Level logLevel = Logfile.Level.Finer;
        #endregion

        #region Properties
        public override String DriverName
        {
            get { return STR_ClassName; }
        }
        #endregion

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="labEquipmentConfiguration"></param>
        public DriverSimActivityNoDelay(LabEquipmentConfiguration labEquipmentConfiguration)
            : base(labEquipmentConfiguration)
        {
            const String methodName = "DriverSimActivityNoDelay";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            /*
             * Nothing to do here
             */

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlSpecification"></param>
        /// <returns>Validation</returns>
        public override Validation Validate(String xmlSpecification)
        {
            const String methodName = "Validate";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            Validation validation;

            try
            {
                /*
                 * Check that parameters are valid
                 */
                validation = base.Validate(xmlSpecification);

                /*
                 * Check the setup Id
                 */
                String setupId = this.experimentSpecification.SetupId;
                switch (setupId)
                {
                    case Consts.STRXML_SetupId_SimActivityVsTimeNoDelay:
                    case Consts.STRXML_SetupId_SimActivityVsDistanceNoDelay:
                        /*
                         * Check that the correct devices have been set
                         */
                        if (this.deviceFlexMotion.GetType().Equals(typeof(DeviceFlexMotionSimulation)) == false)
                        {
                            throw new NullReferenceException(String.Format(STRERR_InvalidDeviceInstance_arg, DeviceFlexMotionSimulation.ClassName));
                        }
                        if (this.deviceST360Counter.GetType().Equals(typeof(DeviceST360CounterSimulation)) == false)
                        {
                            throw new NullReferenceException(String.Format(STRERR_InvalidDeviceInstance_arg, DeviceST360CounterSimulation.ClassName));
                        }
                        if ((this.deviceSerialLcd.GetType().Equals(typeof(DeviceSerialLcdSimulation)) == false) &&
                            (this.deviceSerialLcd.GetType().Equals(typeof(DeviceSerialLcd)) == false))
                        {
                            throw new NullReferenceException(String.Format(STRERR_InvalidDeviceInstance_arg, DeviceSerialLcd.ClassName));
                        }

                        /*
                         * Specification is valid - execution time, one is the smallest number
                         */
                        validation = new Validation(true, 1);
                        break;

                    default:
                        /*
                         * Don't throw an exception, a derived class will want to check the setup Id
                         */
                        validation = new Validation(String.Format(STRERR_InvalidSetupId_arg, setupId));
                        break;
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
                validation = new Validation(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Validation_arg3,
                    validation.Accepted, validation.ExecutionTime, validation.ErrorMessage));

            return validation;
        }

        //-------------------------------------------------------------------------------------------------//

        protected override bool ExecuteInitialising()
        {
            /*
             * Turn off simulated delays
             */
            ((DeviceFlexMotionSimulation)this.deviceFlexMotion).DelaysSimulated = false;
            ((DeviceST360CounterSimulation)this.deviceST360Counter).DelaysSimulated = false;
            if (this.deviceSerialLcd.GetType().Equals(typeof(DeviceSerialLcdSimulation)) == true)
            {
                ((DeviceSerialLcdSimulation)this.deviceSerialLcd).DelaysSimulated = false;
            }

            return base.ExecuteInitialising();
        }

        //-------------------------------------------------------------------------------------------------//

        protected override bool ExecuteFinalising()
        {
            /*
             * Turn simulated delays back on
             */
            ((DeviceFlexMotionSimulation)this.deviceFlexMotion).DelaysSimulated = true;
            ((DeviceST360CounterSimulation)this.deviceST360Counter).DelaysSimulated = true;
            if (this.deviceSerialLcd.GetType().Equals(typeof(DeviceSerialLcdSimulation)) == true)
            {
                ((DeviceSerialLcdSimulation)this.deviceSerialLcd).DelaysSimulated = true;
            }

            return base.ExecuteFinalising();
        }

    }
}
