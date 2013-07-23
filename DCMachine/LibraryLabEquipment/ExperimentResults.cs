using System;
using System.Xml;
using Library.Lab;
using Library.LabEquipment.Devices;
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

        private Measurements measurementData;
        private DeviceRedLion deviceRedLion;

        public Measurements MeasurementData
        {
            get { return measurementData; }
            set { measurementData = value; }
        }

        public void SetDeviceRedLion(DeviceRedLion deviceRedLion)
        {
            this.deviceRedLion = deviceRedLion;
        }

        #endregion

        #region Types

        public struct Measurements
        {
            public float[] speed;
            public float[] armatureVoltage;
            public float[] fieldCurrent;
            public float[] torque;

            public Measurements(int length)
            {
                this.speed = new float[length];
                this.armatureVoltage = new float[length];
                this.fieldCurrent = new float[length];
                this.torque = new float[length];
            }
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
                 * Load the experiment results template XML document from the string
                 */
                XmlDocument xmlDocument = XmlUtilities.GetDocumentFromString(this.xmlTemplate);
                XmlNode xmlNodeRoot = XmlUtilities.GetRootNode(xmlDocument, Consts.STRXML_ExperimentResults);

                /*
                 * Add the experiment specification information to the XML document
                 */
                switch (this.experimentSpecification.SetupId)
                {
                    case Consts.STRXML_SetupId_VoltageVsSpeed:
                    case Consts.STRXML_SetupId_SpeedVsVoltage:
                        XmlUtilities.SetChildValue(xmlNodeRoot, Consts.STRXML_SpeedMin, this.experimentSpecification.SpeedMin);
                        XmlUtilities.SetChildValue(xmlNodeRoot, Consts.STRXML_SpeedMax, this.experimentSpecification.SpeedMax);
                        XmlUtilities.SetChildValue(xmlNodeRoot, Consts.STRXML_SpeedStep, this.experimentSpecification.SpeedStep);
                        break;

                    case Consts.STRXML_SetupId_VoltageVsField:
                    case Consts.STRXML_SetupId_SpeedVsField:
                        XmlUtilities.SetChildValue(xmlNodeRoot, Consts.STRXML_FieldMin, this.experimentSpecification.FieldMin);
                        XmlUtilities.SetChildValue(xmlNodeRoot, Consts.STRXML_FieldMax, this.experimentSpecification.FieldMax);
                        XmlUtilities.SetChildValue(xmlNodeRoot, Consts.STRXML_FieldStep, this.experimentSpecification.FieldStep);
                        break;

                    case Consts.STRXML_SetupId_VoltageVsLoad:
                        XmlUtilities.SetChildValue(xmlNodeRoot, Consts.STRXML_LoadMin, this.experimentSpecification.LoadMin);
                        XmlUtilities.SetChildValue(xmlNodeRoot, Consts.STRXML_LoadMax, this.experimentSpecification.LoadMax);
                        XmlUtilities.SetChildValue(xmlNodeRoot, Consts.STRXML_LoadStep, this.experimentSpecification.LoadStep);
                        break;
                }

                /*
                 * Add the experiment result information to the XML document
                 */
                string descName;
                string descUnits;
                string descFormat;

                /*
                 * Get the motor speed description attributes
                 */
                this.deviceRedLion.GetDCDrive.GetDescriptionSpeed(out descName, out descUnits, out descFormat);
                XmlNode xmlNode = XmlUtilities.GetChildNode(xmlNodeRoot, Consts.STRXML_SpeedVector);

                string name = XmlUtilities.GetAttributeValue(xmlNode, Consts.STRXML_ATTR_Name);
                if (name == null || name.Length == 0)
                {
                    XmlUtilities.SetAttributeValue(xmlNode, Consts.STRXML_ATTR_Name, descName);
                }

                string units = XmlUtilities.GetAttributeValue(xmlNode, Consts.STRXML_ATTR_Units);
                if (units == null || units.Length == 0)
                {
                    XmlUtilities.SetAttributeValue(xmlNode, Consts.STRXML_ATTR_Units, descUnits);
                }

                string format = XmlUtilities.GetAttributeValue(xmlNode, Consts.STRXML_ATTR_Format);
                if (format == null || format.Length == 0)
                {
                    format = descFormat;
                }
                else
                {
                    xmlNode.Attributes.RemoveNamedItem(Consts.STRXML_ATTR_Format);
                }

                /*
                 * Create a CSV string of motor speed measurements
                 */
                XmlUtilities.SetChildValue(xmlNodeRoot, Consts.STRXML_SpeedVector, this.GetCsvString(this.measurementData.speed, format));

                /*
                 * Get the field current description attributes
                 */
                this.deviceRedLion.GetDCDrive.GetDescriptionFieldCurrent(out descName, out descUnits, out descFormat);
                xmlNode = XmlUtilities.GetChildNode(xmlNodeRoot, Consts.STRXML_FieldVector);

                name = XmlUtilities.GetAttributeValue(xmlNode, Consts.STRXML_ATTR_Name);
                if (name == null || name.Length == 0)
                {
                    XmlUtilities.SetAttributeValue(xmlNode, Consts.STRXML_ATTR_Name, descName);
                }

                units = XmlUtilities.GetAttributeValue(xmlNode, Consts.STRXML_ATTR_Units);
                if (units == null || units.Length == 0)
                {
                    XmlUtilities.SetAttributeValue(xmlNode, Consts.STRXML_ATTR_Units, descUnits);
                }

                format = XmlUtilities.GetAttributeValue(xmlNode, Consts.STRXML_ATTR_Format);
                if (format == null || format.Length == 0)
                {
                    format = descFormat;
                }
                else
                {
                    xmlNode.Attributes.RemoveNamedItem(Consts.STRXML_ATTR_Format);
                }

                /*
                 * Create a CSV string of field current measurements
                 */
                XmlUtilities.SetChildValue(xmlNodeRoot, Consts.STRXML_FieldVector, this.GetCsvString(this.measurementData.fieldCurrent, format));

                /*
                 * Get the armature voltage description attributes
                 */
                this.deviceRedLion.GetDCDrive.GetDescriptionArmatureVoltage(out descName, out descUnits, out descFormat);
                xmlNode = XmlUtilities.GetChildNode(xmlNodeRoot, Consts.STRXML_VoltageVector);

                name = XmlUtilities.GetAttributeValue(xmlNode, Consts.STRXML_ATTR_Name);
                if (name == null || name.Length == 0)
                {
                    XmlUtilities.SetAttributeValue(xmlNode, Consts.STRXML_ATTR_Name, descName);
                }

                units = XmlUtilities.GetAttributeValue(xmlNode, Consts.STRXML_ATTR_Units);
                if (units == null || units.Length == 0)
                {
                    XmlUtilities.SetAttributeValue(xmlNode, Consts.STRXML_ATTR_Units, descUnits);
                }

                format = XmlUtilities.GetAttributeValue(xmlNode, Consts.STRXML_ATTR_Format);
                if (format == null || format.Length == 0)
                {
                    format = descFormat;
                }
                else
                {
                    xmlNode.Attributes.RemoveNamedItem(Consts.STRXML_ATTR_Format);
                }

                /*
                 * Create a CSV string of armature voltage measurements
                 */
                XmlUtilities.SetChildValue(xmlNodeRoot, Consts.STRXML_VoltageVector, this.GetCsvString(this.measurementData.armatureVoltage, format));

                /*
                 * Get the load torque description attributes
                 */
                this.deviceRedLion.GetACDrive.GetDescriptionTorque(out descName, out descUnits, out descFormat);
                xmlNode = XmlUtilities.GetChildNode(xmlNodeRoot, Consts.STRXML_LoadVector);

                name = XmlUtilities.GetAttributeValue(xmlNode, Consts.STRXML_ATTR_Name);
                if (name == null || name.Length == 0)
                {
                    XmlUtilities.SetAttributeValue(xmlNode, Consts.STRXML_ATTR_Name, descName);
                }

                units = XmlUtilities.GetAttributeValue(xmlNode, Consts.STRXML_ATTR_Units);
                if (units == null || units.Length == 0)
                {
                    XmlUtilities.SetAttributeValue(xmlNode, Consts.STRXML_ATTR_Units, descUnits);
                }

                format = XmlUtilities.GetAttributeValue(xmlNode, Consts.STRXML_ATTR_Format);
                if (format == null || format.Length == 0)
                {
                    format = descFormat;
                }
                else
                {
                    xmlNode.Attributes.RemoveNamedItem(Consts.STRXML_ATTR_Format);
                }

                /*
                 * Create a CSV string of load torque measurements
                 */
                XmlUtilities.SetChildValue(xmlNodeRoot, Consts.STRXML_LoadVector, this.GetCsvString(this.measurementData.torque, format));

                /*
                 * Save the experiment results information to an XML string
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
        private String GetCsvString(float[] floatArray, String format)
        {
            String csvString = String.Empty;

            for (int i = 0; i < floatArray.Length; i++)
            {
                csvString += String.Format("{0:s}{1:s}", (i > 0) ? Consts.CHRCSV_SplitterChar.ToString() : String.Empty, floatArray[i].ToString(format));
            }

            return csvString;
        }

    }
}
