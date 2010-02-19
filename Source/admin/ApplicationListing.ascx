<%@ Import Namespace="Engage.Dnn.Employment"%>
<%@ Import namespace="System.Globalization"%>
<%@ Import namespace="DotNetNuke.Common.Utilities"%>
<%@ Import namespace="DotNetNuke.Services.Localization"%>
<%@ Control language="C#" Inherits="Engage.Dnn.Employment.Admin.ApplicationListing" Codebehind="ApplicationListing.ascx.cs" AutoEventWireup="false" %>

<asp:Repeater ID="JobsRepeater" runat="server">
    <HeaderTemplate>
    </HeaderTemplate>                
    <ItemTemplate>
        <table class="applicationsTable Normal">
        <colgroup>
            <col class="applicantColumn" />
            <col class="dateColumn" />
            <col class="salaryColumn" />
            <col class="statusColumn" />
            <col class="leadColumn" />
            <col class="documentsColumn" />
        </colgroup>
        <caption>
            <a rel="Bookmark" href='<%#GetJobDetailUrl(Eval("JobId"))%>' id='<%#((int)Eval("JobId")).ToString(CultureInfo.InvariantCulture)%>' name='<%#((int)Eval("JobId")).ToString(CultureInfo.InvariantCulture)%>'>
                <span class="SubHead">
                    <%# string.Format(CultureInfo.CurrentCulture, Localization.GetString("JobInLocation", LocalResourceFile), Eval("Title"), Eval("LocationName"), Eval("StateName")) %>
                </span>
            </a>
        </caption>
        <asp:Repeater ID="ApplicationsRepeater" runat="server">
            <HeaderTemplate>
                <tr>
                    <th class="nowrap" scope="col"><asp:Label runat="server" resourcekey="ApplicantHeaderLabel" /></th>
                    <th class="nowrap" scope="col"><asp:Label runat="server" resourcekey="DateAppliedHeaderLabel" /></th>
                    <th class="nowrap" scope="col"><asp:Label runat="server" resourcekey="SalaryHeaderLabel" /></th>
                    <th class="nowrap" scope="col"><asp:Label runat="server" resourcekey="StatusHeaderLabel" /></th>
                    <th class="nowrap" scope="col"><asp:Label runat="server" resourcekey="LeadHeaderLabel" /></th>
                    <th class="nowrap" scope="col"><asp:Label runat="server" resourcekey="ViewHeaderLabel" /></th>
               </tr>
            </HeaderTemplate>
            <ItemTemplate>
                <tr class='<%# (int)DataBinder.Eval(Container, "ItemIndex") % 2 == 0 ? "applicationRow" : "alternatingApplicationRow" %>'>
                    <td>
                        <asp:Label ID="UserNameLabel" runat="server" CssClass="Subhead">
                            <%# GetUserName(Eval("UserId") as int?)%>
                        </asp:Label><br />
                        <asp:DropDownList ID="UserStatusDropDownList" runat="server" CssClass="NormalTextBox" AutoPostBack="true" />
                        <asp:HiddenField ID="UserIdHiddenField" runat="server" Value='<%#GetUserId(Eval("UserId")) %>' />
                    </td>
                    <td class="nowrap"><%# Eval("AppliedForDate", "{0:d}")%></td>
                    <td><%# Eval("SalaryRequirement") %></td>
                    <td>
                        <asp:DropDownList ID="ApplicationStatusDropDownList" runat="server" CssClass="NormalTextBox" AutoPostBack="true" />
                        <asp:HiddenField ID="ApplicationIdHiddenField" runat="server" Value='<%#((int)Eval("ApplicationId")).ToString(CultureInfo.InvariantCulture) %>' />
                    </td>
                    <td class="nowrap" rowspan="2">
                        <asp:Repeater ID="PropertiesRepeater" runat="server">
                            <ItemTemplate>
                                <asp:Label runat="server" ID="ApplicationPropertyLabel" /><br />
                            </ItemTemplate>
                        </asp:Repeater>
                    </td>
                    <td class="nowrap" rowspan="2">
                        <asp:Repeater ID="DocumentsRepeater" runat="server">
                            <ItemTemplate>
                                <asp:HyperLink runat="server" ID="DocumentLink" Target="_blank" /><br />
                            </ItemTemplate>
                        </asp:Repeater>
                    </td>
                </tr>
                <tr class='<%# (int)DataBinder.Eval(Container, "ItemIndex") % 2 == 0 ? "applicationRow" : "alternatingApplicationRow" %> alignLeft'>
                    <td colspan="4" class="messageCell">
                        <asp:Label runat="server" ID="MessageHeaderLabel" CssClass="NormalBold" resourcekey="MessageHeaderLabel" />
                        <%# Eval("Message") %>
                    </td>
                </tr>
            </ItemTemplate>
        </asp:Repeater>
        </table>
    </ItemTemplate>
    <FooterTemplate>
    </FooterTemplate>
</asp:Repeater>
<asp:LinkButton ID="BackButton" runat="server" CssClass="Normal" resourcekey="btnBack" />