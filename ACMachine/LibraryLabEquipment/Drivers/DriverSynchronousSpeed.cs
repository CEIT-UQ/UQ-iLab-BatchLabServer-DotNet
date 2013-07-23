using System;
using System.Diagnostics;
using Library.Lab;
using Library.Lab.Types;
using Library.LabEquipment.Devices;
using Library.LabEquipment.Engine;

namespace Library.LabEquipment.Drivers
{
    public class DriverSynchronousSpeed : DriverEquipment
    {
        #region Constants
        private const string STR_ClassName = "DriverSynchronousSpeed";
        private const Logfile.Level logLevel = Logfile.Level.Finer;
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
            OpenConnection, InitialiseACDrive, SetMaximumCurrent,

            // ExecuteStarting
            EnableDrivePower, SetSpeed, StartDrive, CheckSpeed,

            // ExecuteRunning
            MeasurementDelay, TakeMeasurements,

            // ExecuteStopping
            StopDrive, DisableDrivePower,

            // ExecuteFinalising
            CloseConnection,
        }

        #endregion

        //-----------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="labEquipmentConfiguration"></param>
        public DriverSynchronousSpeed(LabEquipmentConfiguration labEquipmentConfiguration)
            : base(labEquipmentConfiguration)
        {
            const string methodName = "DriverSynchronousSpeed";
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
                if (setupId.Equals(Consts.STRXML_SetupId_SynchronousSpeed) == false)
                {
                    throw new ApplicationException(String.Format(STRERR_InvalidSetupId_arg, setupId));
                }

                /*
                 * Specification is valid
                 */
                validation = new Validation(true, this.GetExecutionTime(this.experimentSpecification));
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                validation = new Validation(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Validation_arg3,
                    validation.Accepted, validation.ExecutionTime, validation.ErrorMessage));

            return validation;
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
                             * Open the network connection to the RedLion
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
                            if (this.deviceRedLion.GetACDrive.Initialise() == false)
                            {
                                throw new ApplicationException(this.deviceRedLion.GetACDrive.LastError);
                            }

                            thisState = States.SetMaximumCurrent;
                            break;

                        case States.SetMaximumCurrent:

                            if (this.deviceRedLion.GetACDrive.SetMaximumCurrent(ACDrive.MAXIMUM_MaximumCurrent) == false)
                            {
                                throw new ApplicationException(this.deviceRedLion.GetACDrive.LastError);
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
                States thisState = States.EnableDrivePower;

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
                        case States.EnableDrivePower:

                            if (this.deviceRedLion.GetACDrive.EnableDrivePower() == false)
                            {
                                throw new ApplicationException(this.deviceRedLion.GetACDrive.LastError);
                            }

                            thisState = States.SetSpeed;
                            break;

                        case States.SetSpeed:

                            if (this.deviceRedLion.GetACDrive.SetControlSpeed(ACDrive.SPEED_Synchronous) == false)
                            {
                                throw new ApplicationException(this.deviceRedLion.GetACDrive.LastError);
                            }

                            thisState = States.StartDrive;
                            break;

                        case States.StartDrive:

                            if (this.deviceRedLion.GetACDrive.StartDriveSyncSpeed() == false)
                            {
                                throw new ApplicationException(this.deviceRedLion.GetACDrive.LastError);
                            }

                            /*
                             * Check if skipping speed checking for debugging purposes
                             */
                            thisState = (this.deviceRedLion.CheckSpeedEnabled == true) ? States.CheckSpeed : States.Done;
                            break;

                        case States.CheckSpeed:

                            int actualSpeed;
                            if (this.deviceRedLion.GetACDrive.GetSpeed(out actualSpeed) == false)
                            {
                                throw new ApplicationException(this.deviceRedLion.GetACDrive.LastError);
                            }

                            /*
                             * Check speed is within error of required speed
                             */
                            int requiredSpeed = ACDrive.SPEED_Synchronous;
                            if (actualSpeed < requiredSpeed - ACDrive.SPEED_Error || actualSpeed > requiredSpeed + ACDrive.SPEED_Error)
                            {
                                throw new ApplicationException(String.Format(STRERR_RunningSpeedNotObtained_arg2, requiredSpeed, actualSpeed));
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
                this.experimentResults.MeasurementData = new ExperimentResults.Measurements(1);
                Measurement measurementsToAverage = new Measurement();
                int measurementCount = this.measurementCount;
                States lastState = States.None;
                States thisState = States.MeasurementDelay;

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

                            int actualSpeed;
                            if (this.deviceRedLion.GetACDrive.GetSpeed(out actualSpeed) == false)
                            {
                                throw new ApplicationException(this.deviceRedLion.GetACDrive.LastError);
                            }

                            /*
                             * Check speed is within error of required speed
                             */
                            int requiredSpeed = ACDrive.SPEED_Synchronous;
                            if (actualSpeed < requiredSpeed - ACDrive.SPEED_Error || actualSpeed > requiredSpeed + ACDrive.SPEED_Error)
                            {
                                throw new ApplicationException(String.Format(STRERR_RunningSpeedNotObtained_arg2, requiredSpeed, actualSpeed));
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
                            measurementsToAverage.voltageVsd += measurement.voltageVsd;
                            measurementsToAverage.currentVsd += measurement.currentVsd;
                            measurementsToAverage.powerFactorVsd += measurement.powerFactorVsd;
                            measurementsToAverage.speed += measurement.speed;

                            /*
                             * Check if all measurements have been taken
                             */
                            if (--measurementCount == 0)
                            {
                                /*
                                 * Average the measurements
                                 */
                                measurementsToAverage.voltageVsd /= this.measurementCount;
                                measurementsToAverage.currentVsd /= this.measurementCount;
                                measurementsToAverage.powerFactorVsd /= this.measurementCount;
                                measurementsToAverage.speed /= this.measurementCount;

                                /*
                                 * Save the measurements to the results
                                 */
                                this.experimentResults.MeasurementData.voltageVsd[0] = measurementsToAverage.voltageVsd;
                                this.experimentResults.MeasurementData.currentVsd[0] = measurementsToAverage.currentVsd;
                                this.experimentResults.MeasurementData.powerFactorVsd[0] = measurementsToAverage.powerFactorVsd;
                                this.experimentResults.MeasurementData.speed[0] = measurementsToAverage.speed;
                            }

                            thisState = (measurementCount == 0) ? States.Done : States.MeasurementDelay;
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
                States thisState = States.StopDrive;

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
                        case States.StopDrive:

                            if (this.deviceRedLion.GetACDrive.StopDriveSyncSpeed() == false)
                            {
                                if (lastError == null)
                                {
                                    lastError = this.deviceRedLion.GetACDrive.LastError;
                                }
                            }

                            thisState = States.DisableDrivePower;
                            break;

                        case States.DisableDrivePower:

                            if (this.deviceRedLion.GetACDrive.DisableDrivePower() == false)
                            {
                                if (lastError == null)
                                {
                                    lastError = this.deviceRedLion.GetACDrive.LastError;
                                }
                            }

                            thisState = States.SetSpeed;
                            break;

                        case States.SetSpeed:

                            if (this.deviceRedLion.GetACDrive.SetControlSpeed(0) == false)
                            {
                                if (lastError == null)
                                {
                                    lastError = this.deviceRedLion.GetACDrive.LastError;
                                }
                            }

                            thisState = States.SetMaximumCurrent;
                            break;

                        case States.SetMaximumCurrent:

                            if (this.deviceRedLion.GetACDrive.SetMaximumCurrent(ACDrive.DEFAULT_MaximumCurrent) == false)
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
