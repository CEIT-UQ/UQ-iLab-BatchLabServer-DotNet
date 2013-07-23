using System;
using System.IO;
using System.Web.Hosting;
using Library.Lab;
using Library.LabClient;
using Library.LabClient.Engine;
using Library.Lab.Utilities;

namespace LabClientHtml
{
    public class Global : System.Web.HttpApplication
    {
        private const string STR_ClassName = "Global";

        private static ConfigProperties configProperties = null;

        public static ConfigProperties ConfigProperties
        {
            get { return configProperties; }
            set { configProperties = value; }
        }

        //---------------------------------------------------------------------------------------//

        protected void Application_Start(object sender, EventArgs e)
        {
            const string methodName = "Application_Start";

            /*
             * Set the filepath for the log files
             */
            String rootFilePath = HostingEnvironment.ApplicationPhysicalPath;
            String filePath = AppSetting.GetAppSetting(LabConsts.STRCFG_LogFilesPath);
            Logfile.SetFilePath(Path.Combine(rootFilePath, filePath));

            /*
             * Set the logging level
             */
            try
            {
                String logLevel = AppSetting.GetAppSetting(LabConsts.STRCFG_LogLevel);
                Logfile.Level level = (Logfile.Level)Enum.Parse(typeof(Logfile.Level), logLevel, true);
                Logfile.SetLevel(level);
            }
            catch
            {
                Logfile.SetLevel(Logfile.Level.Info);
            }

            Logfile.Write(String.Empty);
            Logfile.WriteCalled(STR_ClassName, methodName);

            /*
             * Get configuration properties
             */
            Global.configProperties = new ConfigProperties();

            Logfile.WriteCompleted(STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        protected void Application_Error(object sender, EventArgs e)
        {
            const string methodName = "Application_Error";

            Logfile.WriteCalled(STR_ClassName, methodName);

            /*
             * An unhandled error has occured
             */
            Exception ex = Server.GetLastError();
            Logfile.WriteException(ex);

            /*
             * Clear the error from the server
             */
            Server.ClearError();

            Logfile.WriteCompleted(STR_ClassName, methodName);
        }

        //---------------------------------------------------------------------------------------//

        protected void Application_End(object sender, EventArgs e)
        {
            const string methodName = "Application_End";
            Logfile.WriteCalled(STR_ClassName, methodName);

            /*
             * Close logfile class
             */
            Logfile.Close();

            Logfile.WriteCompleted(STR_ClassName, methodName);
        }
    }
}