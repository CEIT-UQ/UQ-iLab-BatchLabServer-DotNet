using System;
using System.Xml;
using Library.Lab;
using Library.LabEquipment.Devices;
using Library.LabEquipment.Drivers;
using Library.LabEquipment.Engine;
using Library.LabEquipment.Engine.Drivers;

namespace Library.LabEquipment
{
    public class EquipmentEngine : LabEquipmentEngine
    {
        #region Constants
        private const String STR_ClassName = "EquipmentEngine";
        private const Logfile.Level logLevel = Logfile.Level.Fine;
        /*
         * String constants for exception messages
         */
        private const String STRERR_InvalidDeviceType_arg2 = "Invalid Device Type: {0:s} - {1:s}";
        #endregion

        #region Variables
        private DeviceFlexMotion deviceFlexMotion;
        private DeviceST360Counter deviceST360Counter;
        private DeviceSerialLcd deviceSerialLcd;
        private DeviceFlexMotionSimulation deviceFlexMotionSimulation;
        private DeviceST360CounterSimulation deviceST360CounterSimulation;
        private DeviceSerialLcdSimulation deviceSerialLcdSimulation;
        #endregion

        #region Properties

        private bool disablePowerdown;

        public bool DisablePowerdown
        {
            get { return disablePowerdown; }
            set { disablePowerdown = value; }
        }

        #endregion

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="labEquipmentConfiguration"></param>
        public EquipmentEngine(LabEquipmentConfiguration labEquipmentConfiguration)
            : base(labEquipmentConfiguration)
        {
            const String methodName = "EquipmentEngine";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            try
            {
                /*
                 * Create instances of the simulation devices to be used by the simulation drivers
                 * so that both the hardware drivers and simulation drivers are available for the setups
                 */
                this.deviceFlexMotionSimulation = new DeviceFlexMotionSimulation(this.labEquipmentConfiguration);
                this.deviceST360CounterSimulation = new DeviceST360CounterSimulation(this.labEquipmentConfiguration);
                this.deviceSerialLcdSimulation = new DeviceSerialLcdSimulation(this.labEquipmentConfiguration);

                /*
                 * Initialise the simulated devices
                 */
                this.deviceFlexMotionSimulation.Initialise();
                this.deviceST360CounterSimulation.Initialise();
                this.deviceSerialLcdSimulation.Initialise();

                /*
                 * Determine FlexMotion device to use
                 */
                XmlDocument xmlDocument = XmlUtilities.GetDocumentFromString(labEquipmentConfiguration.GetXmlDeviceConfiguration(DeviceFlexMotion.ClassName));
                XmlNode xmlNodeDevice = XmlUtilities.GetRootNode(xmlDocument, LabConsts.STRXML_Device);
                String deviceType = XmlUtilities.GetChildValue(xmlNodeDevice, Consts.STRXML_Type);

                /*
                 * Create instance of the FlexMotion device to use
                 */
                switch (deviceType)
                {
                    case Consts.STRXML_TypeNone:
                        this.deviceFlexMotion = new DeviceFlexMotion(this.labEquipmentConfiguration);
                        break;

                    case Consts.STRXML_TypeSimulation:
                        this.deviceFlexMotion = this.deviceFlexMotionSimulation;
                        break;

                    case Consts.STRXML_TypeHardware:
                        this.deviceFlexMotion = new DeviceFlexMotionHardware(this.labEquipmentConfiguration);
                        break;

                    default:
                        throw new ApplicationException(String.Format(STRERR_InvalidDeviceType_arg2, DeviceFlexMotion.ClassName, deviceType));
                }

                /*
                 * Determine ST360Counter device to use
                 */
                xmlDocument = XmlUtilities.GetDocumentFromString(labEquipmentConfiguration.GetXmlDeviceConfiguration(DeviceST360Counter.ClassName));
                xmlNodeDevice = XmlUtilities.GetRootNode(xmlDocument, LabConsts.STRXML_Device);
                deviceType = XmlUtilities.GetChildValue(xmlNodeDevice, Consts.STRXML_Type);

                /*
                 * Create instance of the ST360Counter device to use
                 */
                switch (deviceType)
                {
                    case Consts.STRXML_TypeNone:
                        this.deviceST360Counter = new DeviceST360Counter(this.labEquipmentConfiguration);
                        break;

                    case Consts.STRXML_TypeSimulation:
                        this.deviceST360Counter = this.deviceST360CounterSimulation;
                        break;

                    case Consts.STRXML_TypeSerial:
                        this.deviceST360Counter = new DeviceST360CounterSerial(this.labEquipmentConfiguration);
                        break;

                    case Consts.STRXML_TypeNetwork:
                        this.deviceST360Counter = new DeviceST360CounterNetwork(this.labEquipmentConfiguration);
                        break;

                    default:
                        throw new ApplicationException(String.Format(STRERR_InvalidDeviceType_arg2, DeviceST360Counter.ClassName, deviceType));
                }

                /*
                 * Determine DeviceSerialLcd device to use
                 */
                xmlDocument = XmlUtilities.GetDocumentFromString(labEquipmentConfiguration.GetXmlDeviceConfiguration(DeviceSerialLcd.ClassName));
                xmlNodeDevice = XmlUtilities.GetRootNode(xmlDocument, LabConsts.STRXML_Device);
                deviceType = XmlUtilities.GetChildValue(xmlNodeDevice, Consts.STRXML_Type);

                /*
                 * Create instance of the DeviceSerialLcd device to use
                 */
                switch (deviceType)
                {
                    case Consts.STRXML_TypeNone:
                        this.deviceSerialLcd = new DeviceSerialLcd(this.labEquipmentConfiguration);
                        break;

                    case Consts.STRXML_TypeSimulation:
                        this.deviceSerialLcd = this.deviceSerialLcdSimulation;
                        break;

                    case Consts.STRXML_TypeSerial:
                        this.deviceSerialLcd = new DeviceSerialLcdSerial(this.labEquipmentConfiguration);
                        break;

                    case Consts.STRXML_TypeNetwork:
                        this.deviceSerialLcd = new DeviceSerialLcdNetwork(this.labEquipmentConfiguration);
                        break;

                    default:
                        throw new ApplicationException(String.Format(STRERR_InvalidDeviceType_arg2, DeviceSerialLcd.ClassName, deviceType));
                }

                /*
                 * Initialise properties
                 */
                this.initialiseDelay = this.deviceFlexMotion.InitialiseDelay + this.deviceST360Counter.InitialiseDelay + this.deviceSerialLcd.InitialiseDelay;
                this.disablePowerdown = false;
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
        /// <param name="setupId"></param>
        /// <returns>DriverGeneric</returns>
        protected override DriverGeneric GetDriver(String setupId)
        {
            const String methodName = "GetDriver";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_SetupId_arg, setupId));

            DriverGeneric driverGeneric = null;

            /*
             * Create an instance of the driver for the specified setup Id
             */
            switch (setupId)
            {
                case Consts.STRXML_SetupId_RadioactivityVsTime:
                case Consts.STRXML_SetupId_RadioactivityVsDistance:
                    driverGeneric = new DriverRadioactivity(this.labEquipmentConfiguration);
                    break;

                case Consts.STRXML_SetupId_RadioactivityVsAbsorber:
                    driverGeneric = new DriverAbsorbers(this.labEquipmentConfiguration);
                    break;

                case Consts.STRXML_SetupId_SimActivityVsTime:
                case Consts.STRXML_SetupId_SimActivityVsDistance:
                    driverGeneric = new DriverSimActivity(this.labEquipmentConfiguration);
                    break;

                case Consts.STRXML_SetupId_SimActivityVsAbsorber:
                    driverGeneric = new DriverSimAbsorbers(this.labEquipmentConfiguration);
                    break;

                case Consts.STRXML_SetupId_SimActivityVsTimeNoDelay:
                case Consts.STRXML_SetupId_SimActivityVsDistanceNoDelay:
                    driverGeneric = new DriverSimActivityNoDelay(this.labEquipmentConfiguration);
                    break;

                case Consts.STRXML_SetupId_SimActivityVsAbsorberNoDelay:
                    driverGeneric = new DriverSimAbsorbersNoDelay(this.labEquipmentConfiguration);
                    break;
            }

            /*
             * If a driver instance was created, set the devices in the driver instance
             */
            if (driverGeneric != null)
            {
                switch (setupId)
                {
                    case Consts.STRXML_SetupId_RadioactivityVsTime:
                    case Consts.STRXML_SetupId_RadioactivityVsDistance:
                        ((DriverRadioactivity)driverGeneric).SetDeviceFlexMotion(this.deviceFlexMotion);
                        ((DriverRadioactivity)driverGeneric).SetDeviceST360Counter(this.deviceST360Counter);
                        ((DriverRadioactivity)driverGeneric).SetDeviceSerialLcd(this.deviceSerialLcd);
                        break;

                    case Consts.STRXML_SetupId_RadioactivityVsAbsorber:
                        ((DriverAbsorbers)driverGeneric).SetDeviceFlexMotion(this.deviceFlexMotion);
                        ((DriverAbsorbers)driverGeneric).SetDeviceST360Counter(this.deviceST360Counter);
                        ((DriverAbsorbers)driverGeneric).SetDeviceSerialLcd(this.deviceSerialLcd);
                        break;

                    case Consts.STRXML_SetupId_SimActivityVsTime:
                    case Consts.STRXML_SetupId_SimActivityVsDistance:
                        ((DriverSimActivity)driverGeneric).SetDeviceFlexMotion(this.deviceFlexMotionSimulation);
                        ((DriverSimActivity)driverGeneric).SetDeviceST360Counter(this.deviceST360CounterSimulation);
                        ((DriverSimActivity)driverGeneric).SetDeviceSerialLcd(this.deviceSerialLcdSimulation);
                        break;

                    case Consts.STRXML_SetupId_SimActivityVsAbsorber:
                        ((DriverSimAbsorbers)driverGeneric).SetDeviceFlexMotion(this.deviceFlexMotionSimulation);
                        ((DriverSimAbsorbers)driverGeneric).SetDeviceST360Counter(this.deviceST360CounterSimulation);
                        ((DriverSimAbsorbers)driverGeneric).SetDeviceSerialLcd(this.deviceSerialLcdSimulation);
                        break;

                    case Consts.STRXML_SetupId_SimActivityVsTimeNoDelay:
                    case Consts.STRXML_SetupId_SimActivityVsDistanceNoDelay:
                        ((DriverSimActivityNoDelay)driverGeneric).SetDeviceFlexMotion(this.deviceFlexMotionSimulation);
                        ((DriverSimActivityNoDelay)driverGeneric).SetDeviceST360Counter(this.deviceST360CounterSimulation);
                        ((DriverSimActivityNoDelay)driverGeneric).SetDeviceSerialLcd(this.deviceSerialLcdSimulation);
                        break;

                    case Consts.STRXML_SetupId_SimActivityVsAbsorberNoDelay:
                        ((DriverSimAbsorbersNoDelay)driverGeneric).SetDeviceFlexMotion(this.deviceFlexMotionSimulation);
                        ((DriverSimAbsorbersNoDelay)driverGeneric).SetDeviceST360Counter(this.deviceST360CounterSimulation);
                        ((DriverSimAbsorbersNoDelay)driverGeneric).SetDeviceSerialLcd(this.deviceSerialLcdSimulation);
                        break;
                }
            }
            else
            {
                driverGeneric = base.GetDriver(setupId);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    driverGeneric.DriverName);

            return driverGeneric;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>bool</returns>
        protected override bool PowerupEquipment()
        {
            const String methodName = "PowerupEquipment";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;

            try
            {
                success = this.deviceFlexMotion.EnablePower();
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //---------------------------------------------------------------------------------------//

        public bool _InitialiseEquipment()
        {
            return this.InitialiseEquipment();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns>bool</returns>
        protected override bool InitialiseEquipment()
        {
            const String methodName = "InitialiseEquipment";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;

            try
            {
                /*
                 * Initialise the equipment devices
                 */
                if (this.deviceFlexMotion.Initialise() == false)
                {
                    throw new ApplicationException(this.deviceFlexMotion.LastError);
                }
                if (this.deviceST360Counter.Initialise() == false)
                {
                    throw new ApplicationException(this.deviceST360Counter.LastError);
                }
                if (this.deviceSerialLcd.Initialise() == false)
                {
                    throw new ApplicationException(this.deviceSerialLcd.LastError);
                }

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <returns>bool</returns>
        protected override bool PowerdownEquipment()
        {
            const String methodName = "PowerdownEquipment";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;

            try
            {
                /*
                 * Close devices before powering down
                 */
                if (this.deviceST360Counter != null)
                {
                    this.deviceST360Counter.Close();
                }
                if (this.deviceSerialLcd != null)
                {
                    this.deviceSerialLcd.Close();
                }

                /*
                 * Powerdown the equipment
                 */
                if (this.disablePowerdown == false)
                {
                    if (this.deviceFlexMotion.DisablePower() == false)
                    {
                        throw new Exception(this.deviceFlexMotion.LastError);
                    }
                }

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName,
                    String.Format(STRLOG_Success_arg, success));

            return success;
        }
    }
}
