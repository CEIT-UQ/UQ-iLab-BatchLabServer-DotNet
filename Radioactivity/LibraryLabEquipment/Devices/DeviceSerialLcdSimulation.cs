using System;
using Library.Lab;
using Library.Lab.Utilities;
using Library.LabEquipment.Engine;

namespace Library.LabEquipment.Devices
{
    public class DeviceSerialLcdSimulation : DeviceSerialLcd
    {
        #region Constants
        private const String STR_ClassName = "DeviceSerialLcdSimulation";
        private const Logfile.Level logLevel = Logfile.Level.Finer;
        /*
         * String constants
         */
        private const String STR_HardwareFirmwareVersion = "Simulation";
        #endregion

        #region Properties
        public new static String ClassName
        {
            get { return STR_ClassName; }
        }

        private bool delaysSimulated;

        public bool DelaysSimulated
        {
            get { return delaysSimulated; }
            set { delaysSimulated = value; }
        }
        #endregion

        //---------------------------------------------------------------------------------------//

        public DeviceSerialLcdSimulation(LabEquipmentConfiguration labEquipmentConfiguration)
            : base(labEquipmentConfiguration)
        {
            const String methodName = "DeviceSerialLcdSimulation";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            try
            {
                /*
                 * Initialise properties
                 */
                this.delaysSimulated = true;
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
                 * Get the firmware version and display
                 */
                this.WriteLine(LineNumber.One, DeviceSerialLcd.ClassName);
                this.WriteLine(LineNumber.Two, this.GetHardwareFirmwareVersion());

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

        public override String GetHardwareFirmwareVersion()
        {
            return STR_HardwareFirmwareVersion;
        }

        //---------------------------------------------------------------------------------------//

        public override double GetWriteLineTime()
        {
            return this.writeLineTime;
        }

        //---------------------------------------------------------------------------------------//

        public override bool WriteLine(LineNumber lineno, String message)
        {
            /*
             * Write the message to the logfile
             */
            base.WriteLine(lineno, message);

            /*
             * Check if simulating delays
             */
            if (this.delaysSimulated == true)
            {
                Delay.MilliSeconds((int)(this.writeLineTime * 1000));
            }

            return true;
        }
    }
}
