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
        private const String STRERR_Distance = "Distance";
        private const String STRERR_Duration = "Duration";
        private const String STRERR_Repeat = "Repeat";
        private const String STRERR_TotalTime = "TotalTime";
        private const String STRERR_ValueLessThanMinimum_arg2 = "{0:s}: Less than minimum ({1:d})!";
        private const String STRERR_ValueGreaterThanMaximum_arg2 = "{0:s}: Greater than maximum ({1:d})!";
        #endregion

        private Range distance;
        private Range duration;
        private Range repeat;
        private Range totalTime;

        //-------------------------------------------------------------------------------------------------//

        [XmlElement("distance")]
        public Range Distance
        {
            get { return distance; }
            set { distance = value; }
        }

        [XmlElement("duration")]
        public Range Duration
        {
            get { return duration; }
            set { duration = value; }
        }

        [XmlElement("repeat")]
        public Range Repeat
        {
            get { return repeat; }
            set { repeat = value; }
        }

        [XmlElement("totalTime")]
        public Range TotalTime
        {
            get { return totalTime; }
            set { totalTime = value; }
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

        public void ValidateDistance(int distance)
        {
            if (distance < this.distance.Minimum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueLessThanMinimum_arg2, STRERR_Distance, this.distance.Minimum));
            }
            if (distance > this.distance.Maximum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueGreaterThanMaximum_arg2, STRERR_Distance, this.distance.Maximum));
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public void ValidateDuration(int duration)
        {
            if (duration < this.duration.Minimum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueLessThanMinimum_arg2, STRERR_Duration, this.duration.Minimum));
            }
            if (duration > this.duration.Maximum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueGreaterThanMaximum_arg2, STRERR_Duration, this.duration.Maximum));
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public void ValidateRepeat(int repeat)
        {
            if (repeat < this.repeat.Minimum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueLessThanMinimum_arg2, STRERR_Repeat, this.repeat.Minimum));
            }
            if (repeat > this.repeat.Maximum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueGreaterThanMaximum_arg2, STRERR_Repeat, this.repeat.Maximum));
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public void ValidateTotaltime(int totaltime)
        {
            if (totaltime < this.totalTime.Minimum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueLessThanMinimum_arg2, STRERR_TotalTime, this.totalTime.Minimum));
            }
            if (totaltime > this.totalTime.Maximum)
            {
                throw new ArgumentException(String.Format(STRERR_ValueGreaterThanMaximum_arg2, STRERR_TotalTime, this.totalTime.Maximum));
            }
        }
    }

    //=================================================================================================//

    [Serializable]
    public class Range
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
}
