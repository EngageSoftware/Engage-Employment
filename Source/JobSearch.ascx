<%@ Import Namespace="System.Globalization"%>
<%@ Control language="C#" Inherits="Engage.Dnn.Employment.JobSearch" AutoEventWireup="false" Codebehind="JobSearch.ascx.cs" %>
<asp:Label ID="ErrorMessageLabel" runat="server" ForeColor="Red" ></asp:Label>
<asp:Panel ID="SearchInputPanel" runat="server" DefaultButton="SearchButton">
    <div class="job_search_table">
        <table class="Normal">
            <tr >
                <td class="SubHead"><asp:Label runat="server" resourcekey="lblCategoryHeader" /></td>
                <td colspan="2">
                    <asp:DropDownList ID="CategoryDropDownList" runat="server"/>
                </td>
            </tr>
            <tr>
                <td class="SubHead"><asp:Label runat="server" resourcekey="lblJobTitleHeader" /></td>
                <td colspan="2">
                    <asp:DropDownList ID="JobTitleDropDownList" runat="server"/>
                </td>
            </tr>
            <tr>
                <td class="SubHead"><asp:Label runat="server" ID="LocationHeaderLabel"/></td>
                <td><asp:DropDownList ID="LocationDropDownList" runat="server"/></td>
                <td><asp:RadioButtonList ID="LocationRadioButtonList" runat="server" AutoPostBack="true" RepeatDirection="Horizontal" /></td>
             </tr>
             <tr>
                <td class="SubHead"><asp:Label runat="server" ID="KeywordsHeaderLabel" resourcekey="lblKeywordsHeader" /></td>
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
            <td><%#HttpUtility.HtmlEncode((string)Eval("CategoryName")) %></td>
            <td><a href="<%# GetJobDetailUrl(Eval("JobId")) %>"><%# HttpUtility.HtmlEncode((string)Eval("JobTitle")) %></a></td>
            <td><%# HttpUtility.HtmlEncode(string.Format(CultureInfo.CurrentCulture, Localize("Location", LocalResourceFile), Eval("LocationName"), Eval("StateName"), Eval("StateAbbreviation"))) %></td>
            <td><%# HttpUtility.HtmlEncode(Convert.ToDateTime(this.Eval("PostedDate")).ToString("d MMM yyyy", CultureInfo.CurrentCulture))%></td>
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
<asp:Label ID="NoResultsLabel" runat="server" CssClass="Normal" resourcekey="lblNoResults" EnableViewState="false" Visible="false" />
</div>

<asp:Panel ID="SaveSearchPanel" runat="server" Visible="false" DefaultButton="SaveSearchButton">
    <br />
    <table class="job_search_table nospacing Normal">
        <tr>
            <td>
                <asp:Label runat="server" ID="SaveHeaderLabel" CssClass="SubHead" resourcekey="lblSaveHeader" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:Label runat="server" ID="SearchNameLabel" CssClass="Normal" resourcekey="lblSearchName" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:TextBox ID="txtSearchName" runat="server" MaxLength="255" EnableViewState="false" />&nbsp;
                <asp:RequiredFieldValidator ID="SearchNameRequiredFieldValidator" runat="server" ControlToValidate="txtSearchName" ValidationGroup="saveSearch" resourcekey="rfvSearchName" Display="None" />
                <asp:ValidationSummary ID="ValidationSummary" runat="server" DisplayMode="BulletList" ShowMessageBox="true" ShowSummary="false" ValidationGroup="saveSearch" />
            </td>
        </tr>
        <tr>
            <td>
                <asp:LinkButton ID="SaveSearchButton" runat="server" CssClass="CommandButton" resourcekey="btnSaveSearch" ValidationGroup="saveSearch" />
            </td>
        </tr>
    </table>
</asp:Panel>
