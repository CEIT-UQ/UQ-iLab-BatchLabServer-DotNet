using System;
using System.IO;
using System.Web.Hosting;
using Library.Lab;
using Library.Lab.Utilities;

namespace LabServer.Redirect
{
    public class Global : System.Web.HttpApplication
    {
        #region Consts
        private const string STR_ClassName = "Global";
        private const Logfile.Level logLevel = Logfile.Level.Info;
        /*
         * String constants for logfile messages
         */
        private const String STRLOG_LabServerUrl_arg = "LabServerUrl: {0:s}";
        private const String STRLOG_LabStatus_arg2 = "Online: {0:s}  LabStatusMessage: {1:s}";
        /*
         * String constants for exception messages
         */
        private const String STRERR_NotSpecified_arg = "Not specified: {0:s}";
        #endregion

        #region Properties

        private static String labServerUrl;
        private static bool online;
        private static String labStatusMessage;

        public static String LabServerUrl
        {
            get { return labServerUrl; }
        }

        public static bool Online
        {
            get { return online; }
        }

        public static String LabStatusMessage
        {
            get { return labStatusMessage; }
        }

        #endregion

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
                logFilesPath = AppSetting.GetAppSetting(Consts.STRCFG_LogFilesPath);
            }
            catch (Exception ex)
            {
                Logfile.Write(ex.Message);
                throw new ApplicationException(String.Format(STRERR_NotSpecified_arg, Consts.STRCFG_LogFilesPath));
            }
            if (logFilesPath == null)
            {
                throw new ApplicationException(String.Format(STRERR_NotSpecified_arg, Consts.STRCFG_LogFilesPath));
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
                String _logLevel = AppSetting.GetAppSetting(Consts.STRCFG_LogLevel);
                Logfile.Level level = (Logfile.Level)Enum.Parse(typeof(Logfile.Level), _logLevel, true);
                Logfile.SetLevel(level);
            }
            catch
            {
                Logfile.SetLevel(Logfile.Level.Info);
            }

            Logfile.Write(String.Empty);
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            /*
             * Get the LabServer URL
             */
            String _labServerUrl;
            try
            {
                _labServerUrl = AppSetting.GetAppSetting(Consts.STRCFG_LabServerUrl);
            }
            catch (Exception ex)
            {
                Logfile.Write(ex.Message);
                throw new ApplicationException(String.Format(STRERR_NotSpecified_arg, Consts.STRCFG_LabServerUrl));
            }
            if (_labServerUrl == null)
            {
                throw new ApplicationException(String.Format(STRERR_NotSpecified_arg, Consts.STRCFG_LabServerUrl));
            }

            Logfile.Write(String.Format(STRLOG_LabServerUrl_arg, _labServerUrl));

            /*
             * Set LabServer's service url
             */
            Global.labServerUrl = _labServerUrl;

            /*
             * Get the LabServer online status
             */
            bool _online = true;
            String _labStatusMessage = String.Empty;
            try
            {
                if ((_online = AppSetting.GetBoolAppSetting(Consts.STRCFG_Online)) == false)
                {
                    try
                    {
                        _labStatusMessage = AppSetting.GetAppSetting(Consts.STRCFG_LabStatusMessage);
                    }
                    catch (Exception ex)
                    {
                        Logfile.Write(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Logfile.Write(ex.Message);
            }

            Logfile.Write(String.Format(STRLOG_LabStatus_arg2, _online, _labStatusMessage));

            /*
             * Set LabServer's online status
             */
            Global.online = _online;
            Global.labStatusMessage = _labStatusMessage;

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        protected void Application_Error(object sender, EventArgs e)
        {
            const String methodName = "Application_Error";

            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            /*
             * An unhandled error has occured
             */
            Exception ex = Server.GetLastError();
            Logfile.WriteException(ex);

            /*
             * Clear the error from the server
             */
            Server.ClearError();

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        protected void Application_End(object sender, EventArgs e)
        {
            const String methodName = "Application_End";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            /*
             * Close logfile class
             */
            Logfile.Close();

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }
    }
}