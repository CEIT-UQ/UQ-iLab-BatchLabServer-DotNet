using System;
using System.Xml;
using Library.Lab;

namespace Library.LabEquipment.Engine
{
    public class LabExperimentSpecification
    {
        #region Constants
        private const string STR_ClassName = "LabExperimentSpecification";
        private const Logfile.Level logLevel = Logfile.Level.Finer;
        /*
         * String constants for exception messages
         */
        private const string STRERR_NotSpecified_arg = "{0:s}: Not specified!";
        private const string STRERR_XmlSpecification = "xmlSpecification";
        //
        protected const string STRERR_InvalidSetupId_arg = "Invalid SetupId: {0:s}";
        #endregion

        #region Variables
        protected XmlNode xmlNodeSpecification;
        #endregion

        #region Properties
        public static string ClassName
        {
            get { return STR_ClassName; }
        }

        protected String setupId;

        public String SetupId
        {
            get { return setupId; }
        }
        #endregion

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlSpecification"></param>
        public LabExperimentSpecification(string xmlSpecification)
        {
            const string methodName = "LabExperimentSpecification";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            try
            {
                /*
                 * Check that parameters are valid
                 */
                if (xmlSpecification == null)
                {
                    throw new ArgumentNullException(STRERR_XmlSpecification);
                }
                if (xmlSpecification.Trim().Length == 0)
                {
                    throw new ArgumentException(String.Format(STRERR_NotSpecified_arg, STRERR_XmlSpecification));
                }

                /*
                 * Load the experiment specification XML document from the string
                 */
                XmlDocument document = XmlUtilities.GetDocumentFromString(xmlSpecification);
                XmlNode nodeRoot = XmlUtilities.GetRootNode(document, LabConsts.STRXML_ExperimentSpecification);

                /*
                 * Get the setup Id and check that it exists - search is case-sensitive
                 */
                this.setupId = XmlUtilities.GetChildValue(nodeRoot, LabConsts.STRXML_SetupId, false);

                /*
                 * Save a copy of the experiment specification XML node for the derived class
                 */
                this.xmlNodeSpecification = nodeRoot.Clone();
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw ex;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return XmlUtilities.ToXmlString(this.xmlNodeSpecification);
        }
    }
}
