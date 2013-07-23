using System;
using System.Diagnostics;
using System.Threading;
using Library.Lab;
using Library.Lab.Types;
using Library.LabEquipment.Engine.Devices;
using Library.LabEquipment.Engine.Drivers;
using Library.LabEquipment.Engine.Types;
using Library.Lab.Utilities;

namespace Library.LabEquipment.Engine
{
    public class LabEquipmentEngine : IDisposable
    {
        #region Constants
        private const string STR_ClassName = "LabEquipmentEngine";
        private const Logfile.Level logLevel = Logfile.Level.Finer;
        /*
         * String constants
         */
        private const string STR_NotInitialised = "Not Initialised!";
        private const string STR_PoweringUp = "Powering up";
        private const string STR_Initialising = "Initialising";
        private const string STR_PoweringDown = "Powering down";
        private const string STR_Ready = "Ready";
        private const string STR_PoweredDown = "Powered down";
        /*
         * String constants for logfile messages
         */
        private const string STRLOG_TimeUntilReady_arg2 = "RunState: {0:s}  TimeUntilReady: {1:d} seconds";
        private const string STRLOG_StateChange_arg = "[LE: {0:s}->{1:s}]";
        //
        protected const string STRLOG_Dispose_arg2 = "disposing: {0:s}  disposed: {1:s}";
        protected const string STRLOG_ExecutionId_arg = "ExecutionId: {0:d}";
        protected const string STRLOG_SetupId_arg = "SetupId: {0:s}";
        protected const string STRLOG_EquipmentStatus_arg2 = "Online: {0:s}  StatusMessage: {1:s}";
        protected const string STRLOG_Validation_arg3 = "Accepted: {0:s}  ExecutionTime: {1:d}  ErrorMessage: {2:s}";
        protected const string STRLOG_ExecutionStatus_arg3 = "ExecuteStatus: {0:s}  ResultStatus: {1:s}  ErrorMessage: {2:s}";
        protected const string STRLOG_ExecutionStatus_arg5 = "ExecutionId: {0:d}  ExecuteStatus: {1:s}  ResultStatus: {2:s}  TimeRemaining: {3:d}  ErrorMessage: {4:s}";
        /*
         * String constants for logfile messages
         */
        protected const string STRLOG_Success_arg = "Success: {0}";
        /*
         * String constants for exception messages
         */
        private const string STRERR_InvalidSetupId_arg = "Invalid SetupId: {0:s}";
        private const string STRERR_ThreadFailedToStart = "Thread failed to start!";
        private const string STRERR_InvalidExecutionId_arg = "Invalid ExecutionId: {0:d}";
        //
        protected const string STRERR_AlreadyExecuting = "Already executing!";
        protected const string STRERR_FailedToStartExecution = "Failed to start execution!";
        #endregion

        #region Variables
        private bool debugTrace = false;
        private bool disposed = false;
        private Thread threadLabEquipmentEngine;
        private States runState;
        private bool powerdownEnabled;
        private bool powerdownSuspended;
        private int powerupTimeRemaining;
        private int poweroffTimeRemaining;
        private bool statusReady;
        private String statusMessage;
        private DateTime initialiseStartTime;
        //
        protected LabEquipmentConfiguration labEquipmentConfiguration;
        protected WaitNotify signalStartExecution;
        protected DriverGeneric driver;
        #endregion

        #region Properties

        public static string ClassName
        {
            get { return STR_ClassName; }
        }

        protected int initialiseDelay;
        protected int powerdownTimeout;
        protected int poweroffDelay;
        protected int powerupDelay;
        protected int timeUntilPowerdown;
        protected bool running;

        public int InitialiseDelay
        {
            get { return initialiseDelay; }
        }

        public int PowerdownTimeout
        {
            get { return powerdownTimeout; }
        }

        public int PoweroffDelay
        {
            get { return poweroffDelay; }
        }

        public int PowerupDelay
        {
            get { return powerupDelay; }
        }

        public int TimeUntilPowerdown
        {
            get { return timeUntilPowerdown; }
        }

        public bool Running
        {
            get { return running; }
        }

        #endregion

        #region Types
        private enum States
        {
            PowerOff, PowerUp, PowerUpDelay, PowerOnInit, PowerOnReady, PowerdownSuspended, DriverExecution, PowerDown, PowerOffDelay, Done
        }
        #endregion

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="labEquipmentConfiguration"></param>
        public LabEquipmentEngine(LabEquipmentConfiguration labEquipmentConfiguration)
        {
            const string methodName = "LabEquipmentEngine";
            Logfile.WriteCalled(Logfile.Level.Info, STR_ClassName, methodName);

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
                 * Save to local variables
                 */
                this.labEquipmentConfiguration = labEquipmentConfiguration;

                /*
                 * Initialise local variables
                 */
                this.signalStartExecution = new WaitNotify();
                if (this.signalStartExecution == null)
                {
                    throw new ArgumentNullException(WaitNotify.STR_ClassName);
                }
                this.runState = States.PowerOff;
                this.powerdownEnabled = this.labEquipmentConfiguration.PowerdownEnabled;
                this.powerdownSuspended = false;
                this.statusReady = false;
                this.statusMessage = STR_NotInitialised;

                /*
                 * Initialise properties
                 */
                this.initialiseDelay = this.labEquipmentConfiguration.InitialiseDelay;
                this.powerdownTimeout = this.labEquipmentConfiguration.PowerdownTimeout;
                this.poweroffDelay = this.labEquipmentConfiguration.PoweroffDelay;
                this.powerupDelay = this.labEquipmentConfiguration.PowerupDelay;
                this.timeUntilPowerdown = 0;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw ex;
            }

            Logfile.WriteCompleted(Logfile.Level.Config, STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>bool</returns>
        public bool Start()
        {
            const string methodName = "Start";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;

            try
            {
                /*
                 * Create a new thread and start it
                 */
                this.threadLabEquipmentEngine = new Thread(new ThreadStart(LabEquipmentEngineThread));
                if (this.threadLabEquipmentEngine != null)
                {
                    this.threadLabEquipmentEngine.Start();

                    /*
                     * Give it a chance to start running and then check that it has started
                     */
                    for (int i = 0; i < 5; i++)
                    {
                        if ((success = this.running) == true)
                        {
                            break;
                        }

                        Delay.MilliSeconds(500);
                        Trace.WriteLine("!");
                    }

                    if (success == false)
                    {
                        throw new ApplicationException(STRERR_ThreadFailedToStart);
                    }
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                            String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>int</returns>
        public int GetTimeUntilReady()
        {
            const string methodName = "GetTimeUntilReady";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            int timeUntilReady;

            switch (this.runState)
            {
                case States.Done:
                case States.PowerOff:
                case States.PowerUp:
                    timeUntilReady = this.powerupDelay + this.initialiseDelay;
                    break;
                case States.PowerUpDelay:
                    timeUntilReady = this.powerupTimeRemaining + this.initialiseDelay;
                    break;
                case States.PowerOnInit:
                    /*
                     * Don't say initialisation has completed until it actually has
                     */
                    TimeSpan timeSpan = DateTime.Now - this.initialiseStartTime;
                    timeUntilReady = this.initialiseDelay - Convert.ToInt32(timeSpan.TotalSeconds);
                    if (timeUntilReady < 1)
                    {
                        timeUntilReady = 1;
                    }
                    break;
                case States.PowerDown:
                    timeUntilReady = this.poweroffDelay + this.powerupDelay + this.initialiseDelay;
                    break;
                case States.PowerOffDelay:
                    timeUntilReady = this.poweroffTimeRemaining + this.powerupDelay + this.initialiseDelay;
                    break;
                default:
                    timeUntilReady = 0;
                    break;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_TimeUntilReady_arg2, Enum.GetName(typeof(States), this.runState), timeUntilReady));

            return timeUntilReady;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>LabEquipmentStatus</returns>
        public LabEquipmentStatus GetLabEquipmentStatus()
        {
            const string methodName = "GetLabEquipmentStatus";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            LabEquipmentStatus labEquipmentStatus = new LabEquipmentStatus(this.statusReady, this.statusMessage);

            if (this.driver != null)
            {
                ExecutionStatus executionStatus = this.driver.GetExecutionStatus();
                labEquipmentStatus.StatusMessage = Enum.GetName(typeof(ExecutionStatus.Status), executionStatus.ExecuteStatus);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_EquipmentStatus_arg2, labEquipmentStatus.Online, labEquipmentStatus.StatusMessage));

            return labEquipmentStatus;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlSpecification"></param>
        /// <returns>Validation</returns>
        public Validation Validate(String xmlSpecification)
        {
            const string methodName = "Validate";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            Validation validation = new Validation();

            try
            {
                /*
                 * Create an instance of LabExperimentSpecification and get the setup Id
                 */
                LabExperimentSpecification labExperimentSpecification = LabExperimentSpecification.XmlParse(xmlSpecification);

                /*
                 * Get the driver for the SetupId and validate the experiment specification
                 */
                validation = this.GetDriver(labExperimentSpecification.SetupId).Validate(xmlSpecification);

                /*
                 * Check that the specification is accepted before adding in the time until ready
                 */
                if (validation.Accepted == true)
                {
                    validation.ExecutionTime = validation.ExecutionTime + this.GetTimeUntilReady();
                }

            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                validation.ErrorMessage = ex.Message;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Validation_arg3, validation.Accepted, validation.ExecutionTime, validation.ErrorMessage));

            return validation;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlSpecification"></param>
        /// <returns>ExecutionStatus</returns>
        public ExecutionStatus StartExecution(String xmlSpecification)
        {
            const string methodName = "StartExecution";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            ExecutionStatus executionStatus;

            try
            {
                /*
                 * Check if an experiment is already running
                 */
                if (this.driver != null)
                {
                    executionStatus = this.driver.GetExecutionStatus();
                    if (executionStatus.ExecuteStatus != ExecutionStatus.Status.Completed)
                    {
                        throw new ApplicationException(STRERR_AlreadyExecuting);
                    }
                }

                /*
                 * Create an instance of LabExperimentSpecification and get the setup Id
                 */
                LabExperimentSpecification labExperimentSpecification = LabExperimentSpecification.XmlParse(xmlSpecification);

                /*
                 * Get the driver for the setup Id and validate the experiment specification
                 */
                this.driver = this.GetDriver(labExperimentSpecification.SetupId);
                Validation validation = this.driver.Validate(xmlSpecification);
                if (validation.Accepted == false)
                {
                    throw new ApplicationException(validation.ErrorMessage);
                }

                /*
                 * Generate a random number for the execution Id
                 */
                Random random = new Random();
                this.driver.ExecutionId = random.Next();

                /*
                 * Start the driver executing but this may require powering up the equipment first
                 */
                if (this.SuspendPowerdown() == false)
                {
                    throw new ApplicationException(STRERR_FailedToStartExecution);
                }

                /*
                 * Tell the thread that there is an experiment to execute
                 */
                this.signalStartExecution.Notify();

                /*
                 * Get the execution status and update the execution time including the time until the LabEquipment is ready
                 */
                executionStatus = this.driver.GetExecutionStatus();
                executionStatus.TimeRemaining = executionStatus.TimeRemaining + this.GetTimeUntilReady();

            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                executionStatus = new ExecutionStatus();
                executionStatus.ResultStatus = ExecutionStatus.Status.Failed;
                executionStatus.ErrorMessage = ex.Message;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_ExecutionStatus_arg5, executionStatus.ExecutionId,
                    Enum.GetName(typeof(ExecutionStatus.Status), executionStatus.ExecuteStatus),
                    Enum.GetName(typeof(ExecutionStatus.Status), executionStatus.ResultStatus),
                    executionStatus.TimeRemaining, executionStatus.ErrorMessage));

            return executionStatus;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionId"></param>
        /// <returns></returns>
        public ExecutionStatus GetExecutionStatus(int executionId)
        {
            const string methodName = "GetExecutionStatus";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_ExecutionId_arg, executionId));

            ExecutionStatus executionStatus = new ExecutionStatus();

            try
            {
                if (this.driver != null)
                {
                    /*
                     * Get the execution status and check the execution Id
                     */
                    executionStatus = this.driver.GetExecutionStatus();
                    if (executionStatus.ExecutionId != executionId && executionStatus.ExecutionId != 0)
                    {
                        throw new ApplicationException(String.Format(STRERR_InvalidExecutionId_arg, executionId));
                    }

                    /*
                     * Check if the experiment has completed
                     */
                    if (executionStatus.ExecuteStatus.Equals(ExecutionStatus.Status.Completed) == true)
                    {
                        /*
                         * Check if the experiment has completed successfully
                         */
                        if (executionStatus.ResultStatus.Equals(ExecutionStatus.Status.Completed) == false)
                        {
                            /*
                             * The driver is no longer needed
                             */
                            this.driver = null;
                        }

                        /*
                         * Experiment has completed but the results are yet to be retrieved
                         */
                        this.ResumePowerdown();
                    }
                    else
                    {
                        /*
                         *  Update the execution time including the time until the LabEquipment is ready
                         */
                        executionStatus.TimeRemaining = executionStatus.TimeRemaining + this.GetTimeUntilReady();
                    }
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_ExecutionStatus_arg5, executionStatus.ExecutionId,
                    Enum.GetName(typeof(ExecutionStatus.Status), executionStatus.ExecuteStatus),
                    Enum.GetName(typeof(ExecutionStatus.Status), executionStatus.ResultStatus),
                    executionStatus.TimeRemaining, executionStatus.ErrorMessage));

            return executionStatus;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionId"></param>
        /// <returns>string</returns>
        public string GetExperimentResults(int executionId)
        {
            const string methodName = "GetExperimentResults";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_ExecutionId_arg, executionId));

            string experimentResults = null;

            try
            {
                if (this.driver != null)
                {
                    /*
                     * Get the execution status and check the execution Id
                     */
                    ExecutionStatus executionStatus = this.driver.GetExecutionStatus();
                    if (executionStatus.ExecutionId != executionId)
                    {
                        throw new ApplicationException(String.Format(STRERR_InvalidExecutionId_arg, executionId));
                    }

                    /*
                     * Get the results from the driver
                     */
                    experimentResults = this.driver.GetExperimentResults();

                    /*
                     * The driver is no longer needed
                     */
                    this.driver = null;
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    experimentResults);

            return experimentResults;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionId"></param>
        /// <returns>bool</returns>
        public bool CancelLabExecution(int executionId)
        {
            const string methodName = "CancelLabExecution";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_ExecutionId_arg, executionId));

            bool success = false;

            try
            {
                if (this.driver != null)
                {
                    /*
                     * Get the execution status and check the execution Id
                     */
                    ExecutionStatus executionStatus = this.driver.GetExecutionStatus();
                    if (executionStatus.ExecutionId != executionId)
                    {
                        throw new ApplicationException(String.Format(STRERR_InvalidExecutionId_arg, executionId));
                    }

                    /*
                     * Cancel driver execution
                     */
                    this.driver.Cancel = true;
                    success = this.driver.Cancel;
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="setupId"></param>
        /// <returns>DriverGeneric</returns>
        protected virtual DriverGeneric GetDriver(String setupId)
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
                case LabConsts.STRXML_SetupId_Generic:
                    driverGeneric = new DriverGeneric(this.labEquipmentConfiguration);
                    break;
                default:
                    throw new ApplicationException(String.Format(STRERR_InvalidSetupId_arg, setupId));
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
        protected virtual bool PowerupEquipment()
        {
            const string methodName = "PowerupEquipment";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = true;

            /*
             * Nothing to do here, this will be overridden
             */

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>bool</returns>
        protected virtual bool InitialiseEquipment()
        {
            const string methodName = "InitialiseEquipment";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;

            /*
             * Create and initialise the generic device, this will be overridden
             */
            try
            {
                DeviceGeneric device = new DeviceGeneric(this.labEquipmentConfiguration, DeviceGeneric.ClassName);
                success = device.Initialise();
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
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
        protected virtual bool PowerdownEquipment()
        {
            const string methodName = "PowerdownEquipment";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = true;

            /*
             * Nothing to do here, this will be overridden
             */

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>bool</returns>
        protected bool SuspendPowerdown()
        {
            const string methodName = "SuspendPowerdown";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = true;

            /*
             * Check if powerdown is enabled
             */
            if (this.powerdownEnabled == true && this.powerdownSuspended == false)
            {
                /*
                 * Start by suspending equipment powerdown
                 */
                this.powerdownSuspended = true;

                /*
                 * Check if the thread is still running
                 */
                if (this.running == false)
                {
                    /*
                     * Start the lab equipment engine
                     */
                    success = this.Start();
                }
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        protected void ResumePowerdown()
        {
            const string methodName = "ResumePowerdown";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            /*
             * Check if powerdown is enabled
             */
            if (this.powerdownEnabled == true)
            {
                /*
                 * The equipment may already be powered down, doesn't matter
                 */
                this.powerdownSuspended = false;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        #region Close and Dispose

        /// <summary>
        /// Do not make this method virtual. A derived class should not be allowed to override this method.
        /// </summary>
        public void Close()
        {
            const string methodName = "Close";
            Logfile.WriteCalled(Logfile.Level.Info, STR_ClassName, methodName);

            /*
             * Calls the Dispose method without parameters
             */
            this.Dispose();

            Logfile.WriteCompleted(Logfile.Level.Info, STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Implement IDisposable. Do not make this method virtual. A derived class should not be able
        /// to override this method.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);

            /*
             * Take yourself off the Finalization queue to prevent finalization code for this object
             * from executing a second time.
             */
            GC.SuppressFinalize(this);
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Use C# destructor syntax for finalization code. This destructor will run only if the Dispose
        /// method does not get called. It gives your base class the opportunity to finalize. Do not provide
        /// destructors in types derived from this class.
        /// </summary>
        ~LabEquipmentEngine()
        {
            Trace.WriteLine("~LabEquipmentEngine():");

            /*
             * Do not re-create Dispose clean-up code here. Calling Dispose(false) is optimal in terms of
             * readability and maintainability.
             */
            this.Dispose(false);
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios:
        /// 1. If disposing equals true, the method has been called directly or indirectly by a user's code.
        ///    Managed and unmanaged resources can be disposed.
        /// 2. If disposing equals false, the method has been called by the runtime from inside the finalizer
        ///    and you should not reference other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            const string methodName = "Dispose";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                String.Format(STRLOG_Dispose_arg2, disposing.ToString(), this.disposed.ToString()));

            /*
             * Check to see if Dispose has already been called
             */
            if (this.disposed == false)
            {
                /*
                 * If disposing equals true, dispose all managed and unmanaged resources.
                 */
                if (disposing == true)
                {
                    /*
                     * Dispose managed resources here. Anything that has a Dispose() method.
                     * For example: component.Dispose();
                     */
                }

                /*
                 * Release unmanaged resources here. If disposing is false, only the following
                 * code is executed.
                 */

                /*
                 * Stop the LabEquipmentEngine thread
                 */
                if (this.running == true)
                {
                    this.running = false;

                    try
                    {
                        this.threadLabEquipmentEngine.Join();
                    }
                    catch (Exception ex)
                    {
                        Logfile.WriteError(ex.ToString());
                    }

                    /*
                     * Powerdown the equipment
                     */
                    PowerdownEquipment();
                }

                /*
                 * Note disposing has been done.
                 */
                this.disposed = true;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        #endregion

        //=================================================================================================//

        public void LabEquipmentEngineThread()
        {
            const string methodName = "LabEquipmentEngineThread";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            /*
             * Initialise state machine
             */
            States lastState = States.PowerOff;
            this.runState = States.PowerUp;
            this.running = true;

            /*
             * Allow other threads to check the state of this thread
             */
            Delay.MilliSeconds(500);

            /*
             * State machine loop
             */
            try
            {
                while (this.runState != States.Done)
                {
                    /*
                     * Chec if a state change has occurred
                     */
                    if (this.runState != lastState)
                    {
                        String logMessage = String.Format(STRLOG_StateChange_arg,
                            Enum.GetName(typeof(States), lastState), Enum.GetName(typeof(States), this.runState));
                        if (debugTrace == true)
                        {
                            Trace.WriteLine(logMessage);
                        }
                        //Logfile.Write(logMessage);

                        lastState = this.runState;
                    }

                    switch (this.runState)
                    {
                        case States.PowerUp:
                            /*
                             * Powerup the equipment
                             */
                            this.statusMessage = STR_PoweringUp;
                            if (this.PowerupEquipment() == false)
                            {
                                /*
                                 * Equipment failed to powerup
                                 */
                                this.statusReady = false;
                                this.statusMessage = STR_NotInitialised;
                                this.runState = States.PowerOff;
                                break;
                            }

                            this.powerupTimeRemaining = this.powerupDelay;
                            this.runState = States.PowerUpDelay;
                            break;

                        case States.PowerUpDelay:
                            /*
                             * Wait a bit
                             */
                            Delay.MilliSeconds(1000);

                            /*
                             * Check if powerup delay has timed out
                             */
                            if (--this.powerupTimeRemaining > 0)
                            {
                                /*
                                 * Equipment is still powering up
                                 */
                                if (debugTrace == true)
                                {
                                    Trace.WriteLine("[u]");
                                }
                                continue;
                            }

                            /*
                             * Equipment is now powered up
                             */
                            this.initialiseStartTime = DateTime.Now;
                            this.runState = States.PowerOnInit;
                            break;

                        case States.PowerOnInit:
                            /*
                             * Set the intialisation start time
                             */
                            this.statusMessage = STR_Initialising;

                            /*
                             * Initialise the equipment
                             */
                            if (this.InitialiseEquipment() == false)
                            {
                                /*
                                 * Equipment failed to initialise
                                 */
                                this.statusReady = false;
                                this.statusMessage = STR_NotInitialised;
                                this.runState = States.PowerDown;
                                break;
                            }

                            /*
                             * Equipment is now ready to use
                             */
                            this.statusReady = true;
                            this.statusMessage = STR_Ready;
                            this.timeUntilPowerdown = this.powerdownTimeout;

                            /*
                             * Suspend equipment powerdown if not enabled
                             */
                            if (this.powerdownEnabled == false)
                            {
                                this.powerdownSuspended = true;
                            }
                            else
                            {
                                /*
                                 * Log the time remaining before the equipment is powered down
                                 */
                                LogPowerDown(this.powerdownTimeout, true);
                            }

                            this.runState = States.PowerOnReady;
                            break;

                        case States.PowerOnReady:
                            /*
                             * Wait a bit
                             */
                            Delay.MilliSeconds(1000);

                            /*
                             * Check if LabEquipment is closing
                             */
                            if (this.running == false)
                            {
                                this.runState = States.PowerDown;
                                break;
                            }

                            /*
                             * Check if equipment powerdown is suspended
                             */
                            if (this.powerdownSuspended == true)
                            {
                                this.runState = States.PowerdownSuspended;
                                break;
                            }

                            /*
                             * Log the time remaining before power is removed
                             */
                            this.LogPowerDown(this.timeUntilPowerdown);

                            /*
                             * Check powerdown timeout
                             */
                            if (--this.timeUntilPowerdown == 0)
                            {
                                /*
                                 * Equipment is powering down
                                 */
                                this.runState = States.PowerDown;
                                break;
                            }

                            /*
                             * Still counting down
                             */
                            if (debugTrace == true)
                            {
                                Trace.WriteLine("[t]");
                            }
                            break;

                        case States.PowerdownSuspended:
                            /*
                             * Check if there is an experiment to execute
                             */
                            if (this.signalStartExecution.Wait(1000) == true)
                            {
                                /*
                                 * Start execution of the experiment specification
                                 */
                                this.runState = States.DriverExecution;
                                break;
                            }

                            /*
                             * Check if LabEquipment is closing
                             */
                            if (this.running == false)
                            {
                                this.runState = States.PowerDown;
                                break;
                            }

                            /*
                             * Check if powerdown is no longer suspended
                             */
                            if (this.powerdownSuspended == false)
                            {
                                /*
                                 * Reset the powerdown timeout
                                 */
                                this.timeUntilPowerdown = this.powerdownTimeout;
                                this.runState = States.PowerOnReady;
                            }
                            break;

                        case States.DriverExecution:
                            /*
                             * Execute the experiment
                             */
                            this.driver.Execute();
                            this.signalStartExecution.Reset();
                            this.runState = States.PowerdownSuspended;
                            break;

                        case States.PowerDown:
                            /*
                             * Powerdown the equipment
                             */
                            this.statusMessage = STR_PoweringDown;
                            this.PowerdownEquipment();

                            this.poweroffTimeRemaining = this.poweroffDelay;
                            this.runState = States.PowerOffDelay;
                            break;

                        case States.PowerOffDelay:
                            /*
                             * Wait a bit
                             */
                            Delay.MilliSeconds(1000);

                            /*
                             * Check if LabEquipment is closing
                             */
                            if (this.running == false)
                            {
                                this.runState = States.PowerOff;
                                break;
                            }

                            /*
                             * Check timeout
                             */
                            if (--this.poweroffTimeRemaining > 0)
                            {
                                /*
                                 * Poweroff delay is still counting down
                                 */
                                if (debugTrace == true)
                                {
                                    Trace.WriteLine("[o]");
                                }
                            }
                            else
                            {
                                /*
                                 * Check if powerup has been requested
                                 */
                                if (this.powerdownSuspended == true)
                                {
                                    this.runState = States.PowerUp;
                                }
                                else
                                {
                                    /*
                                     * Powerdown has completed
                                     */
                                    this.statusMessage = STR_PoweredDown;
                                    this.runState = States.PowerOff;
                                }
                            }
                            break;

                        case States.PowerOff:
                            this.runState = States.Done;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
            }

            /*
             * Thread is no longer running
             */
            this.running = false;

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seconds"></param>
        private void LogPowerDown(int seconds)
        {
            LogPowerDown(seconds, false);
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="seconds"></param>
        /// <param name="logItNow"></param>
        private void LogPowerDown(int seconds, bool logItNow)
        {
            const string STR_PowerdownIn = "Powerdown in ";
            const string STR_Minutes = "{0:d} minute{1:s}";
            const string STR_And = " and ";
            const string STR_Seconds = "{0:d} second{1:s}";

            int minutes = seconds / 60;
            String strMinutes = String.Format(STR_Minutes, minutes, ((minutes != 1) ? "s" : ""));
            String strSeconds = String.Format(STR_Seconds, seconds, ((seconds != 1) ? "s" : ""));

            if (logItNow == true && seconds > 0)
            {
                /*
                 * Log message now
                 */
                String logMessage = STR_PowerdownIn;
                seconds %= 60;
                if (minutes > 0)
                {
                    logMessage += strMinutes;
                    if (seconds != 0)
                    {
                        logMessage += STR_And;
                    }
                }
                if (seconds != 0)
                {
                    logMessage += strSeconds;
                }
                Logfile.Write(logMessage);
            }
            else
            {
                if (minutes > 5)
                {
                    if (seconds % (5 * 60) == 0)
                    {
                        /*
                         * Log message every 5 minutes
                         */
                        Logfile.Write(STR_PowerdownIn + strMinutes);
                    }
                }
                else if (seconds > 5)
                {
                    if (seconds % 60 == 0 && seconds != 0)
                    {
                        /*
                         * Log message every minute
                         */
                        Logfile.Write(STR_PowerdownIn + strMinutes);
                    }
                }
                else if (seconds > 0)
                {
                    /*
                     * Log message every second
                     */
                    //
                    Logfile.Write(STR_PowerdownIn + strSeconds);
                }
            }
        }
    }
}
