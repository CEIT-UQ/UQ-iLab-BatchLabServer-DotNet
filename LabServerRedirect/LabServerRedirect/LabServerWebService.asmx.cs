using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml;
using System.Xml.Serialization;
using Library.Lab.Types;

namespace LabServer.Redirect
{
    [WebService(Namespace = "http://ilab.mit.edu")]
    [XmlType(Namespace = "http://ilab.mit.edu")]
    public class LabServerWebService : System.Web.Services.WebService
    {
        public Proxy.AuthHeader authHeader;

        //-------------------------------------------------------------------------------------------------//

        public LabServerWebService()
        {
            this.authHeader = new Proxy.AuthHeader();
        }

        //-------------------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public bool Cancel(int experimentID)
        {
            return (Global.Online == true) ?
                this.GetLabServerProxy().Cancel(experimentID) :
                false;
        }

        //-------------------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public WaitEstimate GetEffectiveQueueLength(string userGroup, int priorityHint)
        {
            return (Global.Online == true) ?
                this.GetLabServerProxy().GetEffectiveQueueLength(userGroup, priorityHint) :
                new WaitEstimate();
        }

        //-------------------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public LabExperimentStatus GetExperimentStatus(int experimentID)
        {
            return (Global.Online == true) ?
                this.GetLabServerProxy().GetExperimentStatus(experimentID) :
                new LabExperimentStatus(new ExperimentStatus(StatusCodes.Unknown));
        }

        //-------------------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public string GetLabConfiguration(string userGroup)
        {
            return (Global.Online == true) ?
                this.GetLabServerProxy().GetLabConfiguration(userGroup) :
                null;
        }

        //-------------------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public string GetLabInfo()
        {
            return (Global.Online == true) ?
                this.GetLabServerProxy().GetLabInfo() :
                null;
        }

        //-------------------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public LabStatus GetLabStatus()
        {
            return (Global.Online == true) ?
                this.GetLabServerProxy().GetLabStatus() :
                new LabStatus(false, Global.LabStatusMessage);
        }

        //-------------------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public ResultReport RetrieveResult(int experimentID)
        {
            return (Global.Online == true) ?
                this.GetLabServerProxy().RetrieveResult(experimentID) :
                new ResultReport(StatusCodes.Unknown, Global.LabStatusMessage);
        }

        //-------------------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public SubmissionReport Submit(int experimentID, string experimentSpecification, string userGroup, int priorityHint)
        {
            return (Global.Online == true) ?
                this.GetLabServerProxy().Submit(experimentID, experimentSpecification, userGroup, priorityHint) :
                new SubmissionReport(experimentID, new ValidationReport(Global.LabStatusMessage), new WaitEstimate(), 0.0);
        }

        //-------------------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public ValidationReport Validate(string experimentSpecification, string userGroup)
        {
            return (Global.Online == true) ?
                this.GetLabServerProxy().Validate(experimentSpecification, userGroup) :
                new ValidationReport(Global.LabStatusMessage);
        }

        //=================================================================================================//

        private Proxy.LabServerWebService GetLabServerProxy()
        {
            /*
             * Create instance of LabServerWebService proxy
             */
            Proxy.LabServerWebService labServerProxy = new Proxy.LabServerWebService();
            labServerProxy.Url = Global.LabServerUrl;
            labServerProxy.AuthHeaderValue = this.authHeader;

            return labServerProxy;
        }
    }
}
