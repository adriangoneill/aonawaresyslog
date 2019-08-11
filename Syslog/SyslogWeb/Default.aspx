<%@ Page Language="C#" EnableViewState="true" MasterPageFile="~/Site.Master" AutoEventWireup="true" Codebehind="Default.aspx.cs"
    Inherits="Aonaware.SyslogWeb.Default" Title="Aonaware Syslog Daemon" %>

<asp:Content ID="DefaultContent" ContentPlaceHolderID="ContentPlaceHolder" runat="server">
    <h1>
        Aonaware Syslog Daemon</h1>
    <p>
        <asp:DataGrid ID="dgMain" runat="server" AllowPaging="True" AllowCustomPaging="True"
            Width="100%" PageSize="15" OnPageIndexChanged="dgMain_PageIndexChanged">
            <AlternatingItemStyle BackColor="LightCyan"></AlternatingItemStyle>
            <HeaderStyle Font-Bold="True" BackColor="Bisque"></HeaderStyle>
            <PagerStyle Mode="NumericPages"></PagerStyle>
        </asp:DataGrid></p>
</asp:Content>
