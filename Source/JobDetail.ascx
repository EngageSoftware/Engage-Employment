<%@ Control language="C#" Inherits="Engage.Dnn.Employment.JobDetail" AutoEventWireup="false" Codebehind="JobDetail.ascx.cs" %>
<%@ Import Namespace="System.Globalization" %>
<div id="employment" class="job-details employmentTable">
    <table>
        <tr class="jd-title">
            <td class="SubHead em-label">
                <asp:Label runat="server" resourcekey="lblTitleHeader" EnableViewState="false" />
            </td>
            <td class="Head em-value">
                <%= HttpUtility.HtmlEncode(this.CurrentJob.Title) %>
            </td>
        </tr>
        <tr class="jd-location">
            <td class="SubHead em-label">
                <asp:Label runat="server" resourcekey="lblLocationHeader" EnableViewState="false" />
            </td>
            <td class="em-value">
                <%= HttpUtility.HtmlEncode(CurrentJob.LocationName) %></td>
        </tr>
        <tr class="jd-region">
            <td class="SubHead em-label">
                <asp:Label runat="server" resourcekey="lblStateHeader" EnableViewState="false" />
            </td>
            <td class="em-value">
                <%= HttpUtility.HtmlEncode(CurrentJob.StateName) %></td>
        </tr>
        <tr class="jd-post-date">
            <td class="SubHead em-label">
                <asp:Label runat="server" resourcekey="lblDatePostedHeader" EnableViewState="false" />
            </td>
            <td class="em-value">
                <%= HttpUtility.HtmlEncode(CurrentJob.StartDate.ToString(Localize("StartDate.Format"), CultureInfo.CurrentCulture))%>
            </td>
        </tr>
        <% if (ShowCloseDate) { %>
        <tr class="jd-close-date">
            <td class="SubHead em-label">
                <asp:Label runat="server" resourcekey="lblCloseDateHeader" EnableViewState="false" />
            </td>
            <td class="em-value">
                <%= HttpUtility.HtmlEncode(CurrentJob.ExpireDate.HasValue ? CurrentJob.ExpireDate.Value.ToString(Localize("CloseDate.Format"), CultureInfo.CurrentCulture) : Localize("No Close Date"))%>
            </td>
        </tr>
        <% } %>
        <tr class="jd-position-desc">
            <td class="SubHead em-label">
                <asp:Label runat="server" resourcekey="lblPositionHeader" EnableViewState="false" />
            </td>
            <td class="em-value">
                <%= CurrentJob.Description %></td>
        </tr>
        <tr class="jd-reqd-qual">
            <td class="SubHead em-label">
                <asp:Label runat="server" resourcekey="lblRequiredQualificationsHeader" EnableViewState="false" />
            </td>
            <td class="em-value">
                <%= CurrentJob.RequiredQualifications %></td>
        </tr>
        <tr class="jd-desd-qual">
            <td class="SubHead em-label">
                <asp:Label runat="server" resourcekey="lblDesiredQualificationsHeader" EnableViewState="false" />
            </td>
            <td class="em-value">
                <%= CurrentJob.DesiredQualifications %></td>
        </tr>
        <tr class="jd-action-row">
            <td colspan="2">
                <asp:LinkButton ID="NextActionButton" runat="server" CssClass="action-btn" />
                <asp:Label ID="EmailErrorLabel" runat="server" CssClass="email-error NormalRed" resourcekey="lblEmailError" Visible="false" EnableViewState="false" />
            </td>            
        </tr>
        <tr class="jd-email-row">
            <td colspan="2">
                <asp:LinkButton ID="EmailFriendButton" runat="server" CssClass="email-btn" resourcekey="btnEmailFriend" EnableViewState="false" />
            </td>            
        </tr>
        <tr class="jd-return-row">
            <td colspan="2">
                <asp:LinkButton ID="BackButton" runat="server" CssClass="return-btn" resourcekey="btnBack" EnableViewState="false" />
            </td>
        </tr>
    </table>
</div>
<div><asp:Label ID="SuccessLabel" runat="server" CssClass="success-message NormalRed" EnableViewState="false" Visible="false"/></div>
<div id="ApplicantInfoSection" runat="server" visible="false" class="job-application">
    <table>
        <tr id="ApplicationMessageRow" runat="server" class="ja-message">
            <td class="SubHead em-label">
                <asp:Label runat="server" resourcekey="lblApplicationMessageHeader" EnableViewState="false" /><asp:Label ID="MessageRequiredLabel" runat="server" resourcekey="Required" CssClass="SubSubHead" />
            </td>
            <td class="em-input">
                <asp:TextBox ID="ApplicationMessageTextBox" runat="server" Columns="30" Rows="5" TextMode="MultiLine" EnableViewState="false"/>
                <asp:RequiredFieldValidator ID="ApplicationMessageRequiredValidator" runat="server" ControlToValidate="ApplicationMessageTextBox" EnableViewState="false" Display="None" ValidationGroup="apply" resourcekey="rfvEmailMessage" SetFocusOnError="true" />
            </td>
        </tr>
        <tr id="SalaryRow" runat="server" class="ja-salary">
            <td class="SubHead em-label">
                <asp:Label runat="server" resourcekey="lblApplicationSalaryHeader" EnableViewState="false" /><asp:Label ID="SalaryRequiredLabel" runat="server" resourcekey="Required" CssClass="SubSubHead" />
            </td>
            <td class="em-input">
                <asp:TextBox ID="SalaryTextBox" runat="server" MaxLength="255" Columns="35" EnableViewState="false"/>
                <asp:RequiredFieldValidator ID="SalaryRequiredTextBox" runat="server" ControlToValidate="SalaryTextBox" EnableViewState="false" Display="None" ValidationGroup="apply" SetFocusOnError="true" resourcekey="rfvSalaryRequirements" />
            </td>
        </tr>
        <tr id="SalaryMessageRow" runat="server" class="ja-salary-message"><td colspan="2">
            <asp:Label runat="server" EnableViewState="false" resourcekey="lblApplicationSalaryMessage" />
        </td></tr>
        <tr id="LeadRow" runat="server" class="ja-lead">
            <td class="SubHead em-label">
                <asp:Label runat="server" resourcekey="lblLeadHeader" EnableViewState="false" /><asp:Label ID="LeadRequiredLabel" runat="server" resourcekey="Required" CssClass="SubSubHead" />
            </td>
            <td class="em-input">
                <asp:DropDownList ID="LeadDropDownList" runat="server"/>
            </td>
        </tr>
        <tr id="CoverLetterRow" runat="server" class="ja-cover-letter">
            <td class="SubHead em-label">
                <asp:Label runat="server" resourcekey="lblApplicationCoverLetterHeader" EnableViewState="false" /><asp:Label ID="CoverLetterRequiredLabel" runat="server" resourcekey="Required" CssClass="SubSubHead" />
            </td>
            <td class="em-input">
                <asp:Panel runat="server" Visible="false"> <%-- This Panel needs to be the direct parent of this HyperLink for code in the codebehind --%>
                    <asp:HyperLink ID="CoverLetterLink" runat="server" Target="_blank" /><br />
                </asp:Panel>
                <asp:FileUpload ID="CoverLetterUpload" runat="server" EnableViewState="false" />
                <asp:RequiredFieldValidator ID="CoverLetterFileRequiredValidator" runat="server" ControlToValidate="CoverLetterUpload" EnableViewState="false" ValidationGroup="apply" SetFocusOnError="True" Display="None" resourcekey="rfvCoverLetterFile" />
                <asp:RegularExpressionValidator ID="CoverLetterFileExtensionValidator" runat="server" ControlToValidate="CoverLetterUpload" EnableViewState="false" ValidationGroup="apply" SetFocusOnError="true" Display="None" />
            </td>
        </tr>
        <tr class="ja-resume">
            <td class="SubHead em-label">
                <asp:Label runat="server" resourcekey="lblApplicationResumeHeader" EnableViewState="false" /><asp:Label ID="ResumeRequiredLabel" runat="server" resourcekey="Required" CssClass="SubSubHead" />
            </td>
            <td class="em-input">
                <asp:Panel runat="server" Visible="false"> <%-- This Panel needs to be the direct parent of this HyperLink for code in the codebehind --%>
                    <asp:HyperLink ID="ResumeLink" runat="server" Target="_blank" /><br />
                </asp:Panel>
                <asp:FileUpload ID="ResumeUpload" runat="server" EnableViewState="false" />
                <asp:RequiredFieldValidator ID="ResumeFileRequiredValidator" runat="server" ControlToValidate="ResumeUpload" EnableViewState="false" ValidationGroup="apply" SetFocusOnError="True" Display="None" resourcekey="rfvResumeFile" />
                <asp:RegularExpressionValidator ID="ResumeFileExtensionValidator" runat="server" ControlToValidate="ResumeUpload" EnableViewState="false" ValidationGroup="apply" SetFocusOnError="true" Display="None" />
            </td>
        </tr>
        <tr id="ResumeMessageRow" runat="server" class="ja-resume-message"><td colspan="2">
            <asp:Label runat="server" EnableViewState="false" resourcekey="lblApplicationResumeMessage" />
        </td></tr>
        <tr class="ja-apply-row">
            <td class="ja-apply-cell">
                <asp:LinkButton ID="ApplyButton" runat="server" CssClass="apply-btn Normal" EnableViewState="false" ValidationGroup="apply" resourcekey="btnApply" />
            </td>
            <td class="alignRight">
                <asp:Label runat="server" EnableViewState="false" resourcekey="lblRequired" CssClass="SubSubHead alignRight" />
            </td>
        </tr>
        <tr class="ja-errors">
            <td colspan="2">
                <%--<asp:CustomValidator ID="cvAlreadyApplied" runat="server" EnableViewState="false" ValidationGroup="apply" OnServerValidate="cvAlreadyApplied_ServerValidate" SetFocusOnError="True" Display="None" resourcekey="cvAlreadyApplied" />--%>
                <asp:ValidationSummary CssClass="NormalRed" runat="server" EnableViewState="false" DisplayMode="BulletList" ShowMessageBox="true" ShowSummary="false" ValidationGroup="apply" />
            </td>
        </tr>
    </table>
</div>
<div id="EmailFriendSection" runat="server" visible="false" class="email_jobs" enableviewstate="false">
    <table>
        <tr class="ej-from-name">
            <td class="SubHead em-label">
                <asp:Label runat="server" resourcekey="lblEmailFromNameHeader" EnableViewState="false" />
            </td>
            <td class="em-input">
                <asp:TextBox ID="FromNameTextBox" runat="server" CssClass="NormalTextBox" EnableViewState="false" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="FromNameTextBox" EnableViewState="false" ValidationGroup="email" SetFocusOnError="True" Display="None" resourcekey="rfvFromName"/>
            </td>
        </tr>
        <tr class="ej-from-email">
            <td class="SubHead em-label">
                <asp:Label runat="server" resourcekey="lblEmailFromAddressHeader" EnableViewState="false" />
            </td>
            <td class="em-input">
                <asp:TextBox ID="FromAddressTextBox" runat="server" CssClass="NormalTextBox" EnableViewState="false" />
                <asp:RegularExpressionValidator ID="FriendFromEmailRegexValidator" runat="server" EnableViewState="false" ControlToValidate="FromAddressTextBox" Display="None" ValidationGroup="email" SetFocusOnError="true" resourcekey="regexFriendFromEmailValidation" />
            </td>
        </tr>
        <tr class="ej-to-email">
            <td class="SubHead em-label">
                <asp:Label runat="server" resourcekey="lblEmailToAddressHeader" EnableViewState="false" />
            </td>
            <td class="em-input">
                <asp:TextBox ID="SendToAddressTextBox" runat="server" CssClass="NormalTextBox" EnableViewState="false" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="SendToAddressTextBox" Display="None" EnableViewState="false" ValidationGroup="email" SetFocusOnError="True" resourcekey="rfvSendToAddress" />
                <asp:RegularExpressionValidator ID="FriendEmailRegexValidator" runat="server" EnableViewState="false" ControlToValidate="SendToAddressTextBox" Display="None" ValidationGroup="email" SetFocusOnError="true" resourcekey="regexFriendEmailValidation" />
                <br/>
                <asp:Label runat="server" resourcekey="lblEmailToAddressMessage" EnableViewState="false" />
            </td>
        </tr>
        <tr class="ej-message">
            <td class="SubHead em-label">
                <asp:Label runat="server" resourcekey="lblEmailMessageHeader" EnableViewState="false" />
            </td>
            <td class="em-input">
                <asp:TextBox ID="FriendEmailMessageTextBox" runat="server" CssClass="NormalTextBox" Columns="30" Rows="5" TextMode="MultiLine" EnableViewState="false"/>
            </td>
        </tr>
        <tr class="ej-send">
            <td colspan="2">
                <asp:LinkButton ID="SendToFriendButton" runat="server" CssClas="send-btn" ValidationGroup="email" resourcekey="btnSendToFriend" EnableViewState="false" />
                <asp:ValidationSummary runat="server" DisplayMode="BulletList" ShowMessageBox="true" ShowSummary="false" ValidationGroup="email" EnableViewState="false" />
            </td>
        </tr>
    </table>
</div>
