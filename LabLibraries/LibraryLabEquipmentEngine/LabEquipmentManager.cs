using System;
using Library.Lab;
using Library.Lab.Types;

namespace Library.LabEquipment.Engine
{
    public class LabEquipmentManager
    {
        #region Constants
        private const string STR_ClassName = "LabEquipmentManager";
        private const Logfile.Level logLevel = Logfile.Level.Fine;
        /*
         * String constants
         */
        protected const string STR_NotInitialised = "Not Initialised!";
        /*
         * String constants for logfile messages
         */
        private const string STRLOG_EquipmentConfigFilename_arg = "equipmentConfigFilename: {0:s}";
        //
        protected const string STRLOG_ExecutionId_arg = "ExecutionId: {0:d}";
        protected const string STRLOG_Success_arg = "Success: {0}";
        #endregion

        #region Variables
        protected LabEquipmentConfiguration labEquipmentConfiguration;
        protected LabEquipmentEngine labEquipmentEngine;
        #endregion

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configProperties"></param>
        public LabEquipmentManager(ConfigProperties configProperties)
        {
            const string methodName = "LabEquipmentManager";
            Logfile.WriteCalled(Logfile.Level.Info, STR_ClassName, methodName);

            try
            {
                /*
                 * Check that parameters are valid
                 */
                if (configProperties == null)
                {
                    throw new ArgumentNullException(ConfigProperties.ClassName);
                }

                /*
                 * Create class instances and objects that are used by the LabEquipmentEngine
                 */
                this.labEquipmentConfiguration = new LabEquipmentConfiguration(configProperties);
                if (labEquipmentConfiguration == null)
                {
                    throw new ArgumentNullException(LabEquipmentConfiguration.ClassName);
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw;
            }

            Logfile.WriteCompleted(Logfile.Level.Info, STR_ClassName, methodName);
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>bool</returns>
        public virtual bool Create()
        {
            const string methodName = "Create";
            Logfile.WriteCalled(Logfile.Level.Info, STR_ClassName, methodName);

            bool success = false;

            try
            {
                /*
                 * Create an instance of the lab equipment engine
                 */
                this.labEquipmentEngine = new LabEquipmentEngine(this.labEquipmentConfiguration);
                if (this.labEquipmentEngine == null)
                {
                    throw new ArgumentNullException(LabEquipmentEngine.ClassName);
                }

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(STR_ClassName, methodName, ex.Message);
            }

            Logfile.WriteCompleted(Logfile.Level.Info, STR_ClassName, methodName);

            return success;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>bool</returns>
        public bool Start()
        {
            const string methodName = "Start";
            Logfile.WriteCalled(Logfile.Level.Info, STR_ClassName, methodName);

            bool success = false;

            if (this.labEquipmentEngine != null)
            {
                success = this.labEquipmentEngine.Start();
            }

            Logfile.WriteCompleted(Logfile.Level.Info, STR_ClassName, methodName,
                String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //---------------------------------------------------------------------------------------//

        public int GetTimeUntilReady()
        {
            const string methodName = "GetTimeUntilReady";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            int timeUntilReady = -1;

            if (this.labEquipmentEngine != null)
            {
                timeUntilReady = this.labEquipmentEngine.GetTimeUntilReady();
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return timeUntilReady;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>int</returns>
        public int GetTimeUntilPowerdown()
        {
            const string methodName = "GetTimeUntilPowerdown";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            int timeUntilPowerdown = -1;

            if (this.labEquipmentEngine != null)
            {
                timeUntilPowerdown = this.labEquipmentEngine.TimeUntilPowerdown;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return timeUntilPowerdown;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>LabEquipmentStatus</returns>
        public LabEquipmentStatus GetLabEquipmentStatus()
        {
            const string methodName = "GetLabEquipmentStatus";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            LabEquipmentStatus labEquipmentStatus;

            if (this.labEquipmentEngine != null)
            {
                labEquipmentStatus = this.labEquipmentEngine.GetLabEquipmentStatus();
            }
            else
            {
                labEquipmentStatus = new LabEquipmentStatus(false, STR_NotInitialised);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return labEquipmentStatus;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlSpecification"></param>
        /// <returns>Validation</returns>
        public Validation Validate(string xmlSpecification)
        {
            const string methodName = "Validate";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            Validation validation = null;
            if (this.labEquipmentEngine != null)
            {
                validation = this.labEquipmentEngine.Validate(xmlSpecification);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return validation;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlSpecification"></param>
        /// <returns>ExecutionStatus</returns>
        public ExecutionStatus StartLabExecution(string xmlSpecification)
        {
            const string methodName = "StartLabExecution";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            ExecutionStatus executionStatus = null;

            if (this.labEquipmentEngine != null)
            {
                executionStatus = this.labEquipmentEngine.StartExecution(xmlSpecification);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return executionStatus;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionId"></param>
        /// <returns>ExecutionStatus</returns>
        public ExecutionStatus GetLabExecutionStatus(int executionId)
        {
            const string methodName = "GetLabExecutionStatus";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            ExecutionStatus executionStatus = null;

            if (this.labEquipmentEngine != null)
            {
                executionStatus = this.labEquipmentEngine.GetExecutionStatus(executionId);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return executionStatus;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionId"></param>
        /// <returns>string</returns>
        public string GetLabExecutionResults(int executionId)
        {
            const string methodName = "GetLabExecutionResults";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            String labExecutionResults = null;

            if (this.labEquipmentEngine != null)
            {
                labExecutionResults = this.labEquipmentEngine.GetExperimentResults(executionId);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return labExecutionResults;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="executionId"></param>
        /// <returns>bool</returns>
        public bool CancelLabExecution(int executionId)
        {
            const string methodName = "CancelLabExecution";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool cancelled = false;

            if (this.labEquipmentEngine != null)
            {
                cancelled = this.labEquipmentEngine.CancelLabExecution(executionId);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return cancelled;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        public void Close()
        {
            const string methodName = "Close";
            Logfile.WriteCalled(Logfile.Level.Info, STR_ClassName, methodName);

            if (this.labEquipmentEngine != null)
            {
                this.labEquipmentEngine.Close();
            }

            Logfile.WriteCompleted(Logfile.Level.Info, STR_ClassName, methodName);
        }
    }
}
