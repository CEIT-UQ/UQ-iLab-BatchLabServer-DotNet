using System;
using System.IO;
using System.Web;
using System.Web.Hosting;
using Library.Lab;
using Library.Lab.Utilities;
using Library.LabEquipment;
using Library.LabEquipment.Engine;

namespace LabEquipment
{
    public class Global : System.Web.HttpApplication
    {
        private const String STR_ClassName = "Global";
        private const Logfile.Level logLevel = Logfile.Level.Info;
        /*
         * String constants for exception messages
         */
        private const String STRERR_NotSpecified_arg = "Not specified: {0:s}";
        private const String STRERR_EquipmentManagerCreateFailed = "Failed to create EquipmentManager!";
        private const String STRERR_EquipmentManagerStartFailed = "Failed to start EquipmentManager!";

        public static ConfigProperties configProperties = null;
        public static EquipmentManager equipmentManager = null;

        //-------------------------------------------------------------------------------------------------//

        protected void Application_Start(object sender, EventArgs e)
        {
            const String methodName = "Application_Start";

            /*
             * Get the logfiles path
             */
            String logFilesPath;
            try
            {
                logFilesPath = AppSetting.GetAppSetting(LabConsts.STRCFG_LogFilesPath);
            }
            catch (Exception ex)
            {
                Logfile.Write(ex.Message);
                throw new ApplicationException(String.Format(STRERR_NotSpecified_arg, LabConsts.STRCFG_LogFilesPath));
            }
            if (logFilesPath == null)
            {
                throw new ApplicationException(String.Format(STRERR_NotSpecified_arg, LabConsts.STRCFG_LogFilesPath));
            }

            /*
             * Set the filepath for the log files
             */
            String rootFilePath = HostingEnvironment.ApplicationPhysicalPath;
            Logfile.SetFilePath(Path.Combine(rootFilePath, logFilesPath));

            /*
             * Set the logging level
             */
            try
            {
                String strLogLevel = AppSetting.GetAppSetting(LabConsts.STRCFG_LogLevel);
                Logfile.Level level = (Logfile.Level)Enum.Parse(typeof(Logfile.Level), strLogLevel, true);
                Logfile.SetLevel(level);
            }
            catch
            {
                Logfile.SetLevel(Logfile.Level.Info);
            }

            Logfile.Write(String.Empty);
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            /*
             * Get the equipment configuration filename
             */
            String xmlEquipmentConfigFilename;
            try
            {
                xmlEquipmentConfigFilename = AppSetting.GetAppSetting(LabConsts.STRCFG_XmlEquipmentConfigFilename);
            }
            catch (Exception ex)
            {
                Logfile.Write(ex.Message);
                throw new ApplicationException(String.Format(STRERR_NotSpecified_arg, LabConsts.STRCFG_XmlEquipmentConfigFilename));
            }
            if (xmlEquipmentConfigFilename == null)
            {
                throw new ApplicationException(String.Format(STRERR_NotSpecified_arg, LabConsts.STRCFG_XmlEquipmentConfigFilename));
            }

            try
            {
                /*
                 * Get configuration properties
                 */
                configProperties = new ConfigProperties();
                configProperties.XmlEquipmentConfigPath = Path.Combine(rootFilePath, xmlEquipmentConfigFilename);

                /*
                 * Create experiment manager and start it
                 */
                equipmentManager = new EquipmentManager(configProperties);
                if (equipmentManager.Create() == false)
                {
                    throw new ApplicationException(STRERR_EquipmentManagerCreateFailed);
                }
                if (equipmentManager.Start() == false)
                {
                    throw new ApplicationException(STRERR_EquipmentManagerStartFailed);
                }
            }
            catch (Exception ex)
            {
                this.Application_End(sender, e);
                throw ex;
            }
            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        protected void Application_End(object sender, EventArgs e)
        {
            const String methodName = "Application_End";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            /*
             * Close equipment manager
             */
            if (equipmentManager != null)
            {
                equipmentManager.Close();
            }

            /*
             * Close logfile class
             */
            Logfile.Close();

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }
    }
}