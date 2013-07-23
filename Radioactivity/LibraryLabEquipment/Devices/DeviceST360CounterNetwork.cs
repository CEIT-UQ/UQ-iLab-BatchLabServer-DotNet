using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using Library.Lab;
using Library.Lab.Utilities;
using Library.LabEquipment.Engine;

namespace Library.LabEquipment.Devices
{
    public class DeviceST360CounterNetwork : DeviceST360CounterSerial
    {
        #region Constants
        private const String STR_ClassName = "DeviceST360CounterNetwork";
        private const Logfile.Level logLevel = Logfile.Level.Finer;
        /*
         * String constants for logfile messages
         */
        private const String STRLOG_IPaddrPort_arg2 = "IPaddr: {0:s} - Port: {1:d}";
        private const String STRLOG_OpeningTcpClientConnection = "Opening TCP client connection...";
        private const String STRLOG_ClosingTcpClientConnection = "Closing TCP client connection...";
        #endregion

        #region Variables
        private IPAddress ipAddress;
        private int port;
        private TcpClient tcpClient;
        private AsyncObject asyncObject;
        #endregion

        #region Properties
        public new static String ClassName
        {
            get { return STR_ClassName; }
        }
        #endregion

        #region Types
        private class AsyncObject
        {
            public const int BUFFER_SIZE = 128;

            public NetworkStream tcpClientStream = null;
            public byte[] receiveBuffer = new byte[BUFFER_SIZE];
        }
        #endregion

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="labEquipmentConfiguration"></param>
        public DeviceST360CounterNetwork(LabEquipmentConfiguration labEquipmentConfiguration)
            : base(labEquipmentConfiguration)
        {
            const String methodName = "DeviceST360CounterNetwork";
            Logfile.WriteCalled(Logfile.Level.Info, STR_ClassName, methodName);

            try
            {
                /*
                 * Network settings
                 */
                XmlNode xmlNode = XmlUtilities.GetChildNode(this.xmlNodeDevice, Consts.STRXML_Network);
                String ipAddr = XmlUtilities.GetChildValue(xmlNode, Consts.STRXML_IPaddr);
                this.port = XmlUtilities.GetChildValueAsInt(xmlNode, Consts.STRXML_Port);
                this.ipAddress = IPAddress.Parse(ipAddr);

                Logfile.Write(String.Format(STRLOG_IPaddrPort_arg2, this.ipAddress.ToString(), this.port));
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
                this.disposed = false;

                /*
                 * Open a connection to the specified IP address and port
                 */
                Logfile.Write(STRLOG_OpeningTcpClientConnection);
                this.tcpClient = new TcpClient();
                this.tcpClient.Connect(this.ipAddress, this.port);

                /*
                 * Create async object for TcpClient receive callback
                 */
                this.asyncObject = new AsyncObject();
                this.asyncObject.tcpClientStream = this.tcpClient.GetStream();

                /*
                 * Begin receiving data from the network
                 */
                this.asyncObject.tcpClientStream.BeginRead(asyncObject.receiveBuffer, 0, AsyncObject.BUFFER_SIZE,
                    new AsyncCallback(TcpClientReceiveCallback), asyncObject);

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
                 * Close the TCP client
                 */
                if (this.tcpClient != null)
                {
                    /*
                     * Close the network associated with the TCP client
                     */
                    try
                    {
                        NetworkStream networkStream = this.tcpClient.GetStream();
                        if (networkStream != null)
                        {
                            networkStream.Close();
                        }
                    }
                    catch
                    {
                    }

                    /*
                     * Close the TCP client
                     */
                    Logfile.Write(STRLOG_ClosingTcpClientConnection);
                    this.tcpClient.Close();
                    this.tcpClient = null;
                }

                /*
                 * Note disposing has been done.
                 */
                this.disposed = true;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        protected override bool SendData(byte[] data, int dataLength)
        {
            bool success = false;

            try
            {
                /*
                 * Write the data on the network
                 */
                this.tcpClient.GetStream().Write(data, 0, dataLength);

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            return success;
        }

        //=================================================================================================//

        private void TcpClientReceiveCallback(IAsyncResult asyncResult)
        {
            /*
             * Retrieve the state object
             */
            AsyncObject asyncStateObject = (AsyncObject)asyncResult.AsyncState;

            try
            {
                int bytesRead;

                /*
                 * Read the data from the TcpClient
                 */
                if ((bytesRead = asyncStateObject.tcpClientStream.EndRead(asyncResult)) == 0)
                {
                    throw new IOException();
                }

                //Trace.WriteLine(String.Format("TcpClientReceiveCallback: bytesRead={0:d}", bytesRead));
                Trace.Write(".");

                /*
                 * Pass data on for processing
                 */
                this.ReceiveData(asyncStateObject.receiveBuffer, bytesRead);

                /*
                 * Begin receiving more data from the tcpClient
                 */
                asyncStateObject.tcpClientStream.BeginRead(asyncStateObject.receiveBuffer, 0, AsyncObject.BUFFER_SIZE,
                    new AsyncCallback(TcpClientReceiveCallback), this.asyncObject);
            }
            catch (Exception)
            {
                Trace.WriteLine("TcpClient closed connection");
                asyncStateObject.tcpClientStream = null;
            }
        }

    }
}
