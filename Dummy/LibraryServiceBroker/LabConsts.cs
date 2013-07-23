using System;

namespace Library.ServiceBroker
{
    public class LabConsts
    {
        /*
         * Application configuration file key strings
         */
        public const String STRCFG_LogFilesPath = "LogFilesPath";
        public const String STRCFG_LogLevel = "LogLevel";
        public const String STRCFG_SqlConnection = "SqlConnection";
        public const String STRCFG_ServiceBrokerGuid = "ServiceBrokerGuid";
        public const String STRCFG_Authenticating = "Authenticating";
        public const String STRCFG_LogAuthentication = "LogAuthentication";
        public const String STRCFG_CouponId = "CouponId";
        public const String STRCFG_CouponPasskey = "CouponPasskey";
        public const String STRCFG_LabServer_arg = "LabServer{0:d}";
        /*
         * Constants for LabServer CSV string information
         */
        public const int INDEX_LabServerGuid = 0;
        public const int INDEX_LabServerUrl = 1;
        public const int INDEX_LabServerOutPasskey = 2;
        public const int INDEX_LabServerInPasskey = 3;
        public const char CHRCSV_SplitterChar = ',';
    }
}
