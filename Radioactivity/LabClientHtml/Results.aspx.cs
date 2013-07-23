using System;
using Library.Lab;
using Library.Lab.Types;
using Library.LabClient;
using Library.LabClient.Engine;

namespace LabClientHtml
{
    public partial class Results : System.Web.UI.Page
    {
        #region Constants
        private const String STR_ClassName = "Results";
        private const Logfile.Level logLevel = Logfile.Level.Fine;
        /*
         * String constants
         */
        private const String STR_ResultStatus_arg2 = "Experiment {0:d} - {1:s}";
        private const String STR_CsvFilename_arg2 = "{0:s}_{1:s}";
        /*
         * String constants for exception messages
         */
        private const String STRERR_ExperimentId = "Experiment Id";
        private const String STRERR_ValueNotSpecified_arg = "{0:s}: Not specified!";
        private const String STRERR_ValueNotValid_arg = "{0:s}: Not valid!";
        private const String STRERR_ExperimentFailed_arg3 = "Experiment {0:d} - {1:s}: {2:s}";
        private const String STRERR_NoResultsAvailable = "No experiment results available!";
        private const String STRERR_WrongExperimentType_arg = "Wrong experiment type: {0:s}";
        #endregion

        #region Variables
        private LabClientSession labClientSession;
        #endregion

        //-------------------------------------------------------------------------------------------------//

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
                 * Not a postback, initialise page controls
                 */
                this.ShowMessageInfo(null);
                this.PopulateCompletedIds();
                this.btnSave.Enabled = false;
                this.lblResultsTableValue.Visible = false;
            }
        }

        //-------------------------------------------------------------------------------------------------//

        protected void ddlExperimentIds_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.txbExperimentId.Text = this.ddlExperimentIds.SelectedValue;
        }

        //-------------------------------------------------------------------------------------------------//

        protected void btnRetrieve_Click(object sender, EventArgs e)
        {
            const String methodName = "btnRetrieve_Click";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            /*
             * Initialise controls
             */
            this.btnSave.Enabled = false;
            this.lblResultsTableValue.Visible = false;

            try
            {
                /*
                 * Get the experiment Id
                 */
                int experimentId = this.ParseExperimentId(txbExperimentId.Text);

                /*
                 * Retrieve the experiment results
                 */
                ResultReport resultReport = this.labClientSession.ServiceBrokerAPI.RetrieveResult(experimentId);
                if (resultReport == null)
                {
                    throw new ApplicationException(String.Format(STR_ResultStatus_arg2, experimentId, StatusCodes.Unknown.ToString()));
                }

                /*
                 * Get the experiment result status
                 */
                StatusCodes statusCode = resultReport.StatusCode;
                if (statusCode == StatusCodes.Unknown || statusCode == StatusCodes.Cancelled)
                {
                    throw new ApplicationException(String.Format(STR_ResultStatus_arg2, experimentId, statusCode.ToString()));
                }

                if (statusCode == StatusCodes.Failed)
                {
                    throw new ApplicationException(String.Format(STRERR_ExperimentFailed_arg3, experimentId, statusCode.ToString(), resultReport.ErrorMessage));
                }

                /*
                 * Get result information
                 */
                if (resultReport.XmlExperimentResults == null)
                {
                    throw new ApplicationException(STRERR_NoResultsAvailable);
                }

                ExperimentResult experimentResult = new ExperimentResult(resultReport.XmlExperimentResults);

                /*
                 * Check for correct experiment type
                 */
                if (experimentResult.Title.Equals(this.labClientSession.Title, StringComparison.OrdinalIgnoreCase) == false)
                {
                    throw new ApplicationException(String.Format(STRERR_WrongExperimentType_arg, experimentResult.Title));
                }

                /*
                 * Finally.... display the results on the web page
                 */
                experimentResult.CreateHtmlResultInfo();
                this.lblResultsTableValue.Text = experimentResult.GetHtmlResultInfo();
                this.lblResultsTableValue.Visible = true;

                /*
                 * Build a CSV string from the result report and store in a hidden label
                 */
                experimentResult.CreateCsvResultInfo();
                this.hfRetrievedExperimentId.Value = experimentResult.ExperimentId.ToString();
                this.hfCsvExperimentResults.Value = experimentResult.GetCsvResultInfo();
                this.btnSave.Enabled = true;

                ShowMessageInfo(String.Format(STR_ResultStatus_arg2, experimentId, statusCode.ToString()));
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
                ShowMessageError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        protected void btnSave_Click(object sender, EventArgs e)
        {
            const String methodName = "btnSave_Click";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            try
            {
                /*
                 * Download the result string as an Excel csv file
                 */
                String filename = String.Format(STR_CsvFilename_arg2, this.labClientSession.Title, this.hfRetrievedExperimentId.Value);
                String attachmentCsv = String.Format(Consts.STRRSP_AttachmentCsv_arg, filename);
                Response.ContentType = Consts.STRRSP_ContentTypeCsv;
                Response.Clear();
                Response.AddHeader(Consts.STRRSP_Disposition, attachmentCsv);
                Response.Write(this.hfCsvExperimentResults.Value);
                Response.Flush();
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
                ShowMessageError(ex.Message);
            }

            /*
             * End the http response which raises the System.Web.HttpApplication.EndRequest event
             */
            Response.End();
        }

        //=================================================================================================//

        private void PopulateCompletedIds()
        {
            const String methodName = "PopulateCompletedIds";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            /*
             * Initialise controls
             */
            this.txbExperimentId.Text = null;
            this.ddlExperimentIds.Visible = false;

            /*
             * Get the completed experiment Ids
             */
            int[] completedIds = this.labClientSession.CompletedIds;
            if (completedIds != null)
            {
                if (completedIds.Length == 1)
                {
                    /*
                     * Show the one that has been completed
                     */
                    this.txbExperimentId.Text = completedIds[0].ToString();
                }
                else if (completedIds.Length > 1)
                {
                    /*
                     * More than one has been completed, show them all
                     */
                    this.ddlExperimentIds.Items.Clear();
                    for (int i = 0; i < completedIds.Length; i++)
                    {
                        this.ddlExperimentIds.Items.Add(completedIds[i].ToString());
                    }
                    this.ddlExperimentIds.Visible = true;
                    this.txbExperimentId.Text = completedIds[0].ToString();
                }
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        private int ParseExperimentId(String value)
        {
            int experimentId = 0;

            /*
             * Get the experiment Id
             */
            if (value == null || value.Trim().Length == 0)
            {
                throw new ArgumentException(String.Format(STRERR_ValueNotSpecified_arg, STRERR_ExperimentId));
            }

            try
            {
                experimentId = Int32.Parse(value);
            }
            catch (FormatException)
            {
                throw new ArgumentException(String.Format(STRERR_ValueNotValid_arg, STRERR_ExperimentId));
            }

            if (experimentId <= 0)
            {
                throw new ArgumentException(String.Format(STRERR_ValueNotValid_arg, STRERR_ExperimentId));
            }

            return experimentId;
        }
        //-------------------------------------------------------------------------------------------------//

        private void ShowMessageInfo(String message)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = Consts.STRSTL_InfoMessage;
            lblMessage.Visible = (message != null);
        }

        //-------------------------------------------------------------------------------------------------//

        private void ShowMessageError(String message)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = Consts.STRSTL_ErrorMessage;
            lblMessage.Visible = (message != null);
        }
    }
}
