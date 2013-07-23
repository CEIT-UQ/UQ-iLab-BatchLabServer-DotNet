<%@ Page Language="C#" MasterPageFile="~/LabClient.Master" AutoEventWireup="true"
    CodeBehind="Status.aspx.cs" Inherits="LabClientHtml.Status" Title="Status" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="./resources/css/labclient.css" rel="stylesheet" type="text/css" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentTitle" runat="server">
    Status
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPage" runat="server">
    <form id="form1" name="form1" runat="server">
    <table>
        <tr>
            <th colspan="2">
                LabServer Status
            </th>
        </tr>
        <tr>
            <td class="label">
                Status:
            </td>
            <td class="information">
                <asp:Label ID="lblOnline" runat="server" Text="Online" ForeColor="Green" Visible="false"></asp:Label>
                <asp:Label ID="lblOffline" runat="server" Text="Offine" ForeColor="Red" Visible="false"></asp:Label>
            </td>
        </tr>
        <tr>
            <td class="label">
                Message:
            </td>
            <td class="information">
                <asp:Label ID="lblQueueStatusMessage" runat="server"></asp:Label>
                <br />
                <asp:Label ID="lblLabStatusMessage" runat="server"></asp:Label>
            </td>
        </tr>
        <tr>
            <td class="label">
                &nbsp;
            </td>
            <td class="buttons">
                <asp:Button ID="btnRefresh" runat="server" Text="Refresh" CssClass="hcommandButton"
                    OnClick="btnRefresh_Click" />
            </td>
        </tr>
        <tr>
            <th colspan="2">
                Experiment Status
            </th>
        </tr>
        <tr>
            <td class="label">
                Experiment Id:
            </td>
            <td class="information">
                <asp:TextBox ID="txbExperimentId" runat="server" Width="60px"></asp:TextBox>
                <asp:DropDownList ID="ddlExperimentIds" runat="server" Width="66px" AutoPostBack="true"
                    OnSelectedIndexChanged="ddlExperimentIDs_SelectedIndexChanged">
                </asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td class="label">
                &nbsp;
            </td>
            <td class="buttons">
                <asp:Button ID="btnCheck" runat="server" Text="Check" CssClass="hcommandButton" OnClick="btnCheck_Click" />
                <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="hcommandButton"
                    OnClick="btnCancel_Click" />
            </td>
        </tr>
    </table>
    <p>
        <asp:Label ID="lblMessage" runat="server" CssClass="infomessage"></asp:Label>
    </p>
    </form>
</asp:Content>
