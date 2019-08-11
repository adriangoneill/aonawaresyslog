<%@ Page Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" Codebehind="Configuration.aspx.cs"
    Inherits="Aonaware.SyslogWeb.Configuration" Title="Server Configuration" %>

<asp:Content ID="ConfigurationContent" ContentPlaceHolderID="ContentPlaceHolder" runat="server">
    <h1>
        Server Configuration</h1>
    <asp:Label ID="lblResults" runat="server" ForeColor="Red"></asp:Label><br />
    <asp:Panel ID="pnlMain" runat="server">
        <table cellspacing="1" cellpadding="1" width="75%" border="0">
            <tr>
                <td>
                    Syslog Server Port:</td>
                <td>
                    <asp:TextBox ID="txtPort" runat="server"></asp:TextBox>
                    <asp:RangeValidator ID="rngPort" runat="server" ControlToValidate="txtPort" ErrorMessage="Invalid Port"
                        Type="Integer"></asp:RangeValidator>
                    <asp:RequiredFieldValidator ID="reqPort" runat="server" ControlToValidate="txtPort"
                        ErrorMessage="Please enter a port"></asp:RequiredFieldValidator></td>
            </tr>
            <tr>
                <td>
                    Message retention period (days):</td>
                <td>
                    <asp:TextBox ID="txtCleanup" runat="server"></asp:TextBox>
                    <asp:RangeValidator ID="rngCleanup" runat="server" ControlToValidate="txtCleanup"
                        ErrorMessage="Invalid Daycount" Type="Integer"></asp:RangeValidator>
                    <asp:RequiredFieldValidator ID="reqCleanup" runat="server" ControlToValidate="txtCleanup"
                        ErrorMessage="Please enter a number"></asp:RequiredFieldValidator></td>
            </tr>
        </table>
        <p>
            <asp:Button ID="btnSubmit" runat="server" Text="Save Changes" OnClick="btnSubmit_Click"></asp:Button></p>
    </asp:Panel>
</asp:Content>
