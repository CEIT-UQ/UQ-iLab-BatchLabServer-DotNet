using System;
using System.Diagnostics;
using System.Xml;
using Library.Lab;
using Library.LabEquipment.Engine;
using Library.LabEquipment.Engine.Devices;

namespace Library.LabEquipment.Devices
{
    public class DeviceSerialLcd : DeviceGeneric
    {
        #region Constants
        private const String STR_ClassName = "DeviceSerialLcd";
        private const Logfile.Level logLevel = Logfile.Level.Finer;
        /*
         * String constants for logfile messages
         */
        protected const String STRLOG_DeviceLineMessage_arg3 = "{0:s} - {1:d}:[{2:s}]";
        #endregion

        #region Variables
        protected double writeLineTime;
        #endregion

        #region Properties
        public new static String ClassName
        {
            get { return STR_ClassName; }
        }

        protected bool captureCompleted;
        protected int captureCount;

        public bool CaptureCompleted
        {
            get { return captureCompleted; }
        }

        public int CaptureCount
        {
            get { return captureCount; }
        }
        #endregion

        #region Types
        public enum LineNumber
        {
            One = 1,
            Two = 2
        }
        #endregion

        //---------------------------------------------------------------------------------------//

        public DeviceSerialLcd(LabEquipmentConfiguration labEquipmentConfiguration)
            : base(labEquipmentConfiguration, STR_ClassName)
        {
            const String methodName = "DeviceSerialLcd";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            try
            {
                /*
                 * WriteLine time
                 */
                this.writeLineTime = XmlUtilities.GetChildValueAsDouble(this.xmlNodeDevice, Consts.STRXML_WriteLineTime);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw ex;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>bool</returns>
        public override bool Initialise()
        {
            const String methodName = "Initialise";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;

            try
            {
                /*
                 * Initialise local variables
                 */
                this.captureCompleted = false;
                this.captureCount = 0;

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //---------------------------------------------------------------------------------------//

        public virtual String GetHardwareFirmwareVersion()
        {
            return null;
        }

        //---------------------------------------------------------------------------------------//

        public virtual double GetWriteLineTime()
        {
            return 0.0;
        }

        //---------------------------------------------------------------------------------------//

        public virtual bool WriteLine(LineNumber lineno, String message)
        {
            /*
             * Write the message to the logfile
             */
            String line = String.Format(STRLOG_DeviceLineMessage_arg3, ClassName, (int)lineno, (message != null) ? message : String.Empty);
            Logfile.Write(Logfile.Level.Info, line);
            Trace.WriteLine(line);

            return true;
        }

        //---------------------------------------------------------------------------------------//

        public virtual bool StartCapture(int seconds)
        {
            return true;
        }

        //---------------------------------------------------------------------------------------//

        public virtual bool StopCapture()
        {
            return true;
        }

    }
}
