using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Library.LabEquipment.Engine.Types;

namespace Library.LabEquipment.Types
{
    [XmlRoot("experimentSpecification")]
    public class ExperimentSpecification : LabExperimentSpecification
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public new static ExperimentSpecification XmlParse(String xmlString)
        {
            ExperimentSpecification experimentSpecification;

            XmlSerializer serializer = new XmlSerializer(typeof(ExperimentSpecification));
            StringReader stringReader = new StringReader(xmlString);
            experimentSpecification = (ExperimentSpecification)serializer.Deserialize(stringReader);

            return experimentSpecification;
        }
    }
}
