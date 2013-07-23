using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;
using Library.Lab;
using Library.Lab.Types;

namespace LabEquipment
{
    [XmlType(Namespace = "http://ilab.uq.edu.au/")]
    [XmlRoot(Namespace = "http://ilab.uq.edu.au/", IsNullable = false)]
    public class AuthHeader : SoapHeader
    {
        private string identifier;
        private string passkey;

        [XmlElement(ElementName = "identifier")]
        public String Identifier
        {
            get { return identifier; }
            set { identifier = value; }
        }

        [XmlElement(ElementName = "passKey")]
        public String Passkey
        {
            get { return passkey; }
            set { passkey = value; }
        }
    }

    //-------------------------------------------------------------------------------------------------//

    [WebService(Namespace = "http://ilab.uq.edu.au/")]
    public class LabEquipmentService : System.Web.Services.WebService
    {
        #region Constants
        private const string STR_ClassName = "LabEquipmentService";
        private const Logfile.Level logLevel = Logfile.Level.Info;
        /*
         * String constants for logfile messages
         */
        private const string STRLOG_AuthHeaderNull = "AuthHeader: null";
        private const string STRLOG_IdentifierPasskey_arg2 = "Identifier: '{0}'  Passkey: '{1}'";
        private const string STRLOG_ExecutionId_arg = "ExecutionId: {0}";
        private const string STRLOG_LabEquipmentStatus_arg2 = "Online: {0}  StatusMessage: {1}";
        private const string STRLOG_TimeUntilReady_arg = "TimeUntilReady: {0} seconds";
        private const string STRLOG_Validation_arg2 = "Accepted: {0}  ExecutionTime: {1}  ";
        private const string STRLOG_ExecutionStatus_arg3 = "ExecutionId: {0}  ExecuteStatus: {1}  ResultStatus: {2}  ";
        private const string STRLOG_TimeRemaining_arg = "TimeRemaining: {0}  ";
        private const string STRLOG_ErrorMessage_arg = "ErrorMessage: {0}";
        private const string STRLOG_Success_arg = "Success: {0}";
        /*
         * String constants for exception messages
         */
        private const string STRERR_EquipmentManagerCreateFailed = "EquipmentManager.Create() failed!";
        private const string STRERR_EquipmentManagerStartFailed = "EquipmentManager.Start() failed!";
        private const string STRERR_AccessDenied_arg = "LabEquipment Access Denied - {0}";
        private const string STRERR_AuthHeader = "AuthHeader";
        private const string STRERR_LabServerGuid = "LabServer Guid";
        private const string STRERR_Passkey = "Passkey";
        private const string STRERR_NotSpecified_arg = "{0}: Not specified!";
        private const string STRERR_Invalid_arg = "{0}: Invalid!";
        #endregion

        #region Variables
        public AuthHeader authHeader;
        #endregion

        //---------------------------------------------------------------------------------------//

        public LabEquipmentService()
        {
            this.authHeader = new AuthHeader();
        }

        //---------------------------------------------------------------------------------------//

        [WebMethod(Description = "Get the status of the LabEquipment.")]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public LabEquipmentStatus GetLabEquipmentStatus()
        {
            const string methodName = "GetLabEquipmentStatus";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            LabEquipmentStatus labEquipmentStatus = null;

            this.Authenticate(authHeader);

            try
            {
                /*
                 * Pass on to the Equipment Manager
                 */
                labEquipmentStatus = Global.equipmentManager.GetLabEquipmentStatus();
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_LabEquipmentStatus_arg2, labEquipmentStatus.Online, labEquipmentStatus.StatusMessage));

            return labEquipmentStatus;
        }

        //---------------------------------------------------------------------------------------//

        [WebMethod(Description = "Get the time in seconds until the LabEquipment is ready to use.")]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public int GetTimeUntilReady()
        {
            const string methodName = "GetTimeUntilReady";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            int timeUntilReady = -1;

            this.Authenticate(authHeader);

            try
            {
                /*
                 * Pass on to the Equipment Manager
                 */
                timeUntilReady = Global.equipmentManager.GetTimeUntilReady();
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_TimeUntilReady_arg, timeUntilReady));

            return timeUntilReady;
        }

        //---------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public Validation Validate(string xmlSpecification)
        {
            const string methodName = "Validate";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                xmlSpecification);

            Validation validation = null;

            this.Authenticate(authHeader);

            try
            {
                /*
                 * Pass on to the Equipment Manager
                 */
                validation = Global.equipmentManager.Validate(xmlSpecification);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            String logMessage = String.Empty;
            if (validation != null)
            {
                logMessage = String.Format(STRLOG_Validation_arg2, validation.Accepted, validation.ExecutionTime);
                if (validation.ErrorMessage != null)
                {
                    logMessage += String.Format(STRLOG_ErrorMessage_arg, validation.ErrorMessage);
                }
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName, logMessage);

            return validation;
        }

        //---------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public ExecutionStatus StartLabExecution(string xmlSpecification)
        {
            const string methodName = "StartLabExecution";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            ExecutionStatus executionStatus = null;

            this.Authenticate(authHeader);

            try
            {
                /*
                 * Pass on to the Equipment Manager
                 */
                executionStatus = Global.equipmentManager.StartLabExecution(xmlSpecification);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            String logMessage = String.Empty;
            if (executionStatus != null)
            {
                logMessage = String.Format(STRLOG_ExecutionStatus_arg3, executionStatus.ExecutionId,
                        Enum.GetName(typeof(ExecutionStatus.Status), executionStatus.ExecuteStatus),
                        Enum.GetName(typeof(ExecutionStatus.Status), executionStatus.ResultStatus));
                if (executionStatus.TimeRemaining >= 0)
                {
                    logMessage += String.Format(STRLOG_TimeRemaining_arg, executionStatus.TimeRemaining);
                }
                if (executionStatus.ErrorMessage != null && executionStatus.ErrorMessage.Trim() != String.Empty)
                {
                    logMessage += String.Format(STRLOG_ErrorMessage_arg, executionStatus.ErrorMessage);
                }
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName, logMessage);

            return executionStatus;
        }

        //---------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public ExecutionStatus GetLabExecutionStatus(int executionId)
        {
            const string methodName = "GetLabExecutionStatus";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                String.Format(STRLOG_ExecutionId_arg, executionId));

            ExecutionStatus executionStatus = null;

            this.Authenticate(authHeader);

            try
            {
                /*
                 * Pass on to the Equipment Manager
                 */
                executionStatus = Global.equipmentManager.GetLabExecutionStatus(executionId);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            String logMessage = String.Empty;
            if (executionStatus != null)
            {
                logMessage = String.Format(STRLOG_ExecutionStatus_arg3, executionStatus.ExecutionId,
                        Enum.GetName(typeof(ExecutionStatus.Status), executionStatus.ExecuteStatus),
                        Enum.GetName(typeof(ExecutionStatus.Status), executionStatus.ResultStatus));
                if (executionStatus.TimeRemaining >= 0)
                {
                    logMessage += String.Format(STRLOG_TimeRemaining_arg, executionStatus.TimeRemaining);
                }
                if (executionStatus.ErrorMessage != null && executionStatus.ErrorMessage.Trim() != String.Empty)
                {
                    logMessage += String.Format(STRLOG_ErrorMessage_arg, executionStatus.ErrorMessage);
                }
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName, logMessage);

            return executionStatus;
        }

        //---------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public String GetLabExecutionResults(int executionId)
        {
            const string methodName = "GetLabExecutionResults";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                String.Format(STRLOG_ExecutionId_arg, executionId));

            String labExecutionResults = null;

            this.Authenticate(authHeader);

            try
            {
                /*
                 * Pass on to the Equipment Manager
                 */
                labExecutionResults = Global.equipmentManager.GetLabExecutionResults(executionId);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return labExecutionResults;
        }

        //---------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public bool CancelLabExecution(int executionId)
        {
            const string methodName = "CancelLabExecution";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;

            this.Authenticate(authHeader);

            try
            {
                /*
                 * Pass on to the Equipment Manager
                 */
                success = Global.equipmentManager.CancelLabExecution(executionId);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //================================================================================================================//

        private bool Authenticate(AuthHeader authHeader)
        {
            /*
             * Assume this will fail
             */
            bool success = false;

            try
            {
                if (Global.configProperties.Authenticating == true)
                {
                    /*
                     * Check that the AuthHeader is specified
                     */
                    if (authHeader == null)
                    {
                        throw new ArgumentNullException(String.Format(STRERR_NotSpecified_arg, STRERR_AuthHeader));
                    }

                    /*
                     * Verify the LabServer Guid
                     */
                    if (authHeader.Identifier == null)
                    {
                        throw new ArgumentException(String.Format(STRERR_NotSpecified_arg, STRERR_LabServerGuid));
                    }
                    if (Global.configProperties.LabServerGuid.Equals(authHeader.Identifier, StringComparison.OrdinalIgnoreCase) == false)
                    {
                        throw new ArgumentException(String.Format(STRERR_Invalid_arg, STRERR_LabServerGuid));
                    }

                    /*
                     * Verify the passkey
                     */
                    if (authHeader.Passkey == null)
                    {
                        throw new ArgumentException(String.Format(STRERR_NotSpecified_arg, STRERR_Passkey));
                    }
                    if (Global.configProperties.LabServerPasskey.Equals(authHeader.Passkey, StringComparison.OrdinalIgnoreCase) == false)
                    {
                        throw new ArgumentException(String.Format(STRERR_Invalid_arg, STRERR_Passkey));
                    }
                }

                /*
                 * Successfully authenticated
                 */
                success = true;
            }
            catch (Exception ex)
            {
                String message = String.Format(STRERR_AccessDenied_arg, ex.Message);
                Logfile.WriteError(message);

                /*
                 * Throw SoapException back to the caller
                 */
                throw new SoapException(message, SoapException.ClientFaultCode);
            }

            return success;
        }
    }
}
