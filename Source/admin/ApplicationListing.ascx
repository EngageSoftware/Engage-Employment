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
        ExportSettings-IgnorePaging="true" ExportSettings-ExportOnlyData="true" ExportSettings-HideStructureColumns="true"
        ClientSettings-ClientEvents-OnGridCreated="ApplicationListing_GridCreated">
        <MasterTableView DataKeyNames="JobId" CommandItemDisplay="TopAndBottom">
            <CommandItemSettings ShowExportToExcelButton="true" ShowExportToCsvButton="true" />
            <Columns>
                <telerik:GridBoundColumn Display="false" DataField="JobId" UniqueName="JobId" ItemStyle-CssClass="jobIdColumn" />
                <telerik:GridTemplateColumn SortExpression="Title" HeaderText="JobTitleHeaderLabel" UniqueName="Title" ItemStyle-CssClass="jobTitleColumn">
                    <ItemTemplate>
                        <a href='<%#GetJobDetailUrl(Eval("JobId"))%>' id='job-<%#((int)Eval("JobId")).ToString(CultureInfo.InvariantCulture)%>'>
                            <%# HttpUtility.HtmlEncode((string)Eval("Title")) %>
                        </a>
                    </ItemTemplate>
                </telerik:GridTemplateColumn>
                <telerik:GridTemplateColumn SortExpression="LocationName" UniqueName="Location" HeaderText="LocationHeaderLabel" ItemStyle-CssClass="jobLocationColumn">
                    <ItemTemplate>
                        <%# HttpUtility.HtmlEncode(string.Format(CultureInfo.CurrentCulture, this.Localize("Location", this.LocalResourceFile), Eval("LocationName"), Eval("StateName"), Eval("StateAbbreviation"))) %>
                    </ItemTemplate>
                </telerik:GridTemplateColumn>
                <telerik:GridBoundColumn DataField="PostedDate" UniqueName="PostedDate" HeaderText="PostedDateHeaderLabel" DataFormatString="{0:d}" ItemStyle-CssClass="postedDateColumn" />
            </Columns>
            <DetailTables>
                <telerik:GridTableView runat="server" 
                    DataKeyNames="UserId,ApplicationId"
                    HierarchyDefaultExpanded="true">
                    <Columns>
                        <telerik:GridBoundColumn Display="false" DataField="UserId" UniqueName="UserId" ItemStyle-CssClass="userIdColumn" />
                        <telerik:GridTemplateColumn SortExpression="DisplayName" HeaderText="ApplicantHeaderLabel" ItemStyle-CssClass="applicantColumn">
                            <ItemTemplate>
                                <%# HttpUtility.HtmlEncode(GetUserName(Eval("UserId") as int?)) %>
                                <asp:DropDownList runat="server" 
                                    CssClass="NormalTextBox" 
                                    Visible='<%# this.ShowUserStatuses(Eval("UserId") as int?) %>'
                                    AutoPostBack="true" OnSelectedIndexChanged="UserStatusDropDownList_SelectedIndexChanged" 
                                    DataSource="<%# this.UserStatuses %>"
                                    DataTextField="Text"
                                    DataValueField="Value"
                                    SelectedValue='<%# this.GetUserStatus(Eval("UserId") as int?) %>' />
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridBoundColumn DataField="AppliedDate" DataFormatString="{0:d}" HeaderText="DateAppliedHeaderLabel" ItemStyle-CssClass="dateAppliedColumn" />
                        <telerik:GridBoundColumn DataField="SalaryRequirement" HeaderText="SalaryHeaderLabel" ItemStyle-CssClass="salaryColumn" />
                        <telerik:GridTemplateColumn HeaderText="LeadHeaderLabel" ItemStyle-CssClass="leadColumn">
                            <ItemTemplate>
                                <asp:Repeater runat="server" DataSource='<%# GetApplicationProperties((int)Eval("ApplicationId")) %>'>
                                    <HeaderTemplate><ul></HeaderTemplate>
                                    <ItemTemplate>
                                        <li><%# HttpUtility.HtmlEncode(GetLeadText((string)Eval("PropertyValue"))) %></li>
                                    </ItemTemplate>
                                    <FooterTemplate></ul></FooterTemplate>
                                </asp:Repeater>
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridTemplateColumn HeaderText="ViewHeaderLabel" ItemStyle-CssClass="documentsColumn">
                            <ItemTemplate>
                                <asp:Repeater runat="server" DataSource='<%# GetApplicationDocuments((int)Eval("ApplicationId")) %>'>
                                    <HeaderTemplate><ul></HeaderTemplate>
                                    <ItemTemplate>
                                        <li>
                                            <asp:HyperLink runat="server" Target="_blank" NavigateUrl='<%#GetDocumentUrl((int)this.Eval("DocumentId")) %>'>
                                                <%# HttpUtility.HtmlEncode(GetDocumentTypeText((int)this.Eval("DocumentTypeId"))) %>
                                            </asp:HyperLink>
                                        </li>
                                    </ItemTemplate>
                                    <FooterTemplate></ul></FooterTemplate>
                                </asp:Repeater>
                            </ItemTemplate>
                        </telerik:GridTemplateColumn>
                        <telerik:GridTemplateColumn HeaderText="StatusHeaderLabel" ItemStyle-CssClass="statusColumn">
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
                    </Columns>
                    <NestedViewTemplate>
                        <asp:Label runat="server" ID="MessageHeaderLabel" CssClass="NormalBold" resourcekey="MessageHeaderLabel" />
                        <%# HttpUtility.HtmlEncode((string)Eval("Message")) %>
                    </NestedViewTemplate>
                    <NoRecordsTemplate>
                        <%= Localize("No Applications.Text")%>
                    </NoRecordsTemplate>
                </telerik:GridTableView>
            </DetailTables>
        </MasterTableView>
    </telerik:RadGrid>

    <ul class="eng-action-btns">
        <li id="AllLinkWrapper" runat="server" Visible="false"><asp:HyperLink ID="AllLink" runat="server" CssClass="CommandButton" resourcekey="btnAll" /></li>
        <li><asp:HyperLink ID="BackLink" runat="server" CssClass="CommandButton" resourcekey="btnBack" /></li>
    </ul>
</div>

<%--script type="text/javascript">
    (function ($) {
        window.ApplicationListing_GridCreated = function (grid) {
            $.gridSelect(grid);
        };
    }(jQuery));
</script--%>