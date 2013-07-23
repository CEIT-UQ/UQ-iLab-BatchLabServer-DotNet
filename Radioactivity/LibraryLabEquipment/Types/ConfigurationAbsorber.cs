using System;
using System.IO;
using System.Xml.Serialization;

namespace Library.LabEquipment.Types
{
    [XmlRoot("absorber")]
    public class ConfigurationAbsorber
    {
        private String name;
        private String location;
        private int encoderPosition;
        private double selectTime;
        private double returnTime;
        private double absorption;

        //-------------------------------------------------------------------------------------------------//

        [XmlAttribute("name")]
        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        [XmlElement("location")]
        public String _Location
        {
            get { return location; }
            set { location = value; }
        }

        [XmlIgnore()]
        public char Location
        {
            get { return Char.Parse(location); }
            set { location = value.ToString(); }
        }

        [XmlElement("encoderPosition")]
        public int EncoderPosition
        {
            get { return encoderPosition; }
            set { encoderPosition = value; }
        }

        [XmlElement("selectTime")]
        public double SelectTime
        {
            get { return selectTime; }
            set { selectTime = value; }
        }

        [XmlElement("returnTime")]
        public double ReturnTime
        {
            get { return returnTime; }
            set { returnTime = value; }
        }

        [XmlElement("absorption")]
        public double Absorption
        {
            get { return absorption; }
            set { absorption = value; }
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public static ConfigurationAbsorber XmlParse(String xmlString)
        {
            ConfigurationAbsorber configurationAbsorber;

            XmlSerializer serializer = new XmlSerializer(typeof(ConfigurationAbsorber));
            StringReader stringReader = new StringReader(xmlString);
            configurationAbsorber = (ConfigurationAbsorber)serializer.Deserialize(stringReader);

            return configurationAbsorber;
        }
    }
}
