using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using System.Xml;
using Library.Lab;
using Library.Lab.Types;
using Library.LabClient;
using Library.LabClient.Engine;
using Library.LabClient.Engine.Types;

namespace LabClientHtml
{
    public partial class Setup : System.Web.UI.Page
    {
        #region Constants
        private const String STR_ClassName = "Setup";
        private const Logfile.Level logLevel = Logfile.Level.Fine;
        /*
         * String constants
         */
        private const String STR_SelectAbsorber = "Select absorber";
        private const String STR_AvailableAbsorbers = "Available absorbers";
        private const String STR_SelectDistance = "Select distance";
        private const String STR_AvailableDistances = "Available distances";
        private const String STR_HintRange_arg2 = "Range: {0:d} to {1:d}";
        private const String STR_SpecificationValid_arg = "Specification is valid. Execution time will be {0:s}.";
        private const String STR_SubmissionSuccessful_arg2 = "Submission was successful. Experiment # is {0:d} and execution time is {1:s}.";
        private const String STR_ExperimentNumberHasBeenSubmitted_arg = "Experiment {0:d} has been submitted.";
        private const String STR_ExperimentNumbersHaveBeenSubmitted_arg = "Experiments {0:s} have been submitted.";
        /*
         * String constants for XML elements
         */
        public const String STRXML_SomeParameter = "someParameter";
        /*
         * String constants for exception messages
         */
        private const String STRERR_ValidationFailed = "Validation Failed!";
        private const String STRERR_SubmissionFailed = "Submission Failed!";
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
                 * Not a postback, initialise page controls
                 */
                this.ShowMessageInfo(null);
                this.PopulatePage();

                /*
                 * Check if an experiment has been submitted
                 */
                int[] submittedIds = this.labClientSession.SubmittedIds;
                if (submittedIds != null)
                {
                    if (submittedIds.Length == 1)
                    {
                        /*
                         * Show the one that has been submitted
                         */
                        this.ShowMessageInfo(String.Format(STR_ExperimentNumberHasBeenSubmitted_arg, submittedIds[0]));
                    }
                    else if (submittedIds.Length > 1)
                    {
                        /*
                         * More than one has been submitted, show them all
                         */
                        String arg = "";
                        for (int i = 0; i < submittedIds.Length; i++)
                        {
                            if (i > 0)
                            {
                                arg += ", ";
                            }
                            arg += submittedIds[i].ToString();
                        }
                        this.ShowMessageInfo(String.Format(STR_ExperimentNumbersHaveBeenSubmitted_arg, arg));
                    }
                }
                else
                {
                    this.btnSubmit.Enabled = true;
                }
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        protected void ddlExperimentSetups_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.UpdatePage();
        }

        //-------------------------------------------------------------------------------------------------//

        protected void btnAddAbsorber_Click(object sender, EventArgs e)
        {
            /*
             * Add absorber to the selected list and remove from available list
             */
            this.ddlSelectedAbsorbers.Items.Add(this.ddlAvailableAbsorbers.Text);
            this.ddlAvailableAbsorbers.Items.Remove(this.ddlAvailableAbsorbers.Text);

            /*
             * Disable Add button if no more absorbers to select
             */
            this.btnAddAbsorber.Enabled = (this.ddlAvailableAbsorbers.Items.Count > 0);
        }

        //-------------------------------------------------------------------------------------------------//

        protected void btnClearAbsorbers_Click(object sender, EventArgs e)
        {
            try
            {
                /*
                 * Get available absorbers
                 */
                XmlDocument document = XmlUtilities.GetDocumentFromString(this.labClientSession.XmlConfiguration);
                XmlNode nodeConfiguration = XmlUtilities.GetRootNode(document, Consts.STRXML_Configuration);
                XmlNode node = XmlUtilities.GetChildNode(nodeConfiguration, Consts.STRXML_Absorbers);
                ArrayList nodeList = XmlUtilities.GetChildNodeList(node, Consts.STRXML_Absorber);

                /*
                 * Clear the available dropdown list and add each absorber
                 */
                this.ddlAvailableAbsorbers.Items.Clear();
                foreach (XmlNode nodeAbsorber in nodeList)
                {
                    String name = XmlUtilities.GetChildValue(nodeAbsorber, Consts.STRXML_Name);
                    this.ddlAvailableAbsorbers.Items.Add(name);
                }
                this.ddlAvailableAbsorbers.SelectedValue = XmlUtilities.GetAttributeValue(node, Consts.STRXML_ATTR_Default, false);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            /*
             * Clear the selected dropdown list and enable the Add button
             */
            this.ddlSelectedAbsorbers.Items.Clear();
            this.btnAddAbsorber.Enabled = true;
        }

        //-------------------------------------------------------------------------------------------------//

        protected void btnAddDistance_Click(object sender, EventArgs e)
        {
            /*
             * Add distance to the selected list and remove from available list
             */
            this.ddlSelectedDistances.Items.Add(this.ddlAvailableDistances.Text);
            this.ddlAvailableDistances.Items.Remove(this.ddlAvailableDistances.Text);

            /*
             * Disable Add button if no more distances to select
             */
            this.btnAddDistance.Enabled = (this.ddlAvailableDistances.Items.Count > 0);
        }

        //-------------------------------------------------------------------------------------------------//

        protected void btnClearDistances_Click(object sender, EventArgs e)
        {
            try
            {
                /*
                 * Get available distances
                 */
                XmlDocument document = XmlUtilities.GetDocumentFromString(this.labClientSession.XmlConfiguration);
                XmlNode nodeConfiguration = XmlUtilities.GetRootNode(document, Consts.STRXML_Configuration);

                /*
                 * Get all distances
                 */
                XmlNode node = XmlUtilities.GetChildNode(nodeConfiguration, Consts.STRXML_Distances);
                int minimum = XmlUtilities.GetChildValueAsInt(node, Consts.STRXML_Minimum);
                int maximum = XmlUtilities.GetChildValueAsInt(node, Consts.STRXML_Maximum);
                int stepsize = XmlUtilities.GetChildValueAsInt(node, Consts.STRXML_Stepsize);
                ArrayList arrayList = new ArrayList();

                /*
                 * Clear the available dropdown list and add each distance
                 */
                this.ddlAvailableDistances.Items.Clear();
                for (int distance = minimum; distance <= maximum; distance += stepsize)
                {
                    this.ddlAvailableDistances.Items.Add(distance.ToString());
                }
                this.ddlAvailableDistances.SelectedValue = XmlUtilities.GetAttributeValue(node, Consts.STRXML_ATTR_Default, false);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            /*
             * Clear the selected dropdown list and enable the Add button
             */
            this.ddlSelectedDistances.Items.Clear();
            this.btnAddAbsorber.Enabled = true;
        }

        //-------------------------------------------------------------------------------------------------//

        protected void btnValidate_Click(object sender, EventArgs e)
        {
            const String methodName = "btnValidate_Click";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            try
            {
                /*
                 * Create the XML experiment specification string
                 */
                String xmlSpecification = this.CreateXmlSpecification();

                /*
                 * Validate the experiment specification
                 */
                ValidationReport validationReport = this.labClientSession.ServiceBrokerAPI.Validate(xmlSpecification);
                if (validationReport == null)
                {
                    throw new ApplicationException(STRERR_ValidationFailed);
                }

                if (validationReport.Accepted == false)
                {
                    throw new ApplicationException(validationReport.ErrorMessage);
                }

                /*
                 * Validation was accepted
                 */
                String message = String.Format(STR_SpecificationValid_arg, this.labClientSession.FormatTimeMessage((int)validationReport.EstimatedRuntime));
                ShowMessageInfo(message);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
                ShowMessageError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //-------------------------------------------------------------------------------------------------//

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            const String methodName = "btnSubmit_Click";
            Logfile.WriteCalled(logLevel, STR_ClassName, methodName);

            try
            {
                /*
                 * Create the XML experiment specification string
                 */
                String xmlSpecification = this.CreateXmlSpecification();

                /*
                 * Submit the experiment specification
                 */
                ClientSubmissionReport clientSubmissionReport = this.labClientSession.ServiceBrokerAPI.Submit(xmlSpecification);
                if (clientSubmissionReport == null)
                {
                    throw new ApplicationException(STRERR_SubmissionFailed);
                }

                /*
                 * Check if submission was successful
                 */
                ValidationReport validationReport = clientSubmissionReport.ValidationReport;
                if (validationReport.Accepted == false)
                {
                    throw new ApplicationException(validationReport.ErrorMessage);
                }

                /*
                 * Submission was accepted
                 */
                String message = String.Format(STR_SubmissionSuccessful_arg2,
                        clientSubmissionReport.ExperimentId, this.labClientSession.FormatTimeMessage((int)validationReport.EstimatedRuntime));
                ShowMessageInfo(message);

                /*
                 * Update session with submitted id
                 */
                this.labClientSession.AddSubmittedId(clientSubmissionReport.ExperimentId);
                if (this.labClientSession.MultiSubmit == false)
                {
                    /*
                     * Disable futher submission
                     */
                    this.btnSubmit.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
                ShowMessageError(ex.Message);
            }

            Logfile.WriteCompleted(logLevel, STR_ClassName, methodName);
        }

        //=================================================================================================//

        private void PopulatePage()
        {
            /*
             * Experiment setups
             */
            this.ddlExperimentSetups.Items.Clear();
            foreach (KeyValuePair<String, SetupInfo> keyValuePair in this.labClientSession.SetupInfoMap)
            {
                SetupInfo setupInfo = keyValuePair.Value;
                this.ddlExperimentSetups.Items.Add(new ListItem(setupInfo.Name, setupInfo.Name));
                if (this.ddlExperimentSetups.Items.Count == 1)
                {
                    this.lblSetupDescription.Text = setupInfo.Description;
                }
            }
            this.ddlExperimentSetups.SelectedIndex = 0;
            this.ddlExperimentSetups_SelectedIndexChanged(this, null);

            try
            {
                /*
                 * Available sources
                 */
                XmlDocument document = XmlUtilities.GetDocumentFromString(this.labClientSession.XmlConfiguration);
                XmlNode nodeConfiguration = XmlUtilities.GetRootNode(document, Consts.STRXML_Configuration);
                XmlNode node = XmlUtilities.GetChildNode(nodeConfiguration, Consts.STRXML_Sources);
                ArrayList nodeList = XmlUtilities.GetChildNodeList(node, Consts.STRXML_Source);
                foreach (XmlNode nodeSource in nodeList)
                {
                    String name = XmlUtilities.GetChildValue(nodeSource, Consts.STRXML_Name);
                    this.ddlAvailableSources.Items.Add(name);
                }
                this.ddlAvailableSources.SelectedValue = XmlUtilities.GetAttributeValue(node, Consts.STRXML_ATTR_Default, false);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            this.UpdatePage();
        }

        //-------------------------------------------------------------------------------------------------//

        private void UpdatePage()
        {
            SetupInfo setupInfo = null;
            String[] selectedAbsorbers = null;
            String[] selectedDistances = null;

            try
            {
                /*
                 * Get the SetupInfo for the selected setup
                 */
                String key = this.ddlExperimentSetups.SelectedItem.Text;
                if (this.labClientSession.SetupInfoMap.TryGetValue(key, out setupInfo) == false)
                {
                    throw new ArgumentNullException();
                }

                /*
                 * Get the selected and default values for the specified setup
                 */
                XmlDocument document = XmlUtilities.GetDocumentFromString(setupInfo.XmlSetup);
                XmlNode nodeRoot = XmlUtilities.GetRootNode(document, Consts.STRXML_Setup);

                this.txbDuration.Text = XmlUtilities.GetChildValue(nodeRoot, Consts.STRXML_Duration);
                this.txbTrials.Text = XmlUtilities.GetChildValue(nodeRoot, Consts.STRXML_Repeat);

                char[] splitterCharArray = new char[] { Consts.CHRCSV_SplitterChar };

                String csvDistances = XmlUtilities.GetChildValue(nodeRoot, Consts.STRXML_Distance);
                selectedDistances = csvDistances.Split(splitterCharArray);

                try
                {
                    /*
                     * Absorber default values may not exist
                     */
                    String csvAbsorbers = XmlUtilities.GetChildValue(nodeRoot, Consts.STRXML_AbsorberName);
                    selectedAbsorbers = csvAbsorbers.Split(splitterCharArray);
                }
                catch (Exception)
                {
                }

                /*
                 * Validation boundary values
                 */
                document = XmlUtilities.GetDocumentFromString(this.labClientSession.XmlValidation);
                nodeRoot = XmlUtilities.GetRootNode(document, Consts.STRXML_Validation);

                XmlNode nodeValidationRange = XmlUtilities.GetChildNode(nodeRoot, Consts.STRXML_ValidationDuration);
                int minimum = XmlUtilities.GetChildValueAsInt(nodeValidationRange, Consts.STRXML_Minimum);
                int maximum = XmlUtilities.GetChildValueAsInt(nodeValidationRange, Consts.STRXML_Maximum);
                this.txbDuration.ToolTip = String.Format(STR_HintRange_arg2, minimum, maximum);

                nodeValidationRange = XmlUtilities.GetChildNode(nodeRoot, Consts.STRXML_ValidationRepeat);
                minimum = XmlUtilities.GetChildValueAsInt(nodeValidationRange, Consts.STRXML_Minimum);
                maximum = XmlUtilities.GetChildValueAsInt(nodeValidationRange, Consts.STRXML_Maximum);
                this.txbTrials.ToolTip = String.Format(STR_HintRange_arg2, minimum, maximum);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.ToString());
            }

            /*
             * Clear selected lists and populate available lists
             */
            this.btnClearAbsorbers_Click(this, null);
            this.btnClearDistances_Click(this, null);

            /*
             * Show/hide the page controls for the specified setup
             */
            switch (setupInfo.Id)
            {
                case Consts.STRXML_SetupId_RadioactivityVsTime:
                case Consts.STRXML_SetupId_SimActivityVsTime:
                case Consts.STRXML_SetupId_SimActivityVsTimeNoDelay:
                    /*
                     * Hide selected absorber list and hide selected distance list
                     */
                    this.pnlSelectedAbsorbers.Visible = false;
                    this.pnlSelectedDistances.Visible = false;
                    this.btnAddAbsorber.Visible = false;
                    this.btnAddDistance.Visible = false;

                    /*
                     * Update hints
                     */
                    this.ddlAvailableAbsorbers.ToolTip = STR_SelectAbsorber;
                    this.ddlAvailableDistances.ToolTip = STR_SelectDistance;

                    /*
                     * Select distance
                     */
                    this.ddlAvailableDistances.SelectedValue = selectedDistances[0];
                    break;

                case Consts.STRXML_SetupId_RadioactivityVsDistance:
                case Consts.STRXML_SetupId_SimActivityVsDistance:
                case Consts.STRXML_SetupId_SimActivityVsDistanceNoDelay:
                    /*
                     * Hide selected absorber list and show selected distance list
                     */
                    this.pnlSelectedAbsorbers.Visible = false;
                    this.pnlSelectedDistances.Visible = true;
                    this.btnAddAbsorber.Visible = false;
                    this.btnAddDistance.Visible = true;

                    /*
                     * Update hints
                     */
                    this.ddlAvailableAbsorbers.ToolTip = STR_SelectAbsorber;
                    this.ddlAvailableDistances.ToolTip = STR_AvailableDistances;

                    /*
                     * Populate selected distances
                     */
                    foreach (String distance in selectedDistances)
                    {
                        this.ddlAvailableDistances.SelectedValue = distance;
                        this.btnAddDistance_Click(this, null);
                    }

                    /*
                     * Select distance
                     */
                    this.ddlSelectedDistances.SelectedIndex = 0;
                    break;

                case Consts.STRXML_SetupId_RadioactivityVsAbsorber:
                case Consts.STRXML_SetupId_SimActivityVsAbsorber:
                case Consts.STRXML_SetupId_SimActivityVsAbsorberNoDelay:
                    /*
                     * Show selected absorber list and hide selected distance list
                     */
                    this.pnlSelectedAbsorbers.Visible = true;
                    this.pnlSelectedDistances.Visible = false;
                    this.btnAddAbsorber.Visible = true;
                    this.btnAddDistance.Visible = false;

                    /*
                     * Update hints
                     */
                    this.ddlAvailableAbsorbers.ToolTip = STR_AvailableAbsorbers;
                    this.ddlAvailableDistances.ToolTip = STR_SelectDistance;

                    /*
                     * Populate selected absorbers
                     */
                    foreach (String absorber in selectedAbsorbers)
                    {
                        this.ddlAvailableAbsorbers.SelectedValue = absorber;
                        this.btnAddAbsorber_Click(this, null);
                    }

                    /*
                     * Select distance
                     */
                    this.ddlAvailableDistances.SelectedValue = selectedDistances[0];
                    break;
            }

            /*
             * Hide the information message
             */
            this.ShowMessageInfo(null);
        }

        //-------------------------------------------------------------------------------------------------//

        private String CreateXmlSpecification()
        {
            /*
             * Create an instance of the experiment specification
             */
            ExperimentSpecification experimentSpecification = new ExperimentSpecification(this.labClientSession.XmlSpecification);

            /*
             * Add the Setup Id for the selected setup
             */
            SetupInfo setupInfo = null;
            String key = this.ddlExperimentSetups.SelectedItem.Text;
            if (this.labClientSession.SetupInfoMap.TryGetValue(key, out setupInfo) == true)
            {
                experimentSpecification.SetupId = setupInfo.Id;
            }

            /*
             * Add specification information
             */
            experimentSpecification.SetupName = this.ddlExperimentSetups.SelectedItem.Text;
            experimentSpecification.Source = this.ddlAvailableSources.SelectedItem.Text;
            experimentSpecification.Duration = this.txbDuration.Text;
            experimentSpecification.Trials = this.txbTrials.Text;

            /*
             * Create a CSV string of absorbers
             */
            String csvAbsorbers = "";
            foreach (ListItem item in this.ddlSelectedAbsorbers.Items)
            {
                csvAbsorbers += String.Format("{0:s}{1:s}", (csvAbsorbers.Length > 0) ? Consts.STR_CsvSplitter : "", item.Text);
            }

            /*
             * Create a CSV string of distances
             */
            String csvDistances = "";
            foreach (ListItem item in this.ddlSelectedDistances.Items)
            {
                csvDistances += String.Format("{0:s}{1:s}", (csvDistances.Length > 0) ? Consts.STR_CsvSplitter : "", item.Text);
            }

            switch (experimentSpecification.SetupId)
            {
                case Consts.STRXML_SetupId_RadioactivityVsTime:
                case Consts.STRXML_SetupId_SimActivityVsTime:
                case Consts.STRXML_SetupId_SimActivityVsTimeNoDelay:
                    experimentSpecification.Absorbers = this.ddlAvailableAbsorbers.Text;
                    experimentSpecification.Distances = this.ddlAvailableDistances.Text;
                    break;

                case Consts.STRXML_SetupId_RadioactivityVsDistance:
                case Consts.STRXML_SetupId_SimActivityVsDistance:
                case Consts.STRXML_SetupId_SimActivityVsDistanceNoDelay:
                    experimentSpecification.Absorbers = this.ddlAvailableAbsorbers.Text;
                    experimentSpecification.Distances = csvDistances;
                    break;

                case Consts.STRXML_SetupId_RadioactivityVsAbsorber:
                case Consts.STRXML_SetupId_SimActivityVsAbsorber:
                case Consts.STRXML_SetupId_SimActivityVsAbsorberNoDelay:
                    experimentSpecification.Absorbers = csvAbsorbers;
                    experimentSpecification.Distances = this.ddlAvailableDistances.Text;
                    break;
            }

            /*
             * Convert specification information to an XML string
             */
            String xmlSpecification = experimentSpecification.ToXmlString();

            return xmlSpecification;
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
