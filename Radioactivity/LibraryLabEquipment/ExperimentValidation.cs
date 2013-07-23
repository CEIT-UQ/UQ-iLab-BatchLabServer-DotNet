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
        private const string STRERR_Distance = "Distance";
        private const string STRERR_Duration = "Duration";
        private const string STRERR_Repeat = "Repeat";
        private const string STRERR_ValueLessThanMinimum_arg2 = "{0:s}: Less than minimum ({1:d})!";
        private const string STRERR_ValueGreaterThanMaximum_arg2 = "{0:s}: Greater than maximum ({1:d})!";
        #endregion

        #region Variables
        private int distanceMin;
        private int distanceMax;
        private int durationMin;
        private int durationMax;
        private int repeatMin;
        private int repeatMax;
        #endregion

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlValidation"></param>
        public ExperimentValidation(string xmlValidation)
            : base(xmlValidation)
        {
            const string methodName = "ExperimentValidation";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            try
            {
                /*
                 * Get the minimum and maximum values allowed for 'Distance'
                 */
                XmlNode xmlNode = XmlUtilities.GetChildNode(this.xmlNodeValidation, Consts.STRXML_VdnDistance);
                this.distanceMin = XmlUtilities.GetChildValueAsInt(xmlNode, Consts.STRXML_VdnMinimum);
                this.distanceMax = XmlUtilities.GetChildValueAsInt(xmlNode, Consts.STRXML_VdnMaximum);

                /*
                 * Get the minimum and maximum values allowed for 'Duration'
                 */
                xmlNode = XmlUtilities.GetChildNode(this.xmlNodeValidation, Consts.STRXML_VdnDuration);
                this.durationMin = XmlUtilities.GetChildValueAsInt(xmlNode, Consts.STRXML_VdnMinimum);
                this.durationMax = XmlUtilities.GetChildValueAsInt(xmlNode, Consts.STRXML_VdnMaximum);

                /*
                 * Get the minimum and maximum values allowed for 'Repeat'
                 */
                xmlNode = XmlUtilities.GetChildNode(this.xmlNodeValidation, Consts.STRXML_VdnRepeat);
                this.repeatMin = XmlUtilities.GetChildValueAsInt(xmlNode, Consts.STRXML_VdnMinimum);
                this.repeatMax = XmlUtilities.GetChildValueAsInt(xmlNode, Consts.STRXML_VdnMaximum);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
                throw ex;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="distance"></param>
        public void ValidateDistance(int distance)
        {
            if (distance < this.distanceMin)
            {
                throw new ArgumentException(String.Format(STRERR_ValueLessThanMinimum_arg2, STRERR_Distance, this.distanceMin));
            }
            if (distance > this.distanceMax)
            {
                throw new ArgumentException(String.Format(STRERR_ValueGreaterThanMaximum_arg2, STRERR_Distance, this.distanceMax));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="distance"></param>
        public void ValidateDuration(int distance)
        {
            if (distance < this.durationMin)
            {
                throw new ArgumentException(String.Format(STRERR_ValueLessThanMinimum_arg2, STRERR_Duration, this.durationMin));
            }
            if (distance > this.durationMax)
            {
                throw new ArgumentException(String.Format(STRERR_ValueGreaterThanMaximum_arg2, STRERR_Duration, this.durationMax));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="repeat"></param>
        public void ValidateRepeat(int repeat)
        {
            if (repeat < this.repeatMin)
            {
                throw new ArgumentException(String.Format(STRERR_ValueLessThanMinimum_arg2, STRERR_Repeat, this.repeatMin));
            }
            if (repeat > this.repeatMax)
            {
                throw new ArgumentException(String.Format(STRERR_ValueGreaterThanMaximum_arg2, STRERR_Repeat, this.repeatMax));
            }
        }
    }
}
