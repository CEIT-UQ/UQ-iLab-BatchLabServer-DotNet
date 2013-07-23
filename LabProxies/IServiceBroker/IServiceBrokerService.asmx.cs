using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;
using Library.Lab.Types;

namespace IServiceBroker
{
    [XmlTypeAttribute(Namespace = "http://ilab.mit.edu")]
    [XmlRootAttribute(Namespace = "http://ilab.mit.edu", IsNullable = false)]
    public class sbAuthHeader : SoapHeader
    {
        public long couponID;
        public string couponPassKey;
    }

    //-------------------------------------------------------------------------------------------------//

    [WebService(Namespace = "http://ilab.mit.edu")]
    [XmlTypeAttribute(Namespace = "http://ilab.mit.edu")]
    public abstract class ServiceBrokerService : System.Web.Services.WebService
    {
        public sbAuthHeader sbHeader;

        //
        // LabClient to LabServer pass-through web methods
        //

        [WebMethod(EnableSession = true)]
        [SoapHeader("sbHeader", Direction = SoapHeaderDirection.In)]
        public abstract bool Cancel(int experimentID);

        [WebMethod(EnableSession = true)]
        [SoapHeader("sbHeader", Direction = SoapHeaderDirection.In)]
        public abstract WaitEstimate GetEffectiveQueueLength(string labServerID, int priorityHint);

		[WebMethod (EnableSession = true)]
		[SoapHeader("sbHeader", Direction=SoapHeaderDirection.In)]
		public abstract LabExperimentStatus GetExperimentStatus(int experimentID);

        [WebMethod(EnableSession = true)]
        [SoapHeader("sbHeader", Direction = SoapHeaderDirection.In)]
        public abstract string GetLabConfiguration(string labServerID);

        [WebMethod(EnableSession = true)]
        [SoapHeader("sbHeader", Direction = SoapHeaderDirection.In)]
        public abstract string GetLabInfo(string labServerID);

        [WebMethod(EnableSession = true)]
        [SoapHeader("sbHeader", Direction = SoapHeaderDirection.In)]
        public abstract LabStatus GetLabStatus(string labServerID);

        [WebMethod( EnableSession = true)]
        [SoapHeader("sbHeader", Direction = SoapHeaderDirection.In)]
        public abstract ResultReport RetrieveResult(int experimentID);

        [WebMethod(EnableSession = true)]
        [SoapHeader("sbHeader", Direction = SoapHeaderDirection.In)]
        public abstract ClientSubmissionReport Submit(string labServerID, string experimentSpecification,
            int priorityHint, bool emailNotification);

        [WebMethod(EnableSession = true)]
        [SoapHeader("sbHeader", Direction = SoapHeaderDirection.In)]
        public abstract ValidationReport Validate(string labServerID, string experimentSpecification);

        //
        // LabServer to ServiceBroker web method
        //

        [WebMethod()]
        [SoapHeader("sbHeader", Direction = SoapHeaderDirection.In)]
        public abstract void Notify(int experimentID);

    }
}
