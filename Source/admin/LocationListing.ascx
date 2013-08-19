<%@ Import Namespace="System.Globalization"%>
<%@ Control language="C#" Inherits="Engage.Dnn.Employment.Admin.LocationListing" AutoEventWireup="false" Codebehind="LocationListing.ascx.cs" %>

<div class="information"><asp:Label ResourceKey="lblLocationsHeader.Help" runat="server" /></div>

<asp:Button ID="AddButton" runat="server" resourcekey="btnAdd" />
<asp:GridView ID="LocationsGridView" runat="server" AutoGenerateColumns="False" CssClass="employmentTable" BorderStyle="None" GridLines="None">
    <AlternatingRowStyle CssClass="DataGrid_AlternatingItem" />
    <RowStyle CssClass="DataGrid_Item" />
    <SelectedRowStyle CssClass="DataGrid_SelectedItem" />
    <HeaderStyle CssClass="DataGrid_Header" />
    <FooterStyle CssClass="DataGrid_Footer" />
    <EmptyDataTemplate>
        <asp:Label ID="lblNoLocations" runat="server" resourcekey="lblNoLocations" />
    </EmptyDataTemplate>
    <Columns>
        <asp:TemplateField HeaderText="LocationName">
            <ItemTemplate>
                <span class="Normal"><%#HttpUtility.HtmlEncode((string)Eval("LocationName")) %></span>
                <asp:HiddenField ID="LocationIdHiddenField" runat="server" Value='<%#((int)Eval("LocationId")).ToString(CultureInfo.InvariantCulture) %>' />
            </ItemTemplate>
            <EditItemTemplate>
                <asp:TextBox ID="LocationNameTextBox" runat="server" Text='<%#Eval("LocationName") %>' CssClass="NormalTextBox" MaxLength='<%#LocationMaxLength %>' />
                <asp:HiddenField ID="LocationIdHiddenField" runat="server" Value='<%#((int)Eval("LocationId")).ToString(CultureInfo.InvariantCulture) %>' />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="LocationNameTextBox" Display="None" ValidationGroup="Edit" resourcekey="LocationNameRequired" />
                <asp:RegularExpressionValidator runat="server" ControlToValidate="LocationNameTextBox" Display="None" ValidationGroup="Edit" ValidationExpression="<%#MaxLengthValidationExpression %>" ErrorMessage="<%#MaxLengthValidationText %>" />
            </EditItemTemplate>
        </asp:TemplateField>
        <asp:TemplateField HeaderText="ParentState">
            <ItemTemplate>
                <span class="Normal"><%#HttpUtility.HtmlEncode((string)Eval("StateName")) %></span>
            </ItemTemplate>
            <EditItemTemplate>
                <asp:DropDownList ID="ddlState" runat="server" />
            </EditItemTemplate>
        </asp:TemplateField>
        <asp:TemplateField>
            <ItemStyle CssClass="labelColumn" />
            <ItemTemplate>
                <asp:Button runat="server" resourcekey="Edit" CommandName="Edit" />
            </ItemTemplate>
            <EditItemTemplate>
                <asp:Button runat="server" resourcekey="Save" CommandName="Save" CommandArgument='<%# Container.DataItemIndex.ToString(CultureInfo.InvariantCulture) %>' ValidationGroup="Edit" />
            </EditItemTemplate>
        </asp:TemplateField>
        <asp:TemplateField>
            <ItemStyle CssClass="labelColumn" />
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
            <asp:Label ID="NewHeaderLabel" runat="server" resourcekey="LocationName.Header" />
        </th><th>
            <asp:Label ID="NewStateHeaderLabel" runat="server" resourcekey="ParentState.Header" />
        </th><th></th><th></th></tr>
        <tr class='<%=this.NewPanel.CssClass %>'><td>
            <asp:TextBox ID="txtNewLocationName" runat="server" CssClass="NormalTextBox" />
        </td><td>
            <asp:DropDownList ID="NewStateDropDownList" runat="server"/>
        </td><td class="labelColumn">
            <asp:Button ID="SaveNewButton" runat="server" resourcekey="btnSaveNew" ValidationGroup="New" />
        </td><td class="labelColumn">
            <asp:Button ID="CancelNewButton" runat="server" resourcekey="btnCancelNew" CausesValidation="false" />
        </td></tr>
    </table>
    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtNewLocationName" Display="None" ValidationGroup="New" resourcekey="LocationNameRequired" />
    <asp:RegularExpressionValidator ID="regexNewLocationName" runat="server" ControlToValidate="txtNewLocationName" Display="None" ValidationGroup="New" />
</asp:Panel>
<asp:CustomValidator ID="cvDuplicateLocation" runat="server" Display="None" ValidationGroup="Edit" resourcekey="DuplicateLocation.Text" />
<asp:ValidationSummary runat="server" CssClass="NormalRed" DisplayMode="BulletList" ValidationGroup="Edit" />
<asp:ValidationSummary runat="server" CssClass="NormalRed" DisplayMode="BulletList" ValidationGroup="New" />

<asp:LinkButton ID="BackButton" runat="server" CssClass="Normal" resourcekey="btnBack" />