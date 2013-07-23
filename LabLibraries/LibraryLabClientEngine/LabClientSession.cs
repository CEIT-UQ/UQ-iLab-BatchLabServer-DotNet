using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Library.Lab;
using Library.LabClient.Engine.Types;
using Library.ServiceBroker;

namespace Library.LabClient.Engine
{
    public class LabClientSession
    {
        #region Constants
        private const String STR_ClassName = "LabClientSession";
        private const Logfile.Level logLevel = Logfile.Level.Fine;
        /*
         * String constants
         */
        private const String STR_MinutesAnd_arg2 = "{0:d} minute{1:s} and ";
        private const String STR_Seconds_arg2 = "{0:d} second{1:s}";
        /*
         * String constants for logfile messages
         */
        private const String STRLOG_TitleVersion_arg2 = "Title: {0:s}  Version: {1:s}";
        private const String STRLOG_PhotoUrl_arg = "Photo Url: {0:s}";
        private const String STRLOG_LabCameraUrl_arg = "LabCamera Url: {0:s}";
        private const String STRLOG_LabInfoUrl_arg = "LabInfo Url: {0:s}";
        private const String STRLOG_SetupIDName_arg = "Setup Id: {0:s} Name: {1:s}";
        #endregion

        #region Properties
        private String labCameraUrl;
        private String labInfoText;
        private String labInfoUrl;
        private String navmenuPhotoUrl;
        private String title;
        private String version;
        private String xmlConfiguration;
        private String xmlSpecification;
        private String xmlValidation;
        private Dictionary<String, SetupInfo> setupInfoMap;
        private String[] setupNames;
        private int[] submittedIds;
        private int[] completedIds;
        private String feedbackEmailUrl;
        private bool multiSubmit;
        private ServiceBrokerAPI serviceBrokerAPI;

        public String LabCameraUrl
        {
            get { return labCameraUrl; }
        }

        public String LabInfoText
        {
            get { return labInfoText; }
        }

        public String LabInfoUrl
        {
            get { return labInfoUrl; }
        }

        public String NavmenuPhotoUrl
        {
            get { return navmenuPhotoUrl; }
        }

        public String Title
        {
            get { return title; }
        }

        public String Version
        {
            get { return version; }
        }

        public String XmlConfiguration
        {
            get { return xmlConfiguration; }
        }

        public String XmlSpecification
        {
            get { return xmlSpecification; }
        }

        public String XmlValidation
        {
            get { return xmlValidation; }
        }

        public Dictionary<String, SetupInfo> SetupInfoMap
        {
            get { return setupInfoMap; }
        }

        public String[] SetupNames
        {
            get { return setupNames; }
        }

        public int[] SubmittedIds
        {
            get { return submittedIds; }
        }

        public int[] CompletedIds
        {
            get { return completedIds; }
        }

        public String FeedbackEmailUrl
        {
            get { return feedbackEmailUrl; }
            set { feedbackEmailUrl = value; }
        }

        public bool MultiSubmit
        {
            get { return multiSubmit; }
            set { multiSubmit = value; }
        }

        public ServiceBrokerAPI ServiceBrokerAPI
        {
            get { return serviceBrokerAPI; }
            set { serviceBrokerAPI = value; }
        }
        #endregion

        //-------------------------------------------------------------------------------------------------//

        public void ParseLabConfiguration(String xmlLabConfiguration)
        {
            const String methodName = "ParseLabConfiguration";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            try
            {
                String logMessage = Logfile.STRLOG_Newline;

                /*
                 * Load the lab configuration XML document from the string
                 */
                XmlDocument document = XmlUtilities.GetDocumentFromString(xmlLabConfiguration);
                XmlNode nodeLabConfiguration = XmlUtilities.GetRootNode(document, LabConsts.STRXML_LabConfiguration);

                /*
                 * Get information from the lab configuration node
                 */
                this.title = XmlUtilities.GetAttributeValue(nodeLabConfiguration, LabConsts.STRXML_ATTR_Title);
                this.version = XmlUtilities.GetAttributeValue(nodeLabConfiguration, LabConsts.STRXML_ATTR_Version);
                logMessage += String.Format(STRLOG_TitleVersion_arg2, this.title, this.version) + Logfile.STRLOG_Newline;

                XmlNode nodeNavmenuPhoto = XmlUtilities.GetChildNode(nodeLabConfiguration, LabConsts.STRXML_NavmenuPhoto);
                this.navmenuPhotoUrl = XmlUtilities.GetChildValue(nodeNavmenuPhoto, LabConsts.STRXML_Image);
                logMessage += String.Format(STRLOG_PhotoUrl_arg, this.navmenuPhotoUrl) + Logfile.STRLOG_Newline;

                XmlNode nodeLabCamera = XmlUtilities.GetChildNode(nodeLabConfiguration, LabConsts.STRXML_LabCamera, false);
                if (nodeLabCamera != null)
                {
                    String url = XmlUtilities.GetChildValue(nodeLabCamera, LabConsts.STRXML_Url, false);
                    if (url != null && url.Length > 0)
                    {
                        this.labCameraUrl = url;
                    }
                }
                logMessage += String.Format(STRLOG_LabCameraUrl_arg, this.labCameraUrl) + Logfile.STRLOG_Newline;

                XmlNode nodeLabInfo = XmlUtilities.GetChildNode(nodeLabConfiguration, LabConsts.STRXML_LabInfo, false);
                if (nodeLabInfo != null)
                {
                    this.labInfoText = XmlUtilities.GetChildValue(nodeLabInfo, LabConsts.STRXML_Text, false);
                    String url = XmlUtilities.GetChildValue(nodeLabInfo, LabConsts.STRXML_Url, false);
                    if (url != null && url.Length > 0)
                    {
                        this.labInfoUrl = url;
                    }
                }
                logMessage += String.Format(STRLOG_LabInfoUrl_arg, this.labInfoUrl) + Logfile.STRLOG_Newline;

                /*
                 * Get the configuration node and save as an XML string
                 */
                XmlNode nodeConfiguration = XmlUtilities.GetChildNode(nodeLabConfiguration, LabConsts.STRXML_Configuration);
                XmlDocumentFragment documentFragment = document.CreateDocumentFragment();
                documentFragment.AppendChild(nodeConfiguration.CloneNode(true));
                this.xmlConfiguration = XmlUtilities.ToXmlString(documentFragment);

                /*
                 * Get a list of all setups, must have at least one. Also, keep a seperate list of setup names so that they
                 * appear in the same order as in the configuration
                 */
                ArrayList nodeList = XmlUtilities.GetChildNodeList(nodeConfiguration, LabConsts.STRXML_Setup);
                this.setupInfoMap = new Dictionary<String, SetupInfo>();
                this.setupNames = new String[nodeList.Count];
                for (int i = 0; i < nodeList.Count; i++)
                {
                    XmlNode nodeSetup = (XmlNode)nodeList[i];

                    /*
                     * Get the setup Id and name
                     */
                    String setupId = XmlUtilities.GetAttributeValue(nodeSetup, LabConsts.STRXML_ATTR_Id);
                    String setupName = XmlUtilities.GetChildValue(nodeSetup, LabConsts.STRXML_Name);

                    /*
                     * Get the setup description
                     */
                    XmlNode nodeDescription = XmlUtilities.GetChildNode(nodeSetup, LabConsts.STRXML_Description);
                    documentFragment = document.CreateDocumentFragment();
                    documentFragment.AppendChild(nodeDescription.CloneNode(true));
                    String setupDescription = XmlUtilities.ToXmlString(documentFragment);

                    /*
                     * Add setup information to the list
                     */
                    SetupInfo setupInfo = new SetupInfo(setupId);
                    setupInfo.Name = setupName;
                    setupInfo.Description = setupDescription;
                    setupInfo.XmlSetup = XmlUtilities.ToXmlString(nodeSetup);
                    this.setupInfoMap.Add(setupName, setupInfo);
                    this.setupNames[i] = setupName;
                    logMessage += String.Format(STRLOG_SetupIDName_arg, setupId, setupName) + Logfile.STRLOG_Newline;
                }

                Logfile.Write(logLevel, logMessage);

                /*
                 * Get the experiment specification node and save as an XML string
                 */
                XmlNode nodeSpecification = XmlUtilities.GetChildNode(nodeLabConfiguration, LabConsts.STRXML_ExperimentSpecification);
                documentFragment = document.CreateDocumentFragment();
                documentFragment.AppendChild(nodeSpecification.CloneNode(true));
                this.xmlSpecification = XmlUtilities.ToXmlString(documentFragment);

                /*
                 * Get the validation node, if it exists, and save as an XML string
                 */
                XmlNode nodeValidation = XmlUtilities.GetChildNode(nodeLabConfiguration, LabConsts.STRXML_Validation, false);
                if (nodeValidation != null)
                {
                    documentFragment = document.CreateDocumentFragment();
                    documentFragment.AppendChild(nodeValidation.CloneNode(true));
                    this.xmlValidation = XmlUtilities.ToXmlString(documentFragment);
                }

            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        public void AddSubmittedId(int id)
        {
            /*
             * Check if multisubmit is enabled or submitted ids exists
             */
            if (multiSubmit == false || submittedIds == null)
            {
                /*
                 * Create a new array of submitted ids and add the id
                 */
                submittedIds = new int[] { id };
            }
            else
            {
                /*
                 * Create a bigger array of submitted ids and add the id
                 */
                int[] newSubmittedIds = new int[submittedIds.Length + 1];
                Array.Copy(submittedIds, newSubmittedIds, submittedIds.Length);
                newSubmittedIds[submittedIds.Length] = id;
                submittedIds = newSubmittedIds;
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public void DeleteSubmittedId(int id)
        {
            /*
             * Check if submitted id exists
             */
            if (submittedIds != null)
            {
                /*
                 * Check if multisubmit is enabled or one submitted id exists
                 */
                if (multiSubmit == false || (submittedIds.Length == 1 && submittedIds[0] == id))
                {
                    submittedIds = null;
                }
                else
                {
                    /*
                     * Find submitted id
                     */
                    for (int i = 0; i < submittedIds.Length; i++)
                    {
                        if (submittedIds[i] == id)
                        {
                            /*
                             * Create a smaller array of completed iIds
                             */
                            int[] newSubmittedIds = new int[submittedIds.Length - 1];

                            /*
                             * Copy the ids to the new array excluding the id
                             */
                            Array.Copy(submittedIds, 0, newSubmittedIds, 0, i);
                            Array.Copy(submittedIds, i + 1, newSubmittedIds, i, newSubmittedIds.Length - i);
                            submittedIds = newSubmittedIds;
                            break;
                        }
                    }
                }
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public void AddCompletedId(int id)
        {
            /*
             * Check if multisubmit is enabled or completed ids exists
             */
            if (multiSubmit == false || completedIds == null)
            {
                /*
                 * Create a new array of completed ids and add the id
                 */
                completedIds = new int[] { id };
            }
            else
            {
                /*
                 * Create a bigger array of completed ids and add the id
                 */
                int[] newCompletedIds = new int[completedIds.Length + 1];
                Array.Copy(completedIds, newCompletedIds, completedIds.Length);
                newCompletedIds[completedIds.Length] = id;
                completedIds = newCompletedIds;
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public String[] GetSubmittedIds()
        {
            String[] ids = null;

            if (submittedIds != null)
            {
                ids = new String[submittedIds.Length];
                for (int i = 0; i < submittedIds.Length; i++)
                {
                    ids[i] = submittedIds[i].ToString();
                }
            }

            return ids;
        }

        //-------------------------------------------------------------------------------------------------//

        public String FormatTimeMessage(int seconds)
        {
            /*
             * Convert to minutes and seconds
             */
            int minutes = seconds / 60;
            seconds -= minutes * 60;

            String message = String.Empty;
            try
            {
                if (minutes > 0)
                {
                    /*
                     * Display minutes
                     */
                    message += String.Format(STR_MinutesAnd_arg2, minutes, FormatPlural(minutes));
                }
                /*
                 * Display seconds
                 */
                message += String.Format(STR_Seconds_arg2, seconds, FormatPlural(seconds));
            }
            catch (Exception ex)
            {
                message = ex.Message;
                Logfile.WriteError(ex.ToString());
            }

            return message;
        }

        //-------------------------------------------------------------------------------------------------//

        private String FormatPlural(int value)
        {
            return (value == 1) ? String.Empty : "s";
        }
    }
}
