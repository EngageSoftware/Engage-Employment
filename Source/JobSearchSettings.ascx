<%@ Control Language="c#" AutoEventWireup="false" Inherits="Engage.Dnn.Employment.JobSearchSettings" Codebehind="JobSearchSettings.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="label" Src="~/controls/labelControl.ascx" %>
<div style="text-align:left">
    <table cellspacing="0" cellpadding="0" border="0" class="SettingsTable">
	    <tr>
	        <td class="SubHead"><dnn:label ResourceKey="lblJobGroup" runat="server" /></td>
	        <td class="NormalTextBox"><asp:DropDownList ID="ddlJobGroup" runat="server" /></td>
	    </tr>
    </table>
</div>