using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Xml;

namespace Library.Lab.Utilities
{
    public class AppSetting
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "Utilities";

        //
        // String constants for the specification XML string
        //
        private const string STR_Section_appSettings = "appSettings";
        private const string STRXMLROOT_appSettings = "/appSettings";
        private const string STRXMLROOT_appSettings_add = "/appSettings/add";
        private const string STRXMLPARAM_key = "@key";
        private const string STRXMLPARAM_value = "@value";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_AppSettingsNotSpecified = "Configuration file <appSettings> section is not specified!";
        private const string STRLOG_AppSettingsInvalid = "Configuration file <appSettings> section is invalid!";
        private const string STRLOG_KeyNotSpecified = "Configuration file <appSettings> key is not specified!";
        private const string STRLOG_XmlNodeNotFound = "Xml node not found!";

        #endregion

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// <para>Get the value for the specified key from the application's configuration file.</para>
        /// <para>Throws an ArgumentNullException exception if the the key is not specified.
        /// Returns null if the key is specified but its value is not or the value is empty.</para>
        /// <para>Exceptions:</para>
        /// <para>System.ArgumentNullException</para>
        /// </summary>
        /// <param name="key"></param>
        /// <returns>Key's value</returns>
        public static string GetAppSetting(string key)
        {
            // Attempt to get key value from configuration file
            string value = ConfigurationManager.AppSettings[key];
            if (value == null)
            {
                throw new ArgumentNullException(key, STRLOG_KeyNotSpecified);
            }

            // Remove leading and trailing whitespace
            string keyValue = value.Trim();

            // Check length
            if (keyValue.Length == 0)
            {
                keyValue = null;
            }

            // Return key's value
            return keyValue;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// <para>Get the value for the specified key from the application's configuration file.</para>
        /// <para>Throws an ArgumentNullException exception if the the key is not specified or
        /// if the key is specified but its value is not or its value is empty.</para>
        /// <para>Exceptions:</para>
        /// <para>System.ArgumentNullException</para>
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetStringAppSetting(string key)
        {
            // Attempt to get key value from configuration file
            string value = ConfigurationManager.AppSettings[key];
            if (value == null)
            {
                throw new ArgumentNullException(key, STRLOG_KeyNotSpecified);
            }

            string keyValue = value.Trim();
            if (keyValue.Length == 0)
            {
                throw new ArgumentNullException(key, STRLOG_KeyNotSpecified);
            }


            // Return key's value
            return keyValue;
        }

        //---------------------------------------------------------------------------------------//

        public static char GetCharAppSetting(string key)
        {
            // Attempt to get key value from configuration file
            string value = ConfigurationManager.AppSettings[key];
            if (value == null)
            {
                throw new ArgumentNullException(key, STRLOG_KeyNotSpecified);
            }

            value = value.Trim();
            if (value.Length == 0)
            {
                throw new ArgumentNullException(key, STRLOG_KeyNotSpecified);
            }


            // Convert string to an integer
            char keyValue = value[0];

            // Return key's value
            return keyValue;
        }

        //---------------------------------------------------------------------------------------//

        public static int GetIntAppSetting(string key)
        {
            // Attempt to get key value from configuration file
            string value = ConfigurationManager.AppSettings[key];
            if (value == null)
            {
                throw new ArgumentNullException(key, STRLOG_KeyNotSpecified);
            }

            value = value.Trim();
            if (value.Length == 0)
            {
                throw new ArgumentNullException(key, STRLOG_KeyNotSpecified);
            }


            // Convert string to an integer
            int keyValue = Int32.Parse(value);

            // Return key's value
            return keyValue;
        }

        //---------------------------------------------------------------------------------------//

        public static long GetLongAppSetting(string key)
        {
            // Attempt to get key value from configuration file
            string value = ConfigurationManager.AppSettings[key];
            if (value == null)
            {
                throw new ArgumentNullException(key, STRLOG_KeyNotSpecified);
            }

            value = value.Trim();
            if (value.Length == 0)
            {
                throw new ArgumentNullException(key, STRLOG_KeyNotSpecified);
            }


            // Convert string to an integer
            long keyValue = Int64.Parse(value);

            // Return key's value
            return keyValue;
        }

        //---------------------------------------------------------------------------------------//

        public static bool GetBoolAppSetting(string key, bool defaultValue)
        {
            bool value = defaultValue;

            try
            {
                value = GetBoolAppSetting(key);
            }
            catch
            {
            }

            return value;
        }

        //---------------------------------------------------------------------------------------//

        public static bool GetBoolAppSetting(string key)
        {
            // Attempt to get key value from configuration file
            string value = ConfigurationManager.AppSettings[key];
            if (value == null)
            {
                throw new ArgumentNullException(key, STRLOG_KeyNotSpecified);
            }
            else
            {
                value = value.Trim();
                if (value.Length == 0)
                {
                    throw new ArgumentNullException(key, STRLOG_KeyNotSpecified);
                }
            }

            // Convert string to an integer
            bool keyValue = bool.Parse(value);

            // Return key's value
            return keyValue;
        }

        //---------------------------------------------------------------------------------------//

        public static string[] GetAppSettings(string key)
        {
            //
            // Attempt to get key value from configuration file
            //
            string value = ConfigurationManager.AppSettings[key];
            if (value == null)
            {
                throw new ArgumentNullException(key, STRLOG_KeyNotSpecified);
            }
            else
            {
                value = value.Trim();
                if (value.Length == 0)
                {
                    throw new ArgumentNullException(key, STRLOG_KeyNotSpecified);
                }
            }

            //
            // Now we know that at least one key value exists, but we need them all
            //

            //
            // Get the AppSettings section as a raw XML string
            //
            Configuration config = ConfigurationManager.OpenExeConfiguration("");
            AppSettingsSection appSettingSection = (AppSettingsSection)config.GetSection(STR_Section_appSettings);
            string xmlAppSettings = appSettingSection.SectionInformation.GetRawXml();

            //
            // Load the XML AppSettings string
            //
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlAppSettings);

            // Create a key value list
            List<string> keyValueList = new List<string>();

            // Trim whitespace
            key = key.Trim();

            try
            {
                //
                // Get a list of all the 'add' elements in the section
                //
                string strXmlNode = STRXMLROOT_appSettings_add;
                XmlNodeList xmlNodeList = xmlDocument.SelectNodes(strXmlNode);
                if (xmlNodeList == null || xmlNodeList.Count == 0)
                {
                    throw new ArgumentNullException(key, STRLOG_KeyNotSpecified);
                }

                //
                // Scan the list for the specified key
                //
                for (int i = 0; i < xmlNodeList.Count; i++)
                {
                    XmlNode xmlNodeAdd = xmlNodeList.Item(i);

                    //
                    // Get the key
                    //
                    strXmlNode = STRXMLPARAM_key;
                    XmlNode xmlNode = xmlNodeAdd.SelectSingleNode(strXmlNode);
                    if (xmlNode != null)
                    {
                        string nodeValue = xmlNode.InnerXml.Trim();

                        //
                        // Check if this is a match
                        //
                        if (key.Equals(nodeValue, StringComparison.OrdinalIgnoreCase) == true)
                        {

                            //
                            // Get the value 
                            //
                            strXmlNode = STRXMLPARAM_value;
                            xmlNode = xmlNodeAdd.SelectSingleNode(strXmlNode);
                            if (xmlNode != null)
                            {
                                // Add the key value to the list
                                keyValueList.Add(xmlNode.InnerXml.Trim());
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentNullException(key, ex.Message);
            }

            //
            // Copy the key value list to a string array
            //
            string[] stringArray = new string[keyValueList.Count];
            keyValueList.CopyTo(stringArray, 0);

            return stringArray;
        }

    }
}
