using System;
using System.Xml;
using Library.Lab;

namespace Library.LabClient.Engine
{
    public class LabExperimentSpecification
    {
        #region Constants
        private const String STR_ClassName = "LabExperimentSpecification";
        private const Logfile.Level logLevel = Logfile.Level.Finest;
        /*
         * String constants for exception messages
         */
        private const String STRERR_XmlSpecification = "xmlSpecification";
        #endregion

        #region Variables
        protected XmlNode nodeSpecification;
        #endregion

        #region Properties
        protected String setupName;
        protected String setupId;

        public String SetupName
        {
            get { return setupName; }
            set { setupName = value; }
        }

        public String SetupId
        {
            get { return setupId; }
            set { setupId = value; }
        }
        #endregion

        //-------------------------------------------------------------------------------------------------//

        public LabExperimentSpecification(String xmlSpecification)
        {
            const String methodName = "LabExperimentSpecification";
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
                    throw new ArgumentException(STRERR_XmlSpecification);
                }

                /*
                 * Load the experiment specification XML document from the string
                 */
                XmlDocument document = XmlUtilities.GetDocumentFromString(xmlSpecification);
                XmlNode nodeRoot = XmlUtilities.GetRootNode(document, LabConsts.STRXML_ExperimentSpecification);

                /*
                 * Check that all required XML nodes exist
                 */
                XmlUtilities.GetChildNode(nodeRoot, LabConsts.STRXML_SetupName);
                XmlUtilities.GetChildNode(nodeRoot, LabConsts.STRXML_SetupId);

                /*
                 * Save a copy of the experiment specification for the derived class
                 */
                this.nodeSpecification = nodeRoot.CloneNode(true);

            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
                throw ex;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        public virtual String ToXmlString()
        {
            const String methodName = "ToXmlString";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            String xmlString;

            try
            {
                /*
                 * Add the lab experiment specification information to the XML document
                 */
                XmlUtilities.SetChildValue(this.nodeSpecification, LabConsts.STRXML_SetupName, this.setupName);
                XmlUtilities.SetChildValue(this.nodeSpecification, LabConsts.STRXML_SetupId, this.setupId);

                /*
                 * Convert the XML document to an XML string
                 */
                xmlString = XmlUtilities.ToXmlString(this.nodeSpecification);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
                xmlString = null;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);

            return xmlString;
        }
    }
}
