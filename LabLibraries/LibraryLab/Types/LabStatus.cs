using System;
using System.Xml.Serialization;

namespace Library.Lab.Types
{
    /// <summary>
    /// LabStatus
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://ilab.mit.edu")]
    public class LabStatus
    {
        private bool online;
        private String labStatusMessage;

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// True if the LabServer is accepting experiments.
        /// </summary>
        [XmlElement("online")]
        public bool Online
        {
            get { return online; }
            set { online = value; }
        }

        /// <summary>
        /// Domain-dependent human-readable text describing the status of LabServer.
        /// </summary>
        [XmlElement("labStatusMessage")]
        public String LabStatusMessage
        {
            get { return labStatusMessage; }
            set { labStatusMessage = value; }
        }

        //-------------------------------------------------------------------------------------------------//

        public LabStatus()
        {
        }

        public LabStatus(bool online, string labStatusMessage)
        {
            this.online = online;
            this.labStatusMessage = labStatusMessage;
        }
    }
}
