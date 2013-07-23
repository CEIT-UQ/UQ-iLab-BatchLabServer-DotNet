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
        /*
         * String constants for exception messages
         */
        protected const string STRERR_RunningSpeedNotObtained_arg2 = "Running speed of {0:d} RPM not obtained: {1:d} RPM";
        #endregion

        #region Variables
        protected ExperimentSpecification experimentSpecification;
        protected ExperimentResults experimentResults;
        protected int measurementCount;
        protected int measurementDelay;
        #endregion

        #region Properties
        public new static string ClassName
        {
            get { return STR_ClassName; }
        }

        public override string DriverName
        {
            get { return STR_ClassName; }
        }

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

        protected struct Measurement
        {
            public float voltageMut;
            public float currentMut;
            public float powerFactorMut;
            public float voltageVsd;
            public float currentVsd;
            public float powerFactorVsd;
            public int speed;
            public int loadTemperature;
        }

        private enum States
        {
            // TakeMeasurements
            Done,
            MeasureVoltageMut, MeasureCurrentMut, MeasurePowerFactorMut,
            MeasureVoltageVsd, MeasureCurrentVsd, MeasurePowerFactorVsd,
            MeasureSpeed, MeasureLoadTemperature
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

            /*
             * Nothing to do here
             */

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

        protected int GetExecutionTime(ExperimentSpecification experimentSpecification)
        {
            const String methodName = "GetExecutionTime";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            /*
             * Calculate the execution time
             */
            this.executionTimes.Run = this.measurementCount * this.measurementDelay;

            /*
             * Get the total execution time
             */
            int totalExecutionTime = this.executionTimes.TotalExecutionTime;

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_ExecutionTime_arg, totalExecutionTime));

            return totalExecutionTime;
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
                Logfile.Write(logLevel, Logfile.STRLOG_Newline + xmlExperimentResults);
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
                States thisState = States.MeasureVoltageMut;

                /*
                 * State machine loop
                 */
                while (thisState != States.Done)
                {
                    switch (thisState)
                    {
                        case States.MeasureVoltageMut:
                            /*
                             * Measure the voltage
                             */
                            if ((success = this.deviceRedLion.GetPowerMeterMut.GetVoltage(out measurement.voltageMut)) == false)
                            {
                                throw new ApplicationException(this.deviceRedLion.GetPowerMeterMut.LastError);
                            }

                            thisState = States.MeasureCurrentMut;
                            break;

                        case States.MeasureCurrentMut:
                            /*
                             * Measure the current
                             */
                            if ((success = this.deviceRedLion.GetPowerMeterMut.GetCurrent(out measurement.currentMut)) == false)
                            {
                                throw new ApplicationException(this.deviceRedLion.GetPowerMeterMut.LastError);
                            }

                            thisState = States.MeasurePowerFactorMut;
                            break;

                        case States.MeasurePowerFactorMut:
                            /*
                             * Measure the power factor
                             */
                            if ((success = this.deviceRedLion.GetPowerMeterMut.GetPowerFactor(out measurement.powerFactorMut)) == false)
                            {
                                throw new ApplicationException(this.deviceRedLion.GetPowerMeterMut.LastError);
                            }

                            thisState = States.MeasureVoltageVsd;
                            break;

                        case States.MeasureVoltageVsd:
                            /*
                             * Measure the voltage
                             */
                            if ((success = this.deviceRedLion.GetPowerMeterVsd.GetVoltage(out measurement.voltageVsd)) == false)
                            {
                                throw new ApplicationException(this.deviceRedLion.GetPowerMeterMut.LastError);
                            }

                            thisState = States.MeasureCurrentVsd;
                            break;

                        case States.MeasureCurrentVsd:
                            /*
                             * Measure the current
                             */
                            if ((success = this.deviceRedLion.GetPowerMeterVsd.GetCurrent(out measurement.currentVsd)) == false)
                            {
                                throw new ApplicationException(this.deviceRedLion.GetPowerMeterMut.LastError);
                            }

                            thisState = States.MeasurePowerFactorVsd;
                            break;

                        case States.MeasurePowerFactorVsd:
                            /*
                             * Measure the power factor
                             */
                            if ((success = this.deviceRedLion.GetPowerMeterVsd.GetPowerFactor(out measurement.powerFactorVsd)) == false)
                            {
                                throw new ApplicationException(this.deviceRedLion.GetPowerMeterMut.LastError);
                            }

                            thisState = States.MeasureLoadTemperature;
                            break;

                        case States.MeasureLoadTemperature:
                            /*
                             * Measure the temperature of the load resistor bank
                             */
                            if ((success = this.deviceRedLion.GetACDrive.GetLoadTemperature(out measurement.loadTemperature)) == false)
                            {
                                throw new ApplicationException(this.deviceRedLion.GetACDrive.LastError);
                            }

                            thisState = States.MeasureSpeed;
                            break;

                        case States.MeasureSpeed:
                            /*
                             * Measure the drive speed
                             */
                            if ((success = this.deviceRedLion.GetACDrive.GetSpeed(out measurement.speed)) == false)
                            {
                                throw new ApplicationException(this.deviceRedLion.GetACDrive.LastError);
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
