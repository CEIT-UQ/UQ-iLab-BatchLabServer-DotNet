using System;

namespace Library.LabClient.Engine
{
    public class LabConsts
    {
        /*
         * Application configuration file key strings
         */
        public const String STRCFG_LogFilesPath = "LogFilesPath";
        public const String STRCFG_LogLevel = "LogLevel";
        public const String STRCFG_ServiceUrl = "ServiceURL";
        public const String STRCFG_LabServerId = "LabserverID";
        public const String STRCFG_MultiSubmit = "MultiSubmit";
        public const String STRCFG_FeedbackEmail = "FeedbackEmail";
        /*
         * HttpRequest query strings
         */
        public const String STRQRY_CouponId = "CouponId";
        public const String STRQRY_Coupon_Id = "Coupon_Id";
        public const String STRQRY_Passkey = "Passkey";
        public const String STRQRY_ServiceUrl = "ServiceUrl";
        public const String STRQRY_LabServerId = "LabserverId";
        public const String STRQRY_MultiSubmit = "MultiSubmit";
        /*
         * Session variables
         */
        public const String STRSSN_LabClient = "LabClient";
        public const String STRSSN_SubmittedID = "SubmittedID";
        public const String STRSSN_CompletedID = "CompletedID";
        public const String STRSSN_SubmittedIDs = "SubmittedIDs";
        public const String STRSSN_CompletedIDs = "CompletedIDs";
        /*
         * XML elements in the lab configuration String
         */
        public const String STRXML_LabConfiguration = "labConfiguration";
        public const String STRXML_ATTR_Title = "title";
        public const String STRXML_ATTR_Version = "version";
        public const String STRXML_NavmenuPhoto = "navmenuPhoto";
        public const String STRXML_Image = "image";
        public const String STRXML_LabCamera = "labCamera";
        public const String STRXML_Url = "url";
        public const String STRXML_LabInfo = "labInfo";
        public const String STRXML_Text = "text";
        public const String STRXML_Configuration = "configuration";
        public const String STRXML_Setup = "setup";
        public const String STRXML_ATTR_Id = "id";
        public const String STRXML_Name = "name";
        public const String STRXML_Description = "description";
        /*
         * XML elements in the experiment specification String
         */
        public const String STRXML_ExperimentSpecification = "experimentSpecification";
        public const String STRXML_SetupId = "setupId";
        public const String STRXML_Validation = "validation";
        /*
         * XML elements in the experiment results String
         */
        public const String STRXML_ExperimentResult = "experimentResult";
        public const String STRXML_Timestamp = "timestamp";
        public const String STRXML_Title = "title";
        public const String STRXML_Version = "version";
        public const String STRXML_ExperimentId = "experimentId";
        public const String STRXML_UnitId = "unitId";
        public const String STRXML_SetupName = "setupName";
        public const String STRXML_DataType = "dataType";
        /*
         * Result String download response
         */
        public const String STRRSP_ContentTypeCsv = "Application/x-msexcel";
        public const String STRRSP_Disposition = "content-disposition";
        public const String STRRSP_AttachmentCsv_arg = "attachment; filename=\"{0:s}.csv\"";
        /*
         * Webpage URLs
         */
        public const String STRURL_Expired = "~/Expired.htm";
        /*
         * Webpage style classes
         */
        public const String STRSTL_InfoMessage = "infomessage";
        public const String STRSTL_WarningMessage = "warningmessage";
        public const String STRSTL_ErrorMessage = "errormessage";
        /*
         * String constants
         */
        public const char CHRCSV_SplitterChar = ',';
        public const String STR_CsvSplitter = ",";
    }
}
