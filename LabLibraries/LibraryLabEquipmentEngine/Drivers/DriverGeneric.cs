using System;
using System.Diagnostics;
using System.Xml;
using Library.Lab;
using Library.Lab.Types;
using Library.LabEquipment.Engine.Types;
using Library.Lab.Utilities;

namespace Library.LabEquipment.Engine.Drivers
{
    public class DriverGeneric
    {
        #region Constants
        private const string STR_ClassName = "DriverGeneric";
        private const Logfile.Level logLevel = Logfile.Level.Finest;
        /*
         * String constants for logfile messages
         */
        private const string STRLOG_ExecutionTimes_arg6 = "ExecutionTimes:[Initialise: {0:d} - Start: {1:d} - Run: {2:d} - Stop: {3:d} - Finalise: {4:d} - Total: {5:d}]";
        private const string STRLOG_StateChange_arg = "[DG: {0:s}->{1:s}]";
        private const string STRLOG_UpdateExecutionStatus_arg3 = "Success: {0:s}  SuccessStatus: {1:s}  FailedStatus: {2:s}";
        //
        protected const string STRLOG_Validation_arg3 = "Accepted: {0:s}  ExecutionTime: {1:d}  ErrorMessage: {2:s}";
        protected const string STRLOG_ExecutionTime_arg = "ExecutionTime: {0:d}";
        protected const string STRLOG_ActualExecutionTime_arg = "Actual ExecutionTime: {0:f01}";
        protected const string STRLOG_ExecutionStatus_arg5 = "ExecutionId: {0:d}  ExecuteStatus: {1:s}  ResultStatus: {2:s}  TimeRemaining: {3:d}  ErrorMessage: {4:s}";
        protected const string STRLOG_Success_arg = "Success: {0:s}";
        /*
         * String constants for exception messages
         */
        protected const string STRERR_XmlSpecification = "xmlSpecification";
        protected const string STRERR_InvalidSetupId_arg = "Invalid SetupId: {0:s}";
        protected const string STRERR_DeviceNotSet_arg = "Device has not been set: {0:s}";
        protected const string STRERR_InvalidDeviceInstance_arg = "Invalid device instance: {0:s}";
        #endregion

        #region Variables
        private Object executionStatusLock;
        private DateTime completionTime;
        //
        protected XmlNode xmlNodeDriver;
        protected XmlNode xmlNodeValidation;
        protected LabExperimentSpecification labExperimentSpecification;
        protected LabExperimentValidation labExperimentValidation;
        protected String xmlExperimentResultsTemplate;
        protected ExecutionStatus executionStatus;
        #endregion

        #region Properties

        public static String ClassName
        {
            get { return STR_ClassName; }
        }

        public virtual String DriverName
        {
            get { return STR_ClassName; }
        }

        private int executionId;
        protected ExecutionTimes executionTimes;
        protected bool cancelled;

        public int ExecutionId
        {
            get { return executionId; }
            set { executionId = value; }
        }

        public ExecutionTimes ExecutionTimes
        {
            get { return executionTimes; }
        }

        public bool Cancel
        {
            get { return cancelled; }
            set { cancelled = value; }
        }

        #endregion

        #region Types

        private enum States
        {
            None, Initialise, Start, Run, Stop, Finalise, Completed
        }

        private enum ExecuteStates
        {
            WaitOneSecond, ShowMessage, CheckTime, Done
        }
        #endregion

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="labEquipmentConfiguration"></param>
        public DriverGeneric(LabEquipmentConfiguration labEquipmentConfiguration)
        {
            const String methodName = "DriverGeneric";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            try
            {
                /*
                 * Check that parameters are valid
                 */
                if (labEquipmentConfiguration == null)
                {
                    throw new ArgumentNullException(LabEquipmentConfiguration.ClassName);
                }

                /*
                 * Get the driver configuration from the XML String
                 */
                XmlDocument xmlDocument = XmlUtilities.GetDocumentFromString(labEquipmentConfiguration.GetXmlDriverConfiguration(this.DriverName));
                XmlNode nodeRoot = XmlUtilities.GetRootNode(xmlDocument, LabConsts.STRXML_Driver);

                /*
                 * Get the execution times for each of the driver states
                 */
                XmlNode xmlNodeExecutionTimes = XmlUtilities.GetChildNode(nodeRoot, LabConsts.STRXML_ExecutionTimes);
                this.executionTimes = ExecutionTimes.ToObject(XmlUtilities.ToXmlString(xmlNodeExecutionTimes));

                /*
                 * Get the experiment results XML template
                 */
                XmlNode xmlNodeExperimentResults = XmlUtilities.GetChildNode(nodeRoot, LabConsts.STRXML_ExperimentResults);
                this.xmlExperimentResultsTemplate = XmlUtilities.ToXmlString(xmlNodeExperimentResults);

                /*
                 * Initialise local variables
                 */
                this.completionTime = DateTime.Now;
                this.executionId = 0;
                this.executionStatusLock = new Object();
                this.executionStatus = new ExecutionStatus();
                this.executionStatus.ExecuteStatus = ExecutionStatus.Status.Created;
                this.executionStatus.TimeRemaining = this.executionTimes.TotalExecutionTime;
                this.cancelled = false;

                /*
                 * Save a copy of the driver XML node for the derived class
                 */
                this.xmlNodeDriver = nodeRoot.Clone();

                /*
                 * Get the experiment validation from the XML String
                 */
                xmlDocument = XmlUtilities.GetDocumentFromString(labEquipmentConfiguration.XmlValidation);
                nodeRoot = XmlUtilities.GetRootNode(xmlDocument, LabConsts.STRXML_Validation);

                /*
                 * Save a copy of the experiment validation XML node for the derived class
                 */
                this.xmlNodeValidation = nodeRoot.Clone();
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw ex;
            }

            String logMessage = String.Format(STRLOG_ExecutionStatus_arg5, executionStatus.ExecutionId,
                    Enum.GetName(typeof(ExecutionStatus.Status), executionStatus.ExecuteStatus),
                    Enum.GetName(typeof(ExecutionStatus.Status), executionStatus.ResultStatus),
                executionStatus.TimeRemaining, executionStatus.ErrorMessage);

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName, logMessage);
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlSpecification"></param>
        /// <returns>Validation</returns>
        public virtual Validation Validate(String xmlSpecification)
        {
            const String methodName = "Validate";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            Validation validation;

            try
            {
                /*
                 * Create an instance of LabExperimentSpecification and get the setup Id
                 */
                this.labExperimentSpecification = LabExperimentSpecification.XmlParse(xmlSpecification);

                /*
                 * Check the setup Id
                 */
                String setupId = this.labExperimentSpecification.SetupId;
                if (setupId.Equals(LabConsts.STRXML_SetupId_Generic) == false)
                {
                    /*
                     * Don't throw an exception, a derived class will want to check the setup Id
                     */
                    validation = new Validation(String.Format(STRERR_InvalidSetupId_arg, setupId));
                }
                else
                {
                    /*
                     * Calculate the execution time
                     */
                    int executionTime = this.executionTimes.TotalExecutionTime;

                    /*
                     * Specification is valid
                     */
                    validation = new Validation(true, executionTime);
                }
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

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>ExecutionStatus</returns>
        public ExecutionStatus GetExecutionStatus()
        {
            const String methodName = "GetExecutionStatus";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            ExecutionStatus execStatus = new ExecutionStatus();

            /*
             * Copy over the execution status
             */
            lock (this.executionStatusLock)
            {
                execStatus.ExecutionId = this.executionId;
                execStatus.ExecuteStatus = this.executionStatus.ExecuteStatus;
                execStatus.ResultStatus = this.executionStatus.ResultStatus;
                execStatus.ErrorMessage = this.executionStatus.ErrorMessage;
            }

            /*
             * Update the time remaining
             */
            int timeRemaining;
            switch (execStatus.ExecuteStatus)
            {
                case ExecutionStatus.Status.Created:
                    timeRemaining = this.executionTimes.TotalExecutionTime;
                    break;

                case ExecutionStatus.Status.Initialising:
                case ExecutionStatus.Status.Starting:
                case ExecutionStatus.Status.Running:
                case ExecutionStatus.Status.Stopping:
                case ExecutionStatus.Status.Finalising:
                    /*
                     * Get the time in seconds from now until the expected completion time
                     */
                    TimeSpan timeSpan = this.completionTime - DateTime.Now;
                    timeRemaining = Convert.ToInt32(timeSpan.TotalSeconds);

                    /*
                     * Ensure time remaining is greater than zero
                     */
                    if (timeRemaining < 1)
                    {
                        timeRemaining = 1;
                    }
                    break;

                case ExecutionStatus.Status.Done:
                case ExecutionStatus.Status.Completed:
                case ExecutionStatus.Status.Failed:
                case ExecutionStatus.Status.Cancelled:
                    timeRemaining = 0;
                    break;

                default:
                    timeRemaining = -1;
                    break;
            }
            execStatus.TimeRemaining = timeRemaining;

            String logMessage = String.Format(STRLOG_ExecutionStatus_arg5, executionStatus.ExecutionId,
                Enum.GetName(typeof(ExecutionStatus.Status), executionStatus.ExecuteStatus),
                Enum.GetName(typeof(ExecutionStatus.Status), executionStatus.ResultStatus),
                executionStatus.TimeRemaining, executionStatus.ErrorMessage);

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName, logMessage);

            return execStatus;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>String</returns>
        public virtual String GetExperimentResults()
        {
            const String methodName = "GetExperimentResults";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            String xmlExperimentResults = null;

            try
            {
                LabExperimentResults labExperimentResults = new LabExperimentResults();
                xmlExperimentResults = labExperimentResults.ToXmlString();
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return xmlExperimentResults;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        public void Execute()
        {
            const String methodName = "Execute";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            /*
             * Initialise state machine
             */
            DateTime startTime = DateTime.Now;
            States lastState = States.None;
            States thisState = States.Initialise;
            this.UpdateExecutionStatus(true, ExecutionStatus.Status.Initialising, ExecutionStatus.Status.None);

            /*
             * Allow other threads to check the state of this thread
             */
            Delay.MilliSeconds(500);

            /*
             * Log the execution times
             */
            Logfile.Write(String.Format(STRLOG_ExecutionTimes_arg6, this.executionTimes.Initialise, this.executionTimes.Start,
                this.executionTimes.Run, this.executionTimes.Stop, this.executionTimes.Finalise, this.executionTimes.TotalExecutionTime));

            /*
             * State machine loop
             */
            try
            {
                while (thisState != States.Completed)
                {
                    bool success;

                    /*
                     * Display message on each state change
                     */
                    if (thisState != lastState)
                    {
                        String logMessage = String.Format(STRLOG_StateChange_arg,
                            Enum.GetName(typeof(States), lastState), Enum.GetName(typeof(States), thisState));
                        Trace.WriteLine(logMessage);
                        Logfile.Write(logMessage);

                        lastState = thisState;
                    }

                    switch (thisState)
                    {
                        case States.Initialise:
                            /*
                             * Execute this part of the driver
                             */
                            success = this.ExecuteInitialising();
                            this.UpdateExecutionStatus(success, ExecutionStatus.Status.Starting, ExecutionStatus.Status.Completed);
                            thisState = (success == true) ? States.Start : States.Completed;
                            break;

                        case States.Start:
                            /*
                             * Execute this part of the driver
                             */
                            success = this.ExecuteStarting();
                            this.UpdateExecutionStatus(success, ExecutionStatus.Status.Running, ExecutionStatus.Status.Stopping);
                            thisState = (success == true) ? States.Run : States.Stop;
                            break;

                        case States.Run:
                            /*
                             * Execute this part of the driver
                             */
                            success = this.ExecuteRunning();
                            this.UpdateExecutionStatus(success, ExecutionStatus.Status.Stopping, ExecutionStatus.Status.Stopping);
                            thisState = States.Stop;
                            break;

                        case States.Stop:
                            /*
                             * Execute this part of the driver
                             */
                            success = this.ExecuteStopping();
                            this.UpdateExecutionStatus(success, ExecutionStatus.Status.Finalising, ExecutionStatus.Status.Finalising);
                            thisState = States.Finalise;
                            break;

                        case States.Finalise:
                            /*
                             * Execute this part of the driver
                             */
                            success = this.ExecuteFinalising();
                            this.UpdateExecutionStatus(success, ExecutionStatus.Status.Completed, ExecutionStatus.Status.Completed);
                            thisState = States.Completed;
                            break;
                    }
                }

                /*
                 * Log the actual execution time
                 */
                Logfile.Write(String.Format(STRLOG_ActualExecutionTime_arg, (DateTime.Now - startTime).TotalSeconds));
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>bool</returns>
        protected virtual bool ExecuteInitialising()
        {
            const String methodName = "ExecuteInitialising";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = true;
            int seconds = this.executionTimes.Initialise;

            try
            {
                /*
                 * Initialise state machine
                 */
                ExecuteStates thisState = (seconds <= 0) ? ExecuteStates.Done : ExecuteStates.WaitOneSecond;

                /*
                 * State machine loop - Can't throw any exceptions if there were any errors, have to keep on going.
                 */
                while (thisState != ExecuteStates.Done)
                {
                    switch (thisState)
                    {
                        case ExecuteStates.WaitOneSecond:
                            /*
                             * Wait one second before continuing
                             */
                            Delay.MilliSeconds(1000);

                            thisState = ExecuteStates.ShowMessage;
                            break;

                        case ExecuteStates.ShowMessage:
                            /*
                             * Display a message
                             */
                            Trace.WriteLine("[i]");

                            thisState = ExecuteStates.CheckTime;
                            break;

                        case ExecuteStates.CheckTime:
                            /*
                             * Check if time has reached zero
                             */
                            if (--seconds == 0)
                            {
                                thisState = ExecuteStates.Done;
                                break;
                            }

                            thisState = ExecuteStates.WaitOneSecond;
                            break;
                    }

                    /*
                     * Check if the experiment has been cancelled
                     */
                    if (this.cancelled == true)
                    {
                        thisState = ExecuteStates.Done;
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

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>bool</returns>
        protected virtual bool ExecuteStarting()
        {
            const String methodName = "ExecuteStarting";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = true;
            int seconds = this.executionTimes.Start;

            try
            {
                /*
                 * Initialise state machine
                 */
                ExecuteStates thisState = (seconds <= 0) ? ExecuteStates.Done : ExecuteStates.WaitOneSecond;

                /*
                 * State machine loop - Can't throw any exceptions if there were any errors, have to keep on going.
                 */
                while (thisState != ExecuteStates.Done)
                {
                    switch (thisState)
                    {
                        case ExecuteStates.WaitOneSecond:
                            /*
                             * Wait one second before continuing
                             */
                            Delay.MilliSeconds(1000);

                            thisState = ExecuteStates.ShowMessage;
                            break;

                        case ExecuteStates.ShowMessage:
                            /*
                             * Display a message
                             */
                            Trace.WriteLine("[s]");

                            thisState = ExecuteStates.CheckTime;
                            break;

                        case ExecuteStates.CheckTime:
                            /*
                             * Check if time has reached zero
                             */
                            if (--seconds == 0)
                            {
                                thisState = ExecuteStates.Done;
                                break;
                            }

                            thisState = ExecuteStates.WaitOneSecond;
                            break;
                    }

                    /*
                     * Check if the experiment has been cancelled
                     */
                    if (this.cancelled == true)
                    {
                        thisState = ExecuteStates.Done;
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

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>bool</returns>
        protected virtual bool ExecuteRunning()
        {
            const String methodName = "ExecuteRunning";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = true;
            int seconds = this.executionTimes.Run;

            try
            {
                /*
                 * Initialise state machine
                 */
                ExecuteStates thisState = (seconds <= 0) ? ExecuteStates.Done : ExecuteStates.WaitOneSecond;

                /*
                 * State machine loop - Can't throw any exceptions if there were any errors, have to keep on going.
                 */
                while (thisState != ExecuteStates.Done)
                {
                    switch (thisState)
                    {
                        case ExecuteStates.WaitOneSecond:
                            /*
                             * Wait one second before continuing
                             */
                            Delay.MilliSeconds(1000);

                            thisState = ExecuteStates.ShowMessage;
                            break;

                        case ExecuteStates.ShowMessage:
                            /*
                             * Display a message
                             */
                            Trace.WriteLine("[r]");

                            thisState = ExecuteStates.CheckTime;
                            break;

                        case ExecuteStates.CheckTime:
                            /*
                             * Check if time has reached zero
                             */
                            if (--seconds == 0)
                            {
                                thisState = ExecuteStates.Done;
                                break;
                            }

                            thisState = ExecuteStates.WaitOneSecond;
                            break;
                    }

                    /*
                     * Check if the experiment has been cancelled
                     */
                    if (this.cancelled == true)
                    {
                        thisState = ExecuteStates.Done;
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

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>bool</returns>
        protected virtual bool ExecuteStopping()
        {
            const String methodName = "ExecuteStopping";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = true;
            String lastError = null;
            int seconds = this.executionTimes.Stop;

            try
            {
                /*
                 * Initialise state machine
                 */
                ExecuteStates thisState = (seconds <= 0) ? ExecuteStates.Done : ExecuteStates.WaitOneSecond;

                /*
                 * State machine loop - Can't throw any exceptions if there were any errors, have to keep on going.
                 */
                while (thisState != ExecuteStates.Done)
                {
                    switch (thisState)
                    {
                        case ExecuteStates.WaitOneSecond:
                            /*
                             * Wait one second before continuing
                             */
                            Delay.MilliSeconds(1000);

                            thisState = ExecuteStates.ShowMessage;
                            break;

                        case ExecuteStates.ShowMessage:
                            /*
                             * Display a message
                             */
                            Trace.WriteLine("[p]");

                            thisState = ExecuteStates.CheckTime;
                            break;

                        case ExecuteStates.CheckTime:
                            /*
                             * Check if time has reached zero
                             */
                            if (--seconds == 0)
                            {
                                thisState = ExecuteStates.Done;
                                break;
                            }

                            thisState = ExecuteStates.WaitOneSecond;
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

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>bool</returns>
        protected virtual bool ExecuteFinalising()
        {
            const String methodName = "ExecuteFinalising";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = true;
            String lastError = null;
            int seconds = this.executionTimes.Finalise;

            try
            {
                /*
                 * Initialise state machine
                 */
                ExecuteStates thisState = (seconds <= 0) ? ExecuteStates.Done : ExecuteStates.WaitOneSecond;

                /*
                 * State machine loop - Can't throw any exceptions if there were any errors, have to keep on going.
                 */
                while (thisState != ExecuteStates.Done)
                {
                    switch (thisState)
                    {
                        case ExecuteStates.WaitOneSecond:
                            /*
                             * Wait one second before continuing
                             */
                            Delay.MilliSeconds(1000);

                            thisState = ExecuteStates.ShowMessage;
                            break;

                        case ExecuteStates.ShowMessage:
                            /*
                             * Display a message
                             */
                            Trace.WriteLine("[f]");

                            thisState = ExecuteStates.CheckTime;
                            break;

                        case ExecuteStates.CheckTime:
                            /*
                             * Check if time has reached zero
                             */
                            if (--seconds == 0)
                            {
                                thisState = ExecuteStates.Done;
                                break;
                            }

                            thisState = ExecuteStates.WaitOneSecond;
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

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="success"></param>
        /// <param name="successStatus"></param>
        /// <param name="failedStatus"></param>
        private void UpdateExecutionStatus(bool success, ExecutionStatus.Status successStatus, ExecutionStatus.Status failedStatus)
        {
            const String methodName = "UpdateExecutionStatus";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_UpdateExecutionStatus_arg3, success, Enum.GetName(typeof(ExecutionStatus.Status), successStatus),
                    Enum.GetName(typeof(ExecutionStatus.Status), failedStatus)));

            lock (this.executionStatusLock)
            {
                /*
                 * Update result status if execution failed
                 */
                if (success == false)
                {
                    if (this.executionStatus.ResultStatus == ExecutionStatus.Status.None)
                    {
                        this.executionStatus.ResultStatus =
                                (this.cancelled == true) ? ExecutionStatus.Status.Cancelled : ExecutionStatus.Status.Failed;
                    }
                }

                /*
                 * Update result status if execution has completed
                 */
                if (successStatus == ExecutionStatus.Status.Completed)
                {
                    if (this.executionStatus.ResultStatus == ExecutionStatus.Status.None)
                    {
                        this.executionStatus.ResultStatus = ExecutionStatus.Status.Completed;
                    }
                }

                /*
                 * Update execute status only after updating result status
                 */
                this.executionStatus.ExecuteStatus = (success == true) ? successStatus : failedStatus;

                /*
                 * Get the time remaining
                 */
                int timeRemaining = 0;
                switch (this.executionStatus.ExecuteStatus)
                {
                    case ExecutionStatus.Status.Initialising:
                        timeRemaining = this.executionTimes.Initialise
                                + this.executionTimes.Start
                                + this.executionTimes.Run
                                + this.executionTimes.Stop
                                + this.executionTimes.Finalise;
                        break;
                    case ExecutionStatus.Status.Starting:
                        timeRemaining = this.executionTimes.Start
                                + this.executionTimes.Run
                                + this.executionTimes.Stop
                                + this.executionTimes.Finalise;
                        break;
                    case ExecutionStatus.Status.Running:
                        timeRemaining = this.executionTimes.Run
                                + this.executionTimes.Stop
                                + this.executionTimes.Finalise;
                        break;
                    case ExecutionStatus.Status.Stopping:
                        timeRemaining = this.executionTimes.Stop
                                + this.executionTimes.Finalise;
                        break;
                    case ExecutionStatus.Status.Finalising:
                        timeRemaining =
                                this.executionTimes.Finalise;
                        break;
                    case ExecutionStatus.Status.Completed:
                        break;
                }

                /*
                 * Set the time remaining in the execution status and renew the expected completion time
                 */
                this.executionStatus.TimeRemaining = timeRemaining;
                this.completionTime = DateTime.Now.AddSeconds(timeRemaining);

                Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
            }
        }
    }
}
