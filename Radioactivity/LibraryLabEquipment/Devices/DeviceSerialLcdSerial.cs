using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Xml;
using Library.Lab;
using Library.Lab.Utilities;
using Library.LabEquipment.Engine;

namespace Library.LabEquipment.Devices
{
    class DeviceSerialLcdSerial : DeviceSerialLcd
    {
        #region Constants
        private const String STR_ClassName = "DeviceSerialLcdSerial";
        private const Logfile.Level logLevel = Logfile.Level.Finer;
        /*
         * String constants for logfile messages
         */
        private const String STRLOG_SerialPort_arg2 = "SerialPort: {0:s} {1:d}";
        /*
         * String constants for exception messages
         */
        private const String STRERR_ReceiveThreadFailedToStart = "Receive thread failed to start!";
        private const String STRERR_ReportThreadFailedToStart = "Report thread failed to start!";

        /*
         * Constants
         */
        private const int MAX_DATA_LENGTH = 16;
        private const int MAX_PACKET_LENGTH = MAX_DATA_LENGTH + 4;
        private const int LINE_LENGTH = 16;
        private const int MAX_RESPONSE_TIME = 1000;

        /*
         * Command and report packet types
         */
        private const byte PKTRPT_KEY_ACTIVITY = 0x80;
        private const byte PKTRPT_FAN_SPEED = 0x81;
        private const byte PKTRPT_TEMP_SENSOR = 0x82;
        private const byte PKTRPT_CAPTURE_DATA = 0x83;
        #endregion

        #region Variables
        private String serialport;
        private int baudrate;
        private SerialPort serialPort;
        private Thread threadReceive;
        private bool receiveRunning;
        private LCDPacket responsePacket;
        private Object responseSignal;
        private Queue<LCDPacket> reportQueue;
        private Object reportSignal;
        private Thread threadReport;
        private bool reportRunning;
        private ReceiveDataInfo receiveDataInfo;
        #endregion

        #region Properties
        public new static String ClassName
        {
            get { return STR_ClassName; }
        }

        private LCDKey lcdKey;

        public LCDKey LcdKey
        {
            get { return lcdKey; }
        }
        #endregion

        #region Types
        public enum LCDKey
        {
            None = 0, Up = 1, Down = 2, Left = 3, Right = 4, Enter = 5, Exit = 6
        }

        public enum LCDPktType : byte
        {
            GetVersion = 1, WriteLine1 = 7, WriteLine2 = 8, SetCapture = 36
        }

        private class ReceiveDataInfo
        {
            public const int BUFFER_SIZE = 128;
            public byte[] receiveBuffer = new byte[BUFFER_SIZE];
            public int numBytesToRead = BUFFER_SIZE;
            public int bytesRead = 0;
            public int bufferIndex = 0;
            public int startPacketIndex = 0;
            public int expectedPacketLength = -1;
            public bool expectedPacketLengthIsSet = false;
        }
        #endregion

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="labEquipmentConfiguration"></param>
        public DeviceSerialLcdSerial(LabEquipmentConfiguration labEquipmentConfiguration)
            : base(labEquipmentConfiguration)
        {
            const String methodName = "DeviceSerialLcdSerial";
            Logfile.WriteCalled(Logfile.Level.Info, STR_ClassName, methodName);

            try
            {
                /*
                 * Serial port settings
                 */
                XmlNode xmlNode = XmlUtilities.GetChildNode(this.xmlNodeDevice, Consts.STRXML_Serial);
                this.serialport = XmlUtilities.GetChildValue(xmlNode, Consts.STRXML_Port);
                this.baudrate = XmlUtilities.GetChildValueAsInt(xmlNode, Consts.STRXML_Baud);

                Logfile.Write(Logfile.Level.Config, String.Format(STRLOG_SerialPort_arg2, serialport, baudrate));

                /*
                 * Create the receive and report objects
                 */
                this.receiveDataInfo = new ReceiveDataInfo();
                this.reportQueue = new Queue<LCDPacket>();
                this.reportSignal = new Object();
                this.responseSignal = new Object();
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
                 * Create the report thread and start it
                 */
                this.threadReport = new Thread(new ThreadStart(this.ReportThread));
                if (this.threadReport != null)
                {
                    this.threadReport.Start();

                    /*
                     * Give it a chance to start running and then check that it has started
                     */
                    for (int i = 0; i < 5; i++)
                    {
                        if ((success = this.reportRunning) == true)
                        {
                            break;
                        }

                        Delay.MilliSeconds(500);
                        Trace.WriteLine("!");
                    }

                    if (success == false)
                    {
                        throw new ApplicationException(STRERR_ReportThreadFailedToStart);
                    }
                }

                /*
                 * Get the firmware version and display
                 */
                String firmwareVersion = this.GetHardwareFirmwareVersion();
                this.WriteLine(LineNumber.One, methodName);
                this.WriteLine(LineNumber.Two, firmwareVersion);

                /*
                 * Ensure data capture is stopped
                 */
                this.StopCapture();

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
            byte[] version = this.SendReturnData((byte)LCDPktType.GetVersion, null, 0);

            if (version != null)
            {
                return Encoding.ASCII.GetString(version);
            }

            return null;
        }

        //---------------------------------------------------------------------------------------//

        public override bool WriteLine(LineNumber lineno, String message)
        {
            /*
             * Write the message to the logfile
             */
            base.WriteLine(lineno, message);

            /*
             * Pad out the message the the line length and convert to a byte array
             */
            message = this.CreateStringOfLength(message, LINE_LENGTH);
            byte[] data = Encoding.ASCII.GetBytes(message.ToCharArray());

            switch (lineno)
            {
                case LineNumber.One:
                    return this.SendReturnBool((byte)LCDPktType.WriteLine1, data, data.Length);

                case LineNumber.Two:
                    return this.SendReturnBool((byte)LCDPktType.WriteLine2, data, data.Length);
            }

            return false;
        }

        //---------------------------------------------------------------------------------------//

        public override bool StartCapture(int seconds)
        {
            byte[] data = new byte[1] { (byte)seconds };

            this.captureCompleted = false;
            this.captureCount = 0;
            return this.SendReturnBool((byte)LCDPktType.SetCapture, data, data.Length);
        }

        //---------------------------------------------------------------------------------------//

        public override bool StopCapture()
        {
            byte[] data = new byte[1] { 0 };

            return this.SendReturnBool((byte)LCDPktType.SetCapture, data, data.Length);
        }

        //=================================================================================================//

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
                 * Stop the report thread
                 */
                if (this.reportRunning == true)
                {
                    this.reportRunning = false;
                    this.threadReport.Join();
                }

                /*
                 * Close the serial port
                 */
                if (this.serialPort != null && this.serialPort.IsOpen)
                {
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

        protected virtual bool SendData(byte[] data, int dataLength)
        {
            bool success = false;

            try
            {
                /*
                 * Write the data to the serial LCD on the serial port
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

        //---------------------------------------------------------------------------------------//

        protected void ReceiveData(byte[] data, int dataLength)
        {
            ReceiveDataInfo rdi = this.receiveDataInfo;
            int bytesReceived;

            while (dataLength > 0)
            {
                //
                // Copy the data into the receive buffer
                //
                if (dataLength <= rdi.numBytesToRead)
                {
                    //
                    // Copy all of the data to the receive buffer
                    //
                    Array.Copy(data, 0, rdi.receiveBuffer, rdi.bufferIndex, dataLength);
                    bytesReceived = dataLength;
                }
                else
                {
                    //
                    // Copy only some of the data to the receive buffer
                    //
                    Array.Copy(data, 0, rdi.receiveBuffer, rdi.bufferIndex, rdi.numBytesToRead);
                    bytesReceived = rdi.numBytesToRead;
                }
                dataLength -= bytesReceived;

                //
                // Check if dataLength for the packet has been determined
                //
                if (rdi.expectedPacketLengthIsSet || rdi.bytesRead <= 1)
                {
                    //
                    // This covers the case when more than 1 entire packet has been read in at a time
                    //
                    rdi.bytesRead += bytesReceived;
                    rdi.bufferIndex = rdi.startPacketIndex + rdi.bytesRead;
                }

                if (rdi.bytesRead > 1)
                {
                    //
                    // The buffer has the dataLength for the packet
                    //
                    if (rdi.expectedPacketLengthIsSet == false)
                    {
                        rdi.expectedPacketLength = rdi.receiveBuffer[(1 + rdi.startPacketIndex) % rdi.receiveBuffer.Length] + 4;
                        rdi.expectedPacketLengthIsSet = true;
                    }

                    if (rdi.bytesRead >= rdi.expectedPacketLength)
                    {
                        //
                        // The buffer has at least as many bytes for this packet
                        //
                        AddPacket(rdi.receiveBuffer, rdi.startPacketIndex);

                        rdi.expectedPacketLengthIsSet = false;
                        if (rdi.bytesRead == rdi.expectedPacketLength)
                        {
                            //
                            // The buffer contains only the bytes for this packet
                            //
                            rdi.bytesRead = 0;
                            rdi.bufferIndex = rdi.startPacketIndex;
                        }
                        else
                        {
                            //
                            // The buffer also has bytes for the next packet
                            //
                            rdi.startPacketIndex += rdi.expectedPacketLength;
                            rdi.startPacketIndex %= rdi.receiveBuffer.Length;
                            rdi.bytesRead -= rdi.expectedPacketLength;
                            rdi.bufferIndex = rdi.startPacketIndex + rdi.bytesRead;
                        }
                    }
                }

                //
                // Update buffer and remaining space, wrapping around end of buffer if necessary
                //
                rdi.bufferIndex %= rdi.receiveBuffer.Length;
                if (rdi.bufferIndex < rdi.startPacketIndex)
                {
                    rdi.numBytesToRead = rdi.startPacketIndex - rdi.bufferIndex;
                }
                else
                {
                    rdi.numBytesToRead = rdi.receiveBuffer.Length - rdi.bufferIndex;
                }
            }
        }

        //=================================================================================================//

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
                    int bytesRead = this.serialPort.Read(receiveBuffer, 0, receiveBuffer.Length);

                    Trace.WriteLine(String.Format("{0:s}: bytesRead={1:d}", methodName, bytesRead));

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

        //-------------------------------------------------------------------------------------------------//

        private String CreateStringOfLength(String s, int length)
        {
            if (s == null)
            {
                s = String.Empty;
            }
            if (length < s.Length)
            {
                s = s.Substring(0, length);
            }
            else if (length > s.Length)
            {
                s = s + (new String(' ', length - s.Length));
            }

            return s;
        }

        //-------------------------------------------------------------------------------------------------//

        private bool SendReturnBool(byte type, byte[] data, int dataLength)
        {
            LCDPacket packet = Send(type, data, dataLength);

            if (packet != null)
            {
                return (type == (packet.Type & ~0xC0) &&
                    responsePacket.PacketType == LCDPacket.LCDPacketType.NORMAL_RESPONSE);
            }
            else
            {
                return false;
            }
        }

        //-------------------------------------------------------------------------------------------------//

        private byte[] SendReturnData(byte type, byte[] data, int dataLength)
        {
            LCDPacket packet = Send(type, data, dataLength);

            if (packet != null)
            {
                return packet.Data;
            }
            else
            {
                return null;
            }
        }

        //-------------------------------------------------------------------------------------------------//

        private LCDPacket Send(byte packetType, byte[] data, int dataLength)
        {
            ushort crc;

            if ((data == null && dataLength != 0) ||
                (data != null && dataLength > data.Length))
            {
                throw new ArgumentException("bad data sent to Send");
            }

            //
            // Create packet buffer and enter header information
            //
            byte[] packetBuffer = new byte[dataLength + 4];
            packetBuffer[0] = packetType;
            packetBuffer[1] = (byte)dataLength;

            //
            // Enter data if there is any
            //
            if (dataLength != 0)
            {
                Array.Copy(data, 0, packetBuffer, 2, dataLength);
            }

            //
            // Calculate and enter the checksum into the last two bytes of the packet
            //
            crc = CRCGenerator.GenerateCRC(packetBuffer, dataLength + 2, CRCGenerator.CRC_SEED);
            packetBuffer[2 + dataLength + 1] = (byte)(crc >> 8);
            packetBuffer[2 + dataLength] = (byte)crc;

            //
            // Send the request and return the response
            //
            lock (responseSignal)
            {
                responsePacket = null;

                // Write the packet to the serial LCD
                this.SendData(packetBuffer, packetBuffer.Length);

                // Wait for the response packet
                if (Monitor.Wait(responseSignal, MAX_RESPONSE_TIME))
                {
                    return responsePacket;
                }
            }

            return null;
        }

        //---------------------------------------------------------------------------------------//

        private void ReportThread()
        {
            const String methodName = "ReportThread";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            try
            {
                this.reportRunning = true;
                while (this.reportRunning == true)
                {
                    lock (this.reportSignal)
                    {
                        if (Monitor.Wait(this.reportSignal, 1000) == false)
                        {
                            /*
                             * Signal not received before timout
                             */
                            continue;
                        }

                        Trace.WriteLine(String.Format("{0:s}: ReportQueue.Count={1:d}", methodName, reportQueue.Count));
                    }

                    /*
                     * Get packets from the queue and process
                     */
                    while (reportQueue.Count > 0)
                    {
                        LCDPacket packet = reportQueue.Dequeue();

                        switch (packet.Type)
                        {
                            case PKTRPT_CAPTURE_DATA:

                                int value = packet.Data[1];
                                value = (value << 8) + packet.Data[0];
                                this.captureCount = value;
                                this.captureCompleted = true;

                                Trace.WriteLine(String.Format("CaptureData={0:d}", value));
                                break;

                            case PKTRPT_KEY_ACTIVITY:

                                if (packet.DataLength == 1)
                                {
                                    switch (packet.Data[0])
                                    {
                                        case 5:
                                            this.lcdKey = LCDKey.Enter;
                                            break;

                                        case 6:
                                            this.lcdKey = LCDKey.Exit;
                                            break;
                                    }
                                }
                                break;

                            case PKTRPT_TEMP_SENSOR:
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }


            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //---------------------------------------------------------------------------------------//

        #region Packet Handling Routines

        private LCDPacket CreatePacket(byte[] buffer, int startIndex)
        {
            byte packetType = buffer[startIndex];
            byte dataLength = buffer[(startIndex + 1) % buffer.Length];
            byte[] data = new byte[dataLength];
            ushort crc = 0;

            for (int i = 0; i < dataLength; i++)
            {
                data[i] = buffer[(startIndex + 2 + i) % buffer.Length];
            }

            crc |= (ushort)buffer[(startIndex + 2 + dataLength) % buffer.Length];
            crc |= (ushort)(buffer[(startIndex + 2 + dataLength + 1) % buffer.Length] << 8);

            return new LCDPacket(packetType, dataLength, data, crc);
        }

        //---------------------------------------------------------------------------------------//

        protected bool AddPacket(byte[] buffer, int startIndex)
        {
            LCDPacket packet = CreatePacket(buffer, startIndex);
            ushort calculatedCRC = CRCGenerator.GenerateCRC(buffer, startIndex, packet.DataLength + 2, CRCGenerator.CRC_SEED);

            Trace.WriteLine("AddPacket(): " + packet.PacketType.ToString());

            switch (packet.PacketType)
            {
                case LCDPacket.LCDPacketType.NORMAL_RESPONSE:
                    AddResponsePacket(packet);
                    break;

                case LCDPacket.LCDPacketType.NORMAL_REPORT:
                    AddReportPacket(packet);
                    break;

                case LCDPacket.LCDPacketType.ERROR_RESPONSE:
                    AddResponsePacket(packet);
                    break;
            }

            if (calculatedCRC != packet.CRC)
            {
                Trace.WriteLine("CRC ERROR!:" +
                    " Calculated CRC = 0x" + Convert.ToString(calculatedCRC, 16) +
                    " Packet CRC = 0x" + Convert.ToString(packet.CRC, 16)
                    );

                return false;
            }

            return true;
        }

        //---------------------------------------------------------------------------------------//

        private void AddResponsePacket(LCDPacket packet)
        {
            lock (responseSignal)
            {
                responsePacket = packet;
                Monitor.Pulse(responseSignal);
            }
        }

        //---------------------------------------------------------------------------------------//

        private void AddReportPacket(LCDPacket packet)
        {
            lock (reportSignal)
            {
                reportQueue.Enqueue(packet);
                Monitor.Pulse(reportSignal);
            }
        }

        #endregion

        //---------------------------------------------------------------------------------------//

        #region LCDPacket Class

        protected class LCDPacket
        {
            public enum LCDPacketType
            {
                NORMAL_RESPONSE,
                NORMAL_REPORT,
                ERROR_RESPONSE
            };

            private const byte NORMAL_RESPONSE = 0x40;
            private const byte NORMAL_REPORT = 0x80;
            private const byte ERROR_RESPONSE = 0xC0;

            private byte type;
            private byte dataLength;
            private byte[] data;
            private ushort crc;

            public LCDPacket(byte type, byte dataLength, byte[] data)
            {
                this.type = type;
                this.dataLength = dataLength;
                this.data = data;
            }

            public LCDPacket(byte type, byte dataLength, byte[] data, ushort crc)
            {
                this.type = type;
                this.dataLength = dataLength;
                this.data = data;
                this.crc = crc;
            }

            public override String ToString()
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                sb.Append("Type: " + Convert.ToString(type, 16) + "\n");
                sb.Append("DataLength: " + Convert.ToString(dataLength, 16) + "\n");
                sb.Append("Data: ");
                for (int i = 0; i < dataLength; i++)
                {
                    sb.Append(Convert.ToString(data[i], 16) + ", ");
                }

                sb.Append("\n");
                sb.Append("CRC: " + Convert.ToString(crc, 16) + "\n");
                return sb.ToString();
            }

            public byte[] ToByteArray()
            {
                return null;
            }

            public byte Type
            {
                get { return type; }
            }

            public LCDPacketType PacketType
            {
                get
                {
                    switch (type & 0xC0)
                    {
                        case NORMAL_RESPONSE:
                            return LCDPacketType.NORMAL_RESPONSE;

                        case NORMAL_REPORT:
                            return LCDPacketType.NORMAL_REPORT;

                        case ERROR_RESPONSE:
                            return LCDPacketType.ERROR_RESPONSE;

                        default:
                            //throw new InvalidOperationException("Unexpected Packet Type: " +
                            //    System.Convert.ToString(type, 16));
                            return LCDPacketType.ERROR_RESPONSE;
                    }
                }
            }

            public byte DataLength
            {
                get { return dataLength; }
            }
            public byte[] Data
            {
                get { return data; }
            }
            public ushort CRC
            {
                get { return crc; }
            }
        }

        #endregion

        //---------------------------------------------------------------------------------------//

        #region CRCGenerator Class

        private static class CRCGenerator
        {
            public const ushort CRC_SEED = 0xFFFF;

            //
            // CRC lookup table to avoid bit-shifting loops
            //
            static ushort[] crcLookupTable = {
			    0x00000, 0x01189, 0x02312, 0x0329B, 0x04624, 0x057AD, 0x06536, 0x074BF,
			    0x08C48, 0x09DC1, 0x0AF5A, 0x0BED3, 0x0CA6C, 0x0DBE5, 0x0E97E, 0x0F8F7,
			    0x01081, 0x00108, 0x03393, 0x0221A, 0x056A5, 0x0472C, 0x075B7, 0x0643E,
			    0x09CC9, 0x08D40, 0x0BFDB, 0x0AE52, 0x0DAED, 0x0CB64, 0x0F9FF, 0x0E876,
			    0x02102, 0x0308B, 0x00210, 0x01399, 0x06726, 0x076AF, 0x04434, 0x055BD,
			    0x0AD4A, 0x0BCC3, 0x08E58, 0x09FD1, 0x0EB6E, 0x0FAE7, 0x0C87C, 0x0D9F5,
			    0x03183, 0x0200A, 0x01291, 0x00318, 0x077A7, 0x0662E, 0x054B5, 0x0453C,
			    0x0BDCB, 0x0AC42, 0x09ED9, 0x08F50, 0x0FBEF, 0x0EA66, 0x0D8FD, 0x0C974,
			    0x04204, 0x0538D, 0x06116, 0x0709F, 0x00420, 0x015A9, 0x02732, 0x036BB,
			    0x0CE4C, 0x0DFC5, 0x0ED5E, 0x0FCD7, 0x08868, 0x099E1, 0x0AB7A, 0x0BAF3,
			    0x05285, 0x0430C, 0x07197, 0x0601E, 0x014A1, 0x00528, 0x037B3, 0x0263A,
			    0x0DECD, 0x0CF44, 0x0FDDF, 0x0EC56, 0x098E9, 0x08960, 0x0BBFB, 0x0AA72,
			    0x06306, 0x0728F, 0x04014, 0x0519D, 0x02522, 0x034AB, 0x00630, 0x017B9,
			    0x0EF4E, 0x0FEC7, 0x0CC5C, 0x0DDD5, 0x0A96A, 0x0B8E3, 0x08A78, 0x09BF1,
			    0x07387, 0x0620E, 0x05095, 0x0411C, 0x035A3, 0x0242A, 0x016B1, 0x00738,
			    0x0FFCF, 0x0EE46, 0x0DCDD, 0x0CD54, 0x0B9EB, 0x0A862, 0x09AF9, 0x08B70,
			    0x08408, 0x09581, 0x0A71A, 0x0B693, 0x0C22C, 0x0D3A5, 0x0E13E, 0x0F0B7,
			    0x00840, 0x019C9, 0x02B52, 0x03ADB, 0x04E64, 0x05FED, 0x06D76, 0x07CFF,
			    0x09489, 0x08500, 0x0B79B, 0x0A612, 0x0D2AD, 0x0C324, 0x0F1BF, 0x0E036,
			    0x018C1, 0x00948, 0x03BD3, 0x02A5A, 0x05EE5, 0x04F6C, 0x07DF7, 0x06C7E,
			    0x0A50A, 0x0B483, 0x08618, 0x09791, 0x0E32E, 0x0F2A7, 0x0C03C, 0x0D1B5,
			    0x02942, 0x038CB, 0x00A50, 0x01BD9, 0x06F66, 0x07EEF, 0x04C74, 0x05DFD,
			    0x0B58B, 0x0A402, 0x09699, 0x08710, 0x0F3AF, 0x0E226, 0x0D0BD, 0x0C134,
			    0x039C3, 0x0284A, 0x01AD1, 0x00B58, 0x07FE7, 0x06E6E, 0x05CF5, 0x04D7C,
			    0x0C60C, 0x0D785, 0x0E51E, 0x0F497, 0x08028, 0x091A1, 0x0A33A, 0x0B2B3,
			    0x04A44, 0x05BCD, 0x06956, 0x078DF, 0x00C60, 0x01DE9, 0x02F72, 0x03EFB,
			    0x0D68D, 0x0C704, 0x0F59F, 0x0E416, 0x090A9, 0x08120, 0x0B3BB, 0x0A232,
			    0x05AC5, 0x04B4C, 0x079D7, 0x0685E, 0x01CE1, 0x00D68, 0x03FF3, 0x02E7A,
			    0x0E70E, 0x0F687, 0x0C41C, 0x0D595, 0x0A12A, 0x0B0A3, 0x08238, 0x093B1,
			    0x06B46, 0x07ACF, 0x04854, 0x059DD, 0x02D62, 0x03CEB, 0x00E70, 0x01FF9,
			    0x0F78F, 0x0E606, 0x0D49D, 0x0C514, 0x0B1AB, 0x0A022, 0x092B9, 0x08330,
			    0x07BC7, 0x06A4E, 0x058D5, 0x0495C, 0x03DE3, 0x02C6A, 0x01EF1, 0x00F78
		    };

            public static ushort GenerateCRC(byte[] data, int dataLength, ushort seed)
            {
                ushort newCrc;

                newCrc = seed;
                for (int i = 0; i < dataLength; i++)
                {
                    newCrc = (ushort)((newCrc >> 8) ^ crcLookupTable[(newCrc ^ data[i]) & 0xff]);
                }

                return ((ushort)~newCrc);
            }

            public static ushort GenerateCRC(byte[] data, int startIndex, int length, ushort seed)
            {
                ushort newCrc;

                newCrc = seed;
                for (int i = 0; i < length; i++)
                {
                    newCrc = (ushort)((newCrc >> 8) ^ crcLookupTable[(newCrc ^ data[(startIndex + i) % data.Length]) & 0xff]);
                }

                return ((ushort)~newCrc);
            }
        }

        #endregion

    }
}
