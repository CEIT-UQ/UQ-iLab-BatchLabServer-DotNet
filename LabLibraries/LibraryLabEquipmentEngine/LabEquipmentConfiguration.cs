using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Library.Lab;

namespace Library.LabEquipment.Engine
{
    public class LabEquipmentConfiguration
    {
        #region Constants
        private const string STR_ClassName = "LabEquipmentConfiguration";
        private const Logfile.Level logLevel = Logfile.Level.Config;
        /*
         * String constants for logfile messages
         */
        private const string STRLOG_Filename_arg = "Filename: '{0:s}'";
        private const string STRLOG_ParsingEquipmentConfiguration = "Parsing equipment configuration...";
        private const string STRLOG_TitleVersion2_arg = "Title: '{0:s}'  Version: '{0:s}'";
        private const string STRLOG_PowerupDelay_arg = "Powerup Delay: {0:d} secs";
        private const string STRLOG_PowerdownTimeout_arg = "Powerdown Timeout: {0:d} secs";
        private const string STRLOG_PoweroffDelay_arg = "Poweroff Delay: {0:d} secs";
        private const string STRLOG_PowerdownDisabled = "Powerdown disabled";
        private const string STRLOG_DeviceName_arg = "Device: {0:s}";
        private const string STRLOG_InitialiseDelay_arg = "Initialise Delay: {0:d} secs";
        private const string STRLOG_DriverName_arg = "Driver: {0:s}";
        private const string STRLOG_SetupId_arg = "Setup Id: {0:s}";
        /*
         * String constants for exception messages
         */
        private const string STRERR_NotSpecified_arg = "{0:s}: Not specified!";
        private const string STRERR_XmlEquipmentConfigPath = "XmlEquipmentConfigPath";
        private const string STRERR_NumberIsNegative = "Number cannot be negative!";
        /*
         * Constants
         */
        /**
         * Time in seconds to wait after the equipment is powered up if not already specified.
         */
        private const int DELAY_SECS_PowerupDefault = 5;
        /**
         * Minimum time in seconds to wait to power up the equipment after it has been powered down.
         */
        private const int DELAY_SECS_PoweroffMinimum = 5;
        #endregion

        #region Variables
        private Dictionary<string, string> mapDevices;
        private Dictionary<string, string> mapDrivers;
        private Dictionary<string, string> mapSetups;
        #endregion

        #region Properties
        public static string ClassName
        {
            get { return STR_ClassName; }
        }

        private string filename;
        private string title;
        private string version;
        private int powerupDelay;
        private int powerdownTimeout;
        private int poweroffDelay;
        private bool powerdownEnabled;
        private int initialiseDelay;
        private string xmlValidation;

        public string Filename
        {
            get { return filename; }
        }

        public string Title
        {
            get { return title; }
        }

        public string Version
        {
            get { return version; }
        }

        public int PowerupDelay
        {
            get { return powerupDelay; }
        }

        public int PowerdownTimeout
        {
            get { return powerdownTimeout; }
        }

        public int PoweroffDelay
        {
            get { return poweroffDelay; }
        }

        public bool PowerdownEnabled
        {
            get { return powerdownEnabled; }
        }

        public int InitialiseDelay
        {
            get { return initialiseDelay; }
        }

        public string XmlValidation
        {
            get { return xmlValidation; }
        }
        #endregion

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Constructor - Parse the equipment configuration XML string for information specific to the LabEquipment.
        /// </summary>
        /// <param name="configProperties"></param>
        public LabEquipmentConfiguration(ConfigProperties configProperties)
        {
            const string methodName = "LabEquipmentConfiguration";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            String logMessage = Logfile.STRLOG_Newline;

            try
            {
                /*
                 * Check that parameters are valid
                 */
                if (configProperties == null)
                {
                    throw new ArgumentNullException(ConfigProperties.ClassName);
                }
                if (configProperties.XmlEquipmentConfigPath == null)
                {
                    throw new ArgumentNullException(STRERR_XmlEquipmentConfigPath);
                }
                if (configProperties.XmlEquipmentConfigPath.Trim().Length == 0)
                {
                    throw new ArgumentException(String.Format(STRERR_NotSpecified_arg, STRERR_XmlEquipmentConfigPath));
                }
                this.filename = configProperties.XmlEquipmentConfigPath;

                /*
                 * Load the equipment configuration from the specified file
                 */
                XmlDocument xmlDocument = XmlUtilities.GetDocumentFromFile(this.filename);

                /*
                 * Get the document's root node
                 */
                XmlNode xmlNodeEquipmentConfiguration = XmlUtilities.GetRootNode(xmlDocument, LabConsts.STRXML_EquipmentConfig);

                logMessage += STRLOG_ParsingEquipmentConfiguration + Logfile.STRLOG_Newline;

                /*
                 * Get information from the equipment configuration node
                 */
                this.title = XmlUtilities.GetAttributeValue(xmlNodeEquipmentConfiguration, LabConsts.STRXML_ATTR_Title, false);
                this.version = XmlUtilities.GetAttributeValue(xmlNodeEquipmentConfiguration, LabConsts.STRXML_ATTR_Version, false);
                logMessage += String.Format(STRLOG_TitleVersion2_arg, this.title, this.version) + Logfile.STRLOG_Newline;

                /*
                 * Get powerup delay, may not be specified
                 */
                try
                {
                    this.powerupDelay = XmlUtilities.GetChildValueAsInt(xmlNodeEquipmentConfiguration, LabConsts.STRXML_PowerupDelay);
                    if (this.powerupDelay < 0)
                    {
                        throw new ArithmeticException(STRERR_NumberIsNegative);
                    }
                }
                catch (Exception)
                {
                    /*
                     * Powerup delay is not specified, use the default
                     */
                    this.powerupDelay = DELAY_SECS_PowerupDefault;
                }

                /*
                 * Get powerdown timeout, may not be specified
                 */
                try
                {
                    this.powerdownTimeout = XmlUtilities.GetChildValueAsInt(xmlNodeEquipmentConfiguration, LabConsts.STRXML_PowerdownTimeout);
                    if (this.powerdownTimeout < 0)
                    {
                        throw new ArithmeticException(STRERR_NumberIsNegative);
                    }

                    /*
                     * Powerdown timeout is specified so enable powerdown
                     */
                    this.powerdownEnabled = true;
                    this.poweroffDelay = DELAY_SECS_PoweroffMinimum;
                }
                catch (Exception)
                {
                    /*
                     * Powerdown timeout is not specified, disable powerdown
                     */
                    this.powerdownEnabled = false;
                    this.poweroffDelay = 0;
                }

                /*
                 * Log details
                 */
                logMessage += String.Format(STRLOG_PowerupDelay_arg, this.powerupDelay) + Logfile.STRLOG_Newline;

                if (this.powerdownEnabled == true)
                {
                    logMessage += String.Format(STRLOG_PowerdownTimeout_arg, this.powerdownTimeout) + Logfile.STRLOG_Newline;
                    logMessage += String.Format(STRLOG_PoweroffDelay_arg, this.poweroffDelay) + Logfile.STRLOG_Newline;
                }
                else
                {
                    logMessage += STRLOG_PowerdownDisabled + Logfile.STRLOG_Newline;
                }

                /*
                 * Get the device nodes and accumulate the initialise delay
                 */
                this.initialiseDelay = 0;
                this.mapDevices = new Dictionary<string, string>();
                XmlNode xmlNodeDevices = XmlUtilities.GetChildNode(xmlNodeEquipmentConfiguration, LabConsts.STRXML_Devices);
                ArrayList xmlNodeList = XmlUtilities.GetChildNodeList(xmlNodeDevices, LabConsts.STRXML_Device, false);
                for (int i = 0; i < xmlNodeList.Count; i++)
                {
                    XmlNode xmlNodeDevice = (XmlNode)xmlNodeList[i];

                    /*
                     * Check that the required device information exists
                     */
                    String name = XmlUtilities.GetAttributeValue(xmlNodeDevice, LabConsts.STRXML_ATTR_Name, false);
                    logMessage += String.Format(STRLOG_DeviceName_arg, name) + Logfile.STRLOG_Spacer;

                    /*
                     * Get the initialise delay and add to total
                     */
                    XmlNode xmlNodeInitialise = XmlUtilities.GetChildNode(xmlNodeDevice, LabConsts.STRXML_Initialise);
                    bool initialiseEnabled = XmlUtilities.GetChildValueAsBool(xmlNodeInitialise, LabConsts.STRXML_InitialiseEnabled);
                    if (initialiseEnabled == true)
                    {
                        int initialiseDelayDevice = XmlUtilities.GetChildValueAsInt(xmlNodeInitialise, LabConsts.STRXML_InitialiseDelay);
                        this.initialiseDelay += initialiseDelayDevice;
                        logMessage += String.Format(STRLOG_InitialiseDelay_arg, initialiseDelayDevice) + Logfile.STRLOG_Newline;
                    }

                    /*
                     * Add device XML to map
                     */
                    String xmlDevice = XmlUtilities.ToXmlString(xmlNodeDevice);
                    this.mapDevices.Add(name, xmlDevice);
                }

                /*
                 * Get the driver nodes
                 */
                this.mapDrivers = new Dictionary<string, string>();
                XmlNode xmlNodeDrivers = XmlUtilities.GetChildNode(xmlNodeEquipmentConfiguration, LabConsts.STRXML_Drivers);
                xmlNodeList = XmlUtilities.GetChildNodeList(xmlNodeDrivers, LabConsts.STRXML_Driver, false);
                for (int i = 0; i < xmlNodeList.Count; i++)
                {
                    XmlNode xmlNodeDriver = (XmlNode)xmlNodeList[i];

                    /*
                     * Check that the required driver information exists
                     */
                    String name = XmlUtilities.GetAttributeValue(xmlNodeDriver, LabConsts.STRXML_ATTR_Name, false);
                    logMessage += String.Format(STRLOG_DriverName_arg, name) + Logfile.STRLOG_Newline;

                    /*
                     * Add driver XML to map
                     */
                    String xmlDriver = XmlUtilities.ToXmlString(xmlNodeDriver);
                    this.mapDrivers.Add(name, xmlDriver);
                }

                /*
                 * Get the setup nodes
                 */
                this.mapSetups = new Dictionary<string, string>();
                XmlNode xmlNodeSetups = XmlUtilities.GetChildNode(xmlNodeEquipmentConfiguration, LabConsts.STRXML_Setups);
                xmlNodeList = XmlUtilities.GetChildNodeList(xmlNodeSetups, LabConsts.STRXML_Setup, false);
                for (int i = 0; i < xmlNodeList.Count; i++)
                {
                    XmlNode xmlNodeSetup = (XmlNode)xmlNodeList[i];

                    /*
                     * Get the setup id
                     */
                    String id = XmlUtilities.GetAttributeValue(xmlNodeSetup, LabConsts.STRXML_ATTR_Id, false);
                    logMessage += String.Format(STRLOG_SetupId_arg, id) + Logfile.STRLOG_Newline;

                    /*
                     * Get the driver and add to map
                     */
                    String strDriver = XmlUtilities.GetChildValue(xmlNodeSetup, LabConsts.STRXML_Driver);
                    this.mapSetups.Add(id, strDriver);
                }

                /*
                 * Get the validation node and save as an XML string
                 */
                XmlNode xmlNodeValidation = XmlUtilities.GetChildNode(xmlNodeEquipmentConfiguration, LabConsts.STRXML_Validation);
                this.xmlValidation = XmlUtilities.ToXmlString(xmlNodeValidation);

                Logfile.Write(logLevel, logMessage);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="setupId"></param>
        /// <returns>string</returns>
        public string GetDriverName(string setupId)
        {
            string retval;

            if (this.mapSetups.TryGetValue(setupId, out retval) == false)
            {
                retval = null;
            }

            return retval;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceName"></param>
        /// <returns>string</returns>
        public string GetXmlDeviceConfiguration(string deviceName)
        {
            string retval;

            if (this.mapDevices.TryGetValue(deviceName, out retval) == false)
            {
                retval = null;
            }

            return retval;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="driverName"></param>
        /// <returns>string</returns>
        public string GetXmlDriverConfiguration(string driverName)
        {
            string retval;

            if (this.mapDrivers.TryGetValue(driverName, out retval) == false)
            {
                retval = null;
            }

            return retval;
        }
    }
}
