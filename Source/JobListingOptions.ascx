<%@ Control Language="c#" AutoEventWireup="false" Inherits="Engage.Dnn.Employment.JobListingOptions" Codebehind="JobListingOptions.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="label" Src="~/controls/labelControl.ascx" %>
<div style="text-align:left">
    <table class="settingsTable">
        <tr>
	        <td colspan="2">
	            <div class="information">
                    <asp:Label runat="server" resourcekey="JobListingIntroduction"/>
                </div>
	        </td>
	    </tr>
	    <tr>
		    <td class="SubHead labelColumn nowrap">
		        <dnn:label ResourceKey="lblDisplayOption" runat="server" />
		    </td>
		    <td class="contentColumn">
		        <asp:RadioButtonList ID="DisplayOptionRadioButtonList" runat="server"/>
		    </td>
	    </tr>
	    <tr>
		    <td class="SubHead labelColumn nowrap">
		        <dnn:label ResourceKey="lblLimitOption" runat="server" />
		    </td>
		    <td class="contentColumn">
		        <asp:PlaceHolder ID="phLimitOption" runat="server">
		        <%--<asp:UpdatePanel ID="upnlLimitOption" runat="server" RenderMode="Inline" UpdateMode="Conditional"><ContentTemplate>--%>
		            <asp:CheckBox ID="LimitCheckBox" runat="server" /> <asp:Label runat="server" resourcekey="lblLimitTextStart"/><asp:TextBox ID="txtLimit" runat="server" Columns="4"/><asp:Label runat="server" resourcekey="lblLimitTextEnd"/>
		            <asp:RadioButtonList ID="LimitOptionRadioButtonList" runat="server"/>
		            <asp:RequiredFieldValidator ID="LimitRequiredFieldValidator" runat="server" ControlToValidate="txtLimit" Display="None" resourcekey="rfvLimit" ValidationGroup="SaveSettings" />
		            <asp:RangeValidator ID="rvLimit" runat="server" ControlToValidate="txtLimit" Display="None" MinimumValue="1" Operator="DataTypeCheck" Type="Integer" resourcekey="rvLimit" ValidationGroup="SaveSettings" />
		        <%--</ContentTemplate></asp:UpdatePanel>--%>
		        </asp:PlaceHolder>
		    </td>
	    </tr>
	    <tr>
		    <td class="SubHead labelColumn nowrap">
		        <dnn:label ResourceKey="Show Close Date" runat="server" />
		    </td>
		    <td class="contentColumn">
		        <asp:CheckBox ID="ShowCloseDateCheckBox" runat="server"/>
		    </td>
	    </tr>
	    <tr>
	        <td colspan="2">
                <asp:ValidationSummary runat="server" CssClass="NormalRed" DisplayMode="BulletList" ValidationGroup="SaveSettings" />
	        </td>
	    </tr>
	    <tr>
	        <td class="labelColumn">
	            <asp:Button ID="UpdateButton" runat="server" resourcekey="btnUpdate" ValidationGroup="SaveSettings"/>
	        </td>
	        <td class="contentColumn">
	            <asp:Button ID="CancelButton" runat="server" resourcekey="btnCancel" CausesValidation="false" />
	        </td>
	    </tr>
    </table>
</div>