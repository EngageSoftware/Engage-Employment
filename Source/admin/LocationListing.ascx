<%@ Import Namespace="Engage.Dnn.Employment"%>
<%@ Import Namespace="System.Globalization"%>
<%@ Control language="C#" Inherits="Engage.Dnn.Employment.Admin.LocationListing" AutoEventWireup="false" Codebehind="LocationListing.ascx.cs" %>
<%@ Register TagPrefix="dnn" TagName="Label" Src="~/controls/LabelControl.ascx" %>

<div class="information"><asp:Label ResourceKey="lblLocationsHeader.Help" runat="server" /></div>

<asp:Button ID="btnAdd" runat="server" resourcekey="btnAdd" />
<asp:GridView ID="gvLocations" runat="server" AutoGenerateColumns="False" CssClass="employmentTable" BorderStyle="None" GridLines="None">
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
                <span class="Normal"><%#Eval("LocationName") %></span>
                <asp:HiddenField ID="hdnLocationId" runat="server" Value='<%#((int)Eval("LocationId")).ToString(CultureInfo.InvariantCulture) %>' />
            </ItemTemplate>
            <EditItemTemplate>
                <asp:TextBox ID="txtLocationName" runat="server" Text='<%#Eval("LocationName") %>' CssClass="NormalTextBox" MaxLength='<%#LocationMaxLength %>' />
                <asp:HiddenField ID="hdnLocationId" runat="server" Value='<%#((int)Eval("LocationId")).ToString(CultureInfo.InvariantCulture) %>' />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtLocationName" Display="None" ValidationGroup="Edit" resourcekey="LocationNameRequired" />
                <asp:RegularExpressionValidator runat="server" ControlToValidate="txtLocationName" Display="None" ValidationGroup="Edit" ValidationExpression="<%#MaxLengthValidationExpression %>" ErrorMessage="<%#MaxLengthValidationText %>" />
            </EditItemTemplate>
        </asp:TemplateField>
        <asp:TemplateField HeaderText="ParentState">
            <ItemTemplate>
                <span class="Normal"><%#Eval("StateName") %></span>
            </ItemTemplate>
            <EditItemTemplate>
                <asp:DropDownList ID="ddlState" runat="server" />
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
            <asp:Label ID="lblNewHeader" runat="server" resourcekey="LocationName.Header" />
        </th><th>
            <asp:Label ID="lblNewStateHeader" runat="server" resourcekey="ParentState.Header" />
        </th><th></th><th></th></tr>
        <tr class='<%=pnlNew.CssClass %>'><td>
            <asp:TextBox ID="txtNewLocationName" runat="server" CssClass="NormalTextBox" />
        </td><td>
            <asp:DropDownList ID="ddlNewState" runat="server"/>
        </td><td class="labelColumn">
            <asp:Button ID="btnSaveNew" runat="server" resourcekey="btnSaveNew" ValidationGroup="New" />
        </td><td class="labelColumn">
            <asp:Button ID="btnCancelNew" runat="server" resourcekey="btnCancelNew" CausesValidation="false" />
        </td></tr>
    </table>
    <asp:RequiredFieldValidator runat="server" ControlToValidate="txtNewLocationName" Display="None" ValidationGroup="New" resourcekey="LocationNameRequired" />
    <asp:RegularExpressionValidator ID="regexNewLocationName" runat="server" ControlToValidate="txtNewLocationName" Display="None" ValidationGroup="New" />
</asp:Panel>
<asp:CustomValidator ID="cvDuplicateLocation" runat="server" Display="None" ValidationGroup="Edit" resourcekey="DuplicateLocation.Text" />
<asp:ValidationSummary runat="server" CssClass="NormalRed" DisplayMode="BulletList" ValidationGroup="Edit" />
<asp:ValidationSummary runat="server" CssClass="NormalRed" DisplayMode="BulletList" ValidationGroup="New" />

<asp:LinkButton ID="btnBack" runat="server" CssClass="Normal" resourcekey="btnBack" />