using System;
using System.Xml.Serialization;
using System.Web.Services;
using System.Web.Services.Protocols;
using Library.Lab.Types;

namespace ILabEquipment
{
    [XmlType(Namespace = "http://ilab.uq.edu.au/")]
    [XmlRoot(Namespace = "http://ilab.uq.edu.au/", IsNullable = false)]
    public class AuthHeader : SoapHeader
    {
        public string identifier;
        public string passKey;
    }

    //-------------------------------------------------------------------------------------------------//

    [WebService(Namespace = "http://ilab.uq.edu.au/")]
    public abstract class LabEquipmentService : System.Web.Services.WebService
    {
        public AuthHeader authHeader;

        //
        // LabServer to LabEquipment web methods
        //

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public abstract LabEquipmentStatus GetLabEquipmentStatus();

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public abstract int GetTimeUntilReady();

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public abstract Validation Validate(string xmlSpecification);

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public abstract ExecutionStatus StartLabExecution(string xmlSpecification);

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public abstract ExecutionStatus GetLabExecutionStatus(int executionId);

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public abstract String GetLabExecutionResults(int executionId);

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public abstract bool CancelLabExecution(int executionId);
    }
}
