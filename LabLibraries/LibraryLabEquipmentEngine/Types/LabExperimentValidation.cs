using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Library.LabEquipment.Engine.Types
{
    [XmlRoot("validation")]
    public class LabExperimentValidation
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public String ToXmlString()
        {
            XmlSerializer serializer = new XmlSerializer(this.GetType());

            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.OmitXmlDeclaration = true;
            xmlWriterSettings.Indent = true;

            StringBuilder stringBuilder = new StringBuilder();
            XmlWriter xmlWriter = XmlWriter.Create(stringBuilder, xmlWriterSettings);

            serializer.Serialize(xmlWriter, this);

            return stringBuilder.ToString();
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public static LabExperimentValidation XmlParse(String xmlString)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(LabExperimentValidation));
            StringReader stringReader = new StringReader(xmlString);
            return (LabExperimentValidation)serializer.Deserialize(stringReader);
        }
    }
}
