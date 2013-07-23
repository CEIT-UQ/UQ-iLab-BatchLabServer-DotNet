using System;
using System.IO;
using System.Xml;
using Library.Lab;

namespace Library.LabClient.Engine
{
    public class LabExperimentResult
    {
        #region Constants
        private const String STR_ClassName = "LabExperimentResult";
        private const Logfile.Level logLevel = Logfile.Level.Finest;
        /*
         * String constants
         */
        private const String STR_Timestamp = "Timestamp";
        private const String STR_ExperimentId = "Experiment Id";
        private const String STR_UnitId = "Unit Id";
        private const String STR_setupName = "Setup Name";
        //
        private const String STRTBL_ExperimentInformation = "Experiment Information";
        private const String STRTBL_ExperimentSetup = "Experiment Setup";
        private const String STRTBL_ExperimentResults = "Experiment Results";
        private const String STRTBL_Begin = "<table id=\"results\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">";
        private const String STRTBL_Header_arg = "<tr align=\"left\"><th colspan=\"3\"><nobr>{0:s}</nobr></th></tr>";
        protected const String STRTBL_Row_arg2 = "<tr><td class=\"label\">{0:s}:</td><td class=\"dataright\">{1:s}</td></tr>";
        protected const String STRTBL_RowEmpty = "<tr><td colspan=\"3\">&nbsp;</td></tr>";
        private const String STRTBL_End = "</table>";
        //
        private const String STRCSV_ExperimentInformation = "---Experiment Information---";
        private const String STRCSV_ExperimentSetup = "---Experiment Setup---";
        private const String STRCSV_ExperimentResults = "---Experiment Results---";
        protected const String STRCSV_NewLine = "\r\n";
        protected const String STRCSV_Format_arg2 = "{0:s},{1:s}" + STRCSV_NewLine;
        #endregion

        #region Variables
        protected XmlNode nodeExperimentResult;
        protected String tblInformation;
        protected String tblSpecification;
        protected String tblResults;
        protected String csvInformation;
        protected String csvSpecification;
        protected String csvResults;
        #endregion

        #region Properties
        protected String timestamp;
        protected String title;
        protected String version;
        protected int experimentId;
        protected String sbName;
        protected int unitId;
        protected String setupId;
        protected String setupName;

        public String Timestamp
        {
            get { return timestamp; }
        }

        public String Title
        {
            get { return title; }
        }

        public String Version
        {
            get { return version; }
        }

        public int ExperimentId
        {
            get { return experimentId; }
        }

        public String SbName
        {
            get { return sbName; }
        }

        public int UnitId
        {
            get { return unitId; }
        }

        public String SetupId
        {
            get { return setupId; }
        }

        public String SetupName
        {
            get { return setupName; }
        }
        #endregion

        //-------------------------------------------------------------------------------------------------//

        public LabExperimentResult(string xmlExperimentResult)
        {
            const String methodName = "LabExperimentResult";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            try
            {
                /*
                 * Load the lab configuration XML document from the string
                 */
                XmlDocument document = XmlUtilities.GetDocumentFromString(xmlExperimentResult);
                this.nodeExperimentResult = XmlUtilities.GetRootNode(document, LabConsts.STRXML_ExperimentResult);

                /*
                 * Parse the experiment result
                 */
                this.timestamp = XmlUtilities.GetChildValue(nodeExperimentResult, LabConsts.STRXML_Timestamp);
                this.title = XmlUtilities.GetChildValue(nodeExperimentResult, LabConsts.STRXML_Title);
                this.version = XmlUtilities.GetChildValue(nodeExperimentResult, LabConsts.STRXML_Version);
                this.experimentId = XmlUtilities.GetChildValueAsInt(nodeExperimentResult, LabConsts.STRXML_ExperimentId);
                this.unitId = XmlUtilities.GetChildValueAsInt(nodeExperimentResult, LabConsts.STRXML_UnitId);
                this.setupId = XmlUtilities.GetChildValue(nodeExperimentResult, LabConsts.STRXML_SetupId);
                this.setupName = XmlUtilities.GetChildValue(nodeExperimentResult, LabConsts.STRXML_SetupName);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw ex;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        public virtual void CreateHtmlResultInfo()
        {
            /*
             * Experiment information
             */
            StringWriter sw = new StringWriter();
            sw.Write(String.Format(STRTBL_Header_arg, STRTBL_ExperimentInformation));
            sw.Write(String.Format(STRTBL_Row_arg2, STR_Timestamp, this.timestamp));
            sw.Write(String.Format(STRTBL_Row_arg2, STR_ExperimentId, this.experimentId.ToString()));
            sw.Write(String.Format(STRTBL_Row_arg2, STR_UnitId, this.unitId.ToString()));
            tblInformation = sw.ToString();

            /*
             * Experiment setup
             */
            sw = new StringWriter();
            sw.Write(String.Format(STRTBL_Header_arg, STRTBL_ExperimentSetup));
            sw.Write(String.Format(STRTBL_Row_arg2, STR_setupName, this.setupName));
            tblSpecification = sw.ToString();

            /*
             * Experiment results
             */
            sw = new StringWriter();
            sw.Write(String.Format(STRTBL_Header_arg, STRTBL_ExperimentResults));
            tblResults = sw.ToString();
        }

        //-------------------------------------------------------------------------------------------------//

        public virtual String GetHtmlResultInfo()
        {
            return STRTBL_Begin + tblInformation + tblSpecification + tblResults + STRTBL_End;
        }

        //-------------------------------------------------------------------------------------------------//

        public virtual void CreateCsvResultInfo()
        {
            /*
             * Experiment information
             */
            StringWriter sw = new StringWriter();
            sw.Write(STRCSV_NewLine);
            sw.Write(STRCSV_ExperimentInformation + STRCSV_NewLine);
            sw.Write(String.Format(STRCSV_Format_arg2, STR_Timestamp, this.timestamp));
            sw.Write(String.Format(STRCSV_Format_arg2, STR_ExperimentId, this.experimentId.ToString()));
            sw.Write(String.Format(STRCSV_Format_arg2, STR_UnitId, this.unitId.ToString()));
            csvInformation = sw.ToString();

            /*
             * Experiment setup
             */
            sw = new StringWriter();
            sw.Write(STRCSV_NewLine);
            sw.Write(STRCSV_ExperimentSetup + STRCSV_NewLine);
            sw.Write(String.Format(STRCSV_Format_arg2, STR_setupName, this.setupName));
            csvSpecification = sw.ToString();

            /*
             * Experiment results
             */
            sw = new StringWriter();
            sw.Write(STRCSV_NewLine);
            sw.Write(STRCSV_ExperimentResults + STRCSV_NewLine);
            csvResults = sw.ToString();
        }

        //-------------------------------------------------------------------------------------------------//

        public String GetCsvResultInfo()
        {
            return csvInformation + csvSpecification + csvResults;
        }
    }
}
