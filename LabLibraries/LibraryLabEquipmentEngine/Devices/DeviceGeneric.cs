using System;
using System.Diagnostics;
using System.Xml;
using Library.Lab;
using Library.Lab.Utilities;

namespace Library.LabEquipment.Engine.Devices
{
    public class DeviceGeneric : IDisposable
    {
        #region Constants
        private const string STR_ClassName = "DeviceGeneric";
        private const Logfile.Level logLevel = Logfile.Level.Finest;
        /*
         * String constants for logfile messages
         */
        protected const string STRLOG_Dispose_arg2 = "disposing: {0:s}  disposed: {1:s}";
        protected const string STRLOG_Success_arg = "Success: {0:s}";
        #endregion

        #region Variables
        protected bool disposed;
        protected XmlNode xmlNodeDevice;
        #endregion

        #region Properties

        public static string ClassName
        {
            get { return STR_ClassName; }
        }

        private string deviceName;
        protected bool initialiseEnabled;
        protected int initialiseDelay;
        protected string lastError;

        public string DeviceName
        {
            get { return deviceName; }
        }

        public bool InitialiseEnabled
        {
            get { return initialiseEnabled; }
            set { initialiseEnabled = value; }
        }

        public int InitialiseDelay
        {
            get { return initialiseDelay; }
        }

        public string LastError
        {
            get
            {
                string error = lastError;
                lastError = null;
                return error;
            }
        }

        #endregion

        //---------------------------------------------------------------------------------------//

        public DeviceGeneric(LabEquipmentConfiguration labEquipmentConfiguration)
            : this(labEquipmentConfiguration, STR_ClassName)
        {
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="labEquipmentConfiguration"></param>
        public DeviceGeneric(LabEquipmentConfiguration labEquipmentConfiguration, string deviceName)
        {
            const string methodName = "DeviceGeneric";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            try
            {
                /*
                 * Check that parameters are valid
                 */
                if (labEquipmentConfiguration == null)
                {
                    throw new ArgumentNullException(LabEquipmentConfiguration.ClassName);
                }

                /*
                 * Get the device configuration from the XML string
                 */
                XmlDocument xmlDocument = XmlUtilities.GetDocumentFromString(labEquipmentConfiguration.GetXmlDeviceConfiguration(deviceName));
                this.xmlNodeDevice = XmlUtilities.GetRootNode(xmlDocument, LabConsts.STRXML_Device);
                this.deviceName = deviceName;

                /*
                 * Get the initialise information
                 */
                XmlNode xmlNodeInitialise = XmlUtilities.GetChildNode(this.xmlNodeDevice, LabConsts.STRXML_Initialise);
                this.initialiseEnabled = XmlUtilities.GetChildValueAsBool(xmlNodeInitialise, LabConsts.STRXML_InitialiseEnabled);
                this.initialiseDelay = XmlUtilities.GetChildValueAsInt(xmlNodeInitialise, LabConsts.STRXML_InitialiseDelay);
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
        public virtual bool Initialise()
        {
            const string methodName = "Initialise";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            bool success = false;

            try
            {
                this.disposed = false;

                for (int i = 0; i < this.initialiseDelay; i++)
                {
                    Delay.MilliSeconds(1000);
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

        #region Close and Dispose

        /// <summary>
        /// Do not make this method virtual. A derived class should not be allowed to override this method.
        /// </summary>
        public void Close()
        {
            const string methodName = "Close";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            /*
             * Calls the Dispose method without parameters
             */
            this.Dispose();

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Implement IDisposable. Do not make this method virtual. A derived class should not be able
        /// to override this method.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);

            /*
             * Take yourself off the Finalization queue to prevent finalization code for this object
             * from executing a second time.
             */
            GC.SuppressFinalize(this);
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Use C# destructor syntax for finalization code. This destructor will run only if the Dispose
        /// method does not get called. It gives your base class the opportunity to finalize. Do not provide
        /// destructors in types derived from this class.
        /// </summary>
        ~DeviceGeneric()
        {
            Trace.WriteLine("~DeviceGeneric():");

            /*
             * Do not re-create Dispose clean-up code here. Calling Dispose(false) is optimal in terms of
             * readability and maintainability.
             */
            this.Dispose(false);
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
        protected virtual void Dispose(bool disposing)
        {
            const string methodName = "Dispose";
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
                 * Note disposing has been done.
                 */
                this.disposed = true;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        #endregion

    }
}
