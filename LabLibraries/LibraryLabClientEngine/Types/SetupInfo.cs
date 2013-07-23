using System;

namespace Library.LabClient.Engine.Types
{
    public class SetupInfo
    {
        private String id;
        private String name;
        private String description;
        private String xmlSetup;

        public String Id
        {
            get { return id; }
            set { id = value; }
        }

        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        public String Description
        {
            get { return description; }
            set { description = value; }
        }

        public String XmlSetup
        {
            get { return xmlSetup; }
            set { xmlSetup = value; }
        }

        public SetupInfo()
        {
        }

        public SetupInfo(String id)
        {
            this.id = id;
        }
    }
}
