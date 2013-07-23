using System;
using System.Collections.Generic;
using System.Text;
using Library.Lab;
using Library.LabEquipment.Engine;
using Library.Lab.Types;
using Library.LabEquipment.Devices;

namespace Library.LabEquipment.Drivers
{
    public class DriverSimAbsorbersNoDelay : DriverAbsorbers
    {
        #region Constants
        private const String STR_ClassName = "DriverSimAbsorbersNoDelay";
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
        public DriverSimAbsorbersNoDelay(LabEquipmentConfiguration labEquipmentConfiguration)
            : base(labEquipmentConfiguration)
        {
            const String methodName = "DriverSimAbsorbersNoDelay";
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
                 * Check that the equipment does indeed have absorbers
                 */
                if (this.deviceFlexMotion.AbsorbersPresent == false)
                {
                    throw new ApplicationException(STRERR_EquipmentHasNoAbsorbers);
                }

                /*
                 * Check the setup Id
                 */
                String setupId = this.experimentSpecification.SetupId;
                switch (setupId)
                {
                    case Consts.STRXML_SetupId_SimActivityVsAbsorberNoDelay:
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
            ((DeviceSerialLcdSimulation)this.deviceSerialLcd).DelaysSimulated = false;

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
            ((DeviceSerialLcdSimulation)this.deviceSerialLcd).DelaysSimulated = true;

            return base.ExecuteFinalising();
        }
    }
}
