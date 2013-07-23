using System;
using System.Collections.Generic;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;
using Library.Lab;
using Library.Lab.Database;
using Library.Lab.Types;
using Library.LabServer;
using Library.ServiceBroker;
using Library.ServiceBroker.Database;
using Library.ServiceBroker.Database.Types;
using Library.ServiceBroker.Types;
using Library.Lab.Exceptions;

namespace ServiceBroker
{
    [XmlTypeAttribute(Namespace = "http://ilab.mit.edu")]
    [XmlRootAttribute(Namespace = "http://ilab.mit.edu", IsNullable = false)]
    public class sbAuthHeader : SoapHeader
    {
        private long couponId;
        private String couponPasskey;

        [XmlElement(ElementName = "couponID")]
        public long CouponId
        {
            get { return couponId; }
            set { couponId = value; }
        }

        [XmlElement(ElementName = "couponPassKey")]
        public String CouponPasskey
        {
            get { return couponPasskey; }
            set { couponPasskey = value; }
        }
    }

    //-------------------------------------------------------------------------------------------------//

    [WebService(Namespace = "http://ilab.mit.edu")]
    [XmlTypeAttribute(Namespace = "http://ilab.mit.edu")]
    public class ServiceBrokerService : System.Web.Services.WebService
    {
        #region Constants
        private const String STR_ClassName = "ServiceBrokerService";
        private const Logfile.Level logLevel = Logfile.Level.Fine;
        /*
         * String constants
         */
        private const String STR_UserGroup = "DummyServiceBroker";
        /*
         * String constants for logfile messages
         */
        private const String STRLOG_SbAuthHeaderNull = "SbAuthHeader: null";
        private const String STRLOG_CouponIdPasskey_arg2 = "CouponId: {0:d}  CouponPasskey: '{1:s}'";
        private const String STRCFG_ServiceBrokerGuid_arg = "ServiceBrokerGuid: {0:s}";
        private const String STRLOG_LabServerGuid_arg = "LabServerGuid: {0:s}";
        private const string STRLOG_ExperimentId_arg = " ExperimentId: {0:d}";
        private const string STRLOG_NextExperimentId_arg = " Next ExperimentId: {0:d}";
        private const string STRLOG_Success_arg = " Success: {0:s}";
        /*
         * String constants for exception messages
         */
        private const String STRERR_AccessDenied_arg = "ServiceBroker Access Denied: {0:s}";
        private const String STRERR_SbAuthHeaderNull = "SbAuthHeader is null";
        private const String STRERR_CouponIdInvalid_arg = "CouponId {0:d} is invalid";
        private const String STRERR_CouponPasskeyNull = "CouponPasskey is null";
        private const String STRERR_CouponPasskeyInvalid_arg = "CouponPasskey '{0:s}' is invalid";
        private const String STRERR_LabServerIdInvalid = "LabServer Id is invalid";
        private const String STRERR_LabServerIdUnknown_arg = "LabServer Id Unknown: {0:s}";
        #endregion

        #region Variables
        public sbAuthHeader sbHeader;
        //
        private static ExperimentsDB _experimentsDB = null;
        private static Dictionary<String, LabServerAPI> _mapLabServerAPI = null;
        //
        private ConfigProperties configProperties;
        private ExperimentsDB experimentsDB;
        private Dictionary<String, LabServerAPI> mapLabServerAPI;
        #endregion

        //---------------------------------------------------------------------------------------//

        public ServiceBrokerService()
        {
            const String methodName = "ServiceBrokerService";

            /*
             * Check if initialisation needs to be done
             */
            if (_experimentsDB == null)
            {
                Logfile.WriteCalled(logLevel, STR_ClassName, methodName);


                try
                {
                    /*
                     * Create instance of ExperimentsDB
                     */
                    DBConnection dbConnection = Global.ConfigProperties.DbConnection;
                    _experimentsDB = new ExperimentsDB(dbConnection);
                    if (_experimentsDB == null)
                    {
                        throw new ArgumentNullException(ExperimentsDB.ClassName);
                    }

                    /*
                     * Get the next experiment Id from the experiment database
                     */
                    int nextExperimentId = _experimentsDB.GetNextExperimentId();
                    Logfile.Write(String.Format(STRLOG_NextExperimentId_arg, nextExperimentId.ToString()));

                    /*
                     * Create instance of LabServerAPI mapping
                     */
                    _mapLabServerAPI = new Dictionary<string, LabServerAPI>();
                }
                catch (Exception ex)
                {
                    Logfile.WriteError(ex.Message);
                }

                Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
            }

            /*
             * Create local instances
             */
            this.configProperties = Global.ConfigProperties;
            this.experimentsDB = _experimentsDB;
            this.mapLabServerAPI = _mapLabServerAPI;
        }

        //-------------------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("sbHeader", Direction = SoapHeaderDirection.In)]
        public bool Cancel(int experimentID)
        {
            const String methodName = "Cancel";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                String.Format(STRLOG_ExperimentId_arg, experimentID));

            bool success = false;

            try
            {
                this.Authenticate(sbHeader);

                /*
                 * Get the LabServer for the specified experiment
                 */
                ExperimentInfo experimentInfo = this.experimentsDB.RetrieveByExperimentId(experimentID);
                if (experimentInfo != null)
                {
                    /*
                     * Pass to LabServer for processing
                     */
                    LabServerAPI labServerAPI = GetLabServerAPI(experimentInfo.LabServerGuid);
                    success = labServerAPI.Cancel(experimentID);
                }
            }
            catch (ProtocolException ex)
            {
                throw new SoapException(ex.Message, SoapException.ClientFaultCode);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                String.Format(STRLOG_Success_arg, success.ToString()));

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("sbHeader", Direction = SoapHeaderDirection.In)]
        public WaitEstimate GetEffectiveQueueLength(string labServerID, int priorityHint)
        {
            const String methodName = "GetEffectiveQueueLength";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            WaitEstimate waitEstimate = null;

            try
            {
                this.Authenticate(sbHeader);

                /*
                 * Pass to LabServer for processing
                 */
                LabServerAPI labServerAPI = GetLabServerAPI(labServerID);
                waitEstimate = labServerAPI.GetEffectiveQueueLength(STR_UserGroup, priorityHint);
            }
            catch (ProtocolException ex)
            {
                throw new SoapException(ex.Message, SoapException.ClientFaultCode);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            return waitEstimate;
        }

        //-------------------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("sbHeader", Direction = SoapHeaderDirection.In)]
        public LabExperimentStatus GetExperimentStatus(int experimentID)
        {
            const String methodName = "GetExperimentStatus";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_ExperimentId_arg, experimentID));

            LabExperimentStatus labExperimentStatus = null;

            try
            {
                this.Authenticate(sbHeader);

                /*
                 * Get the LabServer for the specified experiment
                 */
                ExperimentInfo experimentInfo = this.experimentsDB.RetrieveByExperimentId(experimentID);
                if (experimentInfo != null)
                {
                    /*
                     * Pass to LabServer for processing
                     */
                    LabServerAPI labServerAPI = GetLabServerAPI(experimentInfo.LabServerGuid);
                    labExperimentStatus = labServerAPI.GetExperimentStatus(experimentID);
                }
            }
            catch (ProtocolException ex)
            {
                throw new SoapException(ex.Message, SoapException.ClientFaultCode);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return labExperimentStatus;
        }

        //-------------------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("sbHeader", Direction = SoapHeaderDirection.In)]
        public string GetLabConfiguration(string labServerID)
        {
            const String methodName = "GetLabConfiguration";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            String labConfiguration = null;

            try
            {
                this.Authenticate(sbHeader);

                /*
                 * Pass to LabServer for processing
                 */
                LabServerAPI labServerAPI = GetLabServerAPI(labServerID);
                labConfiguration = labServerAPI.GetLabConfiguration(STR_UserGroup);
            }
            catch (ProtocolException ex)
            {
                throw new SoapException(ex.Message, SoapException.ClientFaultCode);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return labConfiguration;
        }

        //-------------------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("sbHeader", Direction = SoapHeaderDirection.In)]
        public string GetLabInfo(string labServerID)
        {
            const String methodName = "GetLabInfo";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            String labInfo = null;

            try
            {
                this.Authenticate(sbHeader);

                /*
                 * Pass to LabServer for processing
                 */
                LabServerAPI labServerAPI = GetLabServerAPI(labServerID);
                labInfo = labServerAPI.GetLabInfo();
            }
            catch (ProtocolException ex)
            {
                throw new SoapException(ex.Message, SoapException.ClientFaultCode);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return labInfo;
        }

        //-------------------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("sbHeader", Direction = SoapHeaderDirection.In)]
        public LabStatus GetLabStatus(string labServerID)
        {
            const String methodName = "GetLabStatus";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_LabServerGuid_arg, labServerID));

            LabStatus labStatus = null;

            try
            {
                this.Authenticate(sbHeader);

                /*
                 * Pass to LabServer for processing
                 */
                LabServerAPI labServerAPI = GetLabServerAPI(labServerID);
                labStatus = labServerAPI.GetLabStatus();
            }
            catch (ProtocolException ex)
            {
                throw new SoapException(ex.Message, SoapException.ClientFaultCode);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return labStatus;
        }

        //-------------------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("sbHeader", Direction = SoapHeaderDirection.In)]
        public ResultReport RetrieveResult(int experimentID)
        {
            const String methodName = "RetrieveResult";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_ExperimentId_arg, experimentID));

            ResultReport resultReport = null;

            try
            {
                this.Authenticate(sbHeader);

                /*
                 * Get the LabServer for the specified experiment
                 */
                ExperimentInfo experimentInfo = this.experimentsDB.RetrieveByExperimentId(experimentID);
                if (experimentInfo != null)
                {
                    /*
                     * Pass to LabServer for processing
                     */
                    LabServerAPI labServerAPI = GetLabServerAPI(experimentInfo.LabServerGuid);
                    resultReport = labServerAPI.RetrieveResult(experimentID);
                }
            }
            catch (ProtocolException ex)
            {
                throw new SoapException(ex.Message, SoapException.ClientFaultCode);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return resultReport;
        }

        //-------------------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("sbHeader", Direction = SoapHeaderDirection.In)]
        public ClientSubmissionReport Submit(string labServerID, string experimentSpecification, int priorityHint, bool emailNotification)
        {
            const String methodName = "Submit";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            ClientSubmissionReport clientSubmissionReport = null;

            try
            {
                this.Authenticate(sbHeader);

                /*
                 * Add the LabServer Id mapping to the database and get the experiment Id
                 */
                ExperimentInfo experimentInfo = new ExperimentInfo(labServerID);
                int experimentId = experimentsDB.Add(experimentInfo);

                /*
                 * Pass to LabServer for processing
                 */
                LabServerAPI labServerAPI = this.GetLabServerAPI(labServerID);
                SubmissionReport submissionReport = labServerAPI.Submit(experimentId, experimentSpecification, STR_UserGroup, priorityHint);

                /*
                 * Convert to return type
                 */
                clientSubmissionReport = new ClientSubmissionReport();
                clientSubmissionReport.ExperimentId = submissionReport.ExperimentId;
                clientSubmissionReport.MinTimeToLive = submissionReport.MinTimeToLive;
                clientSubmissionReport.ValidationReport = submissionReport.ValidationReport;
                clientSubmissionReport.WaitEstimate = submissionReport.WaitEstimate;
            }
            catch (ProtocolException ex)
            {
                throw new SoapException(ex.Message, SoapException.ClientFaultCode);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return clientSubmissionReport;
        }

        //-------------------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("sbHeader", Direction = SoapHeaderDirection.In)]
        public ValidationReport Validate(string labServerID, string experimentSpecification)
        {
            const String methodName = "Validate";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            ValidationReport validationReport = null;

            try
            {
                this.Authenticate(sbHeader);

                /*
                 * Pass to LabServer for processing
                 */
                LabServerAPI labServerAPI = this.GetLabServerAPI(labServerID);
                validationReport = labServerAPI.Validate(experimentSpecification, labServerID);
            }
            catch (ProtocolException ex)
            {
                throw new SoapException(ex.Message, SoapException.ClientFaultCode);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return validationReport;
        }

        //---------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("sbHeader", Direction = SoapHeaderDirection.In)]
        public void Notify(int experimentID)
        {
            const String methodName = "Notify";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_ExperimentId_arg, experimentID));

            //this.Authenticate(sbHeader);

            bool success = false;

            try
            {
                /*
                 * Get the LabServer for the specified experiment
                 */
                ExperimentInfo experimentInfo = this.experimentsDB.RetrieveByExperimentId(experimentID);
                if (experimentInfo != null)
                {
                    /*
                     * Pass to LabServer for processing
                     */
                    LabServerAPI labServerAPI = GetLabServerAPI(experimentInfo.LabServerGuid);
                    labServerAPI.RetrieveResult(experimentID);
                }

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                String.Format(STRLOG_Success_arg, success.ToString()));
        }

        //=======================================================================================//

        private void Authenticate(sbAuthHeader sbHeader)
        {
            /*
             * Check if authenticating
             */
            if (this.configProperties.Authenticating == true)
            {
                if (this.configProperties.LogAuthentication == true)
                {
                    if (sbHeader == null)
                    {
                        Logfile.Write(STRLOG_SbAuthHeaderNull);
                    }
                    else
                    {
                        Logfile.Write(String.Format(STRLOG_CouponIdPasskey_arg2, sbHeader.CouponId, sbHeader.CouponPasskey));
                    }
                }
                try
                {
                    /*
                     * Check that AuthHeader is specified
                     */
                    if (sbHeader == null)
                    {
                        throw new ApplicationException(STRERR_SbAuthHeaderNull);
                    }

                    /*
                     * Verify the Coupon Id
                     */
                    if (sbHeader.CouponId != this.configProperties.CouponId)
                    {
                        throw new ApplicationException(String.Format(STRERR_CouponIdInvalid_arg, sbHeader.CouponId));
                    }

                    /*
                     * Verify the Coupon Passkey
                     */
                    if (sbHeader.CouponPasskey == null)
                    {
                        throw new ApplicationException(STRERR_CouponPasskeyNull);
                    }
                    if (sbHeader.CouponPasskey.Equals(this.configProperties.CouponPasskey, StringComparison.OrdinalIgnoreCase) == false)
                    {
                        throw new ApplicationException(String.Format(STRERR_CouponPasskeyInvalid_arg, sbHeader.CouponPasskey));
                    }
                }
                catch (Exception ex)
                {
                    String message = String.Format(STRERR_AccessDenied_arg, ex.Message);
                    Logfile.WriteError(message);
                    throw new ProtocolException(message);
                }
            }
        }

        //---------------------------------------------------------------------------------------//

        private LabServerAPI GetLabServerAPI(String labServerGuid)
        {
            LabServerAPI labServerAPI = null;

            /*
             * Check that the LabServer Guid is specified
             */
            if (labServerGuid == null || labServerGuid.Trim().Length == 0)
            {
                throw new ProtocolException(STRERR_LabServerIdInvalid);
            }

            /*
             * Convert LabServer Guid to uppercase
             */
            labServerGuid = labServerGuid.Trim().ToUpper();

            /*
             * Check if the BatchLabServerAPI for this labServerGuid already exists
             */
            if (this.mapLabServerAPI.TryGetValue(labServerGuid, out labServerAPI) == false)
            {
                /*
                 * Get LabServer information
                 */
                LabServerInfo labServerInfo;
                if (this.configProperties.LabServerInfos.TryGetValue(labServerGuid, out labServerInfo) == false)
                {
                    throw new ProtocolException(String.Format(STRERR_LabServerIdUnknown_arg, labServerGuid));
                }

                /*
                 * Create an instance of LabServerAPI for this LabServer
                 */
                labServerAPI = new LabServerAPI(labServerInfo.ServiceUrl);
                labServerAPI.Identifier = this.configProperties.ServiceBrokerGuid;
                labServerAPI.Passkey = labServerInfo.OutgoingPasskey;

                /*
                 * Add the LabServerAPI to the map for next time
                 */
                this.mapLabServerAPI.Add(labServerGuid, labServerAPI);
            }

            return labServerAPI;
        }
    }
}
