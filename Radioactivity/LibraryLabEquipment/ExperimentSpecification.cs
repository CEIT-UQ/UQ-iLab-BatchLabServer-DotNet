using System;
using Library.Lab;
using Library.LabEquipment.Engine;
using Library.LabEquipment.Engine.Types;

namespace Library.LabEquipment
{
    public class ExperimentSpecification : LabExperimentSpecification
    {
        #region Constants
        private const string STR_ClassName = "ExperimentSpecification";
        private const Logfile.Level logLevel = Logfile.Level.Fine;
        #endregion

        #region Properties
        private string sourceName;
        private string[] absorberNames;
        private int[] distances;
        private int duration;
        private int repeat;

        public string SourceName
        {
            get { return sourceName; }
        }

        public string[] AbsorberNames
        {
            get { return absorberNames; }
        }

        public string CsvAbsorberNames
        {
            get { return this.GetCsvString(absorberNames); }
        }

        public int[] Distances
        {
            get { return distances; }
        }

        public string CsvDistances
        {
            get { return this.GetCsvString(distances); }
        }

        public int Duration
        {
            get { return duration; }
        }

        public int Repeat
        {
            get { return repeat; }
        }
        #endregion

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlSpecification"></param>
        public ExperimentSpecification(string xmlSpecification)
            : base(xmlSpecification)
        {
            const string methodName = "ExperimentSpecification";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            /*
             * Get the experiment parameters from the specification
             */
            try
            {
                char[] splitterCharArray = new char[] { Consts.CHRCSV_SplitterChar };

                /*
                 * Get the source name
                 */
                this.sourceName = XmlUtilities.GetChildValue(this.xmlNodeSpecification, Consts.STRXML_SourceName);

                /*
                 * Get the list of absorber names
                 */
                String csvNames = XmlUtilities.GetChildValue(this.xmlNodeSpecification, Consts.STRXML_AbsorberName);
                String[] csvNamesSplit = csvNames.Split(splitterCharArray);
                this.absorberNames = new string[csvNamesSplit.Length];
                for (int i = 0; i < csvNamesSplit.Length; i++)
                {
                    this.absorberNames[i] = csvNamesSplit[i].Trim();
                }

                /*
                 * Get the list of distances
                 */
                String csvDistances = XmlUtilities.GetChildValue(this.xmlNodeSpecification, Consts.STRXML_Distance);
                String[] csvDistancesSplit = csvDistances.Split(splitterCharArray);
                this.distances = new int[csvDistancesSplit.Length];
                for (int i = 0; i < csvDistancesSplit.Length; i++)
                {
                    this.distances[i] = Int32.Parse(csvDistancesSplit[i]);
                }

                /*
                 * Sort the list of distances with smallest distance first keeping duplicates
                 */
                Array.Sort(this.distances);

                /*
                 * Get the duration
                 */
                this.duration = XmlUtilities.GetChildValueAsInt(this.xmlNodeSpecification, Consts.STRXML_Duration);

                /*
                 * Get the repeat count
                 */
                this.repeat = XmlUtilities.GetChildValueAsInt(this.xmlNodeSpecification, Consts.STRXML_Repeat);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stringArray"></param>
        /// <returns>string</returns>
        private string GetCsvString(String[] stringArray)
        {
            String csvString = String.Empty;

            for (int i = 0; i < stringArray.Length; i++)
            {
                csvString += String.Format("{0:s}{1:s}", (i > 0) ? Consts.CHRCSV_SplitterChar.ToString() : String.Empty, stringArray[i]);
            }

            return csvString;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="intArray"></param>
        /// <returns>string</returns>
        private string GetCsvString(int[] intArray)
        {
            String csvString = String.Empty;

            for (int i = 0; i < intArray.Length; i++)
            {
                csvString += String.Format("{0:s}{1:d}", (i > 0) ? Consts.CHRCSV_SplitterChar.ToString() : String.Empty, intArray[i]);
            }

            return csvString;
        }
    }
}
