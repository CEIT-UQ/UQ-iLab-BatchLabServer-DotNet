using System;
using System.Xml;
using Library.Lab;
using Library.LabEquipment.Engine;
using Library.LabEquipment.Engine.Types;

namespace Library.LabEquipment
{
    public class ExperimentValidation : LabExperimentValidation
    {
        #region Constants
        private const string STR_ClassName = "ExperimentValidation";
        private const Logfile.Level logLevel = Logfile.Level.Fine;
        /*
         * String constants for exception messages
         */
        private const string STRERR_MinimumSpeed = "Minimum Speed";
        private const string STRERR_MaximumSpeed = "Maximum Speed";
        private const string STRERR_SpeedStep = "Speed Step";
        private const string STRERR_MinimumField = "Minimum Field";
        private const string STRERR_MaximumField = "Maximum Field";
        private const string STRERR_FieldStep = "Field Step";
        private const string STRERR_MinimumLoad = "Minimum Load";
        private const string STRERR_MaximumLoad = "Maximum Load";
        private const string STRERR_LoadStep = "Load Step";
        private const string STRERR_ValueLessThanMinimum_arg2 = "{0:s}: Less than minimum ({1:d})!";
        private const string STRERR_ValueGreaterThanMaximum_arg2 = "{0:s}: Greater than maximum ({1:d})!";
        private const string STRERR_MaximumNotGreaterThanMinimum = "Maximum must be greater than minimum!";
        #endregion

        #region Variables
        private int speedMinimum;
        private int speedMaximum;
        private int speedStepMinimum;
        private int speedStepMaximum;
        private int fieldMinimum;
        private int fieldMaximum;
        private int fieldStepMinimum;
        private int fieldStepMaximum;
        private int loadMinimum;
        private int loadMaximum;
        private int loadStepMinimum;
        private int loadStepMaximum;
        #endregion

        //-------------------------------------------------------------------------------------------------//

        public ExperimentValidation(string xmlValidation)
            : base(xmlValidation)
        {
            const string methodName = "ExperimentValidation";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            /*
             * Get the minimum and maximum values allowed for speed
             */
            XmlNode xmlNode = XmlUtilities.GetChildNode(this.xmlNodeValidation, Consts.STRXML_Speed);
            this.speedMinimum = XmlUtilities.GetChildValueAsInt(xmlNode, Consts.STRXML_Minimum);
            this.speedMaximum = XmlUtilities.GetChildValueAsInt(xmlNode, Consts.STRXML_Maximum);
            this.speedStepMinimum = XmlUtilities.GetChildValueAsInt(xmlNode, Consts.STRXML_StepMin);
            this.speedStepMaximum = XmlUtilities.GetChildValueAsInt(xmlNode, Consts.STRXML_StepMax);

            /*
             * Get the minimum and maximum values allowed for field
             */
            xmlNode = XmlUtilities.GetChildNode(this.xmlNodeValidation, Consts.STRXML_Field);
            this.fieldMinimum = XmlUtilities.GetChildValueAsInt(xmlNode, Consts.STRXML_Minimum);
            this.fieldMaximum = XmlUtilities.GetChildValueAsInt(xmlNode, Consts.STRXML_Maximum);
            this.fieldStepMinimum = XmlUtilities.GetChildValueAsInt(xmlNode, Consts.STRXML_StepMin);
            this.fieldStepMaximum = XmlUtilities.GetChildValueAsInt(xmlNode, Consts.STRXML_StepMax);

            /*
             * Get the minimum and maximum values allowed for load
             */
            xmlNode = XmlUtilities.GetChildNode(this.xmlNodeValidation, Consts.STRXML_Load);
            this.loadMinimum = XmlUtilities.GetChildValueAsInt(xmlNode, Consts.STRXML_Minimum);
            this.loadMaximum = XmlUtilities.GetChildValueAsInt(xmlNode, Consts.STRXML_Maximum);
            this.loadStepMinimum = XmlUtilities.GetChildValueAsInt(xmlNode, Consts.STRXML_StepMin);
            this.loadStepMaximum = XmlUtilities.GetChildValueAsInt(xmlNode, Consts.STRXML_StepMax);

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        public void ValidateSpeed(int minimum, int maximum, int stepsize)
        {
            if (minimum < this.speedMinimum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueLessThanMinimum_arg2, STRERR_MinimumSpeed, this.speedMinimum));
            }
            if (minimum > this.speedMaximum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueGreaterThanMaximum_arg2, STRERR_MinimumSpeed, this.speedMaximum));
            }
            if (maximum < this.speedMinimum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueLessThanMinimum_arg2, STRERR_MaximumSpeed, this.speedMinimum));
            }
            if (maximum > this.speedMaximum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueGreaterThanMaximum_arg2, STRERR_MaximumSpeed, this.speedMaximum));
            }
            if (maximum <= minimum)
            {
                throw new ArgumentException(STRERR_MaximumNotGreaterThanMinimum);
            }
            if (stepsize < this.speedStepMinimum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueLessThanMinimum_arg2, STRERR_SpeedStep, this.speedStepMinimum));
            }
            if (stepsize > this.speedStepMaximum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueGreaterThanMaximum_arg2, STRERR_SpeedStep, this.speedStepMaximum));
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public void ValidateField(int minimum, int maximum, int stepsize)
        {
            if (minimum < this.fieldMinimum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueLessThanMinimum_arg2, STRERR_MinimumField, this.fieldMinimum));
            }
            if (minimum > this.fieldMaximum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueGreaterThanMaximum_arg2, STRERR_MinimumField, this.fieldMaximum));
            }
            if (maximum < this.fieldMinimum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueLessThanMinimum_arg2, STRERR_MaximumField, this.fieldMinimum));
            }
            if (maximum > this.fieldMaximum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueGreaterThanMaximum_arg2, STRERR_MaximumField, this.fieldMaximum));
            }
            if (maximum <= minimum)
            {
                throw new ArgumentException(STRERR_MaximumNotGreaterThanMinimum);
            }
            if (stepsize < this.fieldStepMinimum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueLessThanMinimum_arg2, STRERR_FieldStep, this.fieldStepMinimum));
            }
            if (stepsize > this.fieldStepMaximum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueGreaterThanMaximum_arg2, STRERR_FieldStep, this.fieldStepMaximum));
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public void ValidateLoad(int minimum, int maximum, int stepsize)
        {
            if (minimum < this.loadMinimum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueLessThanMinimum_arg2, STRERR_MinimumLoad, this.loadMinimum));
            }
            if (minimum > this.loadMaximum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueGreaterThanMaximum_arg2, STRERR_MinimumLoad, this.loadMaximum));
            }
            if (maximum < this.loadMinimum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueLessThanMinimum_arg2, STRERR_MinimumLoad, this.loadMinimum));
            }
            if (maximum > this.loadMaximum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueGreaterThanMaximum_arg2, STRERR_MinimumLoad, this.loadMaximum));
            }
            if (maximum <= minimum)
            {
                throw new ArgumentException(STRERR_MaximumNotGreaterThanMinimum);
            }
            if (stepsize < this.loadStepMinimum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueLessThanMinimum_arg2, STRERR_LoadStep, this.loadStepMinimum));
            }
            if (stepsize > this.loadStepMaximum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueGreaterThanMaximum_arg2, STRERR_LoadStep, this.loadStepMaximum));
            }
        }
    }
}
