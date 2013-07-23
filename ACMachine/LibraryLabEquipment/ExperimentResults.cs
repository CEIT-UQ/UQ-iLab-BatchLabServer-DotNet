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
            public float[] voltageMut;
            public float[] currentMut;
            public float[] powerFactorMut;
            public float[] voltageVsd;
            public float[] currentVsd;
            public float[] powerFactorVsd;
            public int[] speed;

            public Measurements(int length)
            {
                this.voltageMut = new float[length];
                this.currentMut = new float[length];
                this.powerFactorMut = new float[length];
                this.voltageVsd = new float[length];
                this.currentVsd = new float[length];
                this.powerFactorVsd = new float[length];
                this.speed = new int[length];
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
            const string methodName = "ToXmlString";
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
                 * Add the experiment result information to the XML document
                 */
                string descName;
                string descUnits;
                string descFormat;

                /*
                 * Get the voltage description attributes
                 */
                this.deviceRedLion.GetPowerMeterMut.GetDescriptionVoltage(out descName, out descUnits, out descFormat);
                XmlNode node = XmlUtilities.GetChildNode(xmlNodeRoot, Consts.STRXML_Voltage);

                string name = XmlUtilities.GetAttributeValue(node, Consts.STRXML_ATTR_Name);
                if (name == null || name.Length == 0)
                {
                    XmlUtilities.SetAttributeValue(node, Consts.STRXML_ATTR_Name, descName);
                }

                string units = XmlUtilities.GetAttributeValue(node, Consts.STRXML_ATTR_Units);
                if (units == null || units.Length == 0)
                {
                    XmlUtilities.SetAttributeValue(node, Consts.STRXML_ATTR_Units, descUnits);
                }

                string format = XmlUtilities.GetAttributeValue(node, Consts.STRXML_ATTR_Format);
                if (format == null || format.Length == 0)
                {
                    format = descFormat;
                }
                else
                {
                    node.Attributes.RemoveNamedItem(Consts.STRXML_ATTR_Format);
                }

                /*
                 * Determine which voltage measurement to use
                 */
                switch (this.experimentSpecification.SetupId)
                {
                    case Consts.STRXML_SetupId_SynchronousSpeed:
                        XmlUtilities.SetChildValue(xmlNodeRoot, Consts.STRXML_Voltage, this.measurementData.voltageVsd[0].ToString(format));
                        break;

                    default:
                        XmlUtilities.SetChildValue(xmlNodeRoot, Consts.STRXML_Voltage, this.measurementData.voltageMut[0].ToString(format));
                        break;
                }

                /*
                 * Get the current description attributes
                 */
                this.deviceRedLion.GetPowerMeterMut.GetDescriptionCurrent(out descName, out descUnits, out descFormat);
                node = XmlUtilities.GetChildNode(xmlNodeRoot, Consts.STRXML_Current);

                name = XmlUtilities.GetAttributeValue(node, Consts.STRXML_ATTR_Name);
                if (name == null || name.Length == 0)
                {
                    XmlUtilities.SetAttributeValue(node, Consts.STRXML_ATTR_Name, descName);
                }

                units = XmlUtilities.GetAttributeValue(node, Consts.STRXML_ATTR_Units);
                if (units == null || units.Length == 0)
                {
                    XmlUtilities.SetAttributeValue(node, Consts.STRXML_ATTR_Units, descUnits);
                }

                format = XmlUtilities.GetAttributeValue(node, Consts.STRXML_ATTR_Format);
                if (format == null || format.Length == 0)
                {
                    format = descFormat;
                }
                else
                {
                    node.Attributes.RemoveNamedItem(Consts.STRXML_ATTR_Format);
                }

                /*
                 * Determine which current measurement to use
                 */
                switch (this.experimentSpecification.SetupId)
                {
                    case Consts.STRXML_SetupId_SynchronousSpeed:
                        XmlUtilities.SetChildValue(xmlNodeRoot, Consts.STRXML_Current, this.measurementData.currentVsd[0].ToString(format));
                        break;

                    default:
                        XmlUtilities.SetChildValue(xmlNodeRoot, Consts.STRXML_Current, this.measurementData.currentMut[0].ToString(format));
                        break;
                }

                /*
                 * Get the power factor description attributes
                 */
                this.deviceRedLion.GetPowerMeterMut.GetDescriptionPowerFactor(out descName, out descUnits, out descFormat);
                node = XmlUtilities.GetChildNode(xmlNodeRoot, Consts.STRXML_PowerFactor);

                name = XmlUtilities.GetAttributeValue(node, Consts.STRXML_ATTR_Name);
                if (name == null || name.Length == 0)
                {
                    XmlUtilities.SetAttributeValue(node, Consts.STRXML_ATTR_Name, descName);
                }

                units = XmlUtilities.GetAttributeValue(node, Consts.STRXML_ATTR_Units);
                if (units == null || units.Length == 0)
                {
                    XmlUtilities.SetAttributeValue(node, Consts.STRXML_ATTR_Units, descUnits);
                }

                format = XmlUtilities.GetAttributeValue(node, Consts.STRXML_ATTR_Format);
                if (format == null || format.Length == 0)
                {
                    format = descFormat;
                }
                else
                {
                    node.Attributes.RemoveNamedItem(Consts.STRXML_ATTR_Format);
                }

                /*
                 * Determine which PowerFactor measurement to use
                 */
                switch (this.experimentSpecification.SetupId)
                {
                    case Consts.STRXML_SetupId_SynchronousSpeed:
                        XmlUtilities.SetChildValue(xmlNodeRoot, Consts.STRXML_PowerFactor, this.measurementData.powerFactorVsd[0].ToString(format));
                        break;

                    default:
                        XmlUtilities.SetChildValue(xmlNodeRoot, Consts.STRXML_PowerFactor, this.measurementData.powerFactorMut[0].ToString(format));
                        break;
                }

                /*
                 * Get the speed description attributes
                 */
                this.deviceRedLion.GetACDrive.GetDescriptionSpeed(out descName, out descUnits, out descFormat);
                node = XmlUtilities.GetChildNode(xmlNodeRoot, Consts.STRXML_Speed);

                name = XmlUtilities.GetAttributeValue(node, Consts.STRXML_ATTR_Name);
                if (name == null || name.Length == 0)
                {
                    XmlUtilities.SetAttributeValue(node, Consts.STRXML_ATTR_Name, descName);
                }

                units = XmlUtilities.GetAttributeValue(node, Consts.STRXML_ATTR_Units);
                if (units == null || units.Length == 0)
                {
                    XmlUtilities.SetAttributeValue(node, Consts.STRXML_ATTR_Units, descUnits);
                }

                format = XmlUtilities.GetAttributeValue(node, Consts.STRXML_ATTR_Format);
                if (format == null || format.Length == 0)
                {
                    format = descFormat;
                }
                else
                {
                    node.Attributes.RemoveNamedItem(Consts.STRXML_ATTR_Format);
                }
                XmlUtilities.SetChildValue(xmlNodeRoot, Consts.STRXML_Speed, this.measurementData.speed[0]);

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
    }
}
