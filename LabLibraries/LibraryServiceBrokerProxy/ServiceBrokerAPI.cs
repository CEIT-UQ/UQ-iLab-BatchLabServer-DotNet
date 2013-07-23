using System;
using System.Web.Services.Protocols;
using Library.Lab;
using Library.Lab.Exceptions;
using Library.Lab.Types;

namespace Library.ServiceBroker
{
    public class ServiceBrokerAPI
    {
        #region Constants
        private const string STR_ClassName = "ServiceBrokerAPI";
        private const Logfile.Level logLevel = Logfile.Level.Finer;
        /*
         * String constants for logfile messages
         */
        private const String STRLOG_ServiceUrl_arg = "ServiceUrl: {0:s}";
        private const String STRLOG_ExperimentId_arg = "ExperimentId: {0:d}";
        private const String STRLOG_Success_arg = "Success: {0:s}";
        /*
         * String constants for exception messages
         */
        private const String STRERR_ServiceUrl = "serviceUrl";
        private const String STRERR_ServiceBrokerUnaccessible = "ServiceBroker is unaccessible!";
        #endregion

        #region Variables
        private Proxy.ServiceBrokerService serviceBrokerProxy;
        #endregion

        #region Properties

        public static string ClassName
        {
            get { return STR_ClassName; }
        }

        private long couponId;
        private String couponPasskey;
        private String labServerId;

        public long CouponId
        {
            get { return couponId; }
            set { couponId = value; }
        }

        public String CouponPasskey
        {
            get { return couponPasskey; }
            set { couponPasskey = value; }
        }

        public String LabServerId
        {
            get { return labServerId; }
            set { labServerId = value; }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceUrl"></param>
        public ServiceBrokerAPI(String serviceUrl)
        {
            const String methodName = "ServiceBrokerAPI";
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
                 * Create a proxy for the ServiceBroker's web service and set the web service URL
                 */
                this.serviceBrokerProxy = new Proxy.ServiceBrokerService();
                this.serviceBrokerProxy.Url = serviceUrl;

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

            bool success = false;

            try
            {
                /*
                 * Set the authentication information and call the web service
                 */
                this.SetAuthHeader();
                success = this.serviceBrokerProxy.Cancel(experimentId);
            }
            catch (SoapException ex)
            {
                Logfile.Write(ex.Message);
                throw new ProtocolException(ex.Message);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
                throw new Exception(STRERR_ServiceBrokerUnaccessible);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public WaitEstimate GetEffectiveQueueLength()
        {
            return this.GetEffectiveQueueLength(0);
        }

        //-------------------------------------------------------------------------------------------------//

        public WaitEstimate GetEffectiveQueueLength(int priorityHint)
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
                Proxy.WaitEstimate proxyWaitEstimate = this.serviceBrokerProxy.GetEffectiveQueueLength(this.labServerId, priorityHint);
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
                throw new Exception(STRERR_ServiceBrokerUnaccessible);
            }

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
                Proxy.LabExperimentStatus proxyLabExperimentStatus = this.serviceBrokerProxy.GetExperimentStatus(experimentId);
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
                throw new Exception(STRERR_ServiceBrokerUnaccessible);
            }

            return labExperimentStatus;
        }

        //-------------------------------------------------------------------------------------------------//

        public String GetLabConfiguration()
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
                labConfiguration = this.serviceBrokerProxy.GetLabConfiguration(this.labServerId);
            }
            catch (SoapException ex)
            {
                Logfile.Write(ex.Message);
                throw new ProtocolException(ex.Message);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
                throw new Exception(STRERR_ServiceBrokerUnaccessible);
            }

            return labConfiguration;
        }

        //-------------------------------------------------------------------------------------------------//

        public String GetLabInfo(String labServerId)
        {
            const String methodName = "GetLabConfiguration";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            String labInfo = null;

            try
            {
                /*
                 * Set the authentication information and call the web service
                 */
                this.SetAuthHeader();
                labInfo = this.serviceBrokerProxy.GetLabInfo(labServerId);
            }
            catch (SoapException ex)
            {
                Logfile.Write(ex.Message);
                throw new ProtocolException(ex.Message);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
                throw new Exception(STRERR_ServiceBrokerUnaccessible);
            }

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
                Proxy.LabStatus proxyLabStatus = this.serviceBrokerProxy.GetLabStatus(this.labServerId);
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
                throw new Exception(STRERR_ServiceBrokerUnaccessible);
            }

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
                Proxy.ResultReport proxyResultReport = this.serviceBrokerProxy.RetrieveResult(experimentId);
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
                throw new Exception(STRERR_ServiceBrokerUnaccessible);
            }

            return resultReport;
        }

        //-------------------------------------------------------------------------------------------------//

        public ClientSubmissionReport Submit(String experimentSpecification)
        {
            return this.Submit(experimentSpecification, 0, false);
        }

        //-------------------------------------------------------------------------------------------------//

        public ClientSubmissionReport Submit(String experimentSpecification, int priorityHint, bool emailNotification)
        {
            const String methodName = "Submit";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            ClientSubmissionReport clientSubmissionReport = null;

            try
            {
                /*
                 * Set the authentication information and call the web service
                 */
                this.SetAuthHeader();
                Proxy.ClientSubmissionReport proxyClientSubmissionReport =
                    this.serviceBrokerProxy.Submit(this.labServerId, experimentSpecification, priorityHint, emailNotification);
                clientSubmissionReport = this.ConvertType(proxyClientSubmissionReport);
            }
            catch (SoapException ex)
            {
                Logfile.Write(ex.Message);
                throw new ProtocolException(ex.Message);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
                throw new Exception(STRERR_ServiceBrokerUnaccessible);
            }

            return clientSubmissionReport;
        }

        //-------------------------------------------------------------------------------------------------//

        public ValidationReport Validate(String experimentSpecification)
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
                Proxy.ValidationReport proxyValidationReport = this.serviceBrokerProxy.Validate(this.labServerId, experimentSpecification);
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
                throw new Exception(STRERR_ServiceBrokerUnaccessible);
            }

            return validationReport;
        }

        //-------------------------------------------------------------------------------------------------//

        public void Notify(int experimentId)
        {
            const String methodName = "Notify";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            try
            {
                /*
                 * Set the authentication information and call the web service
                 */
                this.SetAuthHeader();
                this.serviceBrokerProxy.Notify(experimentId);
            }
            catch (SoapException ex)
            {
                Logfile.Write(ex.Message);
                throw new ProtocolException(ex.Message);
            }
            catch (Exception ex)
            {
                //Logfile.WriteError(ex.ToString());
                Logfile.Write(ex.Message);
                throw new Exception(STRERR_ServiceBrokerUnaccessible);
            }
        }

        //=================================================================================================//

        private void SetAuthHeader()
        {
            /*
             * Create authentication header
             */
            Proxy.sbAuthHeader sbAuthHeader = new Proxy.sbAuthHeader();
            sbAuthHeader.couponID = this.couponId;
            sbAuthHeader.couponPassKey = this.couponPasskey;
            this.serviceBrokerProxy.sbAuthHeaderValue = sbAuthHeader;
        }

        //-------------------------------------------------------------------------------------------------//

        private ClientSubmissionReport ConvertType(Proxy.ClientSubmissionReport proxyClientSubmissionReport)
        {
            ClientSubmissionReport clientSubmissionReport = null;

            if (proxyClientSubmissionReport != null)
            {
                clientSubmissionReport = new ClientSubmissionReport();
                clientSubmissionReport.ExperimentId = proxyClientSubmissionReport.experimentID;
                clientSubmissionReport.MinTimeToLive = proxyClientSubmissionReport.minTimeToLive;
                clientSubmissionReport.ValidationReport = this.ConvertType(proxyClientSubmissionReport.vReport);
                clientSubmissionReport.WaitEstimate = this.ConvertType(proxyClientSubmissionReport.wait);
            }

            return clientSubmissionReport;
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
                experimentStatus.IntStatusCode = proxyExperimentStatus.statusCode;
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
                resultReport.IntStatusCode = proxyResultReport.statusCode;
                resultReport.XmlBlobExtension = proxyResultReport.xmlBlobExtension;
                resultReport.XmlResultExtension = proxyResultReport.xmlResultExtension;
                resultReport.WarningMessages = proxyResultReport.warningMessages;
            }

            return resultReport;
        }

        //-------------------------------------------------------------------------------------------------//

        private ValidationReport ConvertType(Proxy.ValidationReport proxyValidationReport)
        {
            ValidationReport validationReport = null;

            if (proxyValidationReport != null)
            {
                validationReport = new ValidationReport();
                validationReport.Accepted = proxyValidationReport.accepted;
                validationReport.WarningMessages = proxyValidationReport.warningMessages;
                validationReport.ErrorMessage = proxyValidationReport.errorMessage;
                validationReport.EstimatedRuntime = proxyValidationReport.estRuntime;
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
