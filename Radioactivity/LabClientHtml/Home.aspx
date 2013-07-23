<%@ Page Language="C#" MasterPageFile="~/LabClient.Master" AutoEventWireup="true"
    CodeBehind="Home.aspx.cs" Inherits="LabClientHtml.Home" Title="Home" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="./resources/css/labclient.css" rel="stylesheet" type="text/css" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentTitle" runat="server">
    Home
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPage" runat="server">
    <form id="form1" name="form1" runat="server">
    <p>
        Welcome! This LabClient enables you to submit an experiment to a remote iLab LabServer
        for processing. The results can then be retrieved, saved to file and possibly displayed.
        A description for each of the LabClient's webpages is provided below.
    </p>
    <asp:Label ID="lblMoreInfo" runat="server"></asp:Label>
    <asp:HyperLink ID="lnkMoreInfo" runat="server" Target="_blank"></asp:HyperLink>
    <h4>
        Setup
    </h4>
    <p>
        Specify the setup for running the experiment. <small>
            <asp:LinkButton ID="lnkbtnSetupInfo" runat="server" OnClick="lnkbtnSetupInfo_Click"></asp:LinkButton>
        </small>
    </p>
    <asp:Literal ID="litSetupInfo" runat="server" Visible="False">
        <p>
            The experiment setup is entered using a selection of text entry boxes, selection
            lists, buttons, etc. The selection of entry methods will vary depending on the particular
            experiment.
        </p>
        <p>
            When the necessary specification entries have been made, the experiment is submitted
            for processing by clicking the <i>Submit</i> button. If the LabServer is currently
            processing another experiment, the submitted experiment will be placed in a queue.
            The specification is always checked for validity when being submitted for processing.
            If there are any errors in the specification then a message is displayed giving
            a brief description of the error. If the experiment has been submitted successfully
            then a message will display the number given to the experiment and the approximate
            amount of time to process the experiment. This time does not include the time that
            the experiment has to wait in the queue.
        </p>
        <p>
            The experiment specification may be checked for validity without being submitted
            by clicking the <i>Validate</i> button. If there are any errors in the specification
            then a message is displayed giving a brief description of the error. If the experiment
            has been validated successfully then the message will display the approximate amount
            of time to process the experiment. This time does not include the time that the
            experiment has to wait in the queue.
        </p>
    </asp:Literal>
    <h4>
        Status
    </h4>
    <p>
        Check the status of an experiment or the LabServer. <small>
            <asp:LinkButton ID="lnkbtnStatusInfo" runat="server" OnClick="lnkbtnStatusInfo_Click"></asp:LinkButton>
        </small>
    </p>
    <asp:Literal ID="litStatusInfo" runat="server" Visible="False">
        <p>
            The status of the LabServer can be checked by clicking the <i>Refresh</i> button.
            If the LabServer is accessible and ready to accept experiments then <span style="color: green">
                Online</span> will be displayed. If the LabServer cannot be accessed or is not
            available to accept experiments then <span style="color: red">Offline</span> will
            be displayed.
        </p>
        <p>
            When the LabServer is online, a message is displayed showing the LabServer's operating
            status and information about the experiment queue. This information includes the
            number of experiments in the queue and the approximate amount time that a newly
            submitted experiment would spend in the queue before being processed.
        </p>
        <p>
            The status of a submitted experiment can be displayed by entering an experiment
            number into the <i>Experiment</i> field and clicking the <i>Check</i> button. If
            the number entered is a valid submitted experiment then the status of that experiment
            is displayed. If the experiment is waiting in the queue then the message shows the
            position of the experiment in the queue and the approximate amount of time that
            the experiment has to wait before being processed. If the experiment is currently
            being processed then the message will show the approximate amount of time remaining
            before the experiment completes.
        </p>
        <p>
            A currently submitted experiment may be cancelled by clicking the <i>Cancel</i>
            button. If the experiment is waiting in the queue for processing then the experiment
            is removed from the queue. If the experiment is currently being processed then a
            best-effort is made to cancel the experiment, but there is no guarantee that the
            experiment can be cancelled. An experiment that has already completed cannot be
            cancelled.
        </p>
    </asp:Literal>
    <h4>
        Results
    </h4>
    <p>
        Retrieve the results for an experiment and process. <small>
            <asp:LinkButton ID="lnkbtnResultsInfo" runat="server" OnClick="lnkbtnResultsInfo_Click"></asp:LinkButton>
        </small>
    </p>
    <asp:Literal ID="litResultsInfo" runat="server" Visible="false">
        <p>
            The results for a completed experiment can be retrieved by entering the experiment
            number into the <i>Experiment</i> field and clicking the <i>Retrieve</i> button.
            If the experiment results are retrieved successfully then the <i>Save</i> button
            is enabled and the results can be saved to a Microsoft Excel file in CSV format.
            A <i>Display</i> button may also become visible allowing the results to be displayed
            in a seperate Java window, but this depends on the particular experiment.
        </p>
    </asp:Literal>
    </form>
</asp:Content>
