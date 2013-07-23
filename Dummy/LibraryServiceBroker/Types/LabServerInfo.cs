using System;

namespace Library.ServiceBroker.Types
{
    public class LabServerInfo
    {
        private String guid;
        private String serviceUrl;
        private String outgoingPasskey;
        private String incomingPasskey;

        public String Guid
        {
            get { return guid; }
        }

        public String ServiceUrl
        {
            get { return serviceUrl; }
        }

        public String OutgoingPasskey
        {
            get { return outgoingPasskey; }
        }

        public String IncomingPasskey
        {
            get { return incomingPasskey; }
        }

        public LabServerInfo(String guid, String serviceUrl, String outgoingPasskey, String incomingPasskey)
        {
            this.guid = guid;
            this.serviceUrl = serviceUrl;
            this.outgoingPasskey = outgoingPasskey;
            this.incomingPasskey = incomingPasskey;
        }
    }
}
