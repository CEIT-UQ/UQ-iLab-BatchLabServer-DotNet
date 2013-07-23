using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;
using Library.Lab.Types;

namespace ILabServer
{
    [XmlTypeAttribute(Namespace = "http://ilab.mit.edu")]
    [XmlRootAttribute(Namespace = "http://ilab.mit.edu", IsNullable = false)]
    public class AuthHeader : SoapHeader
    {
        public string identifier;
        public string passKey;
    }

    //-------------------------------------------------------------------------------------------------//

    [WebService(Namespace = "http://ilab.mit.edu")]
    [XmlTypeAttribute(Namespace = "http://ilab.mit.edu")]
    public abstract class LabServerWebService : System.Web.Services.WebService
    {
        public AuthHeader authHeader;

        //
        // ServiceBroker to LabServer web methods
        //

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public abstract bool Cancel(int experimentID);

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public abstract WaitEstimate GetEffectiveQueueLength(string userGroup, int priorityHint);

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public abstract LabExperimentStatus GetExperimentStatus(int experimentID);

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public abstract string GetLabConfiguration(string userGroup);

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public abstract string GetLabInfo();

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public abstract LabStatus GetLabStatus();

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public abstract ResultReport RetrieveResult(int experimentID);

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public abstract SubmissionReport Submit(int experimentID, string experimentSpecification,
            string userGroup, int priorityHint);

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public abstract ValidationReport Validate(string experimentSpecification, string userGroup);

    }
}
