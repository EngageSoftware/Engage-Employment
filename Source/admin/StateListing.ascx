<%@ Import Namespace="System.Globalization"%>
<%@ Control language="C#" Inherits="Engage.Dnn.Employment.Admin.StateListing" AutoEventWireup="false" Codebehind="StateListing.ascx.cs" %>
<div class="information"><asp:Label ResourceKey="lblStatesHeader.Help" runat="server"/></div>

<asp:Button ID="AddButton" runat="server" resourcekey="btnAdd" />
<asp:GridView ID="StatesGridView" runat="server" AutoGenerateColumns="False" CssClass="employmentTable" BorderStyle="None" GridLines="None">
    <AlternatingRowStyle CssClass="DataGrid_AlternatingItem" />
    <RowStyle CssClass="DataGrid_Item" />
    <SelectedRowStyle CssClass="DataGrid_SelectedItem" />
    <HeaderStyle CssClass="DataGrid_Header" />
    <FooterStyle CssClass="DataGrid_Footer" />
    <EmptyDataTemplate>
        <asp:Label ID="lblNoStates" runat="server" resourcekey="lblNoStates" />
    </EmptyDataTemplate> 
    <Columns>
        <asp:TemplateField HeaderText="StateName">
            <ItemTemplate>
                <span class="Normal"><%#HttpUtility.HtmlEncode((string)Eval("StateName")) %></span>
                <asp:HiddenField ID="StateIdHiddenField" runat="server" Value='<%#((int)Eval("StateId")).ToString(CultureInfo.InvariantCulture) %>' />
            </ItemTemplate>
            <EditItemTemplate>
                <asp:TextBox ID="StateTextBox" runat="server" Text='<%#Eval("StateName") %>' CssClass="NormalTextBox" />
                <asp:HiddenField ID="StateIdHiddenField" runat="server" Value='<%#((int)Eval("StateId")).ToString(CultureInfo.InvariantCulture) %>' />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="StateTextBox" Display="None" ValidationGroup="Edit" resourcekey="StateRequired" />
                <asp:RegularExpressionValidator runat="server" ControlToValidate="StateTextBox" Display="None" ValidationGroup="Edit" ValidationExpression="<%#MaxLengthValidationExpression %>" ErrorMessage="<%#MaxLengthValidationText %>" />
            </EditItemTemplate>
        </asp:TemplateField>
        <asp:TemplateField HeaderText="Abbreviation">
            <ItemTemplate>
                <span class="Normal"><%#HttpUtility.HtmlEncode((string)Eval("Abbreviation")) %></span>
            </ItemTemplate>
            <EditItemTemplate>
                <asp:TextBox ID="txtAbbreviation" runat="server" Text='<%#Eval("Abbreviation") %>' CssClass="NormalTextBox" />
                <asp:RegularExpressionValidator runat="server" ControlToValidate="txtAbbreviation" Display="None" ValidationGroup="Edit" ValidationExpression="<%#MaxAbbreviationLengthValidationExpression %>" ErrorMessage="<%#MaxAbbreviationLengthValidationText %>" />
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
                <asp:Button runat="server" resourcekey="Cancel" CommandName="Cancel" CausesValidation="false"/>
            </EditItemTemplate>
        </asp:TemplateField>
    </Columns>
</asp:GridView>
<asp:Panel ID="NewPanel" runat="server" Visible="false">
    <table class="employmentTable">
        <tr id="rowNewHeader" runat="server" visible="false"><th>
            <asp:Label ID="NewHeaderLabel" runat="server" resourcekey="StateName.Header" />
        </th><th>
            <asp:Label ID="NewAbbreviationHeaderLabel" runat="server" resourcekey="Abbreviation.Header" />
        </th><th></th><th></th></tr>
        <tr class='<%=this.NewPanel.CssClass %>'><td>
            <asp:TextBox ID="txtNewState" runat="server" CssClass="NormalTextBox" />
        </td><td>
            <asp:TextBox ID="txtNewAbbreviation" runat="server" CssClass="NormalTextBox" />
        </td><td class="labelColumn">
            <asp:Button ID="SaveNewButton" runat="server" resourcekey="btnSaveNew" ValidationGroup="New" />
        </td><td class="labelColumn">
            <asp:Button ID="CancelNewButton" runat="server" resourcekey="btnCancelNew" CausesValidation="false" />
        </td></tr>
    </table>
    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtNewState" Display="None" ValidationGroup="New" resourcekey="StateRequired" />
    <asp:RegularExpressionValidator ID="regexNewState" runat="server" ControlToValidate="txtNewState" Display="None" ValidationGroup="New" />
    <asp:RegularExpressionValidator ID="regexNewAbbreviation" runat="server" ControlToValidate="txtNewAbbreviation" Display="None" ValidationGroup="New" />
</asp:Panel>
<asp:CustomValidator ID="cvDuplicateState" runat="server" Display="None" ValidationGroup="Edit" resourcekey="DuplicateState.Text" />
<asp:ValidationSummary runat="server" CssClass="NormalRed" DisplayMode="BulletList" ValidationGroup="Edit" />
<asp:ValidationSummary runat="server" CssClass="NormalRed" DisplayMode="BulletList" ValidationGroup="New" />
<asp:LinkButton ID="BackButton" runat="server" CssClass="Normal" OnClick="BackButton_Click" resourcekey="btnBack" />