using System;
using System.IO;
using Library.Lab;
using Library.LabClient.Engine;

namespace Library.LabClient
{
    public class ExperimentResult : LabExperimentResult
    {
        #region Constants
        private const String STR_ClassName = "LabExperimentResult";
        private const Logfile.Level logLevel = Logfile.Level.Finest;
        /*
         * String constants
         */
        private const String STR_Source = "Source";
        private const String STR_Absorber = "Absorber";
        private const String STR_AbsorberList = "Absorber List";
        private const String STR_Distance = "Distance (mm)";
        private const String STR_DistanceList = "Distance List (mm)";
        private const String STR_Duration = "Duration (secs)";
        private const String STR_Trials = "Trials";
        private const String STR_DataType = "Data Type";
        private const String STR_CountsAtDistance = "Counts at";
        private const String STR_CountsForAbsorber = "Counts for Absorber";
        private const String STR_Millimetres = "mm";
        #endregion

        #region Variables
        private String source;
        private String[] absorberList;
        private int[] distanceList;
        private int duration;
        private int trials;
        private String dataType;
        private int[][] dataVectors;
        #endregion

        //-------------------------------------------------------------------------------------------------//

        public ExperimentResult(String xmlExperimentResult)
            : base(xmlExperimentResult)
        {
            const String methodName = "ExperimentResult";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            try
            {
                /*
                 * Get specification information
                 */
                this.source = XmlUtilities.GetChildValue(this.nodeExperimentResult, Consts.STRXML_SourceName);
                this.duration = XmlUtilities.GetChildValueAsInt(this.nodeExperimentResult, Consts.STRXML_Duration);
                this.trials = XmlUtilities.GetChildValueAsInt(this.nodeExperimentResult, Consts.STRXML_Repeat);

                /*
                 * Get the CSV list of absorbers into a string array
                 */
                char[] splitterCharArray = new char[] { Consts.CHRCSV_SplitterChar };
                String csvString = XmlUtilities.GetChildValue(this.nodeExperimentResult, Consts.STRXML_AbsorberName);
                this.absorberList = csvString.Split(splitterCharArray);

                /*
                 * Get the CSV list of distances into an integer array
                 */
                csvString = XmlUtilities.GetChildValue(this.nodeExperimentResult, Consts.STRXML_Distance);
                String[] csvStringSplit = csvString.Split(splitterCharArray);
                this.distanceList = new int[csvStringSplit.Length];
                for (int i = 0; i < this.distanceList.Length; i++)
                {
                    try
                    {
                        this.distanceList[i] = Int32.Parse(csvStringSplit[i]);
                    }
                    catch (Exception)
                    {
                    }
                }

                /*
                 * Get result information
                 */
                this.dataType = XmlUtilities.GetChildValue(this.nodeExperimentResult, Consts.STRXML_DataType);

                /*
                 * Get the radioactivity counts into a two dimensional array. Each data vector contains the trial counts for
                 * a particular distance and is provided as a comma-seperated-value string.
                 */
                String[] csvStrings = XmlUtilities.GetChildValues(this.nodeExperimentResult, Consts.STRXML_DataVector, false);
                this.dataVectors = new int[csvStrings.Length][];
                for (int i = 0; i < this.dataVectors.Length; i++)
                {
                    csvStringSplit = csvStrings[i].Split(splitterCharArray);
                    this.dataVectors[i] = new int[csvStringSplit.Length];
                    for (int j = 0; j < this.dataVectors[i].Length; j++)
                    {
                        try
                        {
                            this.dataVectors[i][j] = Int32.Parse(csvStringSplit[j]);
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
                throw ex;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        public override void CreateHtmlResultInfo()
        {
            base.CreateHtmlResultInfo();

            this.tblSpecification += this.CreateSpecificationInfo(STRTBL_Row_arg2);
            this.tblResults += this.CreateResultsInfo(STRTBL_Row_arg2);
        }

        //-------------------------------------------------------------------------------------------------//

        public override void CreateCsvResultInfo()
        {
            base.CreateCsvResultInfo();

            this.csvSpecification += this.CreateSpecificationInfo(STRCSV_Format_arg2);
            this.csvResults += this.CreateResultsInfo(STRCSV_Format_arg2);
        }

        //-------------------------------------------------------------------------------------------------//

        private String CreateSpecificationInfo(String strFormat)
        {
            /*
             * Experiment setup
             */
            StringWriter sw = new StringWriter();

            /*
             * Create a CSV string of absorbers
             */
            String csvAbsorbers = "";
            for (int i = 0; i < this.absorberList.Length; i++)
            {
                csvAbsorbers += String.Format("{0:s}{1:s}", (i > 0) ? Consts.STR_CsvSplitter : "", this.absorberList[i]);
            }

            /*
             * Create a CSV string of distances
             */
            String csvDistances = "";
            for (int i = 0; i < this.distanceList.Length; i++)
            {
                csvDistances += String.Format("{0:s}{1:d}", (i > 0) ? Consts.STR_CsvSplitter : "", this.distanceList[i]);
            }

            sw.Write(String.Format(strFormat, STR_Source, this.source));
            sw.Write(String.Format(strFormat, (this.absorberList.Length > 1) ? STR_AbsorberList : STR_Absorber, csvAbsorbers));
            sw.Write(String.Format(strFormat, (this.distanceList.Length > 1) ? STR_DistanceList : STR_Distance, csvDistances));
            sw.Write(String.Format(strFormat, STR_Duration, this.duration.ToString()));
            sw.Write(String.Format(strFormat, STR_Trials, this.trials.ToString()));

            return sw.ToString();
        }

        //-------------------------------------------------------------------------------------------------//

        private String CreateResultsInfo(String strFormat)
        {
            /*
             * Experiment results
             */
            StringWriter sw = new StringWriter();
            sw.Write(String.Format(strFormat, STR_DataType, this.dataType));
            switch (this.setupId)
            {
                case Consts.STRXML_SetupId_RadioactivityVsAbsorber:
                case Consts.STRXML_SetupId_SimActivityVsAbsorber:
                case Consts.STRXML_SetupId_SimActivityVsAbsorberNoDelay:
                    sw.Write(String.Format(strFormat, STR_CountsForAbsorber, ""));
                    break;

                default:
                    sw.Write(String.Format(strFormat, STR_CountsAtDistance, ""));
                    break;
            }

            /*
             * Create a CSV string of radioactivity counts from the data vector
             */
            for (int i = 0; i < this.dataVectors.Length; i++)
            {
                String csvString = "";
                for (int j = 0; j < this.dataVectors[i].Length; j++)
                {
                    csvString += String.Format("{0:s}{1:d}", (j > 0) ? Consts.STR_CsvSplitter : "", this.dataVectors[i][j]);
                }
                switch (this.setupId)
                {
                    case Consts.STRXML_SetupId_RadioactivityVsAbsorber:
                    case Consts.STRXML_SetupId_SimActivityVsAbsorber:
                    case Consts.STRXML_SetupId_SimActivityVsAbsorberNoDelay:
                        sw.Write(String.Format(strFormat, this.absorberList[i], csvString));
                        break;
                    default:
                        sw.Write(String.Format(strFormat, String.Format("{0:d}{1:s}", this.distanceList[i], STR_Millimetres), csvString));
                        break;
                }
            }

            return sw.ToString();
        }
    }
}
