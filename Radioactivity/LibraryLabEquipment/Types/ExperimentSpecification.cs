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
        private String sourceName;
        private String csvAbsorberNames;
        private String csvDistances;
        private int duration;
        private int repeat;
        private String[] absorberNames;
        private int[] distances;

        //-------------------------------------------------------------------------------------------------//

        [XmlElement("sourceName")]
        public String SourceName
        {
            get { return sourceName; }
            set { sourceName = value; }
        }

        [XmlElement("absorberName")]
        public String CsvAbsorberNames
        {
            get { return csvAbsorberNames; }
            set { csvAbsorberNames = value; }
        }

        [XmlElement("distance")]
        public String CsvDistances
        {
            get { return csvDistances; }
            set { csvDistances = value; }
        }

        [XmlElement("duration")]
        public int Duration
        {
            get { return duration; }
            set { duration = value; }
        }

        [XmlElement("repeat")]
        public int Repeat
        {
            get { return repeat; }
            set { repeat = value; }
        }

        [XmlIgnore]
        public String[] AbsorberNames
        {
            get { return absorberNames; }
            set { absorberNames = value; }
        }

        [XmlIgnore]
        public int[] Distances
        {
            get { return distances; }
            set { distances = value; }
        }

        //-------------------------------------------------------------------------------------------------//

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

            char[] splitterCharArray = new char[] { Consts.CHRCSV_SplitterChar };
            String[] csvSplit;

            /*
             * Create the array of absorber names
             */
            csvSplit = experimentSpecification.csvAbsorberNames.Split(splitterCharArray);
            experimentSpecification.absorberNames = new String[csvSplit.Length];
            for (int i = 0; i < csvSplit.Length; i++)
            {
                experimentSpecification.absorberNames[i] = csvSplit[i].Trim();
            }

            /*
             * Recreate the CSV String of absorber names
             */
            experimentSpecification.csvAbsorberNames = String.Empty;
            for (int i = 0; i < experimentSpecification.absorberNames.Length; i++)
            {
                experimentSpecification.csvAbsorberNames += String.Format("{0:s}{1:s}",
                    (i > 0) ? Consts.CHRCSV_SplitterChar.ToString() : String.Empty, experimentSpecification.absorberNames[i]);
            }

            /*
             * Create the array of distances
             */
            csvSplit = experimentSpecification.csvDistances.Split(splitterCharArray);
            experimentSpecification.distances = new int[csvSplit.Length];
            for (int i = 0; i < csvSplit.Length; i++)
            {
                experimentSpecification.distances[i] = Int32.Parse(csvSplit[i]);
            }

            /*
             * Sort the distances with smallest distance first keeping duplicates
             */
            Array.Sort(experimentSpecification.distances);

            /*
             * Recreate the CSV String of distances which are now in sorted order
             */
            experimentSpecification.csvDistances = String.Empty;
            for (int i = 0; i < experimentSpecification.distances.Length; i++)
            {
                experimentSpecification.csvDistances += String.Format("{0:s}{1:d}",
                    (i > 0) ? Consts.CHRCSV_SplitterChar.ToString() : String.Empty, experimentSpecification.distances[i]);
            }

            return experimentSpecification;
        }
    }
}
