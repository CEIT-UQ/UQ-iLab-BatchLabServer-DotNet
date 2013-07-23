using System;
using Library.Lab;
using Library.Lab.Utilities;

namespace Library.LabEquipment.Engine
{
    public class ConfigProperties
    {
        #region Constants
        private const string STR_ClassName = "ConfigProperties";
        private const Logfile.Level logLevel = Logfile.Level.Fine;
        /*
         * String constants for exception messages
         */
        private const string STRERR_LabServer = "labServer";
        #endregion

        #region Properties
        public static string ClassName
        {
            get { return STR_ClassName; }
        }

        private string labServerName;
        private string labServerGuid;
        private string labServerPasskey;
        private bool authenticating;
        private bool logAuthentication;
        private string xmlEquipmentConfigPath;

        public string LabServerName
        {
            get { return labServerName; }
        }

        public string LabServerGuid
        {
            get { return labServerGuid; }
        }

        public string LabServerPasskey
        {
            get { return labServerPasskey; }
        }

        public bool Authenticating
        {
            get { return authenticating; }
        }

        public bool LogAuthentication
        {
            get { return logAuthentication; }
        }

        public string XmlEquipmentConfigPath
        {
            get { return xmlEquipmentConfigPath; }
            set { xmlEquipmentConfigPath = value; }
        }
        #endregion

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        public ConfigProperties()
        {
            const string methodName = "ConfigProperties";
            Logfile.WriteCalled(Logfile.Level.Info, STR_ClassName, methodName);

            try
            {
                /*
                 * Get LabServer information and split into its parts
                 */
                string labServer = AppSetting.GetAppSetting(LabConsts.STRCFG_LabServer);
                if (labServer == null)
                {
                    throw new ArgumentNullException(STRERR_LabServer);
                }
                labServer = labServer.Trim();
                if (labServer.Length == 0)
                {
                    throw new ArgumentException(STRERR_LabServer);
                }
                string[] splitLabServer = labServer.Split(new char[] { LabConsts.CHRCSV_SplitterChar });
                if (splitLabServer.Length != LabConsts.LABSERVER_SIZE)
                {
                    throw new ArgumentException(STRERR_LabServer);
                }

                /*
                 * Check that each part exists
                 */
                for (int i = 0; i < LabConsts.LABSERVER_SIZE; i++)
                {
                    splitLabServer[i] = splitLabServer[i].Trim();
                    if (splitLabServer[i].Length == 0)
                    {
                        throw new ArgumentException(STRERR_LabServer);
                    }
                }
                this.labServerName = splitLabServer[LabConsts.INDEX_LABSERVER_NAME];
                this.labServerGuid = splitLabServer[LabConsts.INDEX_LABSERVER_GUID];
                this.labServerPasskey = splitLabServer[LabConsts.INDEX_LABSERVER_PASSKEY];

                /*
                 * Get LabServer authentication
                 */
                this.authenticating = AppSetting.GetBoolAppSetting(LabConsts.STRCFG_Authenticating, true);
                this.logAuthentication = AppSetting.GetBoolAppSetting(LabConsts.STRCFG_LogAuthentication, false);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw;
            }

            Logfile.WriteCompleted(Logfile.Level.Info, STR_ClassName, methodName);
        }
    }
}
