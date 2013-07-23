using System;
using System.Diagnostics;
using Library.Lab;
using Library.Lab.Types;
using Library.LabEquipment.Devices;
using Library.LabEquipment.Engine;
using Library.LabEquipment.Types;

namespace Library.LabEquipment.Drivers
{
    public class DriverSpeedVsVoltage : DriverEquipment
    {
        #region Constants
        private const string STR_ClassName = "DriverSpeedVsVoltage";
        private const Logfile.Level logLevel = Logfile.Level.Finer;
        /*
         * String constants for exception messages
         */
        private const string STRERR_RunningSpeedNotObtained_arg2 = "Running speed of {0:d} RPM not obtained: {1:f}";
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
        #endregion

        #region Types

        private enum States
        {
            // Common
            None, Done,

            // ExecuteInitialising
            OpenConnection, InitialiseACDrive, ConfigureACDrive, InitialiseDCDrive,

            // ExecuteStarting
            StartACDrive, EnableDCDrive, StartDCDrive,

            // ExecuteRunning
            SetSpeedDCDrive, CheckSpeed, MeasurementDelay, TakeMeasurements,

            // ExecuteStopping
            ResetSpeedDCDrive, StopDCDrive, StopACDrive, DisableACDrive,

            // ExecuteFinalising
            CloseConnection,
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="labEquipmentConfiguration"></param>
        public DriverSpeedVsVoltage(LabEquipmentConfiguration labEquipmentConfiguration)
            : base(labEquipmentConfiguration)
        {
            const string methodName = "DriverSpeedVsVoltage";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            try
            {
                /*
                 * Get the number of measurements to take and the delay between measurements
                 */
                this.measurementCount = XmlUtilities.GetChildValueAsInt(this.xmlNodeDriver, Consts.STRXML_Measurements);
                this.measurementDelay = XmlUtilities.GetChildValueAsInt(this.xmlNodeDriver, Consts.STRXML_MeasurementDelay);
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
                 * Check that parameters are valid
                 */
                base.Validate(xmlSpecification);

                /*
                 * Check the setup Id
                 */
                String setupId = this.experimentSpecification.SetupId;
                if (setupId.Equals(Consts.STRXML_SetupId_SpeedVsVoltage) == false)
                {
                    throw new ApplicationException(String.Format(STRERR_InvalidSetupId_arg, setupId));
                }

                /*
                 * Validate the experiment specification parameters
                 */
                this.experimentValidation.ValidateSpeed(this.experimentSpecification.SpeedMin, this.experimentSpecification.SpeedMax, this.experimentSpecification.SpeedStep);

                /*
                 * Specification is valid
                 */
                validation = new Validation(true, this.GetExecutionTime(this.experimentSpecification));
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

        protected override int GetExecutionTime(ExperimentSpecification experimentSpecification)
        {
            const String methodName = "GetExecutionTime";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            /*
             * Calculate the execution time
             */
            int tallyLength = ((experimentSpecification.SpeedMax - experimentSpecification.SpeedMin) / experimentSpecification.SpeedStep) + 1;
            this.executionTimes.Run = tallyLength * (this.deviceRedLion.GetDCDrive.ChangeSpeedTime + (this.measurementCount * this.measurementDelay));

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
            const string methodName = "ExecuteInitialising";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;

            try
            {
                /*
                 * Get base class to do its bit
                 */
                base.ExecuteInitialising();

                /*
                 * Initialise state machine
                 */
                States lastState = States.None;
                States thisState = States.OpenConnection;

                /*
                 * State machine loop
                 */
                while (thisState != States.Done)
                {
                    /*
                     * Display message on each state change
                     */
                    if (thisState != lastState)
                    {
                        String logMessage = String.Format(STRLOG_StateChange_arg, Enum.GetName(typeof(States), lastState), Enum.GetName(typeof(States), thisState));
                        Trace.WriteLine(logMessage);
                        Logfile.Write(logLevel, logMessage);

                        lastState = thisState;
                    }

                    switch (thisState)
                    {
                        case States.OpenConnection:
                            /*
                             * Open network connection to the RedLion
                             */
                            if (this.deviceRedLion.OpenConnection() == false)
                            {
                                throw new ApplicationException(this.deviceRedLion.LastError);
                            }

                            /*
                             * Check if skipping device initialisation for debugging purposes
                             */
                            thisState = (this.disableInitialise == true) ? States.Done : States.InitialiseACDrive;
                            break;

                        case States.InitialiseACDrive:
                            /*
                             * Initialise the ACDrive device component
                             */
                            if (this.deviceRedLion.GetACDrive.Initialise(true) == false)
                            {
                                throw new ApplicationException(this.deviceRedLion.GetACDrive.LastError);
                            }

                            thisState = States.ConfigureACDrive;
                            break;

                        case States.ConfigureACDrive:
                            /*
                             * Configure the ACDrive device component
                             */
                            if (this.deviceRedLion.GetACDrive.SetMaximumCurrent(ACDrive.LOWER_MaximumCurrent) == false)
                            {
                                throw new ApplicationException(this.deviceRedLion.GetACDrive.LastError);
                            }

                            thisState = States.InitialiseDCDrive;
                            break;

                        case States.InitialiseDCDrive:
                            /*
                             * Initialise the DCDrive device component
                             */
                            if (this.deviceRedLion.GetDCDrive.Initialise() == false)
                            {
                                throw new ApplicationException(this.deviceRedLion.GetDCDrive.LastError);
                            }

                            thisState = States.Done;
                            break;
                    }

                    /*
                     * Check if the experiment has been cancelled
                     */
                    if (this.cancelled == true)
                    {
                        thisState = States.Done;
                    }
                }

                success = (this.cancelled == false);
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

        protected override bool ExecuteStarting()
        {
            const string methodName = "ExecuteStarting";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;

            try
            {
                /*
                 * Initialise state machine
                 */
                States lastState = States.None;
                States thisState = States.StartACDrive;

                /*
                 * State machine loop
                 */
                while (thisState != States.Done)
                {
                    /*
                     * Display message on each state change
                     */
                    if (thisState != lastState)
                    {
                        String logMessage = String.Format(STRLOG_StateChange_arg, Enum.GetName(typeof(States), lastState), Enum.GetName(typeof(States), thisState));
                        Trace.WriteLine(logMessage);
                        Logfile.Write(logLevel, logMessage);

                        lastState = thisState;
                    }

                    switch (thisState)
                    {
                        case States.StartACDrive:

                            if (this.deviceRedLion.GetACDrive.StartDrive() == false)
                            {
                                throw new ApplicationException(this.deviceRedLion.GetACDrive.LastError);
                            }

                            thisState = States.EnableDCDrive;
                            break;

                        case States.EnableDCDrive:

                            if (this.deviceRedLion.GetDCDrive.EnableDrive() == false)
                            {
                                throw new ApplicationException(this.deviceRedLion.GetDCDrive.LastError);
                            }

                            thisState = States.StartDCDrive;
                            break;

                        case States.StartDCDrive:

                            if (this.deviceRedLion.GetDCDrive.StartDrive() == false)
                            {
                                throw new ApplicationException(this.deviceRedLion.GetDCDrive.LastError);
                            }

                            thisState = States.Done;
                            break;
                    }

                    /*
                     * Check if the experiment has been cancelled
                     */
                    if (this.cancelled == true)
                    {
                        thisState = States.Done;
                    }
                }

                success = (this.cancelled == false);
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

        protected override bool ExecuteRunning()
        {
            const string methodName = "ExecuteRunning";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;

            try
            {
                /*
                 * Initialise state machine
                 */
                int tallyLength = ((this.experimentSpecification.SpeedMax - this.experimentSpecification.SpeedMin) / this.experimentSpecification.SpeedStep) + 1;
                this.experimentResults.MeasurementData = new ExperimentResults.Measurements(tallyLength);
                int tallyIndex = 0;
                int speedDCDrive = 0;
                int measurementCount = this.measurementCount;
                Measurement measurementsToAverage = new Measurement();
                States lastState = States.None;
                States thisState = States.SetSpeedDCDrive;

                /*
                 * State machine loop
                 */
                while (thisState != States.Done)
                {
                    /*
                     * Display message on each state change
                     */
                    if (thisState != lastState)
                    {
                        String logMessage = String.Format(STRLOG_StateChange_arg, Enum.GetName(typeof(States), lastState), Enum.GetName(typeof(States), thisState));
                        Trace.WriteLine(logMessage);
                        Logfile.Write(logLevel, logMessage);

                        lastState = thisState;
                    }

                    switch (thisState)
                    {
                        case States.SetSpeedDCDrive:

                            speedDCDrive = this.experimentSpecification.SpeedMin + (tallyIndex * this.experimentSpecification.SpeedStep);
                            if (this.deviceRedLion.GetDCDrive.ChangeSpeed(speedDCDrive) == false)
                            {
                                throw new ApplicationException(this.deviceRedLion.GetACDrive.LastError);
                            }

                            thisState = States.MeasurementDelay;
                            break;

                        case States.MeasurementDelay:
                            /*
                             * Wait a moment before taking the measurements
                             */
                            this.WaitDelay(this.measurementDelay);

                            /*
                             * Check if skipping speed checking for debugging purposes
                             */
                            thisState = (this.deviceRedLion.CheckSpeedEnabled == true) ? States.CheckSpeed : States.TakeMeasurements;
                            break;

                        case States.CheckSpeed:

                            float actualSpeed;
                            if (this.deviceRedLion.GetDCDrive.GetSpeed(out actualSpeed) == false)
                            {
                                throw new ApplicationException(this.deviceRedLion.GetACDrive.LastError);
                            }

                            /*
                             * Check speed is within error of required speed
                             */
                            if (actualSpeed < speedDCDrive - DCDrive.SPEED_Error || actualSpeed > speedDCDrive + DCDrive.SPEED_Error)
                            {
                                throw new ApplicationException(String.Format(STRERR_RunningSpeedNotObtained_arg2, speedDCDrive, actualSpeed));
                            }

                            thisState = States.TakeMeasurements;
                            break;

                        case States.TakeMeasurements:
                            /*
                             * Take a measurement
                             */
                            Measurement measurement;
                            if (this.TakeMeasurements(out measurement) == false)
                            {
                                throw new ApplicationException(this.deviceRedLion.LastError);
                            }

                            /*
                             * Add the measurement to the tally
                             */
                            measurementsToAverage.speed += measurement.speed;
                            measurementsToAverage.torque += measurement.torque;
                            measurementsToAverage.armatureVoltage += measurement.armatureVoltage;
                            measurementsToAverage.fieldCurrent += measurement.fieldCurrent;

                            /*
                             * Check if all measurements have been taken
                             */
                            if (--measurementCount == 0)
                            {
                                /*
                                 * Average the tally
                                 */
                                measurementsToAverage.speed /= this.measurementCount;
                                measurementsToAverage.torque /= this.measurementCount;
                                measurementsToAverage.armatureVoltage /= this.measurementCount;
                                measurementsToAverage.fieldCurrent /= this.measurementCount;

                                /*
                                 * Save the measurements to the results
                                 */
                                this.experimentResults.MeasurementData.speed[tallyIndex] = measurementsToAverage.speed;
                                this.experimentResults.MeasurementData.torque[tallyIndex] = measurementsToAverage.torque;
                                this.experimentResults.MeasurementData.armatureVoltage[tallyIndex] = measurementsToAverage.armatureVoltage;
                                this.experimentResults.MeasurementData.fieldCurrent[tallyIndex] = measurementsToAverage.fieldCurrent;

                                /*
                                 * Check if all measurements have been taken
                                 */
                                if (++tallyIndex == tallyLength)
                                {
                                    thisState = States.Done;
                                    break;
                                }
                                else
                                {
                                    measurementCount = this.measurementCount;
                                    measurementsToAverage = new Measurement();

                                    thisState = States.SetSpeedDCDrive;
                                    break;
                                }
                            }

                            thisState = States.MeasurementDelay;
                            break;
                    }

                    /*
                     * Check if the experiment has been cancelled
                     */
                    if (this.cancelled == true)
                    {
                        thisState = States.Done;
                    }
                }

                success = (this.cancelled == false);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
                this.executionStatus.ErrorMessage = ex.Message;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        protected override bool ExecuteStopping()
        {
            const string methodName = "ExecuteStopping";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;
            string lastError = null;

            try
            {
                /*
                 * Initialise state machine
                 */
                States lastState = States.None;
                States thisState = States.ResetSpeedDCDrive;

                /*
                 * State machine loop - Can't throw any exceptions if there were any errors, have to keep on going.
                 */
                while (thisState != States.Done)
                {
                    /*
                     * Display message on each state change
                     */
                    if (thisState != lastState)
                    {
                        String logMessage = String.Format(STRLOG_StateChange_arg, Enum.GetName(typeof(States), lastState), Enum.GetName(typeof(States), thisState));
                        Trace.WriteLine(logMessage);
                        Logfile.Write(logLevel, logMessage);

                        lastState = thisState;
                    }

                    switch (thisState)
                    {
                        case States.ResetSpeedDCDrive:

                            if (this.deviceRedLion.GetDCDrive.ChangeSpeed(0) == false)
                            {
                                if (lastError == null)
                                {
                                    lastError = this.deviceRedLion.GetDCDrive.LastError;
                                }
                            }

                            thisState = States.StopDCDrive;
                            break;

                        case States.StopDCDrive:
                            /*
                             * Stop the GetDCDrive device component
                             */
                            if (this.deviceRedLion.GetDCDrive.StopDrive() == false)
                            {
                                if (lastError == null)
                                {
                                    lastError = this.deviceRedLion.GetDCDrive.LastError;
                                }
                            }

                            thisState = States.StopACDrive;
                            break;

                        case States.StopACDrive:
                            /*
                             * Stop the GetACDrive device component
                             */
                            if (this.deviceRedLion.GetACDrive.StopDrive() == false)
                            {
                                if (lastError == null)
                                {
                                    lastError = this.deviceRedLion.GetACDrive.LastError;
                                }
                            }

                            thisState = States.DisableACDrive;
                            break;

                        case States.DisableACDrive:

                            if (this.deviceRedLion.GetACDrive.DisableDrivePower() == false)
                            {
                                if (lastError == null)
                                {
                                    lastError = this.deviceRedLion.GetACDrive.LastError;
                                }
                            }

                            thisState = States.Done;
                            break;
                    }

                    /*
                     * Do not check if the experiment has been cancelled, it has finished running anyway
                     */
                }

                /*
                 * Check if there were any errors
                 */
                success = (lastError == null);
                if (success == false)
                {
                    this.executionStatus.ErrorMessage = lastError;
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

        //-----------------------------------------------------------------------------------------------------------//

        protected override bool ExecuteFinalising()
        {
            const string methodName = "ExecuteFinalising";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;
            string lastError = null;

            try
            {
                /*
                 * Initialise state machine
                 */
                States lastState = States.None;
                States thisState = States.CloseConnection;

                /*
                 * State machine loop
                 */
                while (thisState != States.Done)
                {
                    /*
                     * Display message on each state change
                     */
                    if (thisState != lastState)
                    {
                        String logMessage = String.Format(STRLOG_StateChange_arg, Enum.GetName(typeof(States), lastState), Enum.GetName(typeof(States), thisState));
                        Trace.WriteLine(logMessage);
                        Logfile.Write(logLevel, logMessage);

                        lastState = thisState;
                    }

                    switch (thisState)
                    {
                        case States.CloseConnection:
                            /*
                             * Close the network connection to the RedLion
                             */
                            if (this.deviceRedLion.CloseConnection() == false)
                            {
                                if (lastError == null)
                                {
                                    lastError = this.deviceRedLion.LastError;
                                }
                            }

                            thisState = States.Done;
                            break;
                    }

                    /*
                     * Do not check if the experiment has been cancelled, it has finished running anyway
                     */
                }

                /*
                 * Check if there were any errors
                 */
                success = (lastError == null);
                if (success == false)
                {
                    this.executionStatus.ErrorMessage = lastError;
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
