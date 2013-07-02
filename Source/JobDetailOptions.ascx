<%@ Control Language="c#" AutoEventWireup="false" Inherits="Engage.Dnn.Employment.JobDetailOptions" Codebehind="JobDetailOptions.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="label" Src="~/controls/labelControl.ascx" %>
<div style="text-align:left">
    <table class="settingsTable Normal">
	    <tr>
		    <td class="SubHead labelColumn nowrap">
		        <dnn:label ResourceKey="lblApplicationEmailAddress" runat="server" />
		    </td>
		    <td class="contentColumn">
		        <asp:TextBox ID="txtApplicationEmailAddress" runat="server" cssclass="NormalTextBox" Width="75%" />
                <asp:RegularExpressionValidator ID="ApplicationEmailRegexValidator" runat="server" Display="None" ControlToValidate="txtApplicationEmailAddress" resourcekey="ApplicationEmailPatternValidator" ValidationGroup="SaveSettings" />
		    </td>
	    </tr>
	    <tr>
	        <td class="SubHead labelColumn nowrap">
	            <dnn:label ResourceKey="lblFriendEmailAddress" runat="server" />
	        </td>
	        <td class="contentColumn">
	            <asp:TextBox ID="txtFromEmailAddress" runat="server" cssclass="NormalTextBox" Width="75%" />
                <asp:RegularExpressionValidator ID="FromEmailRegexValidator" runat="server" Display="None" ControlToValidate="txtFromEmailAddress" resourcekey="FriendEmailPatternValidator" ValidationGroup="SaveSettings" />
	        </td>
	    </tr>
	    <tr>
	        <td class="SubHead labelColumn nowrap">
	            <dnn:label ResourceKey="lblRequireRegistration" runat="server" />
	        </td>
	        <td class="contentColumn">
	            <asp:CheckBox ID="RequireRegistrationCheckBox" runat="server" />
	        </td>
	    </tr>
	    <tr>
	        <td class="SubHead labelColumn nowrap">
	            <dnn:label ResourceKey="lblDisplayName" runat="server" />
	        </td>
	        <td class="contentColumn">
	            <asp:RadioButtonList ID="DisplayNameRadioButtonList" runat="server" RepeatDirection="Horizontal" />
	        </td>
	    </tr>
	    <tr>
	        <td class="SubHead labelColumn nowrap">
	            <dnn:label ResourceKey="lblDisplayEmail" runat="server" />
	        </td>
	        <td class="contentColumn">
	            <asp:RadioButtonList ID="DisplayEmailRadioButtonList" runat="server" RepeatDirection="Horizontal" />
	        </td>
	    </tr>
	    <tr>
	        <td class="SubHead labelColumn nowrap">
	            <dnn:label ResourceKey="lblDisplayPhone" runat="server" />
	        </td>
	        <td class="contentColumn">
	            <asp:RadioButtonList ID="DisplayPhoneRadioButtonList" runat="server" RepeatDirection="Horizontal" />
	        </td>
	    </tr>
	    <tr>
	        <td class="SubHead labelColumn nowrap">
	            <dnn:label ResourceKey="lblDisplayMessage" runat="server" />
	        </td>
	        <td class="contentColumn">
	            <asp:RadioButtonList ID="DisplayMessageRadioButtonList" runat="server" RepeatDirection="Horizontal" />
	        </td>
	    </tr>
	    <tr>
	        <td class="SubHead labelColumn nowrap">
	            <dnn:label ResourceKey="lblDisplaySalary" runat="server" />
	        </td>
	        <td class="contentColumn">
	            <asp:RadioButtonList ID="DisplaySalaryRadioButtonList" runat="server" RepeatDirection="Horizontal" />
	        </td>
	    </tr>
	    <tr>
	        <td class="SubHead labelColumn nowrap">
	            <dnn:label ResourceKey="lblDisplayCoverLetter" runat="server" />
	        </td>
	        <td class="contentColumn">
	            <asp:RadioButtonList ID="DisplayCoverLetterRadioButtonList" runat="server" RepeatDirection="Horizontal" />
	        </td>
	    </tr>
	    <tr>
	        <td class="SubHead labelColumn nowrap">
	            <dnn:label ResourceKey="lblDisplayResume" runat="server" />
	        </td>
	        <td class="contentColumn">
	            <asp:RadioButtonList ID="DisplayResumeRadioButtonList" runat="server" RepeatDirection="Horizontal" />
	        </td>
	    </tr>
	    <tr>
	        <td class="SubHead labelColumn nowrap">
	            <dnn:label ResourceKey="lblDisplayLead" runat="server" />
	        </td>
	        <td class="contentColumn">
	            <asp:RadioButtonList ID="DisplayLeadRadioButtonList" runat="server" RepeatDirection="Horizontal" AutoPostBack="true"/>
	        </td>
	    </tr>
	    <tr id="rowLeadItems" runat="server">
	        <td class="SubHead labelColumn nowrap"><dnn:label ResourceKey="lblLeadItems" runat="server" /></td>
	        <td class="contentColumn">
                <table class="employmentTable">
                    <tr><td class="contentColumn">
                        <asp:Button ID="NewLeadItemButton" runat="server" resourcekey="btnNewLeadItem"  />
                    </td><td class="labelColumn">&nbsp;</td></tr>
                </table>
	            <asp:GridView ID="LeadItemsGridView" runat="server" AutoGenerateColumns="false" CssClass="employmentTable" BorderStyle="None" GridLines="None">
                    <AlternatingRowStyle CssClass="DataGrid_AlternatingItem" />
                    <RowStyle CssClass="DataGrid_Item" />
                    <SelectedRowStyle CssClass="DataGrid_SelectedItem" />
                    <HeaderStyle CssClass="DataGrid_Header" />
                    <FooterStyle CssClass="DataGrid_Footer" />
                    <EmptyDataTemplate>
                        <asp:Label ID="lblNoLeadItems" runat="server" resourcekey="lblNoLeadItems" />
                    </EmptyDataTemplate>
                    <Columns>
                        <asp:TemplateField HeaderText="Name">
                            <ItemStyle CssClass="contentColumn" />
                            <ItemTemplate>
                                <span class="Normal"><%# HttpUtility.HtmlEncode((string)Eval("Text")) %></span>
                                <asp:HiddenField id="hdnLeadId" runat="server" Value='<%#Eval("EntryID") %>'/>
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:TextBox ID="txtLeadText" runat="server" Text='<%# Bind("Text") %>' />
                                <asp:HiddenField id="hdnLeadId" runat="server" Value='<%#Eval("EntryID") %>'/>
                                <asp:RequiredFieldValidator ID="rfvLeadEdit" runat="server" ControlToValidate="txtLeadText" Display="None" CssClass="NormalRed" ValidationGroup="LeadEdit" resourcekey="rfvLeadEdit" />
                            </EditItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField>
                            <ItemTemplate>
                                <asp:Button ID="btnEdit" runat="server" resourcekey="Edit" CommandName="Edit" />
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:Button ID="btnSave" runat="server" resourcekey="Save" CommandName="Save" CommandArgument='<%# Container.DataItemIndex %>' ValidationGroup="LeadEdit" />
                            </EditItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField>
                            <ItemStyle CssClass="labelColumn" />
                            <ItemTemplate>
                                <asp:Button ID="btnDelete" runat="server" resourcekey="Delete" CommandName="Delete" />
                            </ItemTemplate>
                            <EditItemTemplate>
                                <asp:Button ID="btnCancel" runat="server" resourcekey="Cancel" CommandName="Cancel"/>
                            </EditItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
                <asp:CustomValidator ID="cvLeadEdit" runat="server" Display="None" CssClass="NormalRed" ValidationGroup="LeadEdit" resourcekey="cvLeadEdit" />
                <asp:CustomValidator ID="SaveLeadRequirementValidator" runat="server" Display="None" CssClass="NormalRed" ValidationGroup="SaveSettings" resourcekey="cvSaveLeadRequirement" />
                <asp:Panel ID="NewLeadItemPanel" runat="server" Visible="false">
                    <table class="employmentTable">
                        <tr id="rowNewLeadItemHeader" runat="server" visible="false"><th>
                            <asp:Label ID="NewLeadItemHeaderLabel" runat="server" resourcekey="Name.Header" />
                        </th><th></th></tr>
                        <tr class='<%=this.NewLeadItemPanel.CssClass %>'><td class="contentColumn">
                            <asp:TextBox ID="txtNewLeadText" runat="server" CssClass="NormalTextBox" />
                        </td><td class="labelColumn">
                            <asp:Button ID="SaveNewLeadButton" runat="server" resourcekey="btnSaveNewLead" ValidationGroup="NewLead" />
                        </td></tr>
                        <tr><td colspan="2">
                            <asp:CustomValidator ID="NewLeadUniqueValidator" runat="server" ControlToValidate="txtNewLeadText" Display="None" CssClass="NormalRed" ValidationGroup="NewLead" resourcekey="cvNewLead" />
                            <asp:RequiredFieldValidator ID="NewLeadRequiredFieldValidator" runat="server" ControlToValidate="txtNewLeadText" Display="None" CssClass="NormalRed" ValidationGroup="NewLead" resourcekey="rfvNewLead" />
                        </td></tr>
                    </table>
                </asp:Panel>
            </td>
	    </tr>
	    <tr>
		    <td class="SubHead labelColumn nowrap">
		        <dnn:label ResourceKey="lblEnableDnnSearch" runat="server" />
		    </td>
		    <td class="contentColumn">
		        <asp:CheckBox ID="EnableDnnSearchCheckBox" runat="server"/>
		    </td>
	    </tr>
	    <tr>
		    <td class="SubHead labelColumn nowrap">
		        <dnn:label ResourceKey="lblShowCloseDate" runat="server" />
		    </td>
		    <td class="contentColumn">
		        <asp:CheckBox ID="ShowCloseDateCheckBox" runat="server"/>
		    </td>
	    </tr>
	    <tr>
	        <td colspan="2">
                <asp:ValidationSummary runat="server" CssClass="NormalRed" DisplayMode="BulletList" ValidationGroup="LeadEdit" />
                <asp:ValidationSummary runat="server" CssClass="NormalRed" DisplayMode="BulletList" ValidationGroup="NewLead" />
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