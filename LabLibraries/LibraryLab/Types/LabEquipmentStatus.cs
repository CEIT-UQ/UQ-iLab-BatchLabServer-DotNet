using System;
using System.Xml.Serialization;

namespace Library.Lab.Types
{
    [Serializable()]
    [XmlType(Namespace = "http://ilab.uq.edu.au/")]
    public class LabEquipmentStatus
    {
        private bool online;
        private string statusMessage;

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// True if lab equipment has initialised and is online.
        /// </summary>
        [XmlElement("Online")]
        public bool Online
        {
            get { return this.online; }
            set { this.online = value; }
        }

        /// <summary>
        /// Message indicating the staus of the lab equipment whether it is online or not.
        /// </summary>
        [XmlElement("StatusMessage")]
        public string StatusMessage
        {
            get { return this.statusMessage; }
            set { this.statusMessage = value; }
        }

        //-------------------------------------------------------------------------------------------------//

        public LabEquipmentStatus()
        {
        }

        public LabEquipmentStatus(bool online, string statusMessage)
        {
            this.online = online;
            this.statusMessage = statusMessage;
        }
    }
}
