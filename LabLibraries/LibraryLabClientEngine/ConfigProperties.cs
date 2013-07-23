using System;
using System.Collections.Generic;
using System.Text;
using Library.Lab;
using Library.Lab.Utilities;

namespace Library.LabClient.Engine
{
    public class ConfigProperties
    {
        #region Constants
        private const String STR_ClassName = "ConfigProperties";
        private const Logfile.Level logLevel = Logfile.Level.Fine;
        /*
         * String constants for configuration properties
         */
        private const String STRCFG_ServiceUrl = "ServiceUrl";
        private const String STRCFG_LabServerId = "LabServerId";
        private const String STRCFG_MultiSubmit = "MultiSubmit";
        private const String STRCFG_FeedbackEmail = "FeedbackEmail";
        #endregion

        #region Properties
        private String serviceUrl;
        private String labServerId;
        private bool multiSubmit;
        private String feedbackEmail;

        public String ServiceUrl
        {
            get { return serviceUrl; }
            set { serviceUrl = value; }
        }

        public String LabServerId
        {
            get { return labServerId; }
            set { labServerId = value; }
        }

        public bool MultiSubmit
        {
            get { return multiSubmit; }
            set { multiSubmit = value; }
        }

        public String FeedbackEmail
        {
            get { return feedbackEmail; }
            set { feedbackEmail = value; }
        }
        #endregion

        //-------------------------------------------------------------------------------------------------//

        public ConfigProperties()
        {
            const string methodName = "ConfigProperties";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            try
            {
                /*
                 * Get configuration information
                 */
                this.serviceUrl = AppSetting.GetAppSetting(LabConsts.STRCFG_ServiceUrl);
                this.labServerId = AppSetting.GetAppSetting(LabConsts.STRCFG_LabServerId);
                this.feedbackEmail = AppSetting.GetAppSetting(LabConsts.STRCFG_FeedbackEmail);
                this.multiSubmit = AppSetting.GetBoolAppSetting(LabConsts.STRCFG_MultiSubmit, false);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw;
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }
    }
}
