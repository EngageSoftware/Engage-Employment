<%@ Import Namespace="System.Globalization"%>
<%@ Control language="C#" Inherits="Engage.Dnn.Employment.JobListing" AutoEventWireup="false" Codebehind="JobListing.ascx.cs" %>

<table border="0" cellspacing="0" cellpadding="10">
  <tr>
    <td><%--<asp:HyperLink ID="hlAllHotJobs" runat="server" resourcekey="hlAllHotJobs">View All Hot Jobs</asp:HyperLink>--%></td>
    <td><asp:HyperLink ID="SearchJobsLink" runat="server" resourcekey="hlSearchJobs"/></td>
  </tr>
</table>
<br />
<asp:Repeater ID="JobListingRepeater" runat="server">
	<HeaderTemplate>
	    <div class="jobs_listing_container">
	        <div class="jobs_listing">
	            <asp:Label runat="server" ID="JobListingHeaderLabel" CssClass="SubHead"><%= GetJobListingHeader() %></asp:Label>
    	    </div>		
	        <table id="employment" class="employmentTable">
                <tr>
                    <th class="nowrap"><asp:Label id="CategoryHeaderLabel" runat="server" resourcekey="lblCategoryHeader" /></th>
                    <th class="nowrap"><asp:Label id="PositionHeaderLabel" runat="server" resourcekey="lblPositionHeader" /></th>
                    <th class="nowrap"><asp:Label id="LocationHeaderLabel" runat="server" resourcekey="lblLocationHeader" /></th>
                    <th class="nowrap"><asp:Label id="DateHeaderLabel" runat="server" resourcekey="lblDateHeader" /></th>
                    <% if (ShowCloseDate) { %><th class="nowrap"><asp:Label id="CloseDateHeaderLabel" runat="server" resourcekey="lblCloseDateHeader" /></th> <% } %>
                </tr>
	</HeaderTemplate>
    <ItemTemplate>
                <tr>
                    <td><%# HttpUtility.HtmlEncode((string)Eval("CategoryName")) %></td>
                    <td><a href="<%# GetJobDetailUrl(Eval("JobId")) %>"><%# HttpUtility.HtmlEncode((string)Eval("Title")) %></a></td>
                    <td><%# HttpUtility.HtmlEncode(string.Format(CultureInfo.CurrentCulture, Localize("Location.Text"), this.Eval("LocationName"), this.Eval("StateName"), this.Eval("StateAbbreviation"))) %></td>
                    <td><%# HttpUtility.HtmlEncode(Eval("StartDate", Localize("StartDate.Format")))%></td>
                    <% if (ShowCloseDate) { %><td><%# HttpUtility.HtmlEncode(Eval("ExpireDate") == null ? Localize("No Expire Date") : Eval("ExpireDate", Localize("ExpireDate.Format"))) %></td> <% } %>
                </tr>
    </ItemTemplate>
	<FooterTemplate>
	        </table>
	    </div>
	</FooterTemplate>
</asp:Repeater>
<%-- Replaced UpdatePanel with server-side code to dynamically inject one --%>
<%--<asp:UpdatePanel ID="upnlSavedSearches" runat="server" UpdateMode="Conditional"><ContentTemplate>--%>
        <asp:Repeater ID="SavedSearchesRepeater" runat="server">
            <HeaderTemplate>
                <div id="saved_searches_container">
                    <br />
                    <div class="save_searches">
                        <asp:Label CssClass="SubHead" runat="server" ID="SavedSearchesHeaderLabel" resourcekey="lblSavedSearchesHeader" />
                    </div>
                    <table class="job_listing_table employmentTable">
                        <tr>
                            <th class="nowrap"><asp:Label runat="server" resourcekey="lblSavedSearchDeleteHeader" /></th>
                            <th class="nowrap"><asp:Label runat="server" resourcekey="lblSavedSearchNameHeader" /></th>
                            <th class="nowrap"><asp:Label runat="server" resourcekey="lblSavedSearchCategoryHeader" /></th>
                            <th class="nowrap"><asp:Label runat="server" resourcekey="lblSavedSearchJobTitleHeader" /></th>
                            <th class="nowrap"><asp:Label runat="server" resourcekey="lblSavedSearchLocationHeader" /></th>
                            <th class="nowrap"><asp:Label runat="server" resourcekey="lblSavedSearchStateHeader" /></th>
                            <th class="nowrap"><asp:Label runat="server" resourcekey="lblSavedSearchDateHeader" /></th>
                        </tr>
            </HeaderTemplate>
            <ItemTemplate>
                        <tr>
                            <td><asp:Button ID="DeleteButton" runat="server" resourcekey="btnDelete" CommandName="Delete" OnCommand="DeleteButton_Command"/></td>
                            <td><asp:HyperLink ID="SearchLink" runat="server"><%#HttpUtility.HtmlEncode((string)Eval("Description")) %></asp:HyperLink></td>
                            <td><%#HttpUtility.HtmlEncode((string)Eval("Category")) %></td>
                            <td><%#HttpUtility.HtmlEncode((string)Eval("JobPosition")) %></td>
                            <td><%#HttpUtility.HtmlEncode((string)Eval("LocationName")) %></td>
                            <td><%#HttpUtility.HtmlEncode((string)Eval("StateName"))%></td>
                            <td><%#HttpUtility.HtmlEncode(Eval("CreationDate", "{0:d}"))%></td>
                        </tr>
            </ItemTemplate>
            <FooterTemplate>
                    </table>
                </div>
            </FooterTemplate>
        </asp:Repeater>
<%--</ContentTemplate></asp:UpdatePanel>--%>
<asp:Repeater ID="AppliedJobsRepeater" runat="server">
    <HeaderTemplate>
        <br />
        <div class="job_applied_for">
            <div class="job_applied_for_header">
                <asp:Label runat="server" CssClass="SubHead" ID="AppliedJobsHeaderLabel" resourcekey="lblAppliedJobsHeader" />
            </div>
            <table class="employmentTable job_listing_table">
                <tr>
                    <th class="nowrap"><asp:Label id="CategoryHeaderLabel" runat="server" resourcekey="lblCategoryHeader" /></th>
                    <th class="nowrap"><asp:Label id="PositionHeaderLabel" runat="server" resourcekey="lblPositionHeader" /></th>
                    <th class="nowrap"><asp:Label id="LocationHeaderLabel" runat="server" resourcekey="lblLocationHeader" /></th>
                    <th class="nowrap"><asp:Label id="AppliedDateHeaderLabel" runat="server" resourcekey="lblAppliedDateHeader" /></th>
                    <th class="nowrap"><asp:Label id="UpdateHeaderLabel" runat="server" resourcekey="lblUpdateHeader" /></th>
                </tr>
    </HeaderTemplate>
    <ItemTemplate>
                <tr>
                    <td><%# HttpUtility.HtmlEncode((string)Eval("Job.CategoryName")) %></td>
                    <td><a href="<%# GetJobDetailUrl(Eval("JobId")) %>"><%# HttpUtility.HtmlEncode((string)Eval("Job.Title")) %></a></td>
                    <td><%# HttpUtility.HtmlEncode(string.Format(CultureInfo.CurrentCulture, Localize("Location.Text", this.LocalResourceFile), this.Eval("Job.LocationName"), this.Eval("Job.StateName"), this.Eval("Job.StateAbbreviation"))) %></td>
                    <td><%# HttpUtility.HtmlEncode(string.Format("{0:d MMM yyyy}", Convert.ToDateTime(Eval("AppliedForDate")))) %></td>
                    <td><asp:Button ID="EditApplicationButton" runat="server" resourcekey="btnEditApplication"/></td>
                </tr>
    </ItemTemplate>
    <FooterTemplate>
            </table>
        </div>
    </FooterTemplate>
</asp:Repeater>