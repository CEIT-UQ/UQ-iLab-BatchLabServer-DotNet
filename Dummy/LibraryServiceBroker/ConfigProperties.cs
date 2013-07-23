using System;
using System.Collections.Generic;
using Library.Lab;
using Library.Lab.Database;
using Library.ServiceBroker.Types;
using Library.Lab.Utilities;

namespace Library.ServiceBroker
{
    public class ConfigProperties
    {
        #region Constants
        private const String STR_ClassName = "ConfigProperties";
        private const Logfile.Level logLevel = Logfile.Level.Fine;
        /*
         * String constants for exception messages
         */
        private const String STRERR_ServiceBrokerGuid = "serviceBrokerGuid";
        private const String STRERR_LabServerGuid = "guid";
        private const String STRERR_LabServerServiceUrl = "serviceUrl";
        private const String STRERR_LabServerOutPasskey = "outPasskey";
        private const String STRERR_LabServerInPasskey = "inPasskey";
        /*
         * String constants for logfile messages
         */
        private const String STRLOG_LabServerInfo_arg5 = "LabServer {0:d} - Guid: {1:s}  ServiceUrl: {2:s}  OutPasskey: {3:s}  InPasskey: {4:s}";
        #endregion

        #region Properties

        public static string ClassName
        {
            get { return STR_ClassName; }
        }

        private DBConnection dbConnection;
        private String serviceBrokerGuid;
        private bool authenticating;
        private bool logAuthentication;
        private long couponId;
        private String couponPasskey;
        private Dictionary<String, LabServerInfo> labServerInfos;

        public DBConnection DbConnection
        {
            get { return dbConnection; }
        }

        public String ServiceBrokerGuid
        {
            get { return serviceBrokerGuid; }
        }

        public bool Authenticating
        {
            get { return authenticating; }
        }

        public bool LogAuthentication
        {
            get { return logAuthentication; }
        }

        public long CouponId
        {
            get { return couponId; }
        }

        public String CouponPasskey
        {
            get { return couponPasskey; }
        }

        public Dictionary<String, LabServerInfo> LabServerInfos
        {
            get { return labServerInfos; }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        public ConfigProperties()
        {
            const string methodName = "ConfigProperties";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            try
            {
                /*
                 * Create an instance of the database connection
                 */
                String sqlConnectionString = AppSetting.GetStringAppSetting(LabConsts.STRCFG_SqlConnection);
                this.dbConnection = new DBConnection(sqlConnectionString);

                /*
                 * Get the ServiceBroker Guid
                 */
                this.serviceBrokerGuid = AppSetting.GetStringAppSetting(LabConsts.STRCFG_ServiceBrokerGuid);

                /*
                 * Get LabClient authentication
                 */
                this.authenticating = AppSetting.GetBoolAppSetting(LabConsts.STRCFG_Authenticating, true);
                this.logAuthentication = AppSetting.GetBoolAppSetting(LabConsts.STRCFG_LogAuthentication, false);

                /*
                 * Get coupon Id and passkey for LabClient authentication
                 */
                this.couponId = AppSetting.GetLongAppSetting(LabConsts.STRCFG_CouponId);
                this.couponPasskey = AppSetting.GetStringAppSetting(LabConsts.STRCFG_CouponPasskey);

                /*
                 * Get the LabServer service information
                 */
                char[] splitterCharArray = new char[] { LabConsts.CHRCSV_SplitterChar };
                this.labServerInfos = new Dictionary<String, LabServerInfo>();
                for (int i = 0; true; i++)
                {
                    /*
                     * Get the LabServer info if it exists
                     */
                    String[] splitLabServerInfo;
                    try
                    {
                        String csvLabServerInfo = AppSetting.GetAppSetting(String.Format(LabConsts.STRCFG_LabServer_arg, i));
                        splitLabServerInfo = csvLabServerInfo.Split(splitterCharArray);
                    }
                    catch
                    {
                        break;
                    }

                    /*
                     * Extract guid and check
                     */
                    String guid = splitLabServerInfo[LabConsts.INDEX_LabServerGuid];
                    if (guid == null || guid.Trim().Length == 0)
                    {
                        throw new ArgumentNullException(STRERR_LabServerGuid);
                    }
                    guid = guid.Trim().ToUpper();

                    /*
                     * Extract service url and check
                     */
                    String serviceUrl = splitLabServerInfo[LabConsts.INDEX_LabServerUrl];
                    if (serviceUrl == null || serviceUrl.Trim().Length == 0)
                    {
                        throw new ArgumentNullException(STRERR_LabServerServiceUrl);
                    }
                    serviceUrl = serviceUrl.Trim();

                    /*
                     * Extract the outgoing passkey and check
                     */
                    String outPasskey = splitLabServerInfo[LabConsts.INDEX_LabServerOutPasskey];
                    if (outPasskey == null || outPasskey.Trim().Length == 0)
                    {
                        throw new ArgumentNullException(STRERR_LabServerOutPasskey);
                    }
                    outPasskey = outPasskey.Trim();

                    /*
                     * Extract the incoming passkey and check
                     */
                    String inPasskey = splitLabServerInfo[LabConsts.INDEX_LabServerInPasskey];
                    if (inPasskey == null || inPasskey.Trim().Length == 0)
                    {
                        throw new ArgumentNullException(STRERR_LabServerInPasskey);
                    }
                    inPasskey = inPasskey.Trim();

                    /*
                     * Store information
                     */
                    this.labServerInfos.Add(guid, new LabServerInfo(guid, serviceUrl, outPasskey, inPasskey));

                    Logfile.Write(String.Format(STRLOG_LabServerInfo_arg5, i, guid, serviceUrl, outPasskey, inPasskey));
                }
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
