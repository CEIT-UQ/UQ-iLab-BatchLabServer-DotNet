using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Library.LabEquipment.Engine.Types;

namespace Library.LabEquipment.Types
{
    [XmlRoot("experimentSpecification")]
    public class ExperimentSpecification_Field : LabExperimentSpecification
    {
        private int fieldMin;
        private int fieldMax;
        private int fieldStep;

        //-------------------------------------------------------------------------------------------------//

        [XmlElement("fieldMin")]
        public int FieldMin
        {
            get { return fieldMin; }
            set { fieldMin = value; }
        }

        [XmlElement("fieldMax")]
        public int FieldMax
        {
            get { return fieldMax; }
            set { fieldMax = value; }
        }

        [XmlElement("fieldStep")]
        public int FieldStep
        {
            get { return fieldStep; }
            set { fieldStep = value; }
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public new static ExperimentSpecification_Field XmlParse(String xmlString)
        {
            ExperimentSpecification_Field experimentSpecification_Field;

            XmlSerializer serializer = new XmlSerializer(typeof(ExperimentSpecification_Field));
            StringReader stringReader = new StringReader(xmlString);
            experimentSpecification_Field = (ExperimentSpecification_Field)serializer.Deserialize(stringReader);

            return experimentSpecification_Field;
        }
    }
}
