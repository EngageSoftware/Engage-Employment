<%@ Import Namespace="System.Globalization"%>
<%@ Control language="C#" Inherits="Engage.Dnn.Employment.Admin.PositionListing" AutoEventWireup="false" Codebehind="PositionListing.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<%@ Register TagPrefix="dnn" TagName="TextEditor" Src="~/controls/texteditor.ascx" %>

<span class="Head"><dnn:Label ResourceKey="lblPositionsHeader" runat="server" /></span>

<asp:Button ID="AddButton" runat="server" resourcekey="btnAdd" />
<asp:GridView ID="PositionsGrid" runat="server" AutoGenerateColumns="False" CssClass="employmentTable" BorderStyle="None" GridLines="None">
    <AlternatingRowStyle CssClass="DataGrid_AlternatingItem" />
    <RowStyle CssClass="DataGrid_Item" />
    <SelectedRowStyle CssClass="DataGrid_SelectedItem" />
    <HeaderStyle CssClass="DataGrid_Header" />
    <FooterStyle CssClass="DataGrid_Footer" />
    <EmptyDataTemplate>
        <asp:Label ID="lblNoPositions" runat="server" resourcekey="lblNoPositions" />
    </EmptyDataTemplate>
    <Columns>
        <asp:TemplateField HeaderText="JobTitle">
            <ItemTemplate>
                <span class="Normal"><%#Eval("JobTitle") %></span>
                <asp:HiddenField ID="hdnPositionId" runat="server" Value='<%#((int)Eval("PositionId")).ToString(CultureInfo.InvariantCulture) %>' />
            </ItemTemplate>
            <EditItemTemplate>
                <asp:TextBox ID="txtJobTitle" runat="server" Text='<%#Eval("JobTitle") %>' CssClass="NormalTextBox" MaxLength='<%#JobTitleMaxLength %>' />
                <asp:HiddenField ID="hdnPositionId" runat="server" Value='<%#((int)Eval("PositionId")).ToString(CultureInfo.InvariantCulture) %>' />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtJobTitle" Display="None" ValidationGroup="Edit" resourcekey="JobTitleRequired" />
                <asp:RegularExpressionValidator runat="server" ControlToValidate="txtJobTitle" Display="None" ValidationGroup="Edit" ValidationExpression="<%#MaxLengthValidationExpression %>" ErrorMessage="<%#MaxLengthValidationText %>" />
            </EditItemTemplate>
        </asp:TemplateField>
        <asp:TemplateField HeaderText="JobDescription">
            <ItemTemplate>
                <span class="Normal"><%#Eval("JobDescription") %></span>
            </ItemTemplate>
            <EditItemTemplate>
                <dnn:TextEditor ID="txtJobDescription" runat="server" Text='<%#Eval("JobDescription") %>' Height="400px" Width="400px" ChooseMode="true" HtmlEncode="false" TextRenderMode="raw" />
                <asp:CustomValidator ID="JobDescriptionRequiredValidator" runat="server" Display="None" ValidationGroup="Edit" ValidateEmptyText="true" OnServerValidate="JobDescriptionRequiredValidator_ServerValidate" resourcekey="cvJobDescription" />
            </EditItemTemplate>
        </asp:TemplateField>
        <asp:TemplateField>
            <ItemStyle CssClass="labelColumn" />
            <ItemTemplate>
                <asp:Button ID="btnEdit" runat="server" resourcekey="Edit" CommandName="Edit" />
            </ItemTemplate>
            <EditItemTemplate>
                <asp:Button ID="btnSave" runat="server" resourcekey="Save" CommandName="Save" CommandArgument='<%# Container.DataItemIndex.ToString(CultureInfo.InvariantCulture) %>' ValidationGroup="Edit" />
            </EditItemTemplate>
        </asp:TemplateField>
        <asp:TemplateField>
            <ItemStyle CssClass="labelColumn" />
            <ItemTemplate>
                <asp:Button ID="btnDelete" runat="server" resourcekey="Delete" CommandName="Delete" CausesValidation="false" />
            </ItemTemplate>
            <EditItemTemplate>
                <asp:Button ID="btnCancel" runat="server" resourcekey="Cancel" CommandName="Cancel" CausesValidation="false"/>
            </EditItemTemplate>
        </asp:TemplateField>
    </Columns>
</asp:GridView>
<asp:Panel ID="NewPositionPanel" runat="server" Visible="false">
    <table class="employmentTable">
        <tr id="NewPositionHeaderRow" runat="server" visible="false"><th>
            <asp:Label ID="lblNewHeader" runat="server" resourcekey="JobTitle.Header" />
        </th><th>
            <asp:Label ID="lblNewDescriptionHeader" runat="server" resourcekey="JobDescription.Header" />
        </th><th></th><th></th></tr>
        <tr class='<%=this.NewPositionPanel.CssClass %>'><td>
            <asp:TextBox ID="NewJobTitleTextBox" runat="server" CssClass="NormalTextBox" />
        </td><td>
            <dnn:TextEditor ID="NewJobDescriptionTextEditor" runat="server" Height="400px" Width="400px" ChooseMode="true" HtmlEncode="false" TextRenderMode="raw" EnableViewState="false" />
        </td><td class="labelColumn">
            <asp:Button ID="SaveNewPositionButton" runat="server" resourcekey="btnSaveNew" ValidationGroup="New" />
        </td><td class="labelColumn">
            <asp:Button ID="CancelNewPositionButton" runat="server" resourcekey="btnCancelNew" CausesValidation="false" />
        </td></tr>
    </table>
    <asp:RequiredFieldValidator runat="server" ControlToValidate="NewJobTitleTextBox" Display="None" ValidationGroup="New" resourcekey="JobTitleRequired" />
    <asp:RegularExpressionValidator ID="NewJobTitleLengthValidator" runat="server" ControlToValidate="NewJobTitleTextBox" Display="None" ValidationGroup="New" />
    <asp:CustomValidator runat="server" Display="None" ValidationGroup="New" ValidateEmptyText="true" OnServerValidate="NewJobDescriptionRequiredValidator_ServerValidate" resourcekey="cvJobDescription" />
</asp:Panel>
<asp:CustomValidator ID="DuplicatePositionValidator" runat="server" Display="None" ValidationGroup="Edit" resourcekey="DuplicatePosition.Text" />
<asp:ValidationSummary runat="server" CssClass="NormalRed" DisplayMode="BulletList" ValidationGroup="Edit" />
<asp:ValidationSummary runat="server" CssClass="NormalRed" DisplayMode="BulletList" ValidationGroup="New" />

<asp:LinkButton ID="BackButton" runat="server" CssClass="Normal" resourcekey="btnBack" />