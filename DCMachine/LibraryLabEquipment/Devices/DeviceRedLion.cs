using System;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using Library.Lab;
using Library.LabEquipment.Engine;
using Library.LabEquipment.Engine.Devices;
using Modbus.Device;

namespace Library.LabEquipment.Devices
{
    public class DeviceRedLion : DeviceGeneric
    {
        #region Constants
        private const string STR_ClassName = "DeviceRedLion";
        private const Logfile.Level logLevel = Logfile.Level.Finer;
        /*
         * String constants for logfile messages
         */
        private const string STRLOG_IPaddrPort_arg2 = " IPaddr: {0:s}:{1:d}";
        private const string STRLOG_ReceiveTimout_arg = " Receive Timout: {0:d} millisecs";
        private const string STRLOG_ModbusSlaveId_arg = " Modbus Slave Id: {0:d}";
        private const string STRLOG_CheckSpeedEnabled_arg = " CheckSpeed Enabled: {0:s}";
        /*
         * String constants for exception messages
         */
        private const string STRERR_FailedToOpenNetworkConnection_arg2 = "Failed to open network connection: {0:s}:{1:d}";
        #endregion

        #region Variables
        private IPAddress ipAddress;
        private int port;
        private TcpClient tcpClient;
        private int receiveTimeout;
        private int slaveId;
        #endregion

        #region Properties
        public new static string ClassName
        {
            get { return STR_ClassName; }
        }

        private ACDrive acDrive;
        private DCDrive dcDrive;
        private bool checkSpeedEnabled;

        public ACDrive GetACDrive
        {
            get { return acDrive; }
        }

        public DCDrive GetDCDrive
        {
            get { return dcDrive; }
        }

        public bool CheckSpeedEnabled
        {
            get { return checkSpeedEnabled; }
            set { checkSpeedEnabled = value; }
        }
        #endregion

        #region Types

        public delegate void KeepAliveCallback();

        public struct Measurements
        {
            public float voltageMut;
            public float currentMut;
            public float powerFactorMut;
            public float currentVsd;
            public float voltageVsd;
            public float powerFactorVsd;
            public int speed;
            public int torque;
        }
        #endregion

        //-----------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="labEquipmentConfiguration"></param>
        public DeviceRedLion(LabEquipmentConfiguration labEquipmentConfiguration)
            : base(labEquipmentConfiguration, STR_ClassName)
        {
            const string methodName = "DeviceRedLion";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            try
            {
                string logMessage = Logfile.STRLOG_Newline;

                /*
                 * Get the IP address and port number to use
                 */
                XmlNode xmlNodeNetwork = XmlUtilities.GetChildNode(this.xmlNodeDevice, Consts.STRXML_Network);
                string ipaddr = XmlUtilities.GetChildValue(xmlNodeNetwork, Consts.STRXML_IPaddr, false);
                this.ipAddress = IPAddress.Parse(ipaddr);
                this.port = XmlUtilities.GetChildValueAsInt(xmlNodeNetwork, Consts.STRXML_Port);
                logMessage += String.Format(STRLOG_IPaddrPort_arg2, this.ipAddress.ToString(), this.port) + Logfile.STRLOG_Newline;

                /*
                 * Get the network timeouts
                 */
                XmlNode xmlNodeTimeouts = XmlUtilities.GetChildNode(xmlNodeNetwork, Consts.STRXML_Timeouts);
                this.receiveTimeout = XmlUtilities.GetChildValueAsInt(xmlNodeTimeouts, Consts.STRXML_Receive);
                logMessage += String.Format(STRLOG_ReceiveTimout_arg, this.receiveTimeout) + Logfile.STRLOG_Newline;

                /*
                 * Get Modbus slave identity
                 */
                XmlNode xmlNodeModbus = XmlUtilities.GetChildNode(this.xmlNodeDevice, Consts.STRXML_Modbus, false);
                this.slaveId = XmlUtilities.GetChildValueAsInt(xmlNodeModbus, Consts.STRXML_SlaveId);
                logMessage += String.Format(STRLOG_ModbusSlaveId_arg, this.slaveId) + Logfile.STRLOG_Newline;

                /*
                 * Get status checking
                 */
                XmlNode xmlNodeStatusCheck = XmlUtilities.GetChildNode(this.xmlNodeDevice, Consts.STRXML_StatusCheck, false);
                this.checkSpeedEnabled = XmlUtilities.GetChildValueAsBool(xmlNodeStatusCheck, Consts.STRXML_StatusCheckSpeed);
                logMessage += String.Format(STRLOG_CheckSpeedEnabled_arg, this.checkSpeedEnabled.ToString());

                /*
                 * Log the configuration information
                 */
                Logfile.Write(Logfile.Level.Info, logMessage);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw ex;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-----------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>bool</returns>
        public override bool Initialise()
        {
            const string methodName = "Initialise";
            Logfile.WriteCalled(Logfile.Level.Info, STR_ClassName, methodName);

            bool success = false;

            try
            {
                this.disposed = false;

                if (this.OpenConnection() == false)
                {
                    throw new ApplicationException(this.lastError);
                }

                try
                {
                    if (this.initialiseEnabled == true)
                    {
                        if (this.acDrive.Initialise() == false)
                        {
                            throw new ApplicationException(this.acDrive.LastError);
                        }
                        if (this.dcDrive.Initialise() == false)
                        {
                            throw new ApplicationException(this.dcDrive.LastError);
                        }
                    }
                }
                finally
                {
                    this.CloseConnection();
                }

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                this.lastError = ex.Message;
            }

            Logfile.WriteCompleted(Logfile.Level.Info, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>bool</returns>
        public bool OpenConnection()
        {
            const string methodName = "OpenConnection";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;

            try
            {
                this.lastError = null;

                /*
                 * Open a connection to the specified IP address and port
                 */
                this.tcpClient = new TcpClient();
                this.tcpClient.ReceiveTimeout = this.receiveTimeout;
                this.tcpClient.Connect(this.ipAddress, this.port);

                /*
                 * Create instances of the devices at this IP address
                 */
                ModbusIpMaster modbusIpMaster = ModbusIpMaster.CreateIp(tcpClient);
                this.acDrive = new ACDrive(modbusIpMaster, this.slaveId, this.KeepAlive);
                this.dcDrive = new DCDrive(modbusIpMaster, this.slaveId, this.KeepAlive);

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                this.lastError = String.Format(STRERR_FailedToOpenNetworkConnection_arg2, this.ipAddress.ToString(), this.port);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>bool</returns>
        public bool CloseConnection()
        {
            const string methodName = "CloseConnection";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;

            try
            {
                this.lastError = null;

                if (this.tcpClient != null)
                {
                    /*
                     * Close the network associated with the TCP client
                     */
                    NetworkStream networkStream = this.tcpClient.GetStream();
                    if (networkStream != null)
                    {
                        networkStream.Close();
                    }

                    /*
                     * Close the TCP client
                     */
                    this.tcpClient.Close();
                    this.tcpClient = null;
                }

                success = true;
            }
            catch (Exception ex)
            {
                this.lastError = ex.Message;
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //-----------------------------------------------------------------------------------------------------------//

        public void KeepAlive()
        {
            if (this.tcpClient.Connected == true)
            {
                this.acDrive.KeepAlive();
            }
        }

    }
}
