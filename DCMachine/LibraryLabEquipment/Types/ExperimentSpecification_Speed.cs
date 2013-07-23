using System;
using System.IO;
using System.Xml.Serialization;
using Library.LabEquipment.Engine.Types;

namespace Library.LabEquipment.Types
{
    [XmlRoot("experimentSpecification")]
    public class ExperimentSpecification_Speed : LabExperimentSpecification
    {
        private int speedMin;
        private int speedMax;
        private int speedStep;

        //-------------------------------------------------------------------------------------------------//

        [XmlElement("speedMin")]
        public int SpeedMin
        {
            get { return speedMin; }
            set { speedMin = value; }
        }

        [XmlElement("speedMax")]
        public int SpeedMax
        {
            get { return speedMax; }
            set { speedMax = value; }
        }

        [XmlElement("speedStep")]
        public int SpeedStep
        {
            get { return speedStep; }
            set { speedStep = value; }
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public new static ExperimentSpecification_Speed XmlParse(String xmlString)
        {
            ExperimentSpecification_Speed experimentSpecification_Speed;

            XmlSerializer serializer = new XmlSerializer(typeof(ExperimentSpecification_Speed));
            StringReader stringReader = new StringReader(xmlString);
            experimentSpecification_Speed = (ExperimentSpecification_Speed)serializer.Deserialize(stringReader);

            return experimentSpecification_Speed;
        }
    }
}
