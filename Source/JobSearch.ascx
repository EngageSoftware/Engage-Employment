<%@ Import Namespace="DotNetNuke.Services.Localization"%>
<%@ Import Namespace="System.Globalization"%>
<%@ Control language="C#" Inherits="Engage.Dnn.Employment.JobSearch" AutoEventWireup="false" Codebehind="JobSearch.ascx.cs" %>
<asp:Label ID="lblErrorMessage" runat="server" ForeColor="Red" ></asp:Label>
<asp:Panel ID="pnlSearchInput" runat="server" DefaultButton="SearchButton">
    <div class="job_search_table">
        <table class="Normal">
            <tr >
                <td class="SubHead"><asp:Label runat="server" resourcekey="lblCategoryHeader" /></td>
                <td colspan="2">
                    <asp:DropDownList ID="ddlCategory" runat="server"/>
                </td>
            </tr>
            <tr>
                <td class="SubHead"><asp:Label runat="server" resourcekey="lblJobTitleHeader" /></td>
                <td colspan="2">
                    <asp:DropDownList ID="ddlJobTitle" runat="server"/>
                </td>
            </tr>
            <tr>
                <td class="SubHead"><asp:Label runat="server" ID="lblLocationHeader"/></td>
                <td><asp:DropDownList ID="ddlLocation" runat="server"/></td>
                <td><asp:RadioButtonList ID="LocationRadioButtonList" runat="server" AutoPostBack="true" RepeatDirection="Horizontal" /></td>
             </tr>
             <tr>
                <td class="SubHead"><asp:Label runat="server" ID="lblKeywordsHeader" resourcekey="lblKeywordsHeader" /></td>
                <td colspan="2">
                    <asp:TextBox ID="txtKeywords" runat="server" />
                </td>
            </tr>
            <tr>
                <td colspan="3">
                    <asp:LinkButton ID="SearchButton" runat="server" CssClass="CommandButton" resourcekey="btnSearch" />&nbsp;
                    <asp:LinkButton ID="BackButton" runat="server" CssClass="CommandButton" resourcekey="btnBack" />
                </td>
            </tr>
        </table>
    </div>
</asp:Panel>
<br />
<div class="job_search_table">
<asp:Repeater ID="rpSearchResults" runat="server" EnableViewState="False">
    <HeaderTemplate>
        <table class="Normal">
        <tr>
            <th class="nowrap"><asp:Label runat="server" resourcekey="lblResultsCategoryHeader" /></th>
            <th class="nowrap"><asp:Label runat="server" resourcekey="lblResultsJobTitleHeader" /></th>
            <th class="nowrap"><asp:Label runat="server" resourcekey="lblResultsLocationHeader" /></th>
            <th class="nowrap"><asp:Label runat="server" resourcekey="lblResultsPostedDateHeader" /></th>
        </tr>
    </HeaderTemplate>
    <ItemTemplate>
        <tr>
            <td><%#Eval("CategoryName") %></td>
            <td><a href="<%# GetJobDetailUrl(Eval("JobId")) %>"><%# Eval("JobTitle") %></a></td>
            <td><%# string.Format(CultureInfo.CurrentCulture, Localization.GetString("Location", LocalResourceFile), Eval("LocationName"), Eval("StateName"), Eval("StateAbbreviation")) %></td>
            <td><%# Convert.ToDateTime(Eval("PostedDate")).ToString("d MMM yyyy", CultureInfo.CurrentCulture)%></td>
        </tr>
        <tr>
            <td colspan="4">
                <asp:Label runat="server" CssClass="fieldLabel" resourcekey="lblResultsLabel"/><%# Eval("SearchResults") %>
            </td>
        </tr>
    </ItemTemplate>
    <FooterTemplate>
        </table>
    </FooterTemplate>
</asp:Repeater>
<asp:Label ID="lblNoResults" runat="server" CssClass="Normal" resourcekey="lblNoResults" EnableViewState="false" Visible="false" />
</div>

<asp:Panel ID="pnlSaveSearch" runat="server" Visible="false" DefaultButton="SaveSearchButton">
    <br />
    <table class="job_search_table nospacing Normal">
        <tr>
            <td>
                <asp:Label runat="server" ID="lblSaveHeader" CssClass="SubHead" resourcekey="lblSaveHeader" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:Label runat="server" ID="lblSearchName" CssClass="Normal" resourcekey="lblSearchName" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:TextBox ID="txtSearchName" runat="server" MaxLength="255" EnableViewState="false" />&nbsp;
                <asp:RequiredFieldValidator ID="rfvSearchName" runat="server" ControlToValidate="txtSearchName" ValidationGroup="saveSearch" resourcekey="rfvSearchName" Display="None" />
                <asp:ValidationSummary ID="valSummary" runat="server" DisplayMode="BulletList" ShowMessageBox="true" ShowSummary="false" ValidationGroup="saveSearch" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:LinkButton ID="SaveSearchButton" runat="server" CssClass="CommandButton" resourcekey="btnSaveSearch" ValidationGroup="saveSearch" />
            </td>
        </tr>
    </table>
</asp:Panel>
