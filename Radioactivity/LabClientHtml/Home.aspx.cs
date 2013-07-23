using System;
using Library.Lab;
using Library.LabClient;
using Library.LabClient.Engine;

namespace LabClientHtml
{
    public partial class Home : System.Web.UI.Page
    {
        #region Constants
        private const String STR_ClassName = "Home";
        private const Logfile.Level logLevel = Logfile.Level.Fine;
        /*
         * String constants
         */
        private const String STR_Less = "&#171; Less";
        private const String STR_More = "More &#187;";
        private const String STR_ForMoreInfoSee = "For information specific to this LabClient, see";
        #endregion

        #region Variables
        private LabClientSession labClientSession;
        #endregion

        //---------------------------------------------------------------------------------------//

        protected void Page_Load(object sender, EventArgs e)
        {
            const String methodName = "Page_Load";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            /*
             * Get the LabClientSession information from the session
             */
            this.labClientSession = (LabClientSession)Session[Consts.STRSSN_LabClient];

            if (!IsPostBack)
            {
                 /*
                  * Update LabInfo text and url if specified
                  */
                if (this.labClientSession.LabInfoText != null && this.labClientSession.LabInfoUrl != null)
                {
                    lblMoreInfo.Text = STR_ForMoreInfoSee;
                    lnkMoreInfo.Text = this.labClientSession.LabInfoText;
                    lnkMoreInfo.NavigateUrl = this.labClientSession.LabInfoUrl;
                }               
                 
                /*
                 * Don't display the extra information
                 */
                litSetupInfo.Visible = false;
                lnkbtnSetupInfo.Text = STR_More;
                litStatusInfo.Visible = false;
                lnkbtnStatusInfo.Text = STR_More;
                litResultsInfo.Visible = false;
                lnkbtnResultsInfo.Text = STR_More;
            }
            else
            {
                /*
                 * Clear labels
                 */
                lblMoreInfo.Text = null;
                lnkMoreInfo.Text = null;
            }
        }

        //---------------------------------------------------------------------------------------//

        protected void lnkbtnSetupInfo_Click(object sender, EventArgs e)
        {
            if (litSetupInfo.Visible == false)
            {
                litSetupInfo.Visible = true;
                lnkbtnSetupInfo.Text = STR_Less;
            }
            else
            {
                litSetupInfo.Visible = false;
                lnkbtnSetupInfo.Text = STR_More;
            }
        }

        //---------------------------------------------------------------------------------------//

        protected void lnkbtnStatusInfo_Click(object sender, EventArgs e)
        {
            if (litStatusInfo.Visible == false)
            {
                litStatusInfo.Visible = true;
                lnkbtnStatusInfo.Text = STR_Less;
            }
            else
            {
                litStatusInfo.Visible = false;
                lnkbtnStatusInfo.Text = STR_More;
            }
        }

        //---------------------------------------------------------------------------------------//

        protected void lnkbtnResultsInfo_Click(object sender, EventArgs e)
        {
            if (litResultsInfo.Visible == false)
            {
                litResultsInfo.Visible = true;
                lnkbtnResultsInfo.Text = STR_Less;
            }
            else
            {
                litResultsInfo.Visible = false;
                lnkbtnResultsInfo.Text = STR_More;
            }
        }

    }
}
