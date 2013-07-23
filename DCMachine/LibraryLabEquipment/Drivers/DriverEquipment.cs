using System;
using System.Diagnostics;
using Library.Lab;
using Library.Lab.Types;
using Library.Lab.Utilities;
using Library.LabEquipment.Devices;
using Library.LabEquipment.Engine;
using Library.LabEquipment.Engine.Drivers;
using Library.LabEquipment.Types;

namespace Library.LabEquipment.Drivers
{
    public class DriverEquipment : DriverGeneric
    {
        #region Constants
        private const string STR_ClassName = "DriverEquipment";
        private const Logfile.Level logLevel = Logfile.Level.Finer;
        /*
         * String constants for logfile messages
         */
        protected const string STRLOG_StateChange_arg = "[{0:s}->{1:s}]";
        #endregion

        #region Variables
        protected ExperimentValidation experimentValidation;
        protected ExperimentSpecification experimentSpecification;
        protected ExperimentResults experimentResults;
        protected int measurementCount;
        protected int measurementDelay;
        #endregion

        #region Properties
        protected DeviceRedLion deviceRedLion;

        public void SetDeviceRedLion(DeviceRedLion deviceRedLion)
        {
            this.deviceRedLion = deviceRedLion;
        }

        /*
         * The following are only used for debugging
         */
        protected bool disableInitialise = false;

        public bool DisableInitialise
        {
            get { return disableInitialise; }
            set { disableInitialise = value; }
        }
        #endregion

        #region Types

        private enum States
        {
            // Common
            Done,

            // TakeMeasurements
            MeasureSpeed, MeasureArmatureVoltage, MeasureFieldCurrent, MeasureTorque,
        }

        protected struct Measurement
        {
            public float speed;
            public float armatureVoltage;
            public float fieldCurrent;
            public float torque;
        }
        #endregion

        //-----------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="labEquipmentConfiguration"></param>
        public DriverEquipment(LabEquipmentConfiguration labEquipmentConfiguration)
            : base(labEquipmentConfiguration)
        {
            const string methodName = "DriverEquipment";
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

        //-----------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlSpecification"></param>
        /// <returns>Validation</returns>
        public override Validation Validate(String xmlSpecification)
        {
            const string methodName = "Validate";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            Validation validation;

            try
            {
                /*
                 * Check that the devices have been set
                 */
                if (this.deviceRedLion == null)
                {
                    throw new NullReferenceException(String.Format(STRERR_DeviceNotSet_arg, DeviceRedLion.ClassName));
                }
                if (this.deviceRedLion.GetACDrive == null)
                {
                    throw new NullReferenceException(String.Format(STRERR_DeviceNotSet_arg, ACDrive.ClassName));
                }
                if (this.deviceRedLion.GetDCDrive == null)
                {
                    throw new NullReferenceException(String.Format(STRERR_DeviceNotSet_arg, DCDrive.ClassName));
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

        //-----------------------------------------------------------------------------------------------------------//

        protected virtual int GetExecutionTime(ExperimentSpecification experimentSpecification)
        {
            return 1;
        }

        //-----------------------------------------------------------------------------------------------------------//

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

        //-----------------------------------------------------------------------------------------------------------//

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
                this.experimentResults.SetDeviceRedLion(this.deviceRedLion);
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

        //-----------------------------------------------------------------------------------------------------------//

        protected void WaitDelay(int seconds)
        {
            for (int i = 0; i < seconds; i++)
            {
                Trace.Write(".");
                Delay.MilliSeconds(1000);

                this.deviceRedLion.KeepAlive();
            }
        }

        //-----------------------------------------------------------------------------------------------------------//

        protected bool TakeMeasurements(out Measurement measurement)
        {
            const string methodName = "TakeMeasurements";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;

            /*
             * Create the data structure for storing the measurements
             */
            measurement = new Measurement();

            try
            {
                /*
                 * Initialise state machine
                 */
                States thisState = States.MeasureSpeed;

                /*
                 * State machine loop
                 */
                while (thisState != States.Done)
                {
                    switch (thisState)
                    {
                        case States.MeasureSpeed:

                            if ((success = this.deviceRedLion.GetDCDrive.GetSpeed(out measurement.speed)) == false)
                            {
                                throw new ApplicationException(this.deviceRedLion.GetDCDrive.LastError);
                            }

                            thisState = States.MeasureArmatureVoltage;
                            break;

                        case States.MeasureArmatureVoltage:

                            if ((success = this.deviceRedLion.GetDCDrive.GetArmatureVoltage(out measurement.armatureVoltage)) == false)
                            {
                                throw new ApplicationException(this.deviceRedLion.GetDCDrive.LastError);
                            }

                            thisState = States.MeasureFieldCurrent;
                            break;

                        case States.MeasureFieldCurrent:

                            if ((success = this.deviceRedLion.GetDCDrive.GetFieldCurrent(out measurement.fieldCurrent)) == false)
                            {
                                throw new ApplicationException(this.deviceRedLion.GetDCDrive.LastError);
                            }

                            thisState = States.MeasureTorque;
                            break;

                        case States.MeasureTorque:

                            if ((success = this.deviceRedLion.GetDCDrive.GetTorque(out measurement.torque)) == false)
                            {
                                throw new ApplicationException(this.deviceRedLion.GetDCDrive.LastError);
                            }

                            thisState = States.Done;
                            break;
                    }
                }
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
