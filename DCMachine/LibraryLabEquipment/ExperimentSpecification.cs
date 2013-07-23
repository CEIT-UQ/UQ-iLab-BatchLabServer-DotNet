using System;
using Library.Lab;
using Library.LabEquipment.Engine;

namespace Library.LabEquipment
{
    public class ExperimentSpecification : LabExperimentSpecification
    {
        #region Constants
        private const string STR_ClassName = "ExperimentSpecification";
        private const Logfile.Level logLevel = Logfile.Level.Fine;
        /*
         * String constants for exception messages
         */
        private const string STRERR_SomeParameter = "SomeParameter";
        private const string STRERR_ValueNotSpecified_arg = "{0:s}: Not specified!";
        private const string STRERR_ValueNotNumber_arg = "{0:s}: Not a number!";
        private const string STRERR_ValueNotInteger_arg = "{0:s}: Not an integer!";
        #endregion

        #region Properties
        private int speedMin;
        private int speedMax;
        private int speedStep;
        private int fieldMin;
        private int fieldMax;
        private int fieldStep;
        private int loadMin;
        private int loadMax;
        private int loadStep;

        public int SpeedMin
        {
            get { return speedMin; }
        }

        public int SpeedMax
        {
            get { return speedMax; }
        }

        public int SpeedStep
        {
            get { return speedStep; }
        }

        public int FieldMin
        {
            get { return fieldMin; }
        }

        public int FieldMax
        {
            get { return fieldMax; }
        }

        public int FieldStep
        {
            get { return fieldStep; }
        }

        public int LoadMin
        {
            get { return loadMin; }
        }

        public int LoadMax
        {
            get { return loadMax; }
        }

        public int LoadStep
        {
            get { return loadStep; }
        }
        #endregion

        //-------------------------------------------------------------------------------------------------//

        public ExperimentSpecification(string xmlSpecification)
            : base(xmlSpecification)
        {
            const string methodName = "ExperimentSpecification";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);


            /*
             * Get the experiment parameters from the specification
             */
            switch (this.setupId)
            {
                case Consts.STRXML_SetupId_VoltageVsSpeed:
                case Consts.STRXML_SetupId_SpeedVsVoltage:
                    this.speedMin = XmlUtilities.GetChildValueAsInt(this.xmlNodeSpecification, Consts.STRXML_SpeedMin);
                    this.speedMax = XmlUtilities.GetChildValueAsInt(this.xmlNodeSpecification, Consts.STRXML_SpeedMax);
                    this.speedStep = XmlUtilities.GetChildValueAsInt(this.xmlNodeSpecification, Consts.STRXML_SpeedStep);
                    break;

                case Consts.STRXML_SetupId_VoltageVsField:
                case Consts.STRXML_SetupId_SpeedVsField:
                    this.fieldMin = XmlUtilities.GetChildValueAsInt(this.xmlNodeSpecification, Consts.STRXML_FieldMin);
                    this.fieldMax = XmlUtilities.GetChildValueAsInt(this.xmlNodeSpecification, Consts.STRXML_FieldMax);
                    this.fieldStep = XmlUtilities.GetChildValueAsInt(this.xmlNodeSpecification, Consts.STRXML_FieldStep);
                    break;

                case Consts.STRXML_SetupId_VoltageVsLoad:
                    this.loadMin = XmlUtilities.GetChildValueAsInt(this.xmlNodeSpecification, Consts.STRXML_LoadMin);
                    this.loadMax = XmlUtilities.GetChildValueAsInt(this.xmlNodeSpecification, Consts.STRXML_LoadMax);
                    this.loadStep = XmlUtilities.GetChildValueAsInt(this.xmlNodeSpecification, Consts.STRXML_LoadStep);
                    break;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }
    }
}
