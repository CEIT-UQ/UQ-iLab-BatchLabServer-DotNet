using System;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Library.LabEquipment.Engine.Types;

namespace Library.LabEquipment.Types
{
    public class ExperimentSpecification
    {
        #region Properties

        private String setupId;
        private int fieldMin;
        private int fieldMax;
        private int fieldStep;
        private int loadMin;
        private int loadMax;
        private int loadStep;
        private int speedMin;
        private int speedMax;
        private int speedStep;

        public String SetupId
        {
            get { return setupId; }
            set { setupId = value; }
        }

        public int FieldMin
        {
            get { return fieldMin; }
            set { fieldMin = value; }
        }

        public int FieldMax
        {
            get { return fieldMax; }
            set { fieldMax = value; }
        }

        public int FieldStep
        {
            get { return fieldStep; }
            set { fieldStep = value; }
        }

        public int LoadMin
        {
            get { return loadMin; }
            set { loadMin = value; }
        }

        public int LoadMax
        {
            get { return loadMax; }
            set { loadMax = value; }
        }

        public int LoadStep
        {
            get { return loadStep; }
            set { loadStep = value; }
        }

        public int SpeedMin
        {
            get { return speedMin; }
            set { speedMin = value; }
        }

        public int SpeedMax
        {
            get { return speedMax; }
            set { speedMax = value; }
        }

        public int SpeedStep
        {
            get { return speedStep; }
            set { speedStep = value; }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public String ToXmlString()
        {
            String xmlString = null;

            /*
             * Process the desired specification
             */
            switch (this.SetupId)
            {
                case Consts.STRXML_SetupId_SpeedVsField:
                case Consts.STRXML_SetupId_VoltageVsField:
                    ExperimentSpecification_Field experimentSpecification_Field = new ExperimentSpecification_Field();
                    experimentSpecification_Field.FieldMin = this.fieldMin;
                    experimentSpecification_Field.FieldMax = this.fieldMax;
                    experimentSpecification_Field.FieldStep = this.fieldStep;
                    xmlString = experimentSpecification_Field.ToXmlString();
                    break;

                case Consts.STRXML_SetupId_VoltageVsLoad:
                    ExperimentSpecification_Load experimentSpecification_Load = new ExperimentSpecification_Load();
                    experimentSpecification_Load.LoadMin = this.loadMin;
                    experimentSpecification_Load.LoadMax = this.loadMax;
                    experimentSpecification_Load.LoadStep = this.loadStep;
                    xmlString = experimentSpecification_Load.ToXmlString();
                    break;

                case Consts.STRXML_SetupId_SpeedVsVoltage:
                case Consts.STRXML_SetupId_VoltageVsSpeed:
                    ExperimentSpecification_Speed experimentSpecification_Speed = new ExperimentSpecification_Speed();
                    experimentSpecification_Speed.SpeedMin = this.speedMin;
                    experimentSpecification_Speed.SpeedMax = this.speedMax;
                    experimentSpecification_Speed.SpeedStep = this.speedStep;
                    xmlString = experimentSpecification_Speed.ToXmlString();
                    break;

                default:
                    break;
            }

            return xmlString;
        }

        //-------------------------------------------------------------------------------------------------//

        public static ExperimentSpecification XmlParse(String xmlString)
        {
            ExperimentSpecification experimentSpecification = new ExperimentSpecification();

            /*
             * Get the setup Id to determine which experiment specification to parse
             */
            LabExperimentSpecification labExperimentSpecification = LabExperimentSpecification.XmlParse(xmlString);
            experimentSpecification.SetupId = labExperimentSpecification.SetupId;

            /*
             * Parse the desired specification
             */
            switch (labExperimentSpecification.SetupId)
            {
                case Consts.STRXML_SetupId_SpeedVsField:
                case Consts.STRXML_SetupId_VoltageVsField:
                    ExperimentSpecification_Field experimentSpecification_Field = ExperimentSpecification_Field.XmlParse(xmlString);
                    experimentSpecification.FieldMin = experimentSpecification_Field.FieldMin;
                    experimentSpecification.FieldMax = experimentSpecification_Field.FieldMax;
                    experimentSpecification.FieldStep = experimentSpecification_Field.FieldStep;
                    break;

                case Consts.STRXML_SetupId_VoltageVsLoad:
                    ExperimentSpecification_Load experimentSpecification_Load = ExperimentSpecification_Load.XmlParse(xmlString);
                    experimentSpecification.LoadMin = experimentSpecification_Load.LoadMin;
                    experimentSpecification.LoadMax = experimentSpecification_Load.LoadMax;
                    experimentSpecification.LoadStep = experimentSpecification_Load.LoadStep;
                    break;

                case Consts.STRXML_SetupId_SpeedVsVoltage:
                case Consts.STRXML_SetupId_VoltageVsSpeed:
                    ExperimentSpecification_Speed experimentSpecification_Speed = ExperimentSpecification_Speed.XmlParse(xmlString);
                    experimentSpecification.SpeedMin = experimentSpecification_Speed.SpeedMin;
                    experimentSpecification.SpeedMax = experimentSpecification_Speed.SpeedMax;
                    experimentSpecification.SpeedStep = experimentSpecification_Speed.SpeedStep;
                    break;

                default:
                    break;
            }

            return experimentSpecification;
        }
    }
}
