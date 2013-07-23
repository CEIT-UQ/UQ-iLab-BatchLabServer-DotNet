using System;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace LabServer.Redirect
{
    public class Consts
    {
        /*
         * Application configuration file key strings
         */
        public const String STRCFG_LogFilesPath = "LogFilesPath";
        public const String STRCFG_LogLevel = "LogLevel";
        public const String STRCFG_LabServerUrl = "LabServerUrl";
        public const String STRCFG_Online = "Online";
        public const String STRCFG_LabStatusMessage = "LabStatusMessage";
    }
}
