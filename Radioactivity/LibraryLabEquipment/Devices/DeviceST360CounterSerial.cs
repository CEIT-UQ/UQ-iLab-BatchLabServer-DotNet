using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;
using System.Xml;
using Library.Lab;
using Library.Lab.Utilities;
using Library.LabEquipment.Engine;

namespace Library.LabEquipment.Devices
{
    public class DeviceST360CounterSerial : DeviceST360CounterSimulation
    {
        #region Constants
        private const String STR_ClassName = "DeviceST360CounterSerial";
        private const Logfile.Level logLevel = Logfile.Level.Finer;
        /*
         * String constants for logfile messages
         */
        private const String STRLOG_SerialPortBaudRate_arg2 = "SerialPort: {0:s} - BaudRate: {1:d}";
        private const String STRLOG_OpeningSerialPort = "Opening serial port...";
        private const String STRLOG_ClosingSerialPort = "Closing serial port...";
        private const String STRLOG_ReceiveHandlerThreadIsStarting = "ReceiveHandler thread is starting...";
        private const String STRLOG_ReceiveHandlerThreadIsRunning = "ReceiveHandler thread is running.";
        private const String STRLOG_InterfaceMode_arg = "Interface Mode: {0:s}";
        private const String STRLOG_DisplaySelection_arg = "Display Selection: {0:s}";
        private const String STRLOG_Duration_arg = "Display Selection: {0:d}";
        private const String STRLOG_PresetTime_arg = "PresetTime: {0:d}";
        /*
         * String constants for exception messages
         */
        private const String STRERR_Duration_arg = "Duration {0:d}: ";
        private const String STRERR_DurationLessThanMinimum_arg2 = STRERR_Duration_arg + "Less than minimum of {1:d}!";
        private const String STRERR_DurationGreaterThanMaximum_arg2 = STRERR_Duration_arg + "Greater than maximum of {1:d}!";
        private const String STRERR_DurationNotMultipleOf_arg2 = STRERR_Duration_arg + "Not multiple of {1:d}!";
        private const String STRERR_ReceiveThread = "ReceiveThread";
        private const String STRERR_ReceiveThreadFailedToStart = "Receive thread failed to start!";
        //
        private const String STRERR_FailedToSetInterfaceMode = "Failed to set interface mode!";
        private const String STRERR_FailedToSetDisplaySelection = "Failed to set display selection!";
        private const String STRERR_FailedToSetHighVoltage = "Failed to set High Voltage!";
        private const String STRERR_FailedToSetSpeakerVolume = "Failed to set Speaker Volume!";
        private const String STRERR_FailedToSetPresetTime = "Failed to set Preset Time!";
        private const String STRERR_FailedToPushDisplaySelectSwitch = "Failed to push Display Select switch!";
        private const String STRERR_FailedToPushStartSwitch = "Failed to push Start switch!";
        private const String STRERR_FailedToPushStopSwitch = "Failed to push Stop switch!";
        private const String STRERR_FailedToPushResetSwitch = "Failed to push Reset switch!";
        private const String STRERR_FailedToReadCountingStatus = "Failed to read Counting Status!";
        private const String STRERR_FailedToReadCounts = "Failed to read Counts!";
        private const String STRERR_CaptureTimedOut = "Capture timed out!";
        /*
         * Delays are in millisecs
         */
        protected const int DELAY_MS_IsCounting = 500;
        protected const int MAXIMUM_MS_ResponseTime = 2000;
        #endregion

        #region Constants - Commands
        /*
         * Single byte commands that return 5 bytes which is an echo of the command followed by 4 data bytes
         */
        private const byte CMD_ReadCounts = 0x40;
        private const byte CMD_ReadPresetCounts = 0x41;
        private const byte CMD_ReadElapsedTime = 0x42;
        private const byte CMD_ReadPresetTime = 0x43;
        private const byte CMD_ReadCountsPerSec = 0x44;
        private const byte CMD_ReadCountsPerMin = 0x45;
        private const byte CMD_ReadHighVoltage = 0x46;
        private const byte CMD_ReadAlarmSetPoint = 0x47;
        private const byte CMD_ReadIsCounting = 0x48;
        private const byte CMD_ReadSpeakerVolume = 0x49;
        private const byte CMD_ReadDisplaySelection = 0x4A;
        private const byte CMD_ReadAnalyserInfo = 0x4B;

        private const int DATALEN_CMD_Read = 5;

        /*
         * Single byte commands that return 1 byte which is an echo of the command
         */
        private const byte CMD_InterfaceNone = 0x50;
        private const byte CMD_InterfaceSerial = 0x51;
        private const byte CMD_InterfaceUsb = 0x52;

        private const int DATALEN_CMD_Interface = 1;

        /*
         * Two byte commands that return 2 bytes which is an echo of the command.
         * Write and read the first byte before writing and reading the second byte
         */
        private const byte CMD_SetHighVoltage = 0x80;
        private const byte CMD_SetAlarmRate = 0x82;
        private const byte CMD_SetPresetTime = 0x83;
        private const byte CMD_SetSpeakerVolume = 0x84;
        private const byte CMD_SetCPMRateDisplay = 0x86;
        private const byte CMD_SetCPSRateDisplay = 0x87;

        private const int DATALEN_CMD_Set = 1;

        /*
         * Single byte commands that return 1 byte which is an echo of the command
         */
        private const byte CMD_PushDisplaySelectSwitch = 0xDF;
        private const byte CMD_PushDownSwitch = 0xEF;
        private const byte CMD_PushUpSwitch = 0xF7;
        private const byte CMD_PushResetSwitch = 0xFB;
        private const byte CMD_PushStopSwitch = 0xFD;
        private const byte CMD_PushStartSwitch = 0xFE;

        private const int DATALEN_CMD_Push = 1;
        #endregion

        #region Variables
        private String serialport;
        private int baudrate;
        private SerialPort serialPort;
        private Thread threadReceive;
        private bool receiveRunning;
        //
        private byte[] responsePacket;
        private Object responseSignal;
        private ReceiveDataInfo receiveDataInfo;
        #endregion

        #region Types

        private class ReceiveDataInfo
        {
            public const int BUFFER_SIZE = 128;
            public byte[] receiveBuffer = new byte[BUFFER_SIZE];
            public int numBytesToRead = BUFFER_SIZE;
            public int bytesRead = 0;
            public int bufferIndex = 0;
            public int expectedPacketLength = -1;
        }
        #endregion

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="labEquipmentConfiguration"></param>
        public DeviceST360CounterSerial(LabEquipmentConfiguration labEquipmentConfiguration)
            : base(labEquipmentConfiguration)
        {
            const String methodName = "DeviceST360CounterSerial";
            Logfile.WriteCalled(Logfile.Level.Info, STR_ClassName, methodName);

            try
            {
                /*
                 * Serial port settings
                 */
                XmlNode xmlNode = XmlUtilities.GetChildNode(this.xmlNodeDevice, Consts.STRXML_Serial);
                this.serialport = XmlUtilities.GetChildValue(xmlNode, Consts.STRXML_Port);
                this.baudrate = XmlUtilities.GetChildValueAsInt(xmlNode, Consts.STRXML_Baud);

                /*
                 * Create the receive objects
                 */
                this.receiveDataInfo = new ReceiveDataInfo();
                this.responseSignal = new Object();

                /*
                 * Create an instance of the serial port, set read and write timeouts
                 */
                this.serialPort = new SerialPort(serialport, baudrate);
                this.serialPort.ReadTimeout = 1000;
                this.serialPort.WriteTimeout = 3000;

                Logfile.Write(String.Format(STRLOG_SerialPortBaudRate_arg2, serialport, baudrate));
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw ex;
            }

            Logfile.WriteCompleted(Logfile.Level.Info, STR_ClassName, methodName);
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
                 * Create an instance of the serial port, set read and write timeouts
                 */
                this.serialPort = new SerialPort(this.serialport, this.baudrate);
                this.serialPort.ReadTimeout = 1000;
                this.serialPort.WriteTimeout = 3000;

                /*
                 * Open the serial
                 */
                Logfile.Write(STRLOG_OpeningSerialPort);
                this.serialPort.Open();

                /*
                 * Create the receive thread and start it
                 */
                this.threadReceive = new Thread(new ThreadStart(this.ReceiveThread));
                if (this.threadReceive != null)
                {
                    this.threadReceive.Start();

                    /*
                     * Give it a chance to start running and then check that it has started
                     */
                    for (int i = 0; i < 5; i++)
                    {
                        if ((success = this.receiveRunning) == true)
                        {
                            break;
                        }

                        Delay.MilliSeconds(500);
                        Trace.WriteLine("!");
                    }

                    if (success == false)
                    {
                        throw new ApplicationException(STRERR_ReceiveThreadFailedToStart);
                    }
                }

                /*
                 * Set interface to Serial mode, retry if necessary
                 */
                for (int i = 0; i < 5; i++)
                {
                    if ((success = this.SetInterfaceMode(InterfaceMode.Serial)) == true)
                    {
                        break;
                    }

                    Delay.MilliSeconds(500);
                    Trace.Write('?');
                }

                /*
                 * Configure device
                 */
                if (this.Configure() == false)
                {
                    throw new ApplicationException(this.lastError);
                }

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

        public override double GetCaptureDataTime(int duration)
        {
            /*
             * Check that the duration is valid
             */
            this.GetPresetTime(duration);

            /*
             * Add in the delay to display the counts
             */
            double seconds = duration + (double)(DELAY_MS_Display) / 1000.0;

            /*
             * Do a time adjustment: y = Mx + C
             */
            seconds = (seconds * this.timeAdjustmentCapture[0]) + this.timeAdjustmentCapture[1];

            return seconds;
        }

        //-------------------------------------------------------------------------------------------------//

        public override bool CaptureData(int duration, out int counts)
        {
            const String methodName = "CaptureData";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Duration_arg, duration));

            bool success = false;

            counts = 0;

            try
            {
                /*
                 * Use timeout with retries
                 */
                int retries = 3;
                for (int i = 0; i < retries; i++)
                {
                    /*
                     * Set the duration
                     */
                    if (this.SetPresetTime(duration) == false)
                    {
                        throw new ApplicationException(this.lastError);
                    }

                    /*
                     * Reset the time and count before starting the counter
                     */
                    if (this.PushResetSwitch() == false)
                    {
                        throw new ApplicationException(this.lastError);
                    }

                    /*
                     * Set the display to counts so that we can see the progress
                     */
                    if (this.SetDisplaySelection(DisplaySelection.Counts) == false)
                    {
                        throw new ApplicationException(this.lastError);
                    }

                    /*
                     * Start the counter
                     */
                    if (this.StartCounter() == false)
                    {
                        throw new ApplicationException(this.lastError);
                    }

                    /*
                     * Set a timeout so that we don't wait forever if something goes wrong
                     */
                    int timeout = (duration + 5) * 1000 / DELAY_MS_IsCounting;
                    while (--timeout > 0)
                    {
                        /*
                         * Wait a bit and then check if still counting
                         */
                        Delay.MilliSeconds(DELAY_MS_IsCounting);

                        bool[] isCounting = new bool[1];
                        if (this.IsCounting(isCounting) == false)
                        {
                            throw new ApplicationException(this.lastError);
                        }
                        if (isCounting[0] == false)
                        {
                            break;
                        }
                    }
                    if (timeout == 0)
                    {
                        Logfile.WriteError(STRERR_CaptureTimedOut);

                        /*
                         * Stop the counter
                         */
                        if (this.StopCounter() == false)
                        {
                            throw new ApplicationException(this.lastError);
                        }

                        /*
                         * Retry
                         */
                        continue;
                    }

                    /*
                     * Display the counts for a moment
                     */
                    Thread.Sleep(DELAY_MS_Display);

                    /*
                     * Get the counts and check for error
                     */
                    if (this.GetCounts(out counts) == false)
                    {
                        throw new ApplicationException(this.lastError);
                    }

                    /*
                     * Data captured successfully
                     */
                    break;
                }

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Counts_arg, counts));

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool SetDisplaySelection(DisplaySelection displaySelection)
        {
            const String methodName = "SetDisplaySelection";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                String.Format(STRLOG_DisplaySelection_arg, Enum.GetName(typeof(DisplaySelection), displaySelection)));

            bool success = false;

            try
            {
                while (true)
                {
                    /*
                     * Get the current display selection
                     */
                    byte[] readData = WriteReadData(new byte[] { CMD_ReadDisplaySelection }, 1, DATALEN_CMD_Read);
                    if (readData == null || readData.Length != DATALEN_CMD_Read || readData[0] != CMD_ReadDisplaySelection)
                    {
                        throw new ApplicationException(STRERR_FailedToSetDisplaySelection);
                    }
                    DisplaySelection currentDisplaySelection = (DisplaySelection)readData[DATALEN_CMD_Read - 1];

                    /*
                     * Check if this is the desired display selection
                     */
                    if (currentDisplaySelection == displaySelection)
                    {
                        break;
                    }

                    /*
                     * Move the display selection down by one
                     */
                    readData = WriteReadData(new byte[] { CMD_PushDisplaySelectSwitch }, 1, DATALEN_CMD_Push);
                    if (readData == null || readData.Length != DATALEN_CMD_Push || readData[0] != CMD_PushDisplaySelectSwitch)
                    {
                        throw new ApplicationException(STRERR_FailedToPushDisplaySelectSwitch);
                    }
                }

                success = true;
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool SetHighVoltage(int highVoltage)
        {
            const String methodName = "SetHighVoltage";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                String.Format(STRLOG_HighVoltage_arg, highVoltage));

            bool success = false;

            try
            {
                //
                // Make sure high voltage is within range
                //
                if (highVoltage < MIN_HighVoltage && highVoltage > MAX_HighVoltage)
                {
                    throw new ArgumentOutOfRangeException("SetHighVoltage", "Not in range");
                }

                //
                // Determine value to write for desired high voltage 
                //
                byte highVoltageValue = (byte)(highVoltage / 5);

                //
                // Write the command
                //
                byte[] readData = WriteReadData(new byte[] { CMD_SetHighVoltage }, 1, DATALEN_CMD_Set);
                if (readData == null || readData.Length != DATALEN_CMD_Set || readData[0] != CMD_SetHighVoltage)
                {
                    throw new ApplicationException(STRERR_FailedToSetHighVoltage + highVoltage.ToString());
                }

                //
                // Write the high voltage value
                //
                readData = WriteReadData(new byte[] { highVoltageValue }, 1, DATALEN_CMD_Set);
                if (readData == null || readData.Length != DATALEN_CMD_Set || readData[0] != highVoltageValue)
                {
                    throw new ApplicationException(STRERR_FailedToSetHighVoltage + highVoltage.ToString());
                }

                //
                // Read the high voltage back
                //
                readData = WriteReadData(new byte[] { CMD_ReadHighVoltage }, 1, DATALEN_CMD_Read);
                if (readData == null || readData.Length != DATALEN_CMD_Read || readData[0] != CMD_ReadHighVoltage)
                {
                    throw new ApplicationException(STRERR_FailedToSetHighVoltage + highVoltage.ToString());
                }

                //
                // Extract high voltage value from byte array and compare
                //
                int readHighVoltage = 0;
                for (int i = 1; i < DATALEN_CMD_Read; i++)
                {
                    readHighVoltage = readHighVoltage * 256 + (int)readData[i];
                }
                if (readHighVoltage != highVoltage)
                {
                    throw new ApplicationException(STRERR_FailedToSetHighVoltage + highVoltage.ToString());
                }

                success = true;
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool SetSpeakerVolume(int speakerVolume)
        {
            const String methodName = "SetSpeakerVolume";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_SpeakerVolume_arg, speakerVolume));

            bool success = false;

            try
            {
                /*
                 * Make sure speaker volume is within range
                 */
                if (speakerVolume < MIN_SpeakerVolume && speakerVolume > MAX_SpeakerVolume)
                {
                    throw new ArgumentOutOfRangeException("SetSpeakerVolume", "Not in range");
                }

                /*
                 * Determine value to write for desired speaker volume
                 */
                byte speakerVolumeValue = (byte)speakerVolume;

                /*
                 * Write the command
                 */
                byte[] readData = WriteReadData(new byte[] { CMD_SetSpeakerVolume }, 1, DATALEN_CMD_Set);
                if (readData == null || readData.Length != DATALEN_CMD_Set || readData[0] != CMD_SetSpeakerVolume)
                {
                    throw new ApplicationException(STRERR_FailedToSetSpeakerVolume + speakerVolumeValue.ToString());
                }

                /*
                 * Write the speaker volume value
                 */
                readData = WriteReadData(new byte[] { speakerVolumeValue }, 1, DATALEN_CMD_Set);
                if (readData == null || readData.Length != DATALEN_CMD_Set || readData[0] != speakerVolumeValue)
                {
                    throw new ApplicationException(STRERR_FailedToSetSpeakerVolume + speakerVolumeValue.ToString());
                }

                /*
                 * Read the speaker volume back
                 */
                readData = WriteReadData(new byte[] { CMD_ReadSpeakerVolume }, 1, DATALEN_CMD_Read);
                if (readData == null || readData.Length != DATALEN_CMD_Read || readData[0] != CMD_ReadSpeakerVolume ||
                    readData[DATALEN_CMD_Read - 1] != speakerVolumeValue)
                {
                    throw new ApplicationException(STRERR_FailedToSetSpeakerVolume + speakerVolumeValue.ToString());
                }

                success = true;
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool SetPresetTime(int seconds)
        {
            const String methodName = "SetPresetTime";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                String.Format(STRLOG_PresetTime_arg, seconds));

            bool success = false;

            try
            {
                /*
                 * Determine value to write for desired preset time
                 */
                byte presetTime = (byte)this.GetPresetTime(seconds);

                /*
                 * Write the command
                 */
                byte[] readData = WriteReadData(new byte[] { CMD_SetPresetTime }, 1, DATALEN_CMD_Set);
                if (readData == null || readData.Length != DATALEN_CMD_Set || readData[0] != CMD_SetPresetTime)
                {
                    throw new Exception(STRERR_FailedToSetPresetTime + presetTime.ToString());
                }

                /*
                 * Write the preset time value
                 */
                readData = WriteReadData(new byte[] { presetTime }, 1, DATALEN_CMD_Set);
                if (readData == null || readData.Length != DATALEN_CMD_Set || readData[0] != presetTime)
                {
                    throw new Exception(STRERR_FailedToSetPresetTime + presetTime.ToString());
                }

                /*
                 * Read the preset time back
                 */
                readData = WriteReadData(new byte[] { CMD_ReadPresetTime }, 1, DATALEN_CMD_Read);
                if (readData == null || readData.Length != DATALEN_CMD_Read || readData[0] != CMD_ReadPresetTime ||
                    readData[DATALEN_CMD_Read - 1] != presetTime)
                {
                    throw new Exception(STRERR_FailedToSetPresetTime + presetTime.ToString());
                }

                success = true;
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool StartCounter()
        {
            const String methodName = "StartCounter";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;

            try
            {
                /*
                 * Write the command
                 */
                byte[] readData = WriteReadData(new byte[] { CMD_PushStartSwitch }, 1, DATALEN_CMD_Push);
                if (readData == null || readData.Length != DATALEN_CMD_Push || readData[0] != CMD_PushStartSwitch)
                {
                    throw new Exception(STRERR_FailedToPushStartSwitch);
                }

                success = true;
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool StopCounter()
        {
            const String methodName = "StopCounter";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;

            try
            {
                /*
                 * Write the command
                 */
                byte[] readData = WriteReadData(new byte[] { CMD_PushStopSwitch }, 1, DATALEN_CMD_Push);
                if (readData == null || readData.Length != DATALEN_CMD_Push || readData[0] != CMD_PushStopSwitch)
                {
                    throw new Exception(STRERR_FailedToPushStopSwitch);
                }

                success = true;
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool IsCounting(bool[] isCounting)
        {
            //const String methodName = "IsCounting";
            //Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;

            try
            {
                /*
                 * Write the command
                 */
                byte[] readData = WriteReadData(new byte[] { CMD_ReadIsCounting }, 1, DATALEN_CMD_Read);
                if (readData == null || readData.Length != DATALEN_CMD_Read || readData[0] != CMD_ReadIsCounting)
                {
                    throw new Exception(STRERR_FailedToReadCountingStatus);
                }

                isCounting[0] = (readData[DATALEN_CMD_Read - 1] != 0);

                success = true;
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
            }

            //String logMessage = STRLOG_Success + success.ToString() +
            //    Logfile.STRLOG_Spacer + STRLOG_IsCounting + isCounting[0].ToString();

            //Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool GetCounts(out int counts)
        {
            const String methodName = "GetCounts";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;

            counts = 0;

            try
            {
                /*
                 * Write the command
                 */
                byte[] readData = WriteReadData(new byte[] { CMD_ReadCounts }, 1, DATALEN_CMD_Read);
                if (readData == null || readData.Length != DATALEN_CMD_Read || readData[0] != CMD_ReadCounts)
                {
                    throw new Exception(STRERR_FailedToReadCounts);
                }

                /*
                 * Extract count value from byte array
                 */
                for (int i = 1; i < DATALEN_CMD_Read; i++)
                {
                    counts = counts * 256 + (int)readData[i];
                }

                success = true;
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Counts_arg, counts));

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool PushResetSwitch()
        {
            const String methodName = "PushResetSwitch";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;

            try
            {
                /*
                 * Write the command
                 */
                byte[] readData = WriteReadData(new byte[] { CMD_PushResetSwitch }, 1, DATALEN_CMD_Push);
                if (readData == null || readData.Length != DATALEN_CMD_Push || readData[0] != CMD_PushResetSwitch)
                {
                    throw new Exception(STRERR_FailedToPushResetSwitch);
                }

                success = true;
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //=================================================================================================//

        protected bool SetInterfaceMode(InterfaceMode interfaceMode)
        {
            const String methodName = "SetInterfaceMode";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                String.Format(STRLOG_InterfaceMode_arg, Enum.GetName(typeof(InterfaceMode), interfaceMode)));

            bool success = false;

            try
            {
                /*
                 * Write the command and get the received data, should be the command echoed back
                 */
                byte[] readData = WriteReadData(new byte[] { (byte)interfaceMode }, 1, DATALEN_CMD_Interface);
                if (readData == null || readData.Length != DATALEN_CMD_Interface || readData[0] != (byte)interfaceMode)
                {
                    throw new ApplicationException(STRERR_FailedToSetInterfaceMode);
                }

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                this.lastError = ex.Message;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        protected bool Configure()
        {
            const String methodName = "Configure";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;

            try
            {
                /*
                 * Display the high voltage and set it
                 */
                if (this.SetDisplaySelection(DisplaySelection.HighVoltage) == false)
                {
                    throw new ApplicationException(this.lastError);
                }
                if (this.SetHighVoltage(this.geigerTubeVoltage) == false)
                {
                    throw new ApplicationException(this.lastError);
                }
                Delay.MilliSeconds(DELAY_MS_Display);

                /*
                 * Display the speaker volume and set it
                 */
                if (this.SetDisplaySelection(DisplaySelection.SpeakerVolume) == false)
                {
                    throw new ApplicationException(this.lastError);
                }
                if (this.SetSpeakerVolume(this.speakerVolume) == false)
                {
                    throw new ApplicationException(this.lastError);
                }
                Delay.MilliSeconds(DELAY_MS_Display);

                /*
                 * Set display to counts and clear time and counts
                 */
                if (this.SetDisplaySelection(DisplaySelection.Counts) == false)
                {
                    throw new ApplicationException(this.lastError);
                }
                if (this.PushResetSwitch() == false)
                {
                    throw new ApplicationException(this.lastError);
                }

                success = true;
            }
            catch
            {
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //---------------------------------------------------------------------------------------//

        protected int GetPresetTime(int seconds)
        {
            int presetTime = 0;

            /*
             * Check that duration is within valid range
             */
            if (seconds < MIN_PresetTime)
            {
                throw new ArgumentException(String.Format(STRERR_DurationLessThanMinimum_arg2, seconds, MIN_PresetTime));
            }
            if (seconds > MAX_PresetTime)
            {
                throw new ArgumentException(String.Format(STRERR_DurationGreaterThanMaximum_arg2, seconds, MAX_PresetTime));
            }
            if (seconds >= 10 && seconds % 10 != 0)
            {
                throw new ArgumentException(String.Format(STRERR_DurationNotMultipleOf_arg2, seconds, (int)10));
            }
            if (seconds < 10)
            {
                /*
                 * The preset time is the same as the duration
                 */
                presetTime = seconds;
            }
            else
            {
                /*
                 * The preset time is a multiple of 10: 20 => 11, 30 => 12, ...
                 */
                presetTime = seconds / 10 + 9;
            }

            return presetTime;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios:
        /// 1. If disposing equals true, the method has been called directly or indirectly by a user's code.
        ///    Managed and unmanaged resources can be disposed.
        /// 2. If disposing equals false, the method has been called by the runtime from inside the finalizer
        ///    and you should not reference other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            const String methodName = "Dispose";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                String.Format(STRLOG_Dispose_arg2, disposing.ToString(), this.disposed.ToString()));

            /*
             * Check to see if Dispose has already been called
             */
            if (this.disposed == false)
            {
                /*
                 * If disposing is true, dispose all managed and unmanaged resources.
                 */
                if (disposing == true)
                {
                    /*
                     * Dispose managed resources here. Anything that has a Dispose() method.
                     * For example: component.Dispose();
                     */
                }

                /*
                 * Release unmanaged resources here. If disposing is false, only the following
                 * code is executed.
                 */

                /*
                 * Call base class before closing the serial port
                 */
                base.Dispose(disposing);

                /*
                 * Stop the receive thread
                 */
                if (this.receiveRunning == true)
                {
                    this.receiveRunning = false;
                    this.threadReceive.Join();
                }

                /*
                 * Close the serial port
                 */
                if (this.serialPort != null && this.serialPort.IsOpen)
                {
                    Logfile.Write(STRLOG_ClosingSerialPort);
                    this.serialPort.Close();
                }

                /*
                 * Note disposing has been done.
                 */
                this.disposed = true;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        /// <param name="dataLength"></param>
        /// <returns></returns>
        protected virtual bool SendData(byte[] data, int dataLength)
        {
            bool success = false;

            try
            {
                /*
                 * Write the data on the serial port
                 */
                this.serialPort.Write(data, 0, dataLength);

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        protected void ReceiveData(byte[] data, int dataLength)
        {
            ReceiveDataInfo rdi = this.receiveDataInfo;

            try
            {
                /*
                 * Copy all of the data to the receive buffer
                 */
                Array.Copy(data, 0, rdi.receiveBuffer, rdi.bufferIndex, dataLength);
                rdi.bytesRead += dataLength;
                rdi.bufferIndex += dataLength;

                /*
                 * Check if all of the expected data has been read
                 */
                if (rdi.bytesRead >= rdi.expectedPacketLength)
                {
                    /*
                     * The buffer has at least as many bytes for this packet
                     */
                    this.responsePacket = new byte[rdi.expectedPacketLength];
                    Array.Copy(rdi.receiveBuffer, 0, this.responsePacket, 0, rdi.expectedPacketLength);

                    /*
                     * Signal the waiting thread that the data has been received
                     */
                    lock (responseSignal)
                    {
                        Monitor.Pulse(responseSignal);
                    }
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }
        }

        //=================================================================================================//

        private byte[] WriteReadData(byte[] writeData, int writeCount, int readCount)
        {
            /*
             * Send the request and return the response
             */
            lock (responseSignal)
            {
                responsePacket = null;

                /*
                 * Initialise the receive data info
                 */
                ReceiveDataInfo rdi = this.receiveDataInfo;
                rdi.bytesRead = 0;
                rdi.bufferIndex = 0;
                rdi.expectedPacketLength = readCount;

                /*
                 * Write the data to the serial LCD
                 */
                this.SendData(writeData, writeCount);

                /*
                 * Wait for the response packet
                 */
                if (Monitor.Wait(responseSignal, MAXIMUM_MS_ResponseTime))
                {
                    return responsePacket;
                }
            }

            return null;
        }

        //-------------------------------------------------------------------------------------------------//

        private void ReceiveThread()
        {
            const String methodName = "ReceiveThread";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            const int BUFFER_SIZE = 128;
            byte[] receiveBuffer = new byte[BUFFER_SIZE];

            this.receiveRunning = true;
            while (this.receiveRunning == true)
            {
                try
                {
                    /*
                     * Read the data from the serial port
                     */
                    int bytesRead = this.serialPort.Read(receiveBuffer, 0, BUFFER_SIZE);
                    //Trace.WriteLine("ReceiveHandler: bytesRead=" + bytesRead.ToString());

                    /*
                     * Pass data on for processing
                     */
                    this.ReceiveData(receiveBuffer, bytesRead);
                }
                catch (TimeoutException)
                {
                }
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }
    }
}
