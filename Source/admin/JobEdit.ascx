<%@ Control language="C#" Inherits="Engage.Dnn.Employment.Admin.JobEdit" AutoEventWireup="false" Codebehind="JobEdit.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="TextEditor" Src="~/controls/texteditor.ascx" %>
<span class="Head"><dnn:Label ResourceKey="lblJobsHeader" runat="server" /></span>
<table>
    <tr>
        <td class="SubHead">
            <dnn:label ResourceKey="lblName" controlname="PositionDropDownList" runat="server" />
        </td>
        <td valign="top">
            <asp:DropDownList ID="PositionDropDownList" runat="server"/>
            <asp:CompareValidator runat="server" ControlToValidate="PositionDropDownList" Operator="NotEqual" SetFocusOnError="True" ValueToCompare="-1" Display="None" resourcekey="cvPosition"/>
            <asp:CustomValidator ID="UniquePositionLocationValidator" runat="server" Display="None" resourcekey="cvUniquePositionLocation" />
        </td>
    </tr>
    <tr>
        <td class="SubHead">
            <dnn:label ResourceKey="lblCategory" controlname="CategoryDropDownList" runat="server" />
        </td>
        <td valign="top">
            <asp:DropDownList ID="CategoryDropDownList" runat="server"/>
            <asp:CompareValidator runat="server" ControlToValidate="CategoryDropDownList" Operator="NotEqual" SetFocusOnError="True" ValueToCompare="-1" Display="None" resourcekey="cvCategory"/>
        </td>
    </tr>
    <tr>
        <td class="SubHead">
            <dnn:label ResourceKey="lblLocation" controlname="LocationDropDownList" runat="server" />
        </td>
        <td valign="top">
            <asp:DropDownList ID="LocationDropDownList" runat="server" />
            <asp:CompareValidator runat="server" ControlToValidate="LocationDropDownList" Operator="NotEqual" SetFocusOnError="True" ValueToCompare="-1" Display="None" resourcekey="cvLocation" />
        </td>
    </tr>
    <tr>
        <td class="SubHead">
            <dnn:label ResourceKey="lblSortOrder" controlname="SortOrderTextBox" runat="server" />
        </td>
        <td valign="top">
            <asp:TextBox ID="SortOrderTextBox" runat="server" CssClass="NormalTextBox" />
            <asp:CompareValidator runat="server" ControlToValidate="SortOrderTextBox" Operator="DataTypeCheck" SetFocusOnError="True" Type="Integer" Display="None" resourcekey="cvSortOrder" />
        </td>
    </tr>
    <tr>
        <td class="SubHead">
            <dnn:label ResourceKey="lblIsHot" controlname="IsHotCheckBox" runat="server" />
        </td>
        <td valign="top">
            <asp:CheckBox ID="IsHotCheckBox" runat="server" />
        </td>
    </tr>
    <tr>
        <td class="SubHead">
            <dnn:label ResourceKey="lblIsFilled" controlname="IsFilledCheckBox" runat="server" />
        </td>
        <td valign="top">
            <asp:CheckBox ID="IsFilledCheckBox" runat="server" />
        </td>
    </tr>
    <tr>
        <td class="SubHead">
            <dnn:label ResourceKey="lblStartDate" controlname="StartDateTextBox" runat="server" />
        </td>
        <td valign="top">
            <asp:CheckBox ID="StartDateTextBox" runat="server" />
        </td>
    </tr>
    <tr>
        <td class="SubHead">
            <dnn:label ResourceKey="lblExpireDate" controlname="ExpireDateTextBox" runat="server" />
        </td>
        <td valign="top">
            <asp:CheckBox ID="ExpireDateTextBox" runat="server" />
        </td>
    </tr>
    <tr>
        <td class="SubHead">
            <dnn:label ResourceKey="lblRequiredQualifications" controlname="RequiredQualificationsTextEditor" runat="server" />
        </td>
        <td valign="top">
            <dnn:TextEditor Height="400px" Width="400px" ID="RequiredQualificationsTextEditor" runat="server" ChooseMode="false" HtmlEncode="false" TextRenderMode="raw" />
            <asp:CustomValidator ID="RequiredQualificationsRequiredValidator" runat="server" Display="None"/></td>
    </tr>
    <tr>
        <td class="SubHead">
            <dnn:label ResourceKey="lblDesiredQualifications" controlname="DesiredQualificationsTextEditor" runat="server" />
        </td>
        <td valign="top">
            <dnn:TextEditor Height="400px" Width="400px" ID="DesiredQualificationsTextEditor" runat="server" ChooseMode="false" HtmlEncode="false" TextRenderMode="raw"  />
            <asp:CustomValidator ID="DesiredQualificationsRequiredValidator" runat="server" Display="None"/></td>
    </tr>
    <tr>
        <td class="SubHead">
            <dnn:label ResourceKey="lblEmailAddress" controlname="EmailAddressTextBox" runat="server" />
        </td>
        <td valign="top">
            <asp:TextBox ID="EmailAddressTextBox" runat="server" CssClass="NormalTextBox"/>
            <asp:RegularExpressionValidator ID="EmailAddressRegexValidator" runat="server" Display="None" ControlToValidate="EmailAddressTextBox" resourcekey="regexEmailAddress" />
            <asp:RequiredFieldValidator runat="server" Display="None" ControlToValidate="EmailAddressTextBox" resourcekey="rfvEmailAddress" />
        </td>
    </tr>
    <tr>
        <td colspan="2">&nbsp;</td>
    </tr>
    <tr>
        <td colspan="2">
            <asp:LinkButton ID="UpdateButton" runat="server" CssClass="CommandButton" resourcekey="lnkUpdate"/>&nbsp;
            <asp:LinkButton ID="DeleteButton" runat="server" CssClass="CommandButton" CausesValidation="False" resourcekey="lnkDelete"/>&nbsp;
            <asp:LinkButton ID="CancelButton" runat="server" CssClass="CommandButton" CausesValidation="False" resourcekey="lnkCancel"/>
            <asp:ValidationSummary runat="server" DisplayMode="BulletList" ShowMessageBox="true" ShowSummary="false" />
        </td>
    </tr>
</table>

