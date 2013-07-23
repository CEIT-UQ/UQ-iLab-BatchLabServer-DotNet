using System;
using System.Xml;
using Library.Lab;

namespace Library.LabEquipment.Engine
{
    public class LabExperimentValidation
    {
        #region Constants
        private const string STR_ClassName = "LabExperimentValidation";
        private const Logfile.Level logLevel = Logfile.Level.Finer;
        /*
         * String constants for exception messages
         */
        private const string STRERR_NotSpecified_arg = "{0:s}: Not specified!";
        private const string STRERR_XmlValidation = "xmlValidation";
        #endregion

        #region Variables
        protected XmlNode xmlNodeValidation;
        #endregion

        #region Properties
        public static string ClassName
        {
            get { return STR_ClassName; }
        }
        #endregion

        //---------------------------------------------------------------------------------------//

        public LabExperimentValidation(string xmlValidation)
        {
            const string methodName = "LabExperimentValidation";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            try
            {
                /*
                 * Check that parameters are valid
                 */
                if (xmlValidation == null)
                {
                    throw new ArgumentNullException(STRERR_XmlValidation);
                }
                if (xmlValidation.Trim().Length == 0)
                {
                    throw new ArgumentException(String.Format(STRERR_NotSpecified_arg, STRERR_XmlValidation));
                }

                /*
                 * Load the experiment validation XML document from the string
                 */
                XmlDocument xmlDocument = XmlUtilities.GetDocumentFromString(xmlValidation);
                XmlNode xmlNodeRoot = XmlUtilities.GetRootNode(xmlDocument, LabConsts.STRXML_Validation);

                /*
                 * Save a copy of the validation XML node for the derived class
                 */
                this.xmlNodeValidation = xmlNodeRoot.Clone();
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw ex;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //---------------------------------------------------------------------------------------//

        public override string ToString()
        {
            return XmlUtilities.ToXmlString(this.xmlNodeValidation);
        }
    }
}
