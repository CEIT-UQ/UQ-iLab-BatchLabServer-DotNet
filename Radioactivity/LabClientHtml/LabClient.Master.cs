using System;
using System.Collections.Specialized;
using System.Net;
using System.Web;
using Library.Lab;
using Library.Lab.Types;
using Library.LabClient;
using Library.LabClient.Engine;
using Library.ServiceBroker;

namespace LabClientHtml
{
    public partial class LabClientMaster : System.Web.UI.MasterPage
    {
        #region Constants
        private const String STR_ClassName = "LabClientMaster";
        private const Logfile.Level logLevel = Logfile.Level.Fine;
        /*
         * String constants
         */
        private const String STR_UrlFeedbackEmail_arg = "mailto:{0:s}";
        private const String STR_DefaultNavMenuPhotoUrl = "./resources/img/generic_content32.jpg";
        /*
         * String constants for logfile messages
         */
        private const String STRLOG_LoggingLevel_arg = "LoggingLevel: {0:s}";
        private const String STRLOG_UserHost_arg2 = "UserHost - IP Address: {0:s}  Host Name: {1:s}";
        private const String STRLOG_CannotResolveToHostName = "Cannot resolve to HostName!";
        private const String STRLOG_RequestParams_arg5 = "CouponId: {0:d}  Passkey: {1:s}  ServiceUrl: {2:s}  LabServerId: {3:s}  MultiSubmit: {4:s}";
        private const String STRLOG_GettingLabStatus = "Getting Lab Status...";
        private const String STRLOG_LabStatus_arg2 = "LabStatus - Online: {0:s}  Message: '{1:s}'";
        #endregion

        #region Variables
        private LabClientSession labClientSession;
        #endregion

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// This method gets called before any other Page_Init() or Page_Load() methods.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Init(object sender, EventArgs e)
        {
            const String methodName = "Page_Init";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            /*
             * Get the LabClientSession information from the session
             */
            this.labClientSession = (LabClientSession)Session[Consts.STRSSN_LabClient];

            /*
             * Check if the LabClient session doesn't exists
             */
            if (this.labClientSession == null)
            {
                try
                {
                    /*
                     * Log the caller's IP address and hostname
                     */
                    String hostname;
                    try
                    {
                        IPHostEntry ipHostEntry = Dns.GetHostEntry(Request.UserHostAddress);
                        hostname = ipHostEntry.HostName;
                    }
                    catch
                    {
                        hostname = STRLOG_CannotResolveToHostName;
                    }
                    Logfile.Write(Logfile.Level.Info, String.Format(STRLOG_UserHost_arg2, Request.UserHostAddress, hostname));

                    /*
                     * Get configuration information
                     */
                    ConfigProperties configProperties = Global.ConfigProperties;

                    /*
                     * Get query string values ignoring case when comparing strings
                     */
                    int couponId = 0;
                    String passkey = null;
                    NameValueCollection collection = HttpUtility.ParseQueryString(Request.Url.Query);
                    foreach (String key in collection)
                    {
                        if (key.Equals(Consts.STRQRY_CouponId, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            /* Used with Batch ServiceBroker */
                            couponId = Int32.Parse(collection.Get(key));
                        }
                        else if (key.Equals(Consts.STRQRY_Coupon_Id, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            /* Used with Merged ServiceBroker */
                            couponId = Int32.Parse(collection.Get(key));
                        }
                        else if (key.Equals(Consts.STRQRY_Passkey, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            passkey = collection.Get(key);
                        }
                        else if (key.Equals(Consts.STRQRY_ServiceUrl, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            configProperties.ServiceUrl = collection.Get(key);
                        }
                        else if (key.Equals(Consts.STRQRY_LabServerId, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            configProperties.LabServerId = collection.Get(key);
                        }
                        else if (key.Equals(Consts.STRCFG_MultiSubmit, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            bool value;
                            if (Boolean.TryParse(collection.Get(key), out value) == true)
                            {
                                configProperties.MultiSubmit = value;
                            }
                        }
                    }

                    /*
                     * Check if session has timed out
                     */
                    if (couponId == 0)
                    {
                        throw new HttpException();
                    }

                    /*
                     * Create a ServiceBroker proxy and add authorisation information
                     */
                    ServiceBrokerAPI serviceBrokerAPI = new ServiceBrokerAPI(configProperties.ServiceUrl);
                    serviceBrokerAPI.LabServerId = configProperties.LabServerId;
                    serviceBrokerAPI.CouponId = couponId;
                    serviceBrokerAPI.CouponPasskey = passkey;

                    /*
                     * Create an instance of the LabClientSession and fill in
                     */
                    labClientSession = new LabClientSession();
                    labClientSession.ServiceBrokerAPI = serviceBrokerAPI;
                    labClientSession.MultiSubmit = configProperties.MultiSubmit;
                    labClientSession.FeedbackEmailUrl = configProperties.FeedbackEmail;

                    /*
                     * Set LabClientSession information in the session for access by the web pages
                     */
                    Session[Consts.STRSSN_LabClient] = labClientSession;

                    Logfile.Write(Logfile.Level.Fine, String.Format(STRLOG_RequestParams_arg5,
                            serviceBrokerAPI.CouponId, serviceBrokerAPI.CouponPasskey,
                            configProperties.ServiceUrl, configProperties.LabServerId, configProperties.MultiSubmit));

                    /*
                     * Get the lab status
                     */
                    Logfile.Write(Logfile.Level.Info, STRLOG_GettingLabStatus);
                    LabStatus labStatus = serviceBrokerAPI.GetLabStatus();
                    Logfile.Write(Logfile.Level.Info, String.Format(STRLOG_LabStatus_arg2, labStatus.Online, labStatus.LabStatusMessage));

                    /*
                     * Get information from the lab configuration xml file
                     */
                    String xmlLabConfiguration = serviceBrokerAPI.GetLabConfiguration();
                    this.labClientSession.ParseLabConfiguration(xmlLabConfiguration);
                }
                catch (HttpException)
                {
                    Response.Redirect(Consts.STRURL_Expired);
                }
                catch (Exception ex)
                {
                    Logfile.WriteError(ex.ToString());
                }
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //---------------------------------------------------------------------------------------//

        protected void Page_Load(object sender, EventArgs e)
        {
            const String methodName = "Page_Load";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            this.labClientSession = (LabClientSession)Session[LabConsts.STRSSN_LabClient];
            if (this.labClientSession != null)
            {
                this.lblTitle.Text = this.labClientSession.Title;
                this.lblVersion.Text = this.labClientSession.Version;

                /*
                 * Navmenu photo
                 */
                String url = this.labClientSession.NavmenuPhotoUrl;
                if (url == null || url.Trim().Length == 0)
                {
                    url = STR_DefaultNavMenuPhotoUrl;
                }
                this.imgNavmenuPhoto.ImageUrl = url;

                /*
                 * Feedback email url
                 */
                if (this.labClientSession.FeedbackEmailUrl != null)
                {
                    this.urlFeedbackEmail.NavigateUrl = String.Format(STR_UrlFeedbackEmail_arg, this.labClientSession.FeedbackEmailUrl);
                }

                /*
                 * Lab camera url
                 */
                url = this.labClientSession.LabCameraUrl;
                if (url != null && url.Trim().Length > 0)
                {
                    aCamera.HRef = url;
                }
                this.liCamera.Visible = (url != null);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }
    }
}