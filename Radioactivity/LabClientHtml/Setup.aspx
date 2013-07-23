<%@ Page Language="C#" MasterPageFile="~/LabClient.Master" AutoEventWireup="true"
    CodeBehind="Setup.aspx.cs" Inherits="LabClientHtml.Setup" Title="Setup" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link href="./resources/css/labclient.css" rel="stylesheet" type="text/css" />
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentTitle" runat="server">
    Setup
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="ContentPage" runat="server">
    <form id="form1" name="form1" runat="server">
    <table id="setup" border="0" cellspacing="0" cellpadding="0">
        <tr>
            <th colspan="2">
                Experiment Setups
            </th>
        </tr>
        <tr>
            <td class="label">
                Setup:
            </td>
            <td class="information">
                <asp:DropDownList ID="ddlExperimentSetups" runat="server" CssClass="hselectOneMenu"
                    AutoPostBack="true" OnSelectedIndexChanged="ddlExperimentSetups_SelectedIndexChanged">
                </asp:DropDownList>
            </td>
        </tr>
        <tr>
            <td class="label">
                &nbsp;
            </td>
            <td class="description">
                <asp:Label ID="lblSetupDescription" runat="server"></asp:Label>
            </td>
        </tr>
        <tr>
            <td colspan="2">
                <table>
                    <tr>
                        <td class="label">
                            Source:
                        </td>
                        <td class="data">
                            <asp:DropDownList ID="ddlAvailableSources" runat="server" CssClass="hselectOneMenu"
                                Width="130px" ToolTip="Select source">
                            </asp:DropDownList>
                        </td>
                        <td>
                        </td>
                    </tr>
                    <tr>
                        <td class="label">
                            Absorber:
                        </td>
                        <td class="data">
                            <asp:DropDownList ID="ddlAvailableAbsorbers" runat="server" CssClass="hselectOneMenu"
                                Width="130px" ToolTip="">
                            </asp:DropDownList>
                        </td>
                        <td class="dataright">
                            <asp:Button ID="btnAddAbsorber" runat="server" Text="Add" CssClass="hcommandButton"
                                OnClick="btnAddAbsorber_Click" ToolTip="Add absorber to selected absorbers" />
                        </td>
                    </tr>
                    <asp:Panel ID="pnlSelectedAbsorbers" runat="server">
                        <tr>
                            <td class="label">
                                Selected:
                            </td>
                            <td class="data">
                                <asp:DropDownList ID="ddlSelectedAbsorbers" runat="server" CssClass="hselectOneMenu"
                                    Width="130px" ToolTip="Selected absorbers">
                                </asp:DropDownList>
                            </td>
                            <td class="dataright">
                                <asp:Button ID="btnClearAbsorbers" runat="server" Text="Clear" CssClass="hcommandButton"
                                    OnClick="btnClearAbsorbers_Click" ToolTip="Clear selected absorbers" />
                            </td>
                        </tr>
                    </asp:Panel>
                    <tr>
                        <td class="label">
                            Distance:
                        </td>
                        <td class="data">
                            <asp:DropDownList ID="ddlAvailableDistances" runat="server" CssClass="hselectOneMenu"
                                Width="56px" ToolTip="">
                            </asp:DropDownList>
                            (mm)
                        </td>
                        <td class="dataright">
                            <asp:Button ID="btnAddDistance" runat="server" Text="Add" CssClass="hcommandButton"
                                OnClick="btnAddDistance_Click" ToolTip="Add distance to selected distances" />
                        </td>
                    </tr>
                    <asp:Panel ID="pnlSelectedDistances" runat="server">
                        <tr>
                            <td class="label">
                                Selected:
                            </td>
                            <td class="data">
                                <asp:DropDownList ID="ddlSelectedDistances" runat="server" CssClass="hselectOneMenu"
                                    Width="56px" ToolTip="Selected distances">
                                </asp:DropDownList>
                            </td>
                            <td class="dataright">
                                <asp:Button ID="btnClearDistances" runat="server" Text="Clear" CssClass="hcommandButton"
                                    OnClick="btnClearDistances_Click" ToolTip="Clear selected distances" />
                            </td>
                        </tr>
                    </asp:Panel>
                    <tr>
                        <td class="label">
                            Duration:
                        </td>
                        <td class="data">
                            <asp:TextBox ID="txbDuration" runat="server" Width="50px" ToolTip=""></asp:TextBox>
                            (secs)
                        </td>
                        <td>
                        </td>
                    </tr>
                    <tr>
                        <td class="label">
                            Trials:
                        </td>
                        <td class="data">
                            <asp:TextBox ID="txbTrials" runat="server" Width="50px" ToolTip=""></asp:TextBox>
                        </td>
                        <td>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
        <tr>
            <td class="label">
                &nbsp;
            </td>
            <td class="buttons">
                <asp:Button ID="btnValidate" runat="server" Text="Validate" CssClass="hcommandButton"
                    OnClick="btnValidate_Click" />
                <asp:Button ID="btnSubmit" runat="server" Text="Submit" CssClass="hcommandButton"
                    OnClick="btnSubmit_Click" />
            </td>
        </tr>
    </table>
    <p>
        <asp:Label ID="lblMessage" runat="server" CssClass="infomessage"></asp:Label>
    </p>
    </form>
</asp:Content>
