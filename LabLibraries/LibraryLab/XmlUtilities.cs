using System;
using System.Collections;
using System.IO;
using System.Xml;
using Library.Lab.Exceptions;

namespace Library.Lab
{
    public class XmlUtilities
    {
        /*
         * String constants for exception messages
         */
        private const string STRERR_ValueNotSpecified_arg = "{0:s}: Not specified!";
        private const string STRERR_ValueNotNumber_arg = "{0:s}: Not a number!";
        private const string STRERR_ValueNotInteger_arg = "{0:s}: Not an integer!";
        private const string STRERR_NotSpecified_arg = "{0:s}: Not specified!";
        private const string STRERR_XmlNodeNotFound_arg = "{0:s}: Xml node not found!";
        private const string STRERR_XmlNodeIsNull_arg = "{0:s}: Xml node is null!";
        private const string STRERR_XmlNodeIsEmpty_arg = "{0:s}: Xml node is empty!";
        private const string STRERR_ParentNode = "parentNode";
        private const string STRERR_ChildName = "childName";
        //
        // String constants for error messages
        //
        private const string STRERR_XmlStringIsNotSpecified = "XML string is not specified!";
        private const string STRERR_XmlStringParsingFailed = "XML string parsing failed!";
        private const string STRERR_LoadXmlStringFailed = "Failed to load XML string -> ";
        private const string STRERR_LoadXmlFileFailed = "Failed to load XML file -> ";
        private const string STRERR_XmlNodeNotFound = "Xml node not found!";
        private const string STRERR_XmlNodeIsEmpty = "Xml node is empty!";
        private const string STRERR_XmlNodeListIsEmpty = "Xml node list is empty!";
        private const string STRERR_XmlInvalidNumber = "Invalid number!";
        private const string STRERR_XmlInvalidBoolean = "Invalid boolean!";
        private const string STRERR_XmlNodeListIndexInvalid = "Xml nodelist index is out of range!";
        private const string STRERR_ValueIsNull = "Value is null!";
        private const String STRERR_NodeValueDoesNotExist_Arg = "Node value does not exist: ";

        //-------------------------------------------------------------------------------------------------//

        public static XmlDocument GetDocumentFromString(string xmlString)
        {
            /*
             * Check that the XML string exists
             */
            if (xmlString == null || xmlString.Trim().Length == 0)
            {
                throw new XmlUtilitiesException(STRERR_XmlStringIsNotSpecified);
            }

            /*
             * Load the XML string into a document
             */
            XmlDocument xmlDocument = new XmlDocument();
            try
            {
                xmlDocument.LoadXml(xmlString);
            }
            catch (Exception ex)
            {
                throw new XmlUtilitiesException(STRERR_XmlStringParsingFailed, ex);
            }

            return xmlDocument;
        }

        //-------------------------------------------------------------------------------------------------//

        public static XmlDocument GetDocumentFromFile(string filename)
        {
            //
            // Try to load the XML file, may not exist
            //
            XmlTextReader xmlTextReader = null;
            XmlDocument xmlDocument = null;
            try
            {
                xmlTextReader = new XmlTextReader(filename);
                xmlDocument = new XmlDocument();
                xmlDocument.Load(xmlTextReader);
            }
            catch (FileNotFoundException)
            {
                // File does not exist, but that's ok
                xmlDocument = null;
            }
            catch (XmlException ex)
            {
                throw new XmlException(STRERR_LoadXmlFileFailed + ex.Message);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (xmlTextReader != null)
                {
                    xmlTextReader.Close();
                }
            }

            return xmlDocument;
        }

        //-------------------------------------------------------------------------------------------------//

        public static XmlNode GetRootNode(XmlDocument xmlDocument, string strXmlNode)
        {
            // Remove whitespace and prepend root character
            strXmlNode = "/" + strXmlNode.Trim();

            XmlNode xmlNodeTemp = xmlDocument.SelectSingleNode(strXmlNode);
            if (xmlNodeTemp == null)
            {
                throw new ArgumentNullException(strXmlNode, STRERR_XmlNodeNotFound);
            }

            return xmlNodeTemp;
        }

        //-------------------------------------------------------------------------------------------------//

        public static XmlNode GetXmlNode(XmlDocument xmlDocument, string strXmlNode)
        {
            return GetXmlNode(xmlDocument, strXmlNode, null);
        }

        //-------------------------------------------------------------------------------------------------//

        public static XmlNode GetXmlNode(XmlDocument xmlDocument, string strXmlNode, XmlNamespaceManager xnsManager)
        {
            XmlNode xmlNodeTemp;
            if (xnsManager != null)
            {
                xmlNodeTemp = xmlDocument.SelectSingleNode(strXmlNode, xnsManager);
            }
            else
            {
                xmlNodeTemp = xmlDocument.SelectSingleNode(strXmlNode);
            }
            if (xmlNodeTemp == null)
            {
                throw new ArgumentNullException(strXmlNode, STRERR_XmlNodeNotFound);
            }

            return xmlNodeTemp;
        }

        //-------------------------------------------------------------------------------------------------//

        public static XmlNode GetChildNode(XmlNode parentNode, string childName)
        {
            return GetChildNode(parentNode, childName, true);
        }

        //-------------------------------------------------------------------------------------------------//

        public static XmlNode GetChildNode(XmlNode parentNode, string childName, bool mustExist)
        {
            XmlNode xmlNodeTemp = parentNode.SelectSingleNode(childName);
            if (mustExist == true && xmlNodeTemp == null)
            {
                throw new XmlUtilitiesException(String.Format(STRERR_XmlNodeNotFound_arg, childName));
            }

            return xmlNodeTemp;
        }

        //-------------------------------------------------------------------------------------------------//

        public static string GetAttributeValue(XmlNode parentNode, string childName)
        {
            return GetChildValue(parentNode, "@" + childName, false);
        }

        //-------------------------------------------------------------------------------------------------//

        public static string GetAttributeValue(XmlNode parentNode, string childName, bool mustExist)
        {
            return GetChildValue(parentNode, "@" + childName, mustExist);
        }

        //-------------------------------------------------------------------------------------------------//

        public static String GetValue(XmlNode node)
        {
            return GetValue(node, true);
        }

        //-------------------------------------------------------------------------------------------------//

        public static String GetValue(XmlNode node, bool mustExist)
        {
            String value = null;

            if (node != null)
            {
                value = node.InnerXml.Trim();
            }

            /*
             * Check that the text node has a value
             */
            if (mustExist == true && (value == null || value.Length == 0))
            {
                throw new XmlUtilitiesException(STRERR_NodeValueDoesNotExist_Arg);
            }

            return value;
        }

        //-------------------------------------------------------------------------------------------------//

        public static string GetChildValue(XmlNode parentNode, string childName, bool mustExist)
        {
            return GetChildValue(parentNode, childName, null, mustExist);
        }

        //-------------------------------------------------------------------------------------------------//

        public static string GetChildValue(XmlNode parentNode, string childName)
        {
            return GetChildValue(parentNode, childName, null, false);
        }

        //-------------------------------------------------------------------------------------------------//

        public static string GetChildValue(XmlNode parentNode, string childName, XmlNamespaceManager xnsManager, bool mustExist)
        {
            string childValue = null;

            //
            // Get the specified node
            //
            XmlNode childNode;
            if (xnsManager != null)
            {
                childNode = parentNode.SelectSingleNode(childName, xnsManager);
            }
            else
            {
                childNode = parentNode.SelectSingleNode(childName);
            }
            if (childNode == null && mustExist == true)
            {
                throw new XmlUtilitiesException(String.Format(STRERR_XmlNodeNotFound_arg, childName));
            }

            if (childNode != null)
            {
                childValue = childNode.InnerXml.Trim();
            }

            return childValue;
        }

        //-------------------------------------------------------------------------------------------------//

        public static ArrayList GetChildNodeList(XmlNode parentNode, string childName, bool mustExist)
        {
            ArrayList arrayList = null;

            /*
             * Check that the parent node exists
             */
            if (parentNode == null)
            {
                throw new XmlUtilitiesException(String.Format(STRERR_NotSpecified_arg, STRERR_ParentNode));
            }

            /*
             * Check that the name of the child node has been specified
             */
            if (childName == null || childName.Trim().Length == 0)
            {
                throw new XmlUtilitiesException(String.Format(STRERR_NotSpecified_arg, STRERR_ChildName));
            }

            /*
             * Get all child nodes with the specified name
             */
            XmlNodeList xmlNodeList = parentNode.SelectNodes(childName);
            if (mustExist == true && (xmlNodeList == null || xmlNodeList.Count == 0))
            {
                throw new XmlUtilitiesException(String.Format(STRERR_XmlNodeNotFound_arg, childName));
            }

            /*
             * Copy the child nodes to the array list
             */
            arrayList = new ArrayList();
            for (int i = 0; i < xmlNodeList.Count; i++)
            {
                arrayList.Add(xmlNodeList.Item(i));
            }

            return arrayList;
        }

        //-------------------------------------------------------------------------------------------------//

        public static ArrayList GetChildNodeList(XmlNode parentNode, string childName)
        {
            return GetChildNodeList(parentNode, childName, true);
        }

        //-------------------------------------------------------------------------------------------------//

        public static char GetChildValueAsChar(XmlNode parentNode, string childName)
        {
            /*
             * Get the child node's value which must not be empty
             */
            string value = GetChildValue(parentNode, childName, false);

            /*
             * Convert the string to a char value
             */
            if (value.Length < 1)
            {
                throw new XmlUtilitiesException();
            }

            return value[0];
        }

        //-------------------------------------------------------------------------------------------------//

        public static int GetChildValueAsInt(XmlNode parentNode, string childName)
        {
            int intValue = 0;

            try
            {
                /*
                 * Get the child node's value which must exist
                 */
                string value = GetChildValue(parentNode, childName, true);

                /*
                 * Convert the string to an integer value
                 */
                intValue = Int32.Parse(value);
            }
            catch (FormatException)
            {
                try
                {
                    XmlUtilities.GetChildValueAsDouble(parentNode, childName);
                }
                catch (FormatException)
                {
                    throw new FormatException(String.Format(STRERR_ValueNotNumber_arg, childName));
                }
                throw new FormatException(String.Format(STRERR_ValueNotInteger_arg, childName));
            }

            return intValue;
        }

        //-------------------------------------------------------------------------------------------------//

        public static double GetChildValueAsDouble(XmlNode parentNode, string childName)
        {
            /*
             * Get the child node's value which must not be empty
             */
            string value = GetChildValue(parentNode, childName, false);

            /*
             * Convert the string to a double value
             */
            return Double.Parse(value);
        }

        //-------------------------------------------------------------------------------------------------//

        public static bool GetChildValueAsBool(XmlNode parentNode, string childName)
        {
            /*
             * Get the child node's value which must not be empty
             */
            string value = GetChildValue(parentNode, childName, false);

            /*
             * Convert the string to a boolean value
             */
            return Boolean.Parse(value);
        }

        //-------------------------------------------------------------------------------------------------//

        public static String[] GetChildValues(XmlNode parentNode, String childNames, bool mustExist)
        {
            /*
             * Get the child node array list with the specified name
             */
            ArrayList arrayList = GetChildNodeList(parentNode, childNames, mustExist);

            /*
             * Get the text value of each node in the array list and place in the string array
             */
            String[] values = new String[arrayList.Count];
            for (int i = 0; i < values.Length; i++)
            {
                values[i] = ((XmlNode)arrayList[i]).InnerXml;
            }

            return values;
        }

        //-------------------------------------------------------------------------------------------------//

        public static string GetXmlValue(XmlNodeList xmlNodeList, int index, string strXmlNode, bool allowEmpty)
        {
            if (index < 0 || index >= xmlNodeList.Count)
            {
                throw new ArgumentOutOfRangeException(STRERR_XmlNodeListIndexInvalid);
            }

            // Get node's value
            string innerXml = xmlNodeList.Item(index).InnerXml.Trim();

            //
            // Check if value is empty
            //
            if (allowEmpty == false && innerXml.Length == 0)
            {
                throw new ArgumentNullException(strXmlNode, STRERR_XmlNodeIsEmpty);
            }

            return innerXml;
        }

        //-------------------------------------------------------------------------------------------------//

        public static void SetAttributeValue(XmlNode node, string attributeName, string value)
        {
            SetChildValue(node, "@" + attributeName, null, value, false);
        }

        //-------------------------------------------------------------------------------------------------//

        public static void SetChildValue(XmlNode node, string childName, string value)
        {
            SetChildValue(node, childName, null, value, false);
        }

        //-------------------------------------------------------------------------------------------------//

        public static void SetChildValue(XmlNode node, string childName, bool value)
        {
            SetChildValue(node, childName, value.ToString(), false);
        }

        //-------------------------------------------------------------------------------------------------//

        public static void SetChildValue(XmlNode node, string childName, int value)
        {
            SetChildValue(node, childName, value.ToString(), false);
        }

        //-------------------------------------------------------------------------------------------------//

        public static void SetChildValue(XmlNode node, string childName, double value)
        {
            SetChildValue(node, childName, value.ToString(), false);
        }

        //-------------------------------------------------------------------------------------------------//

        public static void SetChildValue(XmlNode node, string childName, string value, bool allowNull)
        {
            SetChildValue(node, childName, null, value, allowNull);
        }

        //-------------------------------------------------------------------------------------------------//

        public static void SetChildValue(XmlNode node, string childName, XmlNamespaceManager xnsManager, string value, bool allowNull)
        {
            //
            // Get the specified node
            //
            XmlNode xmlNodeTemp;
            if (xnsManager != null)
            {
                xmlNodeTemp = node.SelectSingleNode(childName, xnsManager);
            }
            else
            {
                xmlNodeTemp = node.SelectSingleNode(childName);
            }
            if (xmlNodeTemp == null)
            {
                throw new ArgumentNullException(childName, STRERR_XmlNodeNotFound);
            }

            //
            // Check if value is empty
            //
            if (allowNull == false && value == null)
            {
                throw new ArgumentNullException(childName, STRERR_ValueIsNull);
            }

            //
            // Set node's value
            //
            if (value != null)
            {
                xmlNodeTemp.InnerXml = value.Trim();
            }
            else
            {
                xmlNodeTemp.InnerXml = string.Empty;
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public static void SetValue(XmlNode node, string value)
        {
            /*
             * Check that the node exists
             */
            if (node == null)
            {
                throw new XmlUtilitiesException(String.Format(STRERR_NotSpecified_arg, STRERR_XmlNodeIsNull_arg));
            }

            //
            // Set node's value
            //
            if (value != null)
            {
                node.InnerXml = value.Trim();
            }
            else
            {
                node.InnerXml = string.Empty;
            }
        }

        //-------------------------------------------------------------------------------------------------//

        //public static void SetXmlValues(XmlNode xmlNode, string strXmlNode, string[] valueList, bool allowNull)
        //{
        //    //
        //    // Check if value list is empty
        //    //
        //    if (allowNull == false && (valueList == null || valueList.Length == 0))
        //    {
        //        throw new ArgumentNullException(strXmlNode, STRERR_ValueIsNull);
        //    }

        //    // Get the list of nodes
        //    XmlNodeList xmlNodeList = XmlUtilities.GetXmlNodeList(xmlNode, strXmlNode, true);

        //    if (xmlNodeList.Count > 0 && valueList != null)
        //    {
        //        string outerXml = null;
        //        for (int i = 0; i < valueList.Length; i++)
        //        {
        //            //
        //            // Check if value is empty
        //            //
        //            if (allowNull == false && valueList[i] == null)
        //            {
        //                throw new ArgumentNullException(strXmlNode, STRERR_ValueIsNull);
        //            }

        //            //
        //            // Set node's value
        //            //
        //            xmlNodeList.Item(0).InnerXml = valueList[i].Trim();
        //            outerXml += xmlNodeList.Item(0).OuterXml;
        //        }

        //        // Update node
        //        xmlNode.InnerXml = outerXml;
        //    }
        //}

        //-------------------------------------------------------------------------------------------------//

        public static void SetXmlValues(XmlNode xmlNode, string strXmlNode, int[] valueList, char splitter, bool allowNull)
        {
            //
            // Get the specified node
            //
            XmlNode xmlNodeTemp = xmlNode.SelectSingleNode(strXmlNode);
            if (xmlNodeTemp == null)
            {
                throw new ArgumentNullException(strXmlNode, STRERR_XmlNodeNotFound);
            }

            //
            // Check if value list is empty
            //
            if (allowNull == false && (valueList == null || valueList.Length == 0))
            {
                throw new ArgumentNullException(strXmlNode, STRERR_ValueIsNull);
            }

            //
            // Create the vector string
            //
            StringWriter dataVector = new StringWriter();
            for (int i = 0; i < valueList.Length; i++)
            {
                if (i == 0)
                {
                    dataVector.Write(valueList[i].ToString());
                }
                else
                {
                    dataVector.Write("{0}{1}", splitter, valueList[i].ToString());
                }
            }

            //
            // Set node's value
            //
            xmlNodeTemp.InnerXml = dataVector.ToString();
        }

        //-------------------------------------------------------------------------------------------------//

        public static void SetXmlValues(XmlNode xmlNode, string strXmlNode, float[] valueList, string format, char splitter, bool allowNull)
        {
            //
            // Get the specified node
            //
            XmlNode xmlNodeTemp = xmlNode.SelectSingleNode(strXmlNode);
            if (xmlNodeTemp == null)
            {
                throw new ArgumentNullException(strXmlNode, STRERR_XmlNodeNotFound);
            }

            //
            // Check if value list is empty
            //
            if (allowNull == false && (valueList == null || valueList.Length == 0))
            {
                throw new ArgumentNullException(strXmlNode, STRERR_ValueIsNull);
            }

            //
            // Create the vector string
            //
            StringWriter dataVector = new StringWriter();
            for (int i = 0; i < valueList.Length; i++)
            {
                if (i == 0)
                {
                    dataVector.Write(valueList[i].ToString(format));
                }
                else
                {
                    dataVector.Write("{0}{1}", splitter, valueList[i].ToString(format));
                }
            }

            //
            // Set node's value
            //
            xmlNodeTemp.InnerXml = dataVector.ToString();
        }

        //-------------------------------------------------------------------------------------------------//

        public static string ToXmlString(XmlNode xmlNode)
        {
            string xmlString = string.Empty;

            StringWriter sw = new StringWriter();
            XmlTextWriter xtw = new XmlTextWriter(sw);
            xtw.Formatting = Formatting.Indented;
            xmlNode.WriteTo(xtw);
            xtw.Flush();

            xmlString = sw.ToString();

            return xmlString;
        }

    }
}
