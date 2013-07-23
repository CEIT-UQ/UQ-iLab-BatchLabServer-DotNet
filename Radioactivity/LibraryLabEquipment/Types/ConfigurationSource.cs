using System;
using System.IO;
using System.Xml.Serialization;

namespace Library.LabEquipment.Types
{
    [XmlRoot("source")]
    public class ConfigurationSource
    {
        private String name;
        private String location;
        private int encoderPosition;
        private double selectTime;
        private double returnTime;

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

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public static ConfigurationSource XmlParse(String xmlString)
        {
            ConfigurationSource configurationSource;

            XmlSerializer serializer = new XmlSerializer(typeof(ConfigurationSource));
            StringReader stringReader = new StringReader(xmlString);
            configurationSource = (ConfigurationSource)serializer.Deserialize(stringReader);

            return configurationSource;
        }
    }
}
