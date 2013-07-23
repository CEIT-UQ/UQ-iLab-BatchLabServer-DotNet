using System;
using System.IO;
using System.Xml.Serialization;
using Library.LabEquipment.Engine.Types;

namespace Library.LabEquipment.Types
{
    [XmlRoot("validation")]
    public class ExperimentValidation : LabExperimentValidation
    {
        #region Constants
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

        private Range speed;
        private Range field;
        private Range load;

        //-------------------------------------------------------------------------------------------------//

        [XmlElement("field")]
        public Range Field
        {
            get { return field; }
            set { field = value; }
        }

        [XmlElement("load")]
        public Range Load
        {
            get { return load; }
            set { load = value; }
        }

        [XmlElement("speed")]
        public Range Speed
        {
            get { return speed; }
            set { speed = value; }
        }

        //=================================================================================================//

        [Serializable]
        public class Range
        {
            private int minimum;
            private int maximum;
            private StepRange step;

            //-------------------------------------------------------------------------------------------------//

            [XmlElement("minimum")]
            public int Minimum
            {
                get { return minimum; }
                set { minimum = value; }
            }

            [XmlElement("maximum")]
            public int Maximum
            {
                get { return maximum; }
                set { maximum = value; }
            }

            [XmlElement("step")]
            public StepRange Step
            {
                get { return step; }
                set { step = value; }
            }

            //-------------------------------------------------------------------------------------------------//

            public Range()
            {
                this.step = new StepRange();
            }
        }

        //-------------------------------------------------------------------------------------------------//

        [Serializable]
        public class StepRange
        {
            private int minimum;
            private int maximum;

            //-------------------------------------------------------------------------------------------------//

            [XmlElement("minimum")]
            public int Minimum
            {
                get { return minimum; }
                set { minimum = value; }
            }

            [XmlElement("maximum")]
            public int Maximum
            {
                get { return maximum; }
                set { maximum = value; }
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public ExperimentValidation()
        {
            this.field = new Range();
            this.load = new Range();
            this.speed = new Range();
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlString"></param>
        /// <returns></returns>
        public new static ExperimentValidation XmlParse(String xmlString)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ExperimentValidation));
            StringReader stringReader = new StringReader(xmlString);
            return (ExperimentValidation)serializer.Deserialize(stringReader);
        }

        //-------------------------------------------------------------------------------------------------//

        public void ValidateSpeed(int minimum, int maximum, int stepsize)
        {
            if (minimum < this.speed.Minimum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueLessThanMinimum_arg2, STRERR_MinimumSpeed, this.speed.Minimum));
            }
            if (minimum > this.speed.Maximum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueGreaterThanMaximum_arg2, STRERR_MinimumSpeed, this.speed.Maximum));
            }
            if (maximum < this.speed.Minimum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueLessThanMinimum_arg2, STRERR_MaximumSpeed, this.speed.Minimum));
            }
            if (maximum > this.speed.Maximum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueGreaterThanMaximum_arg2, STRERR_MaximumSpeed, this.speed.Maximum));
            }
            if (maximum <= minimum)
            {
                throw new ArgumentException(STRERR_MaximumNotGreaterThanMinimum);
            }
            if (stepsize < this.speed.Step.Minimum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueLessThanMinimum_arg2, STRERR_SpeedStep, this.speed.Step.Minimum));
            }
            if (stepsize > this.speed.Step.Maximum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueGreaterThanMaximum_arg2, STRERR_SpeedStep, this.speed.Step.Maximum));
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public void ValidateField(int minimum, int maximum, int stepsize)
        {
            if (minimum < this.field.Minimum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueLessThanMinimum_arg2, STRERR_MinimumField, this.field.Minimum));
            }
            if (minimum > this.field.Maximum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueGreaterThanMaximum_arg2, STRERR_MinimumField, this.field.Maximum));
            }
            if (maximum < this.field.Minimum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueLessThanMinimum_arg2, STRERR_MaximumField, this.field.Minimum));
            }
            if (maximum > this.field.Maximum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueGreaterThanMaximum_arg2, STRERR_MaximumField, this.field.Maximum));
            }
            if (maximum <= minimum)
            {
                throw new ArgumentException(STRERR_MaximumNotGreaterThanMinimum);
            }
            if (stepsize < this.field.Step.Minimum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueLessThanMinimum_arg2, STRERR_FieldStep, this.field.Step.Minimum));
            }
            if (stepsize > this.field.Step.Maximum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueGreaterThanMaximum_arg2, STRERR_FieldStep, this.field.Step.Maximum));
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public void ValidateLoad(int minimum, int maximum, int stepsize)
        {
            if (minimum < this.load.Minimum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueLessThanMinimum_arg2, STRERR_MinimumLoad, this.load.Minimum));
            }
            if (minimum > this.load.Maximum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueGreaterThanMaximum_arg2, STRERR_MinimumLoad, this.load.Maximum));
            }
            if (maximum < this.load.Minimum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueLessThanMinimum_arg2, STRERR_MinimumLoad, this.load.Minimum));
            }
            if (maximum > this.load.Maximum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueGreaterThanMaximum_arg2, STRERR_MinimumLoad, this.load.Maximum));
            }
            if (maximum <= minimum)
            {
                throw new ArgumentException(STRERR_MaximumNotGreaterThanMinimum);
            }
            if (stepsize < this.load.Step.Minimum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueLessThanMinimum_arg2, STRERR_LoadStep, this.load.Step.Minimum));
            }
            if (stepsize > this.load.Step.Maximum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueGreaterThanMaximum_arg2, STRERR_LoadStep, this.load.Step.Maximum));
            }
        }
    }
}
