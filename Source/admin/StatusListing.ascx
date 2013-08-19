<%@ Import namespace="System.Globalization"%>
<%@ Control language="C#" Inherits="Engage.Dnn.Employment.Admin.StatusListing" AutoEventWireup="false" Codebehind="StatusListing.ascx.cs" %>
<div class="information"><asp:Label ResourceKey="lblStatusesHeader.Help" runat="server" /></div>

<asp:Button ID="AddButton" runat="server" resourcekey="btnAdd" />
<asp:GridView ID="StatusesGridView" runat="server" AutoGenerateColumns="False" CssClass="employmentTable" BorderStyle="None" GridLines="None">
    <AlternatingRowStyle CssClass="DataGrid_AlternatingItem" />
    <RowStyle CssClass="DataGrid_Item" />
    <SelectedRowStyle CssClass="DataGrid_SelectedItem" />
    <HeaderStyle CssClass="DataGrid_Header" />
    <FooterStyle CssClass="DataGrid_Footer" />
    <EmptyDataTemplate>
        <asp:Label ID="lblNoStatuses" runat="server" resourcekey="lblNoStatuses" />
    </EmptyDataTemplate>
    <Columns>
        <asp:TemplateField HeaderText="StatusName">
            <ItemTemplate>
                <span class="Normal"><%#HttpUtility.HtmlEncode((string)Eval("Status")) %></span>
                <asp:HiddenField ID="StatusIdHiddenField" runat="server" Value='<%#((int)Eval("StatusId")).ToString(CultureInfo.InvariantCulture) %>' />
            </ItemTemplate>
            <EditItemTemplate>
                <asp:TextBox ID="StatusTextBox" runat="server" Text='<%#Eval("Status") %>' CssClass="NormalTextBox" />
                <asp:HiddenField ID="StatusIdHiddenField" runat="server" Value='<%#((int)Eval("StatusId")).ToString(CultureInfo.InvariantCulture) %>' />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="StatusTextBox" Display="None" ValidationGroup="Edit" resourcekey="StatusRequired" />
                <asp:RegularExpressionValidator runat="server" ControlToValidate="StatusTextBox" Display="None" ValidationGroup="Edit" ValidationExpression="<%#MaxLengthValidationExpression %>" ErrorMessage="<%#MaxLengthValidationText %>" />
            </EditItemTemplate>
        </asp:TemplateField>
        <asp:TemplateField ItemStyle-CssClass="labelColumn">
            <ItemTemplate>
                <asp:Button runat="server" resourcekey="Edit" CommandName="Edit" />
            </ItemTemplate>
            <EditItemTemplate>
                <asp:Button runat="server" resourcekey="Save" CommandName="Save" CommandArgument='<%# Container.DataItemIndex.ToString(CultureInfo.InvariantCulture) %>' ValidationGroup="Edit" />
            </EditItemTemplate>
        </asp:TemplateField>
        <asp:TemplateField ItemStyle-CssClass="labelColumn">
            <ItemTemplate>
                <asp:Button ID="DeleteButton" runat="server" resourcekey="Delete" CommandName="Delete" CausesValidation="false" />
            </ItemTemplate>
            <EditItemTemplate>
                <asp:Button runat="server" resourcekey="Cancel" CommandName="Cancel" CausesValidation="false" />
            </EditItemTemplate>
        </asp:TemplateField>
    </Columns>
</asp:GridView>
<asp:Panel ID="NewPanel" runat="server" Visible="false">
    <table class="employmentTable">
        <tr id="rowNewHeader" runat="server" visible="false"><th>
            <asp:Label ID="NewHeaderLabel" runat="server" resourcekey="StatusName.Header" />
        </th><th></th><th></th></tr>
        <tr class='<%=this.NewPanel.CssClass %>'><td>
            <asp:TextBox ID="txtNewStatus" runat="server" CssClass="NormalTextBox" />
        </td><td class="labelColumn">
            <asp:Button ID="SaveNewButton" runat="server" resourcekey="btnSaveNew" ValidationGroup="New" />
        </td><td class="labelColumn">
            <asp:Button ID="CancelNewButton" runat="server" resourcekey="btnCancelNew" CausesValidation="false" />
        </td></tr>
    </table>
    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtNewStatus" Display="None" ValidationGroup="New" resourcekey="StatusRequired" />
    <asp:RegularExpressionValidator ID="regexNewUserStatus" runat="server" ControlToValidate="txtNewStatus" Display="None" ValidationGroup="New" />
</asp:Panel>
<asp:CustomValidator ID="cvDuplicateUserStatus" runat="server" Display="None" ValidationGroup="Edit" resourcekey="DuplicateStatus.Text" />
<asp:ValidationSummary runat="server" CssClass="NormalRed" DisplayMode="BulletList" ValidationGroup="Edit" />
<asp:ValidationSummary runat="server" CssClass="NormalRed" DisplayMode="BulletList" ValidationGroup="New" />

<asp:LinkButton ID="BackButton" runat="server" CssClass="Normal" resourcekey="btnBack" />