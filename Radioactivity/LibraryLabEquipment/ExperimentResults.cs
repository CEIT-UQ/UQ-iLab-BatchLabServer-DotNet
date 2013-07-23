using System;
using System.Xml;
using Library.Lab;
using Library.LabEquipment.Types;

namespace Library.LabEquipment
{
    public class ExperimentResults
    {
        #region Constants
        private const String STR_ClassName = "ExperimentResults";
        private const Logfile.Level logLevel = Logfile.Level.Finer;
        #endregion

        #region Variables
        private ExperimentSpecification experimentSpecification;
        private String xmlTemplate;
        #endregion

        #region Properties
        private String dataType;
        private int[][] dataVectors;

        public String DataType
        {
            get { return dataType; }
            set { dataType = value; }
        }

        public int[][] DataVectors
        {
            get { return dataVectors; }
            set { dataVectors = value; }
        }
        #endregion

        //-------------------------------------------------------------------------------------------------//

        public ExperimentResults(ExperimentSpecification experimentSpecification, String xmlTemplate)
        {
            this.experimentSpecification = experimentSpecification;
            this.xmlTemplate = xmlTemplate;
        }

        //-------------------------------------------------------------------------------------------------//

        public String ToXmlString()
        {
            const String methodName = "ToXmlString";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            String xmlExperimentResults = null;

            try
            {
                /*
                 * Load the experiment results template XML document from the String
                 */
                XmlDocument xmlDocument = XmlUtilities.GetDocumentFromString(this.xmlTemplate);
                XmlNode xmlNodeRoot = XmlUtilities.GetRootNode(xmlDocument, Consts.STRXML_ExperimentResults);

                /*
                 * Add the experiment specification information to the XML document
                 */
                XmlUtilities.SetChildValue(xmlNodeRoot, Consts.STRXML_SourceName, this.experimentSpecification.SourceName);
                XmlUtilities.SetChildValue(xmlNodeRoot, Consts.STRXML_AbsorberName, this.experimentSpecification.CsvAbsorberNames);
                XmlUtilities.SetChildValue(xmlNodeRoot, Consts.STRXML_Distance, this.experimentSpecification.CsvDistances);
                XmlUtilities.SetChildValue(xmlNodeRoot, Consts.STRXML_Duration, this.experimentSpecification.Duration);
                XmlUtilities.SetChildValue(xmlNodeRoot, Consts.STRXML_Repeat, this.experimentSpecification.Repeat);

                /*
                 * Add the experiment result information to the XML document
                 */
                String dataType = XmlUtilities.GetChildValue(xmlNodeRoot, Consts.STRXML_DataType);
                XmlUtilities.SetChildValue(xmlNodeRoot, Consts.STRXML_DataType, dataType);

                /*
                 * Get the XML data vector node and clone it
                 */
                XmlNode xmlNodeDataVector = XmlUtilities.GetChildNode(xmlNodeRoot, Consts.STRXML_DataVector);
                XmlNode xmlNodeClone = xmlNodeDataVector.CloneNode(true);

                switch (this.experimentSpecification.SetupId)
                {
                    case Consts.STRXML_SetupId_RadioactivityVsTime:
                    case Consts.STRXML_SetupId_SimActivityVsTime:
                    case Consts.STRXML_SetupId_SimActivityVsTimeNoDelay:

                        /*
                         * Only one distance to process
                         */
                        int distance = this.experimentSpecification.Distances[0];

                        /*
                         * Create a CSV String of radioactivity counts from the data vector
                         */
                        XmlUtilities.SetValue(xmlNodeDataVector, this.GetCsvString(this.dataVectors[0]));
                        XmlUtilities.SetAttributeValue(xmlNodeDataVector, Consts.STRXML_ATTR_Distance, distance.ToString());
                        break;

                    case Consts.STRXML_SetupId_RadioactivityVsDistance:
                    case Consts.STRXML_SetupId_SimActivityVsDistance:
                    case Consts.STRXML_SetupId_SimActivityVsDistanceNoDelay:

                        /*
                         * Process all distances
                         */
                        int[] distances = this.experimentSpecification.Distances;
                        for (int i = 0; i < distances.Length; i++)
                        {
                            /*
                             * Create a CSV String of radioactivity counts from the data vector
                             */
                            XmlUtilities.SetValue(xmlNodeDataVector, this.GetCsvString(this.dataVectors[i]));
                            XmlUtilities.SetAttributeValue(xmlNodeDataVector, Consts.STRXML_ATTR_Distance, distances[i].ToString());

                            /*
                             * Add a data vector if there are more distances to process
                             */
                            if (i < distances.Length - 1)
                            {
                                xmlNodeDataVector = xmlNodeClone.CloneNode(true);
                                xmlNodeRoot.AppendChild(xmlNodeDataVector);
                            }
                        }
                        break;

                    case Consts.STRXML_SetupId_RadioactivityVsAbsorber:
                    case Consts.STRXML_SetupId_SimActivityVsAbsorber:
                    case Consts.STRXML_SetupId_SimActivityVsAbsorberNoDelay:

                        /*
                         * Process all absorbers
                         */
                        String[] absorberNames = this.experimentSpecification.AbsorberNames;
                        for (int i = 0; i < absorberNames.Length; i++)
                        {
                            /*
                             * Create a CSV String of radioactivity counts from the data vector
                             */
                            XmlUtilities.SetValue(xmlNodeDataVector, this.GetCsvString(this.dataVectors[i]));
                            XmlUtilities.SetAttributeValue(xmlNodeDataVector, Consts.STRXML_ATTR_AbsorberName, absorberNames[i]);

                            /*
                             * Add a data vector if there are more absorbers to process
                             */
                            if (i < absorberNames.Length - 1)
                            {
                                xmlNodeDataVector = xmlNodeClone.CloneNode(true);
                                xmlNodeRoot.AppendChild(xmlNodeDataVector);
                            }
                        }
                        break;

                    default:
                        break;
                }

                /*
                 * Save the experiment results information to an XML String
                 */
                xmlExperimentResults = XmlUtilities.ToXmlString(xmlDocument);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return xmlExperimentResults;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="intArray"></param>
        /// <returns>String</returns>
        private String GetCsvString(int[] intArray)
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
