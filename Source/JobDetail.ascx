<%@ Control language="C#" Inherits="Engage.Dnn.Employment.JobDetail" AutoEventWireup="false" Codebehind="JobDetail.ascx.cs" %>
<div id="employment" class="employmentTable">
    <table>
        <tr>
            <td class="SubHead">
                <asp:Label runat="server" resourcekey="lblTitleHeader" EnableViewState="false" />
            </td>
            <td class="Head">
                <%= CurrentJob.Title %>
            </td>
        </tr>
        <tr>
            <td class="SubHead">
                <asp:Label runat="server" resourcekey="lblLocationHeader" EnableViewState="false" />
            </td>
            <td>
                <%= CurrentJob.LocationName %></td>
        </tr>
        <tr>
            <td class="SubHead">
                <asp:Label runat="server" resourcekey="lblStateHeader" EnableViewState="false" />
            </td>
            <td>
                <%= CurrentJob.StateName %></td>
        </tr>
        <tr>
            <td class="SubHead">
                <asp:Label runat="server" resourcekey="lblDatePostedHeader" EnableViewState="false" />
            </td>
            <td>
                <%= CurrentJob.PostedDate %></td>
        </tr>
        <tr>
            <td class="SubHead">
                <asp:Label runat="server" resourcekey="lblPositionHeader" EnableViewState="false" />
            </td>
            <td>
                <%= CurrentJob.Description %></td>
        </tr>
        <tr>
            <td class="SubHead">
                <asp:Label runat="server" resourcekey="lblRequiredQualificationsHeader" EnableViewState="false" />
            </td>
            <td>
                <%= CurrentJob.RequiredQualifications %></td>
        </tr>
        <tr>
            <td class="SubHead">
                <asp:Label runat="server" resourcekey="lblDesiredQualificationsHeader" EnableViewState="false" />
            </td>
            <td>
                <%= CurrentJob.DesiredQualifications %></td>
        </tr>
        <tr>
            <td colspan="2">
                <asp:LinkButton ID="NextActionButton" runat="server" />
                <asp:Label ID="EmailErrorLabel" runat="server" CssClass="NormalRed" resourcekey="lblEmailError" Visible="false" EnableViewState="false" />
            </td>            
        </tr>
        <tr>
            <td colspan="2">
                <asp:LinkButton ID="EmailFriendButton" runat="server" resourcekey="btnEmailFriend" EnableViewState="false" />
            </td>            
        </tr>
        <tr>
            <td colspan="2">
                <asp:LinkButton ID="BackButton" runat="server" resourcekey="btnBack" EnableViewState="false" />
            </td>
        </tr>
    </table>
</div>
<div><asp:Label ID="ErrorMessageLabel" runat="server" CssClass="NormalRed" EnableViewState="false"/></div>
<div id="ApplicantInfoSection" runat="server" visible="false">
    <table>
        <tr id="ApplicationMessageRow" runat="server">
            <td class="SubHead">
                <asp:Label runat="server" resourcekey="lblApplicationMessageHeader" EnableViewState="false" /><asp:Label ID="MessageRequiredLabel" runat="server" resourcekey="Required" CssClass="SubSubHead" />
            </td>
            <td>
                <asp:TextBox ID="ApplicationMessageTextBox" runat="server" Columns="30" Rows="5" TextMode="MultiLine" EnableViewState="false"/>
                <asp:RequiredFieldValidator ID="ApplicationMessageRequiredValidator" runat="server" ControlToValidate="ApplicationMessageTextBox" EnableViewState="false" Display="None" ValidationGroup="apply" resourcekey="rfvEmailMessage" SetFocusOnError="true" />
            </td>
        </tr>
        <tr id="SalaryRow" runat="server">
            <td class="SubHead">
                <asp:Label runat="server" resourcekey="lblApplicationSalaryHeader" EnableViewState="false" /><asp:Label ID="SalaryRequiredLabel" runat="server" resourcekey="Required" CssClass="SubSubHead" />
            </td>
            <td>
                <asp:TextBox ID="SalaryTextBox" runat="server" MaxLength="255" Columns="35" EnableViewState="false"/>
                <asp:RequiredFieldValidator ID="SalaryRequiredTextBox" runat="server" ControlToValidate="SalaryTextBox" EnableViewState="false" Display="None" ValidationGroup="apply" SetFocusOnError="true" resourcekey="rfvSalaryRequirements" />
            </td>
        </tr>
        <tr id="SalaryMessageRow" runat="server"><td colspan="2">
            <asp:Label runat="server" EnableViewState="false" resourcekey="lblApplicationSalaryMessage" />
        </td></tr>
        <tr id="LeadRow" runat="server">
            <td class="SubHead">
                <asp:Label runat="server" resourcekey="lblLeadHeader" EnableViewState="false" /><asp:Label ID="LeadRequiredLabel" runat="server" resourcekey="Required" CssClass="SubSubHead" />
            </td>
            <td>
                <asp:DropDownList ID="LeadDropDownList" runat="server"/>
            </td>
        </tr>
        <tr id="CoverLetterRow" runat="server">
            <td class="SubHead">
                <asp:Label runat="server" resourcekey="lblApplicationCoverLetterHeader" EnableViewState="false" /><asp:Label ID="CoverLetterRequiredLabel" runat="server" resourcekey="Required" CssClass="SubSubHead" />
            </td>
            <td>
                <asp:Panel runat="server" Visible="false"> <%-- This Panel needs to be the direct parent of this HyperLink for code in the codebehind --%>
                    <asp:HyperLink ID="CoverLetterLink" runat="server" Target="_blank" /><br />
                </asp:Panel>
                <asp:FileUpload ID="CoverLetterUpload" runat="server" EnableViewState="false" />
                <asp:RequiredFieldValidator ID="CoverLetterFileRequiredValidator" runat="server" ControlToValidate="CoverLetterUpload" EnableViewState="false" ValidationGroup="apply" SetFocusOnError="True" Display="None" resourcekey="rfvCoverLetterFile" />
                <asp:RegularExpressionValidator ID="CoverLetterFileExtensionValidator" runat="server" ControlToValidate="CoverLetterUpload" EnableViewState="false" ValidationGroup="apply" SetFocusOnError="true" Display="None" />
            </td>
        </tr>
        <tr>
            <td class="SubHead">
                <asp:Label runat="server" resourcekey="lblApplicationResumeHeader" EnableViewState="false" /><asp:Label ID="ResumeRequiredLabel" runat="server" resourcekey="Required" CssClass="SubSubHead" />
            </td>
            <td>
                <asp:Panel runat="server" Visible="false"> <%-- This Panel needs to be the direct parent of this HyperLink for code in the codebehind --%>
                    <asp:HyperLink ID="ResumeLink" runat="server" Target="_blank" /><br />
                </asp:Panel>
                <asp:FileUpload ID="ResumeUpload" runat="server" EnableViewState="false" />
                <asp:RequiredFieldValidator ID="ResumeFileRequiredValidator" runat="server" ControlToValidate="ResumeUpload" EnableViewState="false" ValidationGroup="apply" SetFocusOnError="True" Display="None" resourcekey="rfvResumeFile" />
                <asp:RegularExpressionValidator ID="ResumeFileExtensionValidator" runat="server" ControlToValidate="ResumeUpload" EnableViewState="false" ValidationGroup="apply" SetFocusOnError="true" Display="None" />
            </td>
        </tr>
        <tr id="ResumeMessageRow" runat="server"><td colspan="2">
            <asp:Label runat="server" EnableViewState="false" resourcekey="lblApplicationResumeMessage" />
        </td></tr>
        <tr><td>
            <asp:LinkButton ID="ApplyButton" runat="server" CssClass="Normal" EnableViewState="false" ValidationGroup="apply" resourcekey="btnApply" />
        </td><td class="alignRight">
            <asp:Label runat="server" EnableViewState="false" resourcekey="lblRequired" CssClass="SubSubHead alignRight" />
        </td></tr>
        <tr>
            <td colspan="2">
                <%--<asp:CustomValidator ID="cvAlreadyApplied" runat="server" EnableViewState="false" ValidationGroup="apply" OnServerValidate="cvAlreadyApplied_ServerValidate" SetFocusOnError="True" Display="None" resourcekey="cvAlreadyApplied" />--%>
                <asp:ValidationSummary CssClass="NormalRed" runat="server" EnableViewState="false" DisplayMode="BulletList" ShowMessageBox="true" ShowSummary="false" ValidationGroup="apply" />
            </td>
        </tr>
    </table>
</div>
<div id="EmailFriendSection" runat="server" visible="false" class="email_jobs" enableviewstate="false">
    <table>
        <tr>
            <td class="SubHead">
                <asp:Label runat="server" resourcekey="lblEmailFromNameHeader" EnableViewState="false" />
            </td>
            <td>
                <asp:TextBox ID="FromNameTextBox" runat="server" Columns="30" EnableViewState="false" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="FromNameTextBox" EnableViewState="false" ValidationGroup="email" SetFocusOnError="True" Display="None" resourcekey="rfvFromName"/>
            </td>
        </tr>
        <tr>
            <td class="SubHead">
                <asp:Label runat="server" resourcekey="lblEmailToAddressHeader" EnableViewState="false" />
            </td>
            <td>
                <asp:TextBox ID="SendToAddressTextBox" runat="server" Columns="35" EnableViewState="false" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="SendToAddressTextBox" Display="None" EnableViewState="false" ValidationGroup="email" SetFocusOnError="True" resourcekey="rfvSendToAddress" />
                <asp:RegularExpressionValidator ID="FriendEmailRegexValidator" runat="server" EnableViewState="false" ControlToValidate="SendToAddressTextBox" Display="None" ValidationGroup="email" SetFocusOnError="true" resourcekey="regexFriendEmailValidation" />
                <br/>
                <asp:Label runat="server" resourcekey="lblEmailToAddressMessage" EnableViewState="false" />
            </td>
        </tr>
        <tr>
            <td class="SubHead">
                <asp:Label runat="server" resourcekey="lblEmailMessageHeader" EnableViewState="false" />
            </td>
            <td>
                <asp:TextBox ID="FriendEmailMessageTextBox" runat="server" Columns="30" Rows="5" TextMode="MultiLine" EnableViewState="false"/>
            </td>
        </tr>
        <tr>
            <td colspan="2">
                <asp:LinkButton ID="SendToFriendButton" runat="server" ValidationGroup="email" resourcekey="btnSendToFriend" EnableViewState="false" />
                <asp:ValidationSummary runat="server" DisplayMode="BulletList" ShowMessageBox="true" ShowSummary="false" ValidationGroup="email" EnableViewState="false" />
            </td>
        </tr>
    </table>
</div>
