using System;
using System.IO;
using System.Xml.Serialization;
using Library.LabEquipment.Engine.Types;

namespace Library.LabEquipment.Types
{
    [XmlRoot("experimentSpecification")]
    public class ExperimentSpecification_Load : LabExperimentSpecification
    {
        private int loadMin;
        private int loadMax;
        private int loadStep;

        //-------------------------------------------------------------------------------------------------//

        [XmlElement("loadMin")]
        public int LoadMin
        {
            get { return loadMin; }
            set { loadMin = value; }
        }

        [XmlElement("loadMax")]
        public int LoadMax
        {
            get { return loadMax; }
            set { loadMax = value; }
        }

        [XmlElement("loadStep")]
        public int LoadStep
        {
            get { return loadStep; }
            set { loadStep = value; }
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public new static ExperimentSpecification_Load XmlParse(String xmlString)
        {
            ExperimentSpecification_Load experimentSpecification_Load;

            XmlSerializer serializer = new XmlSerializer(typeof(ExperimentSpecification_Load));
            StringReader stringReader = new StringReader(xmlString);
            experimentSpecification_Load = (ExperimentSpecification_Load)serializer.Deserialize(stringReader);

            return experimentSpecification_Load;
        }
    }
}
