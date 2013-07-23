<%@ Page Language="C#" MasterPageFile="~/LabClient.Master" AutoEventWireup="true"
    CodeBehind="Results.aspx.cs" Inherits="LabClientHtml.Results" Title="Results" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="./resources/css/labclient.css" rel="stylesheet" type="text/css" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentTitle" runat="server">
    Results
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPage" runat="server">
    <form id="form1" name="form1" runat="server">
    <table>
        <tr>
            <th colspan="2">
                Experiment Results
            </th>
        </tr>
        <tr>
            <td class="label">
                Experiment:
            </td>
            <td class="information">
                <asp:TextBox ID="txbExperimentId" runat="server" Width="60px"></asp:TextBox>
                <asp:DropDownList ID="ddlExperimentIds" runat="server" Width="66px" AutoPostBack="true"
                    OnSelectedIndexChanged="ddlExperimentIds_SelectedIndexChanged">
                </asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td class="label">
                &nbsp;
            </td>
            <td class="buttons">
                <asp:Button ID="btnRetrieve" runat="server" Text="Retrieve" CssClass="hcommandButton"
                    OnClick="btnRetrieve_Click" />
                &nbsp;
                <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="hcommandButton" OnClick="btnSave_Click" />
            </td>
        </tr>
    </table>
    <p>
        <asp:Label ID="lblMessage" runat="server" CssClass="infomessage"></asp:Label>
    </p>
    <asp:Label ID="lblResultsTableValue" runat="server"></asp:Label>
    <asp:HiddenField ID="hfCsvExperimentResults" runat="server"></asp:HiddenField>
    <asp:HiddenField ID="hfRetrievedExperimentId" runat="server"></asp:HiddenField>
    </form>
</asp:Content>
