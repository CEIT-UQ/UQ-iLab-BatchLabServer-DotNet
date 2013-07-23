using System;
using System.Web.Services.Protocols;
using Library.Lab;
using Library.Lab.Types;
using Library.Lab.Exceptions;

namespace Library.LabServer
{
    public class LabServerAPI
    {
        #region Constants
        private const string STR_ClassName = "LabServerAPI";
        private const Logfile.Level logLevel = Logfile.Level.Finer;
        /*
         * String constants for logfile messages
         */
        private const String STRLOG_ServiceUrl_arg = "ServiceUrl: {0:s}";
        /*
         * String constants for exception messages
         */
        private const String STRERR_ServiceUrl = "serviceUrl";
        private const String STRERR_LabServerUnaccessible = "LabServer is unaccessible!";
        #endregion

        #region Variables
        private Proxy.LabServerWebService labServerProxy;
        #endregion

        #region Properties

        public static string ClassName
        {
            get { return STR_ClassName; }
        }

        private String identifier;
        private String passkey;

        public String Identifier
        {
            set { identifier = value; }
        }

        public String Passkey
        {
            set { passkey = value; }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceUrl"></param>
        public LabServerAPI(String serviceUrl)
        {
            const String methodName = "LabServerAPI";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_ServiceUrl_arg, serviceUrl));

            try
            {
                /*
                 * Check that parameters are valid
                 */
                if (serviceUrl == null)
                {
                    throw new ArgumentNullException(STRERR_ServiceUrl);
                }
                serviceUrl = serviceUrl.Trim();
                if (serviceUrl.Length == 0)
                {
                    throw new ArgumentNullException(STRERR_ServiceUrl);
                }

                /*
                 * Create a proxy for the LabServer's web service and set the web service URL
                 */
                this.labServerProxy = new Proxy.LabServerWebService();
                this.labServerProxy.Url = serviceUrl;

            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        public bool Cancel(int experimentId)
        {
            const String methodName = "Cancel";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool cancelled = false;

            try
            {
                /*
                 * Set the authentication information and call the web service
                 */
                this.SetAuthHeader();
                cancelled = this.labServerProxy.Cancel(experimentId);
            }
            catch (SoapException ex)
            {
                Logfile.Write(ex.Message);
                throw new ProtocolException(ex.Message);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
                throw new ProtocolException(STRERR_LabServerUnaccessible);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return cancelled;
        }

        //-------------------------------------------------------------------------------------------------//

        public WaitEstimate GetEffectiveQueueLength(String userGroup, int priorityHint)
        {
            const String methodName = "GetEffectiveQueueLength";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            WaitEstimate waitEstimate = null;

            try
            {
                /*
                 * Set the authentication information and call the web service
                 */
                this.SetAuthHeader();
                Proxy.WaitEstimate proxyWaitEstimate = this.labServerProxy.GetEffectiveQueueLength(userGroup, priorityHint);
                waitEstimate = this.ConvertType(proxyWaitEstimate);
            }
            catch (SoapException ex)
            {
                Logfile.Write(ex.Message);
                throw new ProtocolException(ex.Message);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
                throw new ProtocolException(STRERR_LabServerUnaccessible);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return waitEstimate;
        }

        //-------------------------------------------------------------------------------------------------//

        public LabExperimentStatus GetExperimentStatus(int experimentId)
        {
            const String methodName = "GetExperimentStatus";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            LabExperimentStatus labExperimentStatus = null;

            try
            {
                /*
                 * Set the authentication information and call the web service
                 */
                this.SetAuthHeader();
                Proxy.LabExperimentStatus proxyLabExperimentStatus = this.labServerProxy.GetExperimentStatus(experimentId);
                labExperimentStatus = this.ConvertType(proxyLabExperimentStatus);
            }
            catch (SoapException ex)
            {
                Logfile.Write(ex.Message);
                throw new ProtocolException(ex.Message);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
                throw new ProtocolException(STRERR_LabServerUnaccessible);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return labExperimentStatus;
        }

        //-------------------------------------------------------------------------------------------------//

        public String GetLabConfiguration(String userGroup)
        {
            const String methodName = "GetLabConfiguration";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            String labConfiguration = null;

            try
            {
                /*
                 * Set the authentication information and call the web service
                 */
                this.SetAuthHeader();
                labConfiguration = this.labServerProxy.GetLabConfiguration(userGroup);
            }
            catch (SoapException ex)
            {
                Logfile.Write(ex.Message);
                throw new ProtocolException(ex.Message);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
                throw new ProtocolException(STRERR_LabServerUnaccessible);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return labConfiguration;
        }

        //-------------------------------------------------------------------------------------------------//

        public String GetLabInfo()
        {
            const String methodName = "GetLabInfo";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            String labInfo = null;

            try
            {
                /*
                 * Set the authentication information and call the web service
                 */
                this.SetAuthHeader();
                labInfo = this.labServerProxy.GetLabInfo();
            }
            catch (SoapException ex)
            {
                Logfile.Write(ex.Message);
                throw new ProtocolException(ex.Message);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
                throw new ProtocolException(STRERR_LabServerUnaccessible);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return labInfo;
        }

        //-------------------------------------------------------------------------------------------------//

        public LabStatus GetLabStatus()
        {
            const String methodName = "GetLabStatus";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            LabStatus labStatus = null;

            try
            {
                /*
                 * Set the authentication information and call the web service
                 */
                this.SetAuthHeader();
                Proxy.LabStatus proxyLabStatus = this.labServerProxy.GetLabStatus();
                labStatus = this.ConvertType(proxyLabStatus);
            }
            catch (SoapException ex)
            {
                Logfile.Write(ex.Message);
                throw new ProtocolException(ex.Message);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
                throw new ProtocolException(STRERR_LabServerUnaccessible);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return labStatus;
        }

        //-------------------------------------------------------------------------------------------------//

        public ResultReport RetrieveResult(int experimentId)
        {
            const String methodName = "RetrieveResult";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            ResultReport resultReport = null;

            try
            {
                /*
                 * Set the authentication information and call the web service
                 */
                this.SetAuthHeader();
                Proxy.ResultReport proxyResultReport = this.labServerProxy.RetrieveResult(experimentId);
                resultReport = this.ConvertType(proxyResultReport);
            }
            catch (SoapException ex)
            {
                Logfile.Write(ex.Message);
                throw new ProtocolException(ex.Message);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
                throw new ProtocolException(STRERR_LabServerUnaccessible);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return resultReport;
        }

        //-------------------------------------------------------------------------------------------------//

        public SubmissionReport Submit(int experimentId, String experimentSpecification, String userGroup, int priorityHint)
        {
            const String methodName = "Submit";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            SubmissionReport submissionReport = null;

            try
            {
                /*
                 * Set the authentication information and call the web service
                 */
                this.SetAuthHeader();
                Proxy.SubmissionReport proxySubmissionReport = this.labServerProxy.Submit(experimentId, experimentSpecification, userGroup, priorityHint);
                submissionReport = this.ConvertType(proxySubmissionReport);
            }
            catch (SoapException ex)
            {
                Logfile.Write(ex.Message);
                throw new ProtocolException(ex.Message);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
                throw new ProtocolException(STRERR_LabServerUnaccessible);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return submissionReport;
        }

        //-------------------------------------------------------------------------------------------------//

        public ValidationReport Validate(String experimentSpecification, String userGroup)
        {
            const String methodName = "Validate";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            ValidationReport validationReport = null;

            try
            {
                /*
                 * Set the authentication information and call the web service
                 */
                this.SetAuthHeader();
                Proxy.ValidationReport proxyValidationReport = this.labServerProxy.Validate(experimentSpecification, userGroup);
                validationReport = this.ConvertType(proxyValidationReport);
            }
            catch (SoapException ex)
            {
                Logfile.Write(ex.Message);
                throw new ProtocolException(ex.Message);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
                throw new ProtocolException(STRERR_LabServerUnaccessible);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return validationReport;
        }

        //=================================================================================================//

        private void SetAuthHeader()
        {
            /*
             * Create authentication header
             */
            Proxy.AuthHeader authHeader = new Proxy.AuthHeader();
            authHeader.identifier = this.identifier;
            authHeader.passKey = this.passkey;
            this.labServerProxy.AuthHeaderValue = authHeader;
        }

        //-------------------------------------------------------------------------------------------------//

        private ExperimentStatus ConvertType(Proxy.ExperimentStatus proxyExperimentStatus)
        {
            ExperimentStatus experimentStatus = null;

            if (proxyExperimentStatus != null)
            {
                experimentStatus = new ExperimentStatus();
                experimentStatus.EstimatedRemainingRuntime = proxyExperimentStatus.estRemainingRuntime;
                experimentStatus.EstimatedRuntime = proxyExperimentStatus.estRuntime;
                experimentStatus.StatusCode = (StatusCodes)proxyExperimentStatus.statusCode;
                experimentStatus.WaitEstimate = this.ConvertType(proxyExperimentStatus.wait);
            }

            return experimentStatus;
        }

        //-------------------------------------------------------------------------------------------------//

        private LabExperimentStatus ConvertType(Proxy.LabExperimentStatus proxyLabExperimentStatus)
        {
            LabExperimentStatus labExperimentStatus = null;

            if (proxyLabExperimentStatus != null)
            {
                labExperimentStatus = new LabExperimentStatus();
                labExperimentStatus.MinTimeToLive = proxyLabExperimentStatus.minTimetoLive;
                labExperimentStatus.ExperimentStatus = this.ConvertType(proxyLabExperimentStatus.statusReport);
            }

            return labExperimentStatus;
        }

        //-------------------------------------------------------------------------------------------------//

        private LabStatus ConvertType(Proxy.LabStatus proxyLabStatus)
        {
            LabStatus labStatus = null;

            if (proxyLabStatus != null)
            {
                labStatus = new LabStatus();
                labStatus.Online = proxyLabStatus.online;
                labStatus.LabStatusMessage = proxyLabStatus.labStatusMessage;
            }

            return labStatus;
        }

        //-------------------------------------------------------------------------------------------------//

        private ResultReport ConvertType(Proxy.ResultReport proxyResultReport)
        {
            ResultReport resultReport = null;

            if (proxyResultReport != null)
            {
                resultReport = new ResultReport();
                resultReport.ErrorMessage = proxyResultReport.errorMessage;
                resultReport.XmlExperimentResults = proxyResultReport.experimentResults;
                resultReport.StatusCode = (StatusCodes)proxyResultReport.statusCode;
                resultReport.XmlBlobExtension = proxyResultReport.xmlBlobExtension;
                resultReport.XmlResultExtension = proxyResultReport.xmlResultExtension;
                resultReport.WarningMessages = proxyResultReport.warningMessages;
            }

            return resultReport;
        }

        //-------------------------------------------------------------------------------------------------//

        private SubmissionReport ConvertType(Proxy.SubmissionReport proxySubmissionReport)
        {
            SubmissionReport submissionReport = null;

            if (proxySubmissionReport != null)
            {
                submissionReport = new SubmissionReport();
                submissionReport.ExperimentId = proxySubmissionReport.experimentID;
                submissionReport.MinTimeToLive = proxySubmissionReport.minTimeToLive;
                submissionReport.ValidationReport = this.ConvertType(proxySubmissionReport.vReport);
                submissionReport.WaitEstimate = this.ConvertType(proxySubmissionReport.wait);
            }

            return submissionReport;
        }

        //-------------------------------------------------------------------------------------------------//

        private ValidationReport ConvertType(Proxy.ValidationReport proxyValidationReport)
        {
            ValidationReport validationReport = null;

            if (proxyValidationReport != null)
            {
                validationReport = new ValidationReport();
                validationReport.Accepted = proxyValidationReport.accepted;
                validationReport.ErrorMessage = proxyValidationReport.errorMessage;
                validationReport.EstimatedRuntime = proxyValidationReport.estRuntime;
                validationReport.WarningMessages = proxyValidationReport.warningMessages;
            }

            return validationReport;
        }

        //-------------------------------------------------------------------------------------------------//

        private WaitEstimate ConvertType(Proxy.WaitEstimate proxyWaitEstimate)
        {
            WaitEstimate waitEstimate = null;

            if (proxyWaitEstimate != null)
            {
                waitEstimate = new WaitEstimate();
                waitEstimate.EffectiveQueueLength = proxyWaitEstimate.effectiveQueueLength;
                waitEstimate.EstimatedWait = proxyWaitEstimate.estWait;
            }

            return waitEstimate;
        }
    }
}
