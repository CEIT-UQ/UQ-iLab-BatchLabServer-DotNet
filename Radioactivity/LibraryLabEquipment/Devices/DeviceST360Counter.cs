using System;
using System.Diagnostics;
using System.Threading;
using System.Xml;
using Library.Lab;
using Library.LabEquipment.Engine;
using Library.LabEquipment.Engine.Devices;

namespace Library.LabEquipment.Devices
{
    public class DeviceST360Counter : DeviceGeneric
    {
        #region Constants
        private const String STR_ClassName = "DeviceST360Counter";
        private const Logfile.Level logLevel = Logfile.Level.Finer;
        /*
         * String constants for logfile messages
         */
        private const String STRLOG_HighVoltageSpeakerVolume_arg2 = "High Voltage: {0:d} - SpeakerVolume: {1:d}";
        //
        protected const String STRLOG_HighVoltage_arg = "High Voltage: {0:d}";
        protected const String STRLOG_SpeakerVolume_arg = "SpeakerVolume: {0:d}";
        protected const String STRLOG_DistanceDuration_arg2 = "Distance: {0:d}  Duration: {1:d}";
        protected const String STRLOG_Counts_arg = "Counts: {0:d}";
        /*
         * Constants
         */
        public const int MIN_HighVoltage = 0;
        public const int MAX_HighVoltage = 450;
        public const int DEFAULT_HighVoltage = 400;

        public const int MIN_SpeakerVolume = 0;
        public const int MAX_SpeakerVolume = 5;
        public const int DEFAULT_SpeakerVolume = 0;

        public const int MIN_PresetTime = 1;
        public const int MAX_PresetTime = 60;
        #endregion

        #region Variables
        protected int geigerTubeVoltage;
        protected int speakerVolume;
        protected double[] timeAdjustmentCapture;
        #endregion

        #region Properties
        public new static String ClassName
        {
            get { return STR_ClassName; }
        }
        #endregion

        #region Types

        protected enum InterfaceMode
        {
            None = 0x50, Serial = 0x51, Usb = 0x52
        }

        public enum DisplaySelection
        {
            Counts, Time, Rate, HighVoltage, AlarmPoint, SpeakerVolume
        }

        #endregion

        //---------------------------------------------------------------------------------------//

        public DeviceST360Counter(LabEquipmentConfiguration labEquipmentConfiguration)
            : base(labEquipmentConfiguration, STR_ClassName)
        {
            const String methodName = "DeviceST360Counter";
            Logfile.WriteCalled(Logfile.Level.Info, STR_ClassName, methodName);

            try
            {
                /*
                 * Get Geiger tube high voltage
                 */
                try
                {
                    this.geigerTubeVoltage = XmlUtilities.GetChildValueAsInt(this.xmlNodeDevice, Consts.STRXML_GeigerTubeVoltage);
                }
                catch
                {
                    this.geigerTubeVoltage = DEFAULT_HighVoltage;
                }

                /*
                 * Make sure that the high voltage is within range
                 */
                if (this.geigerTubeVoltage < MIN_HighVoltage)
                {
                    this.geigerTubeVoltage = MIN_HighVoltage;
                }
                else if (this.geigerTubeVoltage > MAX_HighVoltage)
                {
                    this.geigerTubeVoltage = MAX_HighVoltage;
                }

                /*
                 * Get speaker volume
                 */
                try
                {
                    this.speakerVolume = XmlUtilities.GetChildValueAsInt(this.xmlNodeDevice, Consts.STRXML_SpeakerVolume);
                }
                catch
                {
                    this.speakerVolume = MIN_SpeakerVolume;
                }

                /*
                 * Make sure that the speaker volume is within range
                 */
                if (this.speakerVolume < MIN_SpeakerVolume)
                {
                    this.speakerVolume = MIN_SpeakerVolume;
                }
                else if (this.speakerVolume > MAX_SpeakerVolume)
                {
                    this.speakerVolume = MAX_SpeakerVolume;
                }

                /*
                 * Get capture time adjustment: y = Mx + C
                 */
                XmlNode xmlNode = XmlUtilities.GetChildNode(this.xmlNodeDevice, Consts.STRXML_TimeAdjustment);
                String capture = XmlUtilities.GetChildValue(xmlNode, Consts.STRXML_Capture, false);
                String[] strSplit = capture.Split(new char[] { Consts.CHRCSV_SplitterChar });
                this.timeAdjustmentCapture = new double[strSplit.Length];
                for (int i = 0; i < strSplit.Length; i++)
                {
                    this.timeAdjustmentCapture[i] = Double.Parse(strSplit[i]);
                }

                Logfile.Write(String.Format(STRLOG_HighVoltageSpeakerVolume_arg2, this.geigerTubeVoltage, this.speakerVolume));
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw ex;
            }

            Logfile.WriteCompleted(Logfile.Level.Info, STR_ClassName, methodName);
        }

        //---------------------------------------------------------------------------------------//

        public virtual double GetCaptureDataTime(int duration)
        {
            return 0.0;
        }

        //---------------------------------------------------------------------------------------//

        public virtual bool CaptureData(int duration, out int counts)
        {
            counts = 0;
            return true;
        }
    }
}
