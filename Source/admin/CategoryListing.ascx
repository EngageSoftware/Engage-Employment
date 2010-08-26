<%@ Import Namespace="System.Globalization"%>
<%@ Control language="C#" Inherits="Engage.Dnn.Employment.Admin.CategoryListing" AutoEventWireup="false" Codebehind="CategoryListing.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="information"><asp:Label ResourceKey="lblCategoriesHeader.Help" runat="server" /></div>

<asp:Button ID="AddButton" runat="server" resourcekey="btnAdd" />
<asp:GridView ID="CategoriesGridView" runat="server" AutoGenerateColumns="False" CssClass="employmentTable" BorderStyle="None" GridLines="None">
    <AlternatingRowStyle CssClass="DataGrid_AlternatingItem" />
    <RowStyle CssClass="DataGrid_Item" />
    <SelectedRowStyle CssClass="DataGrid_SelectedItem" />
    <HeaderStyle CssClass="DataGrid_Header" />
    <FooterStyle CssClass="DataGrid_Footer" />
    <EmptyDataTemplate>
        <asp:Label ID="lblNoCategories" runat="server" resourcekey="lblNoCategories" />
    </EmptyDataTemplate>
    <Columns>
        <asp:TemplateField HeaderText="CategoryName">
            <ItemTemplate>
                <span class="Normal"><%#HttpUtility.HtmlEncode((string)Eval("CategoryName")) %></span>
                <asp:HiddenField ID="hdnCategoryId" runat="server" Value='<%#((int)Eval("CategoryId")).ToString(CultureInfo.InvariantCulture) %>' />
            </ItemTemplate>
            <EditItemTemplate>
                <asp:TextBox ID="txtCategoryName" runat="server" Text='<%#Eval("CategoryName") %>' CssClass="NormalTextBox" />
                <asp:HiddenField ID="hdnCategoryId" runat="server" Value='<%#((int)Eval("CategoryId")).ToString(CultureInfo.InvariantCulture) %>' />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtCategoryName" Display="None" ValidationGroup="Edit" resourcekey="CategoryNameRequired" />
                <asp:RegularExpressionValidator runat="server" ControlToValidate="txtCategoryName" Display="None" ValidationGroup="Edit" ValidationExpression="<%#MaxLengthValidationExpression %>" ErrorMessage="<%#MaxLengthValidationText %>" />
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
<asp:Panel ID="PanelNew" runat="server" Visible="false">
    <table class="employmentTable">
        <tr id="rowNewHeader" runat="server" visible="false"><th>
            <asp:Label ID="NewHeaderLabel" runat="server" resourcekey="CategoryName.Header" />
        </th><th></th><th></th></tr>
        <tr class='<%=this.PanelNew.CssClass %>'><td>
            <asp:TextBox ID="txtNewCategoryName" runat="server" CssClass="NormalTextBox" />
        </td><td class="labelColumn">
            <asp:Button ID="SaveNewButton" runat="server" resourcekey="btnSaveNew" ValidationGroup="New" />
        </td><td class="labelColumn">
            <asp:Button ID="CancelNewButton" runat="server" resourcekey="btnCancelNew" CausesValidation="false" />
        </td></tr>
    </table>
    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtNewCategoryName" Display="None" ValidationGroup="New" resourcekey="CategoryNameRequired" />
    <asp:RegularExpressionValidator ID="regexNewCategoryName" runat="server" ControlToValidate="txtNewCategoryName" Display="None" ValidationGroup="New" />
</asp:Panel>
<asp:CustomValidator ID="cvDuplicateCategory" runat="server" Display="None" ValidationGroup="Edit" resourcekey="DuplicateCategory.Text" />
<asp:ValidationSummary runat="server" CssClass="NormalRed" DisplayMode="BulletList" ValidationGroup="Edit" />
<asp:ValidationSummary runat="server" CssClass="NormalRed" DisplayMode="BulletList" ValidationGroup="New" />

<asp:LinkButton ID="BackButton" runat="server" CssClass="Normal" resourcekey="btnBack" />