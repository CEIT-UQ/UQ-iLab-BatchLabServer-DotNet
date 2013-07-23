using System;
using System.Diagnostics;
using System.Xml;
using Library.Lab;
using Library.Lab.Types;
using Library.LabEquipment.Devices;
using Library.LabEquipment.Engine;
using Library.LabEquipment.Engine.Drivers;
using Library.LabEquipment.Types;

namespace Library.LabEquipment.Drivers
{
    public class DriverEquipment : DriverGeneric
    {
        #region Constants
        private const String STR_ClassName = "DriverEquipment";
        private const Logfile.Level logLevel = Logfile.Level.Finer;
        /*
         * String constants for serial LCD messages
         */
        protected const String STRLCD_Ready = "Ready.";
        protected const String STRLCD_SelectAbsorber = "Select absorber:";
        protected const String STRLCD_SelectSource = "Select source:";
        protected const String STRLCD_SetDistance = "Set distance:";
        protected const String STRLCD_Distance_arg = "{0:d}mm";
        protected const String STRLCD_CaptureData_arg4 = "{0:d}mm-{1:d}sec-{2:d}/{3:d}";
        protected const String STRLCD_CaptureCounts = "Capture counts:";
        protected const String STRLCD_ReturnSource = "Return source";
        protected const String STRLCD_ReturnAbsorber = "Return absorber";
        protected const String STRLCD_ReturnTube = "Return tube";
        /*
         * String constants for exception messages
         */
        protected const string STRERR_EquipmentHasNoAbsorbers = "Equipment does not have absorbers!";
        #endregion

        #region Variables
        protected ExperimentValidation experimentValidation;
        protected ExperimentSpecification experimentSpecification;
        protected ExperimentResults experimentResults;
        #endregion

        #region Properties

        public new static String ClassName
        {
            get { return STR_ClassName; }
        }

        public override String DriverName
        {
            get { return STR_ClassName; }
        }

        protected DeviceFlexMotion deviceFlexMotion;
        protected DeviceST360Counter deviceST360Counter;
        protected DeviceSerialLcd deviceSerialLcd;

        public void SetDeviceFlexMotion(DeviceFlexMotion deviceFlexMotion)
        {
            this.deviceFlexMotion = deviceFlexMotion;
        }

        public void SetDeviceST360Counter(DeviceST360Counter deviceST360Counter)
        {
            this.deviceST360Counter = deviceST360Counter;
        }

        public void SetDeviceSerialLcd(DeviceSerialLcd deviceSerialLcd)
        {
            this.deviceSerialLcd = deviceSerialLcd;
        }

        #endregion

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="labEquipmentConfiguration"></param>
        public DriverEquipment(LabEquipmentConfiguration labEquipmentConfiguration)
            : base(labEquipmentConfiguration)
        {
            const String methodName = "DriverEquipment";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            try
            {
                /*
                 * Create an instance of ExperimentValidation
                 */
                this.experimentValidation = ExperimentValidation.XmlParse(labEquipmentConfiguration.XmlValidation);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw ex;
            }

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
                 * Check that the devices have been set
                 */
                if (this.deviceFlexMotion == null)
                {
                    throw new NullReferenceException(String.Format(STRERR_DeviceNotSet_arg, DeviceFlexMotion.ClassName));
                }
                if (this.deviceST360Counter == null)
                {
                    throw new NullReferenceException(String.Format(STRERR_DeviceNotSet_arg, DeviceST360Counter.ClassName));
                }
                if (this.deviceSerialLcd == null)
                {
                    throw new NullReferenceException(String.Format(STRERR_DeviceNotSet_arg, DeviceSerialLcd.ClassName));
                }

                /*
                 * Check that base parameters are valid
                 */
                base.Validate(xmlSpecification);

                /*
                 * Create an instance of ExperimentSpecification from the XML specification String
                 */
                this.experimentSpecification = ExperimentSpecification.XmlParse(xmlSpecification);

                /*
                 * Validate the experiment specification parameters
                 */
                foreach (int distance in this.experimentSpecification.Distances)
                {
                    this.experimentValidation.ValidateDistance(distance);
                }
                this.experimentValidation.ValidateDuration(experimentSpecification.Duration);
                this.experimentValidation.ValidateRepeat(experimentSpecification.Repeat);

                /*
                 * Specification is valid so far
                 */
                validation = new Validation(true, 0);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw ex;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Validation_arg3,
                    validation.Accepted, validation.ExecutionTime, validation.ErrorMessage));

            return validation;
        }

        //-------------------------------------------------------------------------------------------------//

        protected virtual int GetExecutionTime(ExperimentSpecification experimentSpecification)
        {
            return 1;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>String</returns>
        public override String GetExperimentResults()
        {
            const String methodName = "GetExperimentResults";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            String xmlExperimentResults = null;

            try
            {
                xmlExperimentResults = this.experimentResults.ToXmlString();

                /*
                 * Log the experiment results
                 */
                Logfile.Write(Logfile.STRLOG_Newline + xmlExperimentResults);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return xmlExperimentResults;
        }

        //-------------------------------------------------------------------------------------------------//

        protected override bool ExecuteInitialising()
        {
            const String methodName = "ExecuteInitialising";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;

            try
            {
                /*
                 * Create an instance of ExperimentResults
                 */
                this.experimentResults = new ExperimentResults(this.experimentSpecification, this.xmlExperimentResultsTemplate);

                /*
                 * Log the experiment specification
                 */
                Logfile.Write(Logfile.STRLOG_Newline + this.experimentSpecification.ToXmlString());

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                this.executionStatus.ErrorMessage = ex.Message;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }
    }
}
