<%@ Import Namespace="DotNetNuke.Services.Localization"%>
<%@ Import Namespace="System.Globalization"%>
<%@ Control language="C#" Inherits="Engage.Dnn.Employment.JobListing" AutoEventWireup="true" Codebehind="JobListing.ascx.cs" %>

<table border="0" cellspacing="0" cellpadding="10">
  <tr>
    <td><%--<asp:HyperLink ID="hlAllHotJobs" runat="server" resourcekey="hlAllHotJobs">View All Hot Jobs</asp:HyperLink>--%></td>
    <td><asp:HyperLink ID="hlSearchJobs" runat="server" resourcekey="hlSearchJobs">Search Jobs</asp:HyperLink></td>
  </tr>
</table>
<br />
<asp:Repeater ID="rpJobListing" runat="server">
	<HeaderTemplate>
	    <div class="jobs_listing_container">
	        <div class="jobs_listing">
	            <asp:Label runat="server" ID="lblJobListingHeader" CssClass="SubHead"><%= GetJobListingHeader() %></asp:Label>
    	    </div>		
	        <table id="employment" class="employmentTable">
                <tr>
                    <th class="nowrap"><asp:Label id="lblCategoryHeader" runat="server" resourcekey="lblCategoryHeader" /></th>
                    <th class="nowrap"><asp:Label id="lblPositionHeader" runat="server" resourcekey="lblPositionHeader" /></th>
                    <th class="nowrap"><asp:Label id="lblLocationHeader" runat="server" resourcekey="lblLocationHeader" /></th>
                    <th class="nowrap"><asp:Label id="lblDateHeader" runat="server" resourcekey="lblDateHeader" /></th>
                </tr>
	</HeaderTemplate>
    <ItemTemplate>
                <tr>
                    <td><%# Eval("CategoryName") %></td>
                    <td><a href="<%# GetJobDetailUrl(Eval("JobId")) %>"><%# Eval("Title") %></a></td>
                    <td><%#string.Format(CultureInfo.CurrentCulture, Localization.GetString("Location.Text", LocalResourceFile), Eval("LocationName"), Eval("StateName"), Eval("StateAbbreviation")) %></td>
                    <td><%# String.Format("{0:d MMM yyyy}", Convert.ToDateTime(Eval("PostedDate"))) %></td>
                </tr>
    </ItemTemplate>
	<FooterTemplate>
	        </table>
	    </div>
	</FooterTemplate>
</asp:Repeater>
<%-- Replaced UpdatePanel with server-side code to dynamically inject one --%>
<%--<asp:UpdatePanel ID="upnlSavedSearches" runat="server" UpdateMode="Conditional"><ContentTemplate>--%>
        <asp:Repeater ID="rpSavedSearches" runat="server">
            <HeaderTemplate>
                <div id="saved_searches_container">
                    <br />
                    <div class="save_searches">
                        <asp:Label CssClass="SubHead" runat="server" ID="lblSavedSearchesHeader" resourcekey="lblSavedSearchesHeader" />
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
                            <td><asp:Button ID="btnDelete" runat="server" resourcekey="btnDelete" CommandName="Delete" OnCommand="btnDelete_Command"/></td>
                            <td><asp:HyperLink ID="lnkSearch" runat="server"><%#Eval("Description") %></asp:HyperLink></td>
                            <td><%#Eval("Category") %></td>
                            <td><%#Eval("JobPosition") %></td>
                            <td><%#Eval("LocationName") %></td>
                            <td><%#Eval("StateName")%></td>
                            <td><%#Eval("CreationDate", "{0:d}")%></td>
                        </tr>
            </ItemTemplate>
            <FooterTemplate>
                    </table>
                </div>
            </FooterTemplate>
        </asp:Repeater>
<%--</ContentTemplate></asp:UpdatePanel>--%>
<asp:Repeater ID="rpAppliedJobs" runat="server">
    <HeaderTemplate>
        <br />
        <div class="job_applied_for">
            <div class="job_applied_for_header">
                <asp:Label runat="server" CssClass="SubHead" ID="lblAppliedJobsHeader" resourcekey="lblAppliedJobsHeader" />
            </div>
            <table class="employmentTable job_listing_table">
                <tr>
                    <th class="nowrap"><asp:Label id="lblCategoryHeader" runat="server" resourcekey="lblCategoryHeader" /></th>
                    <th class="nowrap"><asp:Label id="lblPositionHeader" runat="server" resourcekey="lblPositionHeader" /></th>
                    <th class="nowrap"><asp:Label id="lblLocationHeader" runat="server" resourcekey="lblLocationHeader" /></th>
                    <th class="nowrap"><asp:Label id="lblAppliedDateHeader" runat="server" resourcekey="lblAppliedDateHeader" /></th>
                    <th class="nowrap"><asp:Label id="lblUpdateHeader" runat="server" resourcekey="lblUpdateHeader" /></th>
                </tr>
    </HeaderTemplate>
    <ItemTemplate>
                <tr>
                    <td><%# Eval("Job.CategoryName") %></td>
                    <td><a href="<%# GetJobDetailUrl(Eval("JobId")) %>"><%# Eval("Job.Title") %></a></td>
                    <td><%#string.Format(CultureInfo.CurrentCulture, Localization.GetString("Location.Text", LocalResourceFile), Eval("Job.LocationName"), Eval("Job.StateName"), Eval("Job.StateAbbreviation")) %></td>
                    <td><%# String.Format("{0:d MMM yyyy}", Convert.ToDateTime(Eval("AppliedForDate"))) %></td>
                    <td><asp:Button ID="btnEditApplication" runat="server" resourcekey="btnEditApplication"/></td>
                </tr>
    </ItemTemplate>
    <FooterTemplate>
            </table>
        </div>
    </FooterTemplate>
</asp:Repeater>