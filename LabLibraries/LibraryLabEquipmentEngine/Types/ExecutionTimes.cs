using System;
using System.IO;
using System.Xml.Serialization;

namespace Library.LabEquipment.Engine.Types
{
    [XmlRoot("executionTimes")]
    public class ExecutionTimes
    {
        private int initialise;
        private int start;
        private int run;
        private int stop;
        private int finalise;

        //-------------------------------------------------------------------------------------------------//

        [XmlElement("initialise")]
        public int Initialise
        {
            get { return initialise; }
            set { initialise = value; }
        }

        [XmlElement("start")]
        public int Start
        {
            get { return start; }
            set { start = value; }
        }

        [XmlElement("run")]
        public int Run
        {
            get { return run; }
            set { run = value; }
        }

        [XmlElement("stop")]
        public int Stop
        {
            get { return stop; }
            set { stop = value; }
        }

        [XmlElement("finalise")]
        public int Finalise
        {
            get { return finalise; }
            set { finalise = value; }
        }

        [XmlIgnore]
        public int TotalExecutionTime
        {
            get { return (initialise + start + run + stop + finalise); }
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public static ExecutionTimes ToObject(String xmlString)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ExecutionTimes));
            StringReader stringReader = new StringReader(xmlString);
            return (ExecutionTimes)serializer.Deserialize(stringReader);
        }
    }
}
