using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Library.LabEquipment.Engine.Types;

namespace Library.LabEquipment.Types
{
    [XmlRoot("experimentResults")]
    public class ExperimentResults : LabExperimentResults
    {
        private FloatData voltage;
        private FloatData current;
        private FloatData powerFactor;
        private FloatData speed;

        [XmlElement("voltage")]
        public FloatData Voltage
        {
            get { return voltage; }
            set { voltage = value; }
        }

        [XmlElement("current")]
        public FloatData Current
        {
            get { return current; }
            set { current = value; }
        }

        [XmlElement("powerFactor")]
        public FloatData PowerFactor
        {
            get { return powerFactor; }
            set { powerFactor = value; }
        }

        [XmlElement("speed")]
        public FloatData Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        //---------------------------------------------------------------------------------------//

        public ExperimentResults()
        {
            this.voltage = new FloatData();
            this.current = new FloatData();
            this.powerFactor = new FloatData();
            this.speed = new FloatData();
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public new static ExperimentResults ToObject(String xmlString)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ExperimentResults));
            StringReader stringReader = new StringReader(xmlString);
            return (ExperimentResults)serializer.Deserialize(stringReader);
        }
    }

    public class FloatData
    {
        private String name;
        private String units;
        private String format;
        private float data;

        [XmlAttribute("name")]
        public String Name
        {
            get { return name; }
            set { name = value; }
        }

        [XmlAttribute("units")]
        public String Units
        {
            get { return units; }
            set { units = value; }
        }

        [XmlAttribute("format")]
        public String Format
        {
            get { return format; }
            set { format = value; }
        }

        [XmlElement("data")]
        public float Data
        {
            get { return data; }
            set { data = value; }
        }
    }
}
