<%@ Import namespace="System.Globalization"%>
<%@ Control language="C#" Inherits="Engage.Dnn.Employment.Admin.ApplicationListing" Codebehind="ApplicationListing.ascx.cs" AutoEventWireup="false" %>
<%@ Register TagPrefix="telerik" Namespace="Telerik.Web.UI" Assembly="Telerik.Web.UI" %>

<fieldset class="applicationFilter">
    <fieldset ID="FilterByJobsGroup" runat="server">
        <legend>
            <asp:Label ID="FilterByJobTitle" runat="server" resourceKey="FilterByJobsLabel" CssClass="SubHead"/>
        </legend>

        <div class="search_field">
            <asp:Label ID="FilterByJobTitleLabel" runat="server" resourcekey="FilterByJobTitleLabel" AssociatedControlID="FilterByJobTitleTextBox" />
            <asp:TextBox ID="FilterByJobTitleTextBox" runat="server" CssClass="NormalTextBox" />
        </div>
        <div class="search_field">
            <asp:Label ID="FilterByLocationLabel" runat="server" resourcekey="FilterByLocationLabel" AssociatedControlID="FilterByLocationDropDown"/>
            <asp:DropDownList ID="FilterByLocationDropDown" runat="server" CssClass="NormalTextBox" />
        </div>
    </fieldset>

    <fieldset>
        <legend>
            <asp:Label ID="FilterByApplicationsTitle" runat="server" resourceKey="FilterByApplicationsLabel" CssClass="SubHead"/>
        </legend>

        <div class="search_field">
            <asp:Label ID="FilterByUserStatusLabel" runat="server" resourcekey="FilterByUserStatusLabel" AssociatedControlID="FilterByApplicantStatusDropDown"/>
            <asp:DropDownList ID="FilterByApplicantStatusDropDown" runat="server" CssClass="NormalTextBox" />
        </div>
        <div class="search_field">
            <asp:Label ID="FilterByDateFromLabel" runat="server" resourcekey="FilterByDateFromLabel" AssociatedControlID="FilterByDateFromTextBox"/>
            <asp:TextBox ID="FilterByDateFromTextBox" runat="server" CssClass="DateTextBox DatePicker NormalTextBox" />
            <asp:CompareValidator runat="server" Type="Date" Operator="DataTypeCheck" ControlToValidate="FilterByDateFromTextBox" Display="None" resourcekey="FilterByDateFromTypeValidator" ValidationGroup="SearchDate" />
            <asp:CompareValidator runat="server" Type="Date" Operator="LessThanEqual" ControlToValidate="FilterByDateFromTextBox" ControlToCompare="FilterByDateToTextBox" Display="None" resourcekey="FilterByDateFromCompareValidator" ValidationGroup="SearchDate" />
        </div>
        <div class="search_field">
            <asp:Label ID="FilterByDateToLabel" runat="server" resourcekey="FilterByDateToLabel" AssociatedControlID="FilterByDateToTextBox"/>
            <asp:TextBox ID="FilterByDateToTextBox" runat="server" CssClass="DateTextBox DatePicker NormalTextBox" />
            <asp:CompareValidator runat="server" Type="Date" Operator="DataTypeCheck" ControlToValidate="FilterByDateToTextBox" Display="None" resourcekey="FilterByDateToTypeValidator" ValidationGroup="SearchDate" />
        </div>
        <div class="search_field">
            <asp:Label runat="server" resourcekey="FilterByLeadLabel" AssociatedControlID="FilterByLeadDropDown"/>
            <asp:DropDownList ID="FilterByLeadDropDown" runat="server" CssClass="NormalTextBox" />
        </div>
        <div class="search_field">
            <asp:Label ID="FilterByApplicationStatusLabel" runat="server" resourcekey="FilterByApplicationStatusLabel" AssociatedControlID="FilterByApplicationStatusDropDown"/>
            <asp:DropDownList ID="FilterByApplicationStatusDropDown" runat="server" CssClass="NormalTextBox" />
        </div>
    </fieldset>
    <ul class="search_button eng-action-btns">
        <li><asp:LinkButton ID="SearchButton" runat="server" CssClass="CommandButton" resourcekey="btnApplyFilter.Text" OnClick="SearchButton_Click" ValidationGroup="SearchDate"/></li>
        <li id="AllLinkWrapperTop" runat="server" Visible="false"><asp:HyperLink ID="AllLinkTop" runat="server" CssClass="CommandButton" resourcekey="btnAll" /></li>
    </ul>
    <asp:ValidationSummary runat="server" DisplayMode="BulletList" ShowMessageBox="true" ShowSummary="false" ValidationGroup="SearchDate" />
</fieldset>

<div class="manageApplications">

    <telerik:RadGrid ID="JobsGrid" runat="server" 
        AllowPaging="true" AllowCustomPaging="true" PageSize="10" 
        AutoGenerateColumns="false" 
        CssClass="Normal Engage_RadGrid" Skin="Simple"
        ExportSettings-IgnorePaging="true" ExportSettings-HideStructureColumns="true" ExportSettings-OpenInNewWindow="true">
        <MasterTableView DataKeyNames="JobId" CommandItemDisplay="Top">
            <CommandItemSettings ShowExportToExcelButton="true" ShowExportToCsvButton="true" />
            <Columns>
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
                <telerik:GridBoundColumn DataField="StartDate" UniqueName="StartDate" HeaderText="DateStartHeaderLabel" DataFormatString="{0:d}" ItemStyle-CssClass="startDateColumn" />
            </Columns>
            <DetailTables>
                <telerik:GridTableView runat="server" 
                    DataKeyNames="UserId,ApplicationId"
                    HierarchyDefaultExpanded="true"
                    CommandItemDisplay="Top" CommandItemSettings-ShowExportToCsvButton="true" CommandItemSettings-ShowExportToExcelButton="true">
                    <Columns>
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
                                    SelectedValue='<%# Eval("StatusId") ?? string.Empty %>' />
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
                        <asp:Panel runat="server" CssClass="applicationMessage" Visible="<%# !IsExport %>">
                            <asp:Label runat="server" ID="MessageHeaderLabel" CssClass="NormalBold" resourcekey="MessageHeaderLabel" />
                            <%# HttpUtility.HtmlEncode((string)Eval("Message")) %>
                        </asp:Panel>
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
        <li id="AllLinkWrapperBottom" runat="server" Visible="false"><asp:HyperLink ID="AllLinkBottom" runat="server" CssClass="CommandButton" resourcekey="btnAll" /></li>
        <li><asp:HyperLink ID="BackLink" runat="server" CssClass="CommandButton" resourcekey="btnBack" /></li>
    </ul>
</div>

<script type="text/javascript">
    jQuery(function () { jQuery('.DatePicker').datepicker(datePickerOpts); });

    $(document).ready(function () {
        Sys.WebForms.PageRequestManager.getInstance().add_endRequest(EndRequestHandler);

        function EndRequestHandler(sender, args) {
            jQuery('.DatePicker').datepicker(datePickerOpts);
        }

    });
</script>