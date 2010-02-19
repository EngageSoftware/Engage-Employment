<%@ Import Namespace="System.Globalization"%>
<%@ Control language="C#" Inherits="Engage.Dnn.Employment.Admin.StateListing" AutoEventWireup="false" Codebehind="StateListing.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>
<div class="information"><asp:Label ResourceKey="lblStatesHeader.Help" runat="server"/></div>

<asp:Button ID="btnAdd" runat="server" resourcekey="btnAdd" />
<asp:GridView ID="gvStates" runat="server" AutoGenerateColumns="False" CssClass="employmentTable" BorderStyle="None" GridLines="None">
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
                <span class="Normal"><%#Eval("StateName") %></span>
                <asp:HiddenField ID="hdnStateId" runat="server" Value='<%#((int)Eval("StateId")).ToString(CultureInfo.InvariantCulture) %>' />
            </ItemTemplate>
            <EditItemTemplate>
                <asp:TextBox ID="txtState" runat="server" Text='<%#Eval("StateName") %>' CssClass="NormalTextBox" />
                <asp:HiddenField ID="hdnStateId" runat="server" Value='<%#((int)Eval("StateId")).ToString(CultureInfo.InvariantCulture) %>' />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtState" Display="None" ValidationGroup="Edit" resourcekey="StateRequired" />
                <asp:RegularExpressionValidator runat="server" ControlToValidate="txtState" Display="None" ValidationGroup="Edit" ValidationExpression="<%#MaxLengthValidationExpression %>" ErrorMessage="<%#MaxLengthValidationText %>" />
            </EditItemTemplate>
        </asp:TemplateField>
        <asp:TemplateField HeaderText="Abbreviation">
            <ItemTemplate>
                <span class="Normal"><%#Eval("Abbreviation") %></span>
            </ItemTemplate>
            <EditItemTemplate>
                <asp:TextBox ID="txtAbbreviation" runat="server" Text='<%#Eval("Abbreviation") %>' CssClass="NormalTextBox" />
                <asp:RegularExpressionValidator runat="server" ControlToValidate="txtAbbreviation" Display="None" ValidationGroup="Edit" ValidationExpression="<%#MaxAbbreviationLengthValidationExpression %>" ErrorMessage="<%#MaxAbbreviationLengthValidationText %>" />
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
<asp:Panel ID="pnlNew" runat="server" Visible="false">
    <table class="employmentTable">
        <tr id="rowNewHeader" runat="server" visible="false"><th>
            <asp:Label ID="lblNewHeader" runat="server" resourcekey="StateName.Header" />
        </th><th>
            <asp:Label ID="lblNewAbbreviationHeader" runat="server" resourcekey="Abbreviation.Header" />
        </th><th></th><th></th></tr>
        <tr class='<%=pnlNew.CssClass %>'><td>
            <asp:TextBox ID="txtNewState" runat="server" CssClass="NormalTextBox" />
        </td><td>
            <asp:TextBox ID="txtNewAbbreviation" runat="server" CssClass="NormalTextBox" />
        </td><td class="labelColumn">
            <asp:Button ID="btnSaveNew" runat="server" resourcekey="btnSaveNew" ValidationGroup="New" />
        </td><td class="labelColumn">
            <asp:Button ID="btnCancelNew" runat="server" resourcekey="btnCancelNew" CausesValidation="false" />
        </td></tr>
    </table>
    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtNewState" Display="None" ValidationGroup="New" resourcekey="StateRequired" />
    <asp:RegularExpressionValidator ID="regexNewState" runat="server" ControlToValidate="txtNewState" Display="None" ValidationGroup="New" />
    <asp:RegularExpressionValidator ID="regexNewAbbreviation" runat="server" ControlToValidate="txtNewAbbreviation" Display="None" ValidationGroup="New" />
</asp:Panel>
<asp:CustomValidator ID="cvDuplicateState" runat="server" Display="None" ValidationGroup="Edit" resourcekey="DuplicateState.Text" />
<asp:ValidationSummary runat="server" CssClass="NormalRed" DisplayMode="BulletList" ValidationGroup="Edit" />
<asp:ValidationSummary runat="server" CssClass="NormalRed" DisplayMode="BulletList" ValidationGroup="New" />
<asp:LinkButton ID="btnBack" runat="server" CssClass="Normal" OnClick="btnBack_Click" resourcekey="btnBack" />