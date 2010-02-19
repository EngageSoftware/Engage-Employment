<%@ Control Language="c#" AutoEventWireup="false" Inherits="Engage.Dnn.Employment.JobListingSettings" Codebehind="JobListingSettings.ascx.cs" %>

<asp:Label runat="server" ResourceKey="lblJobGroup.Text" CssClass="SubHead" />
<div class="information"><asp:label ID="HelpTextLabel" runat="server" /></div>
<div class="NormalTextBox"><asp:DropDownList ID="JobGroupDropDownList" runat="server" /></div>