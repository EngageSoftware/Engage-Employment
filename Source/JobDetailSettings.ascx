<%@ Control Language="c#" AutoEventWireup="false" Inherits="Engage.Dnn.Employment.JobDetailSettings" Codebehind="JobDetailSettings.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="label" Src="~/controls/labelControl.ascx" %>
<div style="text-align:left">
    <table class="settingsTable">
	    <tr>
	        <td class="SubHead labelColumn nowrap"><dnn:label ResourceKey="lblJobGroup" runat="server" /></td>
	        <td class="contentColumn"><asp:DropDownList ID="ddlJobGroup" runat="server" cssclass="NormalTextBox" /></td>
	    </tr>
    </table>
</div>