using System;
using Library.Lab;
using Library.Lab.Types;
using Library.LabClient;
using Library.LabClient.Engine;

namespace LabClientHtml
{
    public partial class Status : System.Web.UI.Page
    {
        #region Constants
        private const String STR_ClassName = "Status";
        private const Logfile.Level logLevel = Logfile.Level.Fine;
        /*
         * String constants
         */
        private const String STR_ExperimentStatus_arg2 = "Experiment {0:d} - {1:s}";
        private const String STR_QueueStatusMessage_arg2 = "Queue length is {0:d} and wait time is {1:s}.";
        private const String STR_QueueStatusNotAvailable = "Queue status not available.";
        private const String STR_TimeRemainingIs = " Time remaining is {0:s}";
        private const String STR_QueuePositionRunIn_arg2 = "Queue position is {0:d} and it will run in {1:s}";
        private const String STR_ExperimentCancelled_arg = "Experiment {0:d} has been cancelled.";
        private const String STR_ExperimentNotCancelled_arg = "Experiment {0:d} could not be cancelled!";
        /*
         * String constants for exception messages
         */
        private const String STRERR_ExperimentId = "Experiment Id";
        private const String STRERR_ValueNotSpecified_arg = "{0:s}: Not specified!";
        private const String STRERR_ValueNotValid_arg = "{0:s}: Not valid!";
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
                this.PopulateSubmittedIds();
            }

            /*
             * Refresh the LabServer status
             */
            this.Refresh();
        }

        //-------------------------------------------------------------------------------------------------//

        protected void ddlExperimentIDs_SelectedIndexChanged(object sender, EventArgs e)
        {
            txbExperimentId.Text = ddlExperimentIds.SelectedValue;
        }

        //-------------------------------------------------------------------------------------------------//

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            const String methodName = "btnRefresh_Click";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            this.Refresh();

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        protected void btnCheck_Click(object sender, EventArgs e)
        {
            const String methodName = "btnCheck_Click";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            try
            {
                /*
                 * Get the experiment Id
                 */
                int experimentId = this.ParseExperimentId(txbExperimentId.Text);

                /*
                 * Get the experiment status
                 */
                LabExperimentStatus labExperimentStatus = this.labClientSession.ServiceBrokerAPI.GetExperimentStatus(experimentId);
                if (labExperimentStatus == null)
                {
                    throw new ApplicationException(String.Format(STR_ExperimentStatus_arg2, experimentId, StatusCodes.Unknown.ToString()));
                }
                ExperimentStatus experimentStatus = labExperimentStatus.ExperimentStatus;
                if (experimentStatus == null)
                {
                    throw new ApplicationException(String.Format(STR_ExperimentStatus_arg2, experimentId, StatusCodes.Unknown.ToString()));
                }

                /*
                 * Get the status code
                 */
                StatusCodes statusCode = experimentStatus.StatusCode;
                if (statusCode == StatusCodes.Unknown)
                {
                    throw new ApplicationException(String.Format(STR_ExperimentStatus_arg2, experimentId, StatusCodes.Unknown.ToString()));
                }

                if (statusCode == StatusCodes.Running)
                {
                    /*
                     * Experiment is currently running, display time remaining
                     */
                    int seconds = (int)labExperimentStatus.ExperimentStatus.EstimatedRemainingRuntime;
                    ShowMessageInfo(String.Format(STR_TimeRemainingIs, this.labClientSession.FormatTimeMessage(seconds)));
                }
                else if (statusCode == StatusCodes.Waiting)
                {
                    /*
                     * Experiment is waiting to run, get queue position (zero-based)
                     */
                    WaitEstimate waitEstimate = labExperimentStatus.ExperimentStatus.WaitEstimate;
                    int position = waitEstimate.EffectiveQueueLength;
                    int seconds = (int)waitEstimate.EstimatedWait;
                    seconds = (seconds < 0) ? 0 : seconds;
                    ShowMessageInfo(String.Format(STR_QueuePositionRunIn_arg2, position, this.labClientSession.FormatTimeMessage(seconds)));
                }
                else if (statusCode == StatusCodes.Completed || statusCode == StatusCodes.Failed || statusCode == StatusCodes.Cancelled)
                {
                    /*
                     * Experiment status no longer needs to be checked
                     */
                    this.labClientSession.DeleteSubmittedId(experimentId);
                    this.labClientSession.AddCompletedId(experimentId);
                    PopulateSubmittedIds();
                    ShowMessageInfo(String.Format(STR_ExperimentStatus_arg2, experimentId, statusCode.ToString()));
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
                ShowMessageError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            const String methodName = "btnCancel_Click";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            try
            {
                /*
                 * Get the experiment Id
                 */
                int experimentId = this.ParseExperimentId(txbExperimentId.Text);

                /*
                 * Attempt to cancel the experiment
                 */
                bool cancelled = this.labClientSession.ServiceBrokerAPI.Cancel(experimentId);
                if (cancelled == false)
                {
                    throw new ApplicationException(String.Format(STR_ExperimentNotCancelled_arg, experimentId));
                }

                this.ShowMessageInfo(String.Format(STR_ExperimentCancelled_arg, experimentId));
            }
            catch (Exception ex)
            {
                Logfile.Write(logLevel, ex.ToString());
                ShowMessageError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //=================================================================================================//

        private void PopulateSubmittedIds()
        {
            const String methodName = "PopulateSubmittedIds";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            /*
             * Initialise controls
             */
            this.txbExperimentId.Text = null;
            this.ddlExperimentIds.Visible = false;

            /*
             * Get the submitted experiment Ids
             */
            int[] submittedIds = this.labClientSession.SubmittedIds;
            if (submittedIds != null)
            {
                if (submittedIds.Length == 1)
                {
                    /*
                     * Show the one that has been submitted
                     */
                    this.txbExperimentId.Text = submittedIds[0].ToString();
                }
                else if (submittedIds.Length > 1)
                {
                    /*
                     * More than one has been submitted, show them all
                     */
                    this.ddlExperimentIds.Items.Clear();
                    this.ddlExperimentIds.Items.Add(String.Empty);
                    for (int i = 0; i < submittedIds.Length; i++)
                    {
                        this.ddlExperimentIds.Items.Add(submittedIds[i].ToString());
                    }
                    this.ddlExperimentIds.Visible = true;
                }
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        private void Refresh()
        {
            const String methodName = "Refresh";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            try
            {
                /*
                 * Get the LabServer's status and display
                 */
                LabStatus labStatus = this.labClientSession.ServiceBrokerAPI.GetLabStatus();
                this.lblLabStatusMessage.Text = labStatus.LabStatusMessage;

                /*
                 * Check if the LabServer is online
                 */
                if (labStatus.Online == true)
                {
                    /*
                     * Get the queue length and wait time
                     */
                    WaitEstimate waitEstimate = this.labClientSession.ServiceBrokerAPI.GetEffectiveQueueLength();
                    this.lblQueueStatusMessage.Text = String.Format(STR_QueueStatusMessage_arg2,
                            waitEstimate.EffectiveQueueLength, this.labClientSession.FormatTimeMessage((int)waitEstimate.EstimatedWait));
                }
                else
                {
                    this.lblQueueStatusMessage.Text = STR_QueueStatusNotAvailable;
                }
                this.lblOnline.Visible = (labStatus.Online == true);
                this.lblOffline.Visible = (labStatus.Online == false);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
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
