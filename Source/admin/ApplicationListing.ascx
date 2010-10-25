<%@ Import Namespace="Engage.Dnn.Employment"%>
<%@ Import namespace="System.Globalization"%>
<%@ Import namespace="DotNetNuke.Common.Utilities"%>
<%@ Import namespace="DotNetNuke.Services.Localization"%>
<%@ Control language="C#" Inherits="Engage.Dnn.Employment.Admin.ApplicationListing" Codebehind="ApplicationListing.ascx.cs" AutoEventWireup="false" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>

<div class="manageApplications">
    <telerik:RadGrid ID="JobsGrid" runat="server" 
        AllowPaging="true" AllowCustomPaging="true" PageSize="10" 
        AutoGenerateColumns="false" 
        CssClass="Normal Engage_RadGrid" Skin="Simple"
        ExportSettings-IgnorePaging="true" ExportSettings-HideStructureColumns="true" ExportSettings-OpenInNewWindow="true">
        <MasterTableView DataKeyNames="JobId" CommandItemDisplay="TopAndBottom">
            <CommandItemSettings ShowExportToExcelButton="true" ShowExportToCsvButton="true" />
            <Columns>
                <telerik:GridBoundColumn Display="false" DataField="JobId" UniqueName="JobId" ItemStyle-CssClass="jobIdColumn" />
                <telerik:GridTemplateColumn DataField="Title" HeaderText="JobTitleHeaderLabel" UniqueName="Title" ItemStyle-CssClass="jobTitleColumn">
                    <ItemTemplate>
                        <a href='<%#GetJobDetailUrl(Eval("JobId"))%>' id='job-<%#((int)Eval("JobId")).ToString(CultureInfo.InvariantCulture)%>'>
                            <%# HttpUtility.HtmlEncode((string)Eval("Title")) %>
                        </a>
                    </ItemTemplate>
                </telerik:GridTemplateColumn>
                <telerik:GridBoundColumn HeaderText="JobTitleHeaderLabel" UniqueName="Export-Title" DataField="Title" Visible="false" />
                <telerik:GridTemplateColumn DataField="LocationName" UniqueName="Location" HeaderText="LocationHeaderLabel" ItemStyle-CssClass="jobLocationColumn">
                    <ItemTemplate>
                        <%# HttpUtility.HtmlEncode(string.Format(CultureInfo.CurrentCulture, this.Localize("Location", this.LocalResourceFile), Eval("LocationName"), Eval("StateName"), Eval("StateAbbreviation"))) %>
                    </ItemTemplate>
                </telerik:GridTemplateColumn>
                <telerik:GridBoundColumn DataField="PostedDate" UniqueName="PostedDate" HeaderText="PostedDateHeaderLabel" DataFormatString="{0:d}" ItemStyle-CssClass="postedDateColumn" />
            </Columns>
            <DetailTables>
                <telerik:GridTableView runat="server" 
                    DataKeyNames="UserId,ApplicationId"
                    HierarchyDefaultExpanded="true"
                    CommandItemDisplay="TopAndBottom" CommandItemSettings-ShowExportToCsvButton="true" CommandItemSettings-ShowExportToExcelButton="true">
                    <Columns>
                        <telerik:GridBoundColumn Display="false" DataField="UserId" UniqueName="UserId" ItemStyle-CssClass="userIdColumn" />
                        <telerik:GridTemplateColumn SortExpression="DisplayName" HeaderText="ApplicantHeaderLabel" ItemStyle-CssClass="applicantColumn">
                            <ItemTemplate>
                                <%# HttpUtility.HtmlEncode(GetUserName(Eval("UserId") as int?)) %>
                                <asp:DropDownList runat="server" 
                                    CssClass="NormalTextBox" 
                                    Visible='<%# !this.IsExport && this.ShowUserStatuses(this.Eval("UserId") as int?) %>'
                                    AutoPostBack="true" OnSelectedIndexChanged="UserStatusDropDownList_SelectedIndexChanged" 
                                    DataSource="<%# this.UserStatuses %>"
                                    DataTextField="Text"
                                    DataValueField="Value"
                                    SelectedValue='<%# this.GetUserStatus(Eval("UserId") as int?) %>' />
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridTemplateColumn HeaderText="ApplicantStatusLabel-Export" UniqueName="Export-UserStatus" DataField="Status" Visible="false">
                            <ItemTemplate>
                                <%# HttpUtility.HtmlEncode(GetUserStatusName(Eval("UserId") as int?)) %>
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridBoundColumn DataField="AppliedForDate" DataFormatString="{0:d}" HeaderText="DateAppliedHeaderLabel" ItemStyle-CssClass="dateAppliedColumn" />
                        <telerik:GridBoundColumn DataField="SalaryRequirement" HeaderText="SalaryHeaderLabel" ItemStyle-CssClass="salaryColumn" />
                        <telerik:GridTemplateColumn UniqueName="Properties" HeaderText="LeadHeaderLabel" ItemStyle-CssClass="leadColumn" />
                        <telerik:GridTemplateColumn UniqueName="Documents" HeaderText="ViewHeaderLabel" ItemStyle-CssClass="documentsColumn" />
                        <telerik:GridTemplateColumn UniqueName="ApplicationStatus" HeaderText="StatusHeaderLabel" ItemStyle-CssClass="statusColumn">
                            <ItemTemplate>
                                <asp:DropDownList runat="server" 
                                    CssClass="NormalTextBox" 
                                    Visible='<%# this.ShowApplicationStatuses() %>'
                                    AutoPostBack="true" OnSelectedIndexChanged="ApplicationStatusDropDownList_SelectedIndexChanged" 
                                    DataSource="<%# this.ApplicationStatuses %>"
                                    DataTextField="Text"
                                    DataValueField="Value"
                                    SelectedValue='<%# Eval("StatusId") %>' />
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridTemplateColumn HeaderText="StatusHeaderLabel" UniqueName="Export-ApplicationStatus" Visible="false">
                            <ItemTemplate>
                                <%# HttpUtility.HtmlEncode(GetApplicationStatusName(Eval("StatusId") as int?)) %>
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridBoundColumn HeaderText="MessageHeaderLabel" UniqueName="Export-Message" DataField="Message" Visible="false" />
                    </Columns>
                    <NestedViewTemplate>
                        <asp:PlaceHolder runat="server" Visible="<%# !IsExport %>">
                            <asp:Label runat="server" ID="MessageHeaderLabel" CssClass="NormalBold" resourcekey="MessageHeaderLabel" />
                            <%# HttpUtility.HtmlEncode((string)Eval("Message")) %>
                        </asp:PlaceHolder>
                    </NestedViewTemplate>
                    <NoRecordsTemplate>
                            <%= AreApplicationsFiltered ? Localize("No Applications After Filter.Text") : Localize("No Applications.Text") %>
                    </NoRecordsTemplate>
                </telerik:GridTableView>
            </DetailTables>
            <NoRecordsTemplate>
                <%= AreJobsFiltered ? Localize("No Jobs After Filter.Text") : Localize("No Jobs.Text") %>
            </NoRecordsTemplate>
        </MasterTableView>
    </telerik:RadGrid>

    <ul class="eng-action-btns">
        <li id="AllLinkWrapper" runat="server" Visible="false"><asp:HyperLink ID="AllLink" runat="server" CssClass="CommandButton" resourcekey="btnAll" /></li>
        <li><asp:HyperLink ID="BackLink" runat="server" CssClass="CommandButton" resourcekey="btnBack" /></li>
    </ul>
</div>