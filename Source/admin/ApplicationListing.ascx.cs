// <copyright file="ApplicationListing.ascx.cs" company="Engage Software">
// Engage: Employment - http://www.engagesoftware.com
// Copyright (c) 2004-2011
// by Engage Software ( http://www.engagesoftware.com )
// </copyright>
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

namespace Engage.Dnn.Employment.Admin
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using Data;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Exceptions;

    using Telerik.Web.UI;

    using Utility = Engage.Utility;

    /// <summary>
    /// Lists applications for the available jobs in the portal.
    /// </summary>
    public partial class ApplicationListing : ModuleBase, IActionable
    {
        /// <summary>
        /// Backing field for <see cref="ApplicationStatuses"/>
        /// </summary>
        private readonly IEnumerable<ApplicationStatus> applicationStatuses;

        /// <summary>
        /// Backing field for <see cref="UserStatuses"/>
        /// </summary>
        private readonly IEnumerable<UserStatus> userStatuses;

        /// <summary>
        /// The list of leads
        /// </summary>
        private readonly Dictionary<int, ListEntryInfo> leadsList;

        /// <summary>
        /// Backing field for <see cref="LocationsForSearch"/>
        /// </summary>
        private readonly IEnumerable<Location> locations;

        /// <summary>
        /// Backing field for <see cref="JobId"/>
        /// </summary>
        private int? jobId;

        /// <summary>
        /// Backing field for <see cref="ApplicationStatusId"/>
        /// </summary>
        private int? applicationStatusId;

        /// <summary>
        /// Backing field for <see cref="UserStatusId"/>
        /// </summary>
        private int? userStatusId;

        /// <summary>
        /// The type of export (one of <see cref="RadGrid.ExportToCsvCommandName"/> or <see cref="RadGrid.ExportToExcelCommandName"/>), 
        /// or <c>null</c> if no export is occurring
        /// </summary>
        private string exportType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationListing"/> class.
        /// </summary>
        public ApplicationListing()
        {
            this.userStatuses = UserStatus.LoadStatuses(this.PortalId);
            this.applicationStatuses = ApplicationStatus.GetStatuses(this.PortalId);
            this.leadsList = (new ListController()).GetListEntryInfoCollection(Employment.Utility.LeadListName).Cast<ListEntryInfo>().ToDictionary(entry => entry.EntryID);
            this.locations = Location.LoadLocations(null, this.PortalId);
        }

        /// <summary>
        /// Gets the module actions.
        /// </summary>
        /// <value>The module actions.</value>
        public ModuleActionCollection ModuleActions
        {
            get
            {
                return new ModuleActionCollection(new[]
                    {
                        new ModuleAction(
                            this.GetNextActionID(),
                            this.Localize("ManageStatuses", this.LocalResourceFile),
                            ModuleActionType.AddContent,
                            string.Empty,
                            string.Empty,
                            this.EditUrl(ControlKey.ManageStatuses.ToString()),
                            string.Empty,
                            false,
                            SecurityAccessLevel.Edit,
                            true,
                            false),
                        new ModuleAction(
                            this.GetNextActionID(),
                            this.Localize("ManageApplicationStatuses"),
                            ModuleActionType.AddContent,
                            string.Empty,
                            string.Empty,
                            this.EditUrl(ControlKey.ManageApplicationStatuses.ToString()),
                            string.Empty,
                            false,
                            SecurityAccessLevel.Edit,
                            true,
                            false)
                    });
            }
        }

        /// <summary>
        /// Gets a value indicating whether the application listings are filtered.
        /// </summary>
        /// <value>
        /// <c>true</c> if the application lists are filtered; otherwise, <c>false</c>.
        /// </value>
        protected bool AreApplicationsFiltered
        {
            get { return this.ApplicationStatusId.HasValue; }
        }

        /// <summary>
        /// Gets a value indicating whether the jobs listing is filtered.
        /// </summary>
        /// <value>
        /// <c>true</c> if the jobs list is filtered; otherwise, <c>false</c>.
        /// </value>
        protected bool AreJobsFiltered
        {
            get { return this.JobId.HasValue; }
        }

        /// <summary>
        /// Gets a value indicating whether there's currently an export occurring.
        /// </summary>
        /// <value><c>true</c> if this instance is exporting; otherwise, <c>false</c>.</value>
        protected bool IsExport
        {
            get { return this.exportType != null; }
        }

        /// <summary>
        /// Gets the list of application statuses to display to the user.
        /// </summary>
        /// <value>This portal's application statuses.</value>
        protected IEnumerable<ListItem> ApplicationStatuses
        {
            get 
            {
                yield return new ListItem(this.Localize("NoApplicationStatus"), string.Empty);

                foreach (var applicationStatus in this.applicationStatuses)
                {
                    yield return new ListItem(applicationStatus.StatusName, applicationStatus.StatusId.ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        /// <summary>
        /// Gets the application statuses for search drop down.
        /// </summary>
        /// <value>The application statuses list.</value>
        protected IEnumerable<ListItem> ApplicationStatusesForSearch
        {
            get
            {
                yield return new ListItem(string.Empty, string.Empty);
                yield return new ListItem(this.Localize("FilterNoApplicationStatus"), "0");

                foreach (var applicationStatus in this.applicationStatuses)
                {
                    yield return new ListItem(applicationStatus.StatusName, applicationStatus.StatusId.ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        /// <summary>
        /// Gets the leads for search drop down.
        /// </summary>
        /// <value>The leads for search.</value>
        protected IEnumerable<ListItem> LeadsForSearch
        {
            get
            {
                yield return new ListItem(string.Empty, string.Empty);

                foreach (var leadInfo in this.leadsList)
                {
                    yield return new ListItem(leadInfo.Value.Text, leadInfo.Key.ToString(CultureInfo.InvariantCulture));
                }
            }
        }


        protected IEnumerable<ListItem> LocationsForSearch
        {
            get
            {
                yield return new ListItem(string.Empty, string.Empty);

                foreach (var location in this.locations)
                {
                    yield return new ListItem(location.LocationName, location.LocationId.ToString());
                }
            }
        }
        
        /// <summary>
        /// Gets the list of user statuses to display to the user.
        /// </summary>
        /// <value>The user statuses list.</value>
        protected IEnumerable<ListItem> UserStatuses
        {
            get
            {
                yield return new ListItem(this.Localize("NoStatus.Text"), string.Empty);

                foreach (var userStatus in this.userStatuses)
                {
                    yield return new ListItem(userStatus.Status, userStatus.StatusId.ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        /// <summary>
        /// Gets the user statuses for search drop down.
        /// </summary>
        /// <value>The user statuses list.</value>
        protected IEnumerable<ListItem> UserStatusesForSearch
        {
            get
            {
                yield return new ListItem(string.Empty, string.Empty);
                yield return new ListItem(this.Localize("FilterNoUserStatus.Text"), "0");

                foreach (var userStatus in this.userStatuses)
                {
                    yield return new ListItem(userStatus.Status, userStatus.StatusId.ToString(CultureInfo.InvariantCulture));
                }
            }
        }
        
        /// <summary>
        /// Gets the ID of the job for which to display applications (or <c>null</c> to display all jobs).
        /// </summary>
        /// <value>The Id of the job to display.</value>
        private int? JobId
        {
            get 
            {
                if (!this.jobId.HasValue)
                {
                    int jobIdResult;
                    if (!string.IsNullOrEmpty(this.Request.QueryString["jobId"]) &&
                        int.TryParse(this.Request.QueryString["jobId"], NumberStyles.Integer, CultureInfo.InvariantCulture, out jobIdResult))
                    {
                        this.jobId = jobIdResult;
                    }
                }

                return this.jobId;
            }
        }

        /// <summary>
        /// Gets the ID of the application status by which to filter applications (or <c>null</c> to display all jobs).
        /// </summary>
        /// <value>The ID of the status by which to filter applications.</value>
        private int? ApplicationStatusId
        {
            get 
            {
                if (ViewState["ApplicationStatusId"] != null)
                {
                    this.applicationStatusId = ((int?)ViewState["ApplicationStatusId"] >= 0)? (int?)ViewState["ApplicationStatusId"]: null;
                }
                else if (!this.applicationStatusId.HasValue)
                {
                    int statusId;
                    if (!string.IsNullOrEmpty(this.Request.QueryString["statusId"]) &&
                        int.TryParse(this.Request.QueryString["statusId"], NumberStyles.Integer, CultureInfo.InvariantCulture, out statusId))
                    {
                        this.applicationStatusId = statusId;
                    }
                }

                return this.applicationStatusId;
            }
        }

        /// <summary>
        /// Gets the ID of the user status by which to filter applications (or <c>null</c> to display all jobs).
        /// </summary>
        /// <value>The ID of the user status by which to filter applications.</value>
        private int? UserStatusId
        {
            get 
            {
                if (ViewState["UserStatusId"] != null)
                {
                    this.userStatusId = ((int?)ViewState["UserStatusId"] >= 0) ? ((int?)ViewState["UserStatusId"]) : null;
                }
                else if (!this.userStatusId.HasValue)
                {
                    int statusId;
                    if (!string.IsNullOrEmpty(this.Request.QueryString["userStatusId"]) &&
                        int.TryParse(this.Request.QueryString["userStatusId"], NumberStyles.Integer, CultureInfo.InvariantCulture, out statusId))
                    {
                        this.userStatusId = statusId;
                    }
                }

                return this.userStatusId;
            }
        }

        /// <summary>
        /// Gets the application documents (resumé and cover letter) for the given application.
        /// </summary>
        /// <param name="applicationId">The ID of the application for which to get the documents.</param>
        /// <returns>A <see cref="DataTable"/> with a row for each application property (columns  DocumentId, UserId, FileName, ContentType, ContentLength, ResumeData, RevisionDate, DocumentTypeId)</returns>
        protected static DataTable GetApplicationDocuments(int applicationId)
        {
            return DataProvider.Instance().GetApplicationDocuments(applicationId);
        }

        /// <summary>
        /// Gets the application properties (lead) for the given application.
        /// </summary>
        /// <param name="applicationId">The ID of the application for which to get the properties.</param>
        /// <returns>A <see cref="DataTable"/> with a row for each application property (columns ApplicationId, ApplicationPropertyId, Visibility, PropertyName, PropertyValue)</returns>
        protected static DataTable GetApplicationProperties(int applicationId)
        {
            return DataProvider.Instance().GetApplicationProperties(applicationId);
        }

        /// <summary>
        /// Gets the name of the status with the given ID.
        /// </summary>
        /// <param name="statusId">The ID of the status.</param>
        /// <returns>The name of the status</returns>
        protected static string GetApplicationStatusName(int? statusId)
        {
            if (statusId.HasValue)
            {
                return ApplicationStatus.GetStatusName(statusId.Value);
            }

            return null;
        }

        /// <summary>
        /// Gets the ID of the user who submitted this application, or <see cref="string.Empty"/> if the user is anonymous.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        /// <returns>The user ID as a <see cref="string"/>, or <see cref="string.Empty"/> if <paramref name="userId"/> has no value</returns>
        protected static string GetUserId(object userId)
        {
            var userIdValue = userId as int?;
            if (userIdValue.HasValue)
            {
                return userIdValue.Value.ToString(CultureInfo.InvariantCulture);
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the URL at which to view the document.
        /// </summary>
        /// <param name="documentId">The ID of the document.</param>
        /// <returns>A URL pointing to the document with the given <paramref name="documentId"/></returns>
        protected static string GetDocumentUrl(int documentId)
        {
            return Employment.Utility.GetDocumentUrl(documentId);
        }

        /// <summary>
        /// Gets the text for the lead entry with the given ID.
        /// </summary>
        /// <param name="leadIdValue">The ID of the list entry for the lead to display.</param>
        /// <returns>The display text for the lead</returns>
        protected string GetLeadText(string leadIdValue)
        {
            int leadId;
            if (!int.TryParse(leadIdValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out leadId))
            {
                return string.Empty;
            }

            var leadListEntry = this.leadsList[leadId];
            return leadListEntry.Text;
        }

        /// <summary>
        /// Gets the user-friendly name for the given document type.
        /// </summary>
        /// <param name="documentTypeId">The ID of the document type.</param>
        /// <returns>A localized name for the type of document</returns>
        protected string GetDocumentTypeText(int documentTypeId)
        {
            return this.Localize(DocumentType.GetDocumentType(documentTypeId).Description);
        }

        /// <summary>
        /// Gets the job detail URL.
        /// </summary>
        /// <param name="detailJobId">The job id.</param>
        /// <returns>A URL to the job details page for the job with JobId of <paramref name="detailJobId"/>.</returns>
        protected string GetJobDetailUrl(object detailJobId)
        {
            return Employment.Utility.GetJobDetailUrl(detailJobId, this.JobGroupId, this.PortalSettings);
        }

        /// <summary>
        /// Gets the name of the user, formatted based on the "UserName" localization key.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns>A user name</returns>
        protected string GetUserName(int? userId)
        {
            if (userId.HasValue)
            {
                UserInfo applicationUser = new UserController().GetUser(this.PortalId, userId.Value);

                if (applicationUser != null)
                {
                    return string.Format(
                        CultureInfo.CurrentCulture,
                        this.Localize("UserName", this.LocalSharedResourceFile),
                        applicationUser.DisplayName,
                        applicationUser.FirstName,
                        applicationUser.LastName);
                }
            }

            return this.Localize("AnonymousUser");
        }

        /// <summary>
        /// Gets the user status for the given user.
        /// </summary>
        /// <param name="userId">The ID of the user, or <c>null</c> for an anonymous user.</param>
        /// <returns>The ID of the user's status</returns>
        protected string GetUserStatus(int? userId)
        {
            try
            {
                if (userId.HasValue)
                {
                    var statusId = UserStatusInfo.LoadUserStatus(this.PortalSettings, userId.Value);
                    if (statusId.HasValue)
                    {
                        return statusId.Value.ToString(CultureInfo.InvariantCulture);
                    }
                }
            }
            catch (NullReferenceException)
            {
                // thrown from LoadUserStatus if user doesn't exist (has been deleted after applying).  BD
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the name of the user's status.
        /// </summary>
        /// <param name="userId">The user ID, or <c>null</c> for an anonymous user.</param>
        /// <returns>The name of the status, or <c>null</c> for an anonymous user or user without a status</returns>
        protected string GetUserStatusName(int? userId)
        {
            try
            {
                if (userId.HasValue)
                {
                    var statusId = UserStatusInfo.LoadUserStatus(this.PortalSettings, userId.Value);
                    if (statusId.HasValue)
                    {
                        return UserStatus.LoadStatus(statusId.Value).Status;
                    }
                }
            }
            catch (NullReferenceException)
            {
                // thrown from LoadUserStatus if user doesn't exist (has been deleted after applying).  BD
            }

            return null;
        }

        /// <summary>
        /// Whether to show the user status drop down for the given user
        /// </summary>
        /// <param name="userId">The ID of the user, or <c>null</c> for an anonymous user.</param>
        /// <returns>
        /// <c>true</c> if the user status drop down should be shown for the user; otherwise, <c>false</c>
        /// </returns>
        protected bool ShowUserStatuses(int? userId)
        {
            // not anonymous, we are using user statuses on this portal
            if (!userId.HasValue || !this.ShowUserStatuses())
            {
                return false;
            }

            // the user isn't deleted or superuser (superusers can't have profile properties)
            var user = new UserController().GetUser(this.PortalId, userId.Value);
            return user != null && !user.IsSuperUser;
        }

        /// <summary>
        /// Whether to show the application status drop down.
        /// </summary>
        /// <returns><c>true</c> if the application status drop down should be shown; otherwise, <c>false</c></returns>
        protected bool ShowApplicationStatuses()
        {
            return this.applicationStatuses.Any();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            if (!PermissionController.CanManageApplications(this))
            {
                DenyAccess();
                return;
            }

            base.OnInit(e);

            this.Load += this.Page_Load;

            this.JobsGrid.NeedDataSource += this.JobsGrid_NeedDataSource;
            this.JobsGrid.ItemCreated += this.JobsGrid_ItemCreated;
            this.JobsGrid.ItemCommand += this.JobsGrid_ItemCommand;
            this.JobsGrid.ItemDataBound += this.JobsGrid_ItemDataBound;
            this.JobsGrid.DetailTableDataBind += this.JobsGrid_DetailTableDataBind;

            this.PopulateSearchDropDown();

            try
            {
                this.SetLinks();
                this.LocalizeGrid();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>
        /// Populates list items of all the search drop downs.
        /// </summary>
        private void PopulateSearchDropDown()
        {
            this.FilterByJobsGroup.Visible = !(this.JobId.HasValue);
            ////this.FilterByJobTitleLabel.Visible = !(this.JobId.HasValue);
            ////this.FilterByJobTitleTextBox.Visible = !(this.JobId.HasValue);

            ////this.FilterByLocationLabel.Visible = !(this.JobId.HasValue);
            ////this.FilterByLocationDropDown.Visible = !(this.JobId.HasValue);

            this.FilterByLocationDropDown.DataSource = this.LocationsForSearch;
            this.FilterByLocationDropDown.DataTextField = "Text";
            this.FilterByLocationDropDown.DataValueField = "Value";
            this.FilterByLocationDropDown.DataBind();

            this.FilterByApplicantStatusDropDown.DataSource = this.UserStatusesForSearch;
            this.FilterByApplicantStatusDropDown.DataTextField = "Text";
            this.FilterByApplicantStatusDropDown.DataValueField = "Value";
            this.FilterByApplicantStatusDropDown.SelectedValue = (this.UserStatusId.HasValue) ? this.UserStatusId.ToString() : string.Empty;
            this.FilterByApplicantStatusDropDown.DataBind();
            ////this.FilterByApplicantStatusDropDown.Enabled = !this.UserStatusId.HasValue;

            this.FilterByApplicationStatusDropDown.DataSource = this.ApplicationStatusesForSearch;
            this.FilterByApplicationStatusDropDown.DataTextField = "Text";
            this.FilterByApplicationStatusDropDown.DataValueField = "Value";
            this.FilterByApplicationStatusDropDown.SelectedValue = (this.ApplicationStatusId.HasValue) ? this.ApplicationStatusId.ToString() : string.Empty;
            this.FilterByApplicationStatusDropDown.DataBind();
            ////this.FilterByApplicationStatusDropDown.Enabled = !this.ApplicationStatusId.HasValue;

            this.FilterByLeadDropDown.DataSource = this.LeadsForSearch;
            this.FilterByLeadDropDown.DataTextField = "Text";
            this.FilterByLeadDropDown.DataValueField = "Value";
            this.FilterByLeadDropDown.DataBind();

        }

        /// <summary>
        /// Handles the Click event of the SearchButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void SearchButton_Click(object sender, EventArgs e)
        {
            int tempUserStatusId, tempApplicationStatusId, tempLeadId, tempLocationId;

            ViewState["UserStatusId"] = int.TryParse(this.FilterByApplicantStatusDropDown.SelectedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out tempUserStatusId)
                                            ? (int?)tempUserStatusId
                                            : -1;
            ViewState["ApplicationStatusId"] = int.TryParse(this.FilterByApplicationStatusDropDown.SelectedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out tempApplicationStatusId)
                                                   ? (int?)tempApplicationStatusId
                                                   : -1;
            ViewState["LeadId"] = (int.TryParse(this.FilterByLeadDropDown.SelectedValue, out tempLeadId)) ? (int?)tempLeadId : null;
            ViewState["DateFrom"] = !string.IsNullOrEmpty(this.FilterByDateFromTextBox.Text)
                                        ? (DateTime?)DateTime.Parse(this.FilterByDateFromTextBox.Text, CultureInfo.CurrentCulture)
                                        : null;
            ViewState["DateTo"] = !string.IsNullOrEmpty(this.FilterByDateToTextBox.Text)
                                      ? (DateTime?)DateTime.Parse(this.FilterByDateToTextBox.Text, CultureInfo.CurrentCulture).AddDays(1).AddMilliseconds(-1)
                                      : null;
            ViewState["JobTitle"] = !string.IsNullOrEmpty(this.FilterByJobTitleTextBox.Text) ? this.FilterByJobTitleTextBox.Text.Trim() : null;
            ViewState["LocationId"] = int.TryParse(this.FilterByLocationDropDown.SelectedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out tempLocationId) ? (int?)tempLocationId : null;

            this.JobsGrid.Rebind();
        }

        /// <summary>
        /// Handles the <see cref="ListControl.SelectedIndexChanged"/> event of the Application Status <see cref="DropDownList"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "StatusId", Justification = "StatusId is understandable.")]
        protected void ApplicationStatusDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var applicationStatusDropDownList = (DropDownList)sender;
            var applicationId = (int)Utility.FindParentControl<GridDataItem>(applicationStatusDropDownList).GetDataKeyValue("ApplicationId");
            
            var application = JobApplication.Load(applicationId);

            int statusId;
            if (!Utility.HasValue(applicationStatusDropDownList.SelectedValue))
            {
                application.StatusId = null;
            }
            else if (int.TryParse(applicationStatusDropDownList.SelectedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out statusId))
            {
                application.StatusId = statusId;
            }
            else
            {
                Exceptions.LogException(new InvalidOperationException("During update of application status, StatusId could not be parsed."));
            }

            application.Save(this.UserId);
            ////this.BindApplicationStatusDropDownList(applicationStatusDropDownList, application.StatusId);
        }

        /// <summary>
        /// Handles the <see cref="ListControl.SelectedIndexChanged"/> event of the User Status <see cref="DropDownList"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "StatusId", Justification = "StatusId is understandable.")]
        protected void UserStatusDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var userStatusDropDownList = (DropDownList)sender;
            var applicationRow = Utility.FindParentControl<GridDataItem>(userStatusDropDownList);
            var userId = (int)applicationRow.GetDataKeyValue("UserId");

            int statusId;
            int? statusIdValue = null;
            if (int.TryParse(userStatusDropDownList.SelectedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out statusId))
            {
                statusIdValue = statusId;
            }
            else if (Utility.HasValue(userStatusDropDownList.SelectedValue))
            {
                Exceptions.LogException(new InvalidOperationException("During update of application status, StatusId could not be parsed."));
            }

            UserStatusInfo.UpdateUserStatus(this.PortalSettings, userId, statusIdValue);

            // collapse all other application grids, so that the user's status isn't out of sync
            foreach (GridDataItem item in this.JobsGrid.MasterTableView.Items.Cast<GridDataItem>()
                                            .Where(item => item.Expanded && item != applicationRow.OwnerTableView.ParentItem))
            {
                item.Expanded = false;
            }
        }

        /// <summary>
        /// Handles the <see cref="RadGrid.NeedDataSource"/> event of the <see cref="JobsGrid"/> control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="GridNeedDataSourceEventArgs"/> instance containing the event data.</param>
        private void JobsGrid_NeedDataSource(object source, GridNeedDataSourceEventArgs e)
        {
            if (this.JobId.HasValue)
            {
                this.JobsGrid.DataSource = new[] { Job.Load(this.JobId.Value) };
                this.JobsGrid.VirtualItemCount = 1;

                // TODO: Hide search
                this.AllLinkWrapperBottom.Visible = true;
                this.AllLinkWrapperTop.Visible = true;
            }
            else
            {
                int totalJobCount;
                this.JobsGrid.DataSource = Job.LoadAll(
                    this.JobGroupId,
                    this.PortalId,
                    (string)ViewState["JobTitle"],
                    (int?)ViewState["LocationId"],
                    this.JobsGrid.CurrentPageIndex,
                    this.IsExport ? (int?)null : this.JobsGrid.MasterTableView.PageSize,
                    out totalJobCount);
                this.JobsGrid.VirtualItemCount = totalJobCount;
            }

            this.JobsGrid.MasterTableView.HierarchyDefaultExpanded = this.JobsGrid.VirtualItemCount == 1;
        }

        /// <summary>
        /// Handles the <see cref="RadGrid.ItemCreated"/> event of the <see cref="JobsGrid"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridItemEventArgs"/> instance containing the event data.</param>
        private void JobsGrid_ItemCreated(object sender, GridItemEventArgs e)
        {
            var commandItem = e.Item as GridCommandItem;
            if (commandItem != null)
            {
                // control information from http://www.telerik.com/help/aspnet-ajax/grddefaultbehavior.html
                commandItem.FindControl("AddNewRecordButton").Visible = false;
                commandItem.FindControl("InitInsertButton").Visible = false;
                commandItem.FindControl("RefreshButton").Visible = false;
                commandItem.FindControl("RebindGridButton").Visible = false;

                string tableHeaderResourceKey;
                if (commandItem.OwnerTableView == this.JobsGrid.MasterTableView)
                {
                    tableHeaderResourceKey = "JobsTableHeader";
                }
                else
                {
                    tableHeaderResourceKey = "ApplicationsTableHeader";
                }

                var leftColumn = commandItem.FindControl("AddNewRecordButton").Parent;
                leftColumn.Controls.Add(new LiteralControl(this.Localize(tableHeaderResourceKey)));

                // from http://www.telerik.com/help/aspnet-ajax/grdexportwithajax.html
                AJAX.RegisterPostBackControl(commandItem.FindControl("ExportToExcelButton"));
                AJAX.RegisterPostBackControl(commandItem.FindControl("ExportToCsvButton"));
            }
            else
            {
                // control information from http://www.telerik.com/help/aspnet-ajax/grdaccessingdefaultpagerbuttons.html
                var pagerItem = e.Item as GridPagerItem;
                if (pagerItem != null)
                {
                    ((Label)pagerItem.FindControl("ChangePageSizeLabel")).Text = this.Localize("Change Page Size Label.Text");
                }
            }
        }

        private void JobsGrid_ItemDataBound(object sender, GridItemEventArgs e)
        {
            var dataItem = e.Item as GridDataItem;
            if (dataItem != null)
            {
                if (dataItem.OwnerTableView != this.JobsGrid.MasterTableView)
                {
                    var isHtmlFormat = this.exportType != RadGrid.ExportToCsvCommandName;
                    var application = (JobApplication)e.Item.DataItem;

                    dataItem["Documents"].Text = this.RenderDocumentsCell(isHtmlFormat, application.GetDocuments());
                    dataItem["Properties"].Text = this.RenderPropertiesCell(isHtmlFormat, application.GetApplicationProperties());
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="RadGrid.ItemCommand"/> event of the <see cref="JobsGrid"/> control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="GridCommandEventArgs"/> instance containing the event data.</param>
        private void JobsGrid_ItemCommand(object source, GridCommandEventArgs e)
        {
            if (e.CommandName != RadGrid.ExportToCsvCommandName && e.CommandName != RadGrid.ExportToExcelCommandName)
            {
                return;
            }

            this.exportType = e.CommandName;
            this.JobsGrid.ExportSettings.ExportOnlyData = e.CommandName == RadGrid.ExportToExcelCommandName;

            var exportingTable = e.Item.OwnerTableView;
            if (exportingTable == this.JobsGrid.MasterTableView)
            {
                exportingTable.Columns.FindByUniqueName("Export-Title").Visible = true;
                exportingTable.Columns.FindByUniqueName("Title").Visible = false;

                this.SetExportFileName(string.Format(CultureInfo.CurrentCulture, this.Localize("Jobs Export FileName.Format"), DateTime.Now));
            }
            else
            {
                exportingTable.Columns.FindByUniqueName("Export-UserStatus").Visible = this.ShowUserStatuses();
                exportingTable.Columns.FindByUniqueName("Export-ApplicationStatus").Visible = this.ShowApplicationStatuses();
                exportingTable.Columns.FindByUniqueName("Export-Message").Visible = true;
                exportingTable.Columns.FindByUniqueName("ApplicationStatus").Visible = false;

                var job = Job.Load((int)exportingTable.ParentItem.GetDataKeyValue("JobId"));
                this.SetExportFileName(
                    string.Format(
                        CultureInfo.CurrentCulture,
                        this.Localize("Applications Export FileName.Format"),
                        DateTime.Now,
                        job.Title,
                        job.LocationName,
                        job.StateName,
                        job.StateAbbreviation));

                exportingTable.Rebind();
            }
        }

        /// <summary>
        /// Handles the <see cref="RadGrid.DetailTableDataBind"/> event of the <see cref="JobsGrid"/> control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="GridDetailTableDataBindEventArgs"/> instance containing the event data.</param>
        private void JobsGrid_DetailTableDataBind(object source, GridDetailTableDataBindEventArgs e)
        {
            var parentRow = e.DetailTableView.ParentItem;
            var parentJobId = (int)parentRow.GetDataKeyValue("JobId");

            e.DetailTableView.ExpandCollapseColumn.Display = false;
            e.DetailTableView.Columns.FindByUniqueName("ApplicationStatus").Visible = this.ShowApplicationStatuses();

            ////var userIds = this.UserStatusId.HasValue
            ////                  ? UserStatusInfo.GetUsersWithStatus(this.PortalSettings, this.UserStatusId.Value).Select(user => user.UserId)
            ////                  : Enumerable.Empty<int>();

            int unpagedApplicationCount;
            e.DetailTableView.DataSource = JobApplication.LoadApplicationsForJob(
                parentJobId,
                this.JobGroupId,
                this.ApplicationStatusId,
                this.UserStatusId,
                (int?)ViewState["LeadId"],
                (DateTime?)ViewState["DateFrom"],
                (DateTime?)ViewState["DateTo"],
                e.DetailTableView.CurrentPageIndex, 
                this.IsExport ? (int?)null : e.DetailTableView.PageSize, 
                out unpagedApplicationCount,
                true);
            e.DetailTableView.VirtualItemCount = unpagedApplicationCount;
        }

        /// <summary>
        /// Renders the documents cell for a particular application.
        /// </summary>
        /// <param name="isHtmlFormat">if set to <c>true</c> renders HTML; otherwise, renders plain text.</param>
        /// <param name="documents">The list of documents.</param>
        /// <returns>The HTML for the documents cell</returns>
        private string RenderDocumentsCell(bool isHtmlFormat, IEnumerable<Document> documents)
        {
            var documentsTextBuilder = new StringBuilder();
            if (isHtmlFormat)
            {
                documentsTextBuilder.Append("<ul>");
            }

            foreach (var document in documents)
            {
                var documentType = this.GetDocumentTypeText(document.DocumentTypeId);
                var documentUrl = GetDocumentUrl(document.DocumentId);
                documentsTextBuilder.AppendFormat(
                    CultureInfo.CurrentCulture,
                    isHtmlFormat ? "<li><a href=\"{1}\" target=\"_blank\">{0}</a></li>" : "{0}: {1}{2}",
                    isHtmlFormat ? HttpUtility.HtmlEncode(documentType) : documentType,
                    isHtmlFormat ? HttpUtility.HtmlEncode(documentUrl) : documentUrl,
                    Environment.NewLine);
            }

            if (isHtmlFormat)
            {
                documentsTextBuilder.Append("</ul>");
            }

            return documentsTextBuilder.ToString();
        }

        /// <summary>
        /// Renders the properties/lead cell for a particular application.
        /// </summary>
        /// <param name="isHtmlFormat">if set to <c>true</c> renders HTML; otherwise, renders plain text.</param>
        /// <param name="applicationProperties">The properties for the application.</param>
        /// <returns>The HTML for the properties cell</returns>
        private string RenderPropertiesCell(bool isHtmlFormat, Dictionary<string, string> applicationProperties)
        {
            var propertiesTextBuilder = new StringBuilder();
            if (isHtmlFormat)
            {
                propertiesTextBuilder.Append("<ul>");
            }

            foreach (var propertyValue in applicationProperties.Values)
            {
                var leadText = this.GetLeadText(propertyValue);
                propertiesTextBuilder.AppendFormat(
                    CultureInfo.CurrentCulture,
                    isHtmlFormat ? "<li>{0}</li>" : "{0}{1}",
                    isHtmlFormat ? HttpUtility.HtmlEncode(leadText) : leadText,
                    Environment.NewLine);
            }

            if (isHtmlFormat)
            {
                propertiesTextBuilder.Append("</ul>");
            }

            return propertiesTextBuilder.ToString();
        }

        /// <summary>
        /// Sets the URL for the <see cref="BackLink"/> and <see cref="AllLink"/> controls.
        /// </summary>
        private void SetLinks()
        {
            var mi = Employment.Utility.GetCurrentModuleByDefinition(this.PortalSettings, ModuleDefinition.JobListing, this.JobGroupId);
            this.BackLink.NavigateUrl = mi != null ? Globals.NavigateURL(mi.TabID) : Globals.NavigateURL();

            this.AllLinkTop.NavigateUrl = this.AllLinkBottom.NavigateUrl = this.EditUrl(ControlKey.ManageApplications.ToString());
        }

        /// <summary>
        /// Localizes the static text within the <see cref="JobsGrid"/>.
        /// </summary>
        private void LocalizeGrid()
        {
            this.JobsGrid.SortingSettings.SortedAscToolTip = this.Localize("Sorted Ascending.ToolTip");
            this.JobsGrid.SortingSettings.SortedDescToolTip = this.Localize("Sorted Descending.ToolTip");
            this.JobsGrid.SortingSettings.SortToolTip = this.Localize("Sort.ToolTip");
            this.JobsGrid.HierarchySettings.ExpandTooltip = this.Localize("Expand.ToolTip");
            this.JobsGrid.HierarchySettings.CollapseTooltip = this.Localize("Collapse.ToolTip");

            this.LocalizeGridTable(this.JobsGrid.MasterTableView);
            this.LocalizeDetailGridTable(this.JobsGrid.MasterTableView.DetailTables[0]);
        }

        /// <summary>
        /// Localizes the given <see cref="GridTableView"/> within the <see cref="JobsGrid"/>.
        /// </summary>
        /// <param name="tableView">The table view.</param>
        private void LocalizeGridTable(GridTableView tableView)
        {
            tableView.PagerStyle.FirstPageToolTip = this.Localize("First Page.ToolTip");
            tableView.PagerStyle.PrevPageToolTip = this.Localize("Previous Page.ToolTip");
            tableView.PagerStyle.NextPageToolTip = this.Localize("Next Page.ToolTip");
            tableView.PagerStyle.LastPageToolTip = this.Localize("Last Page.ToolTip");
            tableView.PagerStyle.FirstPageText = this.Localize("First Page.Text");
            tableView.PagerStyle.PrevPageText = this.Localize("Previous Page.Text");
            tableView.PagerStyle.NextPageText = this.Localize("Next Page.Text");
            tableView.PagerStyle.LastPageText = this.Localize("Last Page.Text");
            tableView.PagerStyle.PagerTextFormat = this.Localize("Pager.Format");
            tableView.CommandItemSettings.ExportToExcelText = this.Localize("Export To Excel.ToolTip");
            tableView.CommandItemSettings.ExportToCsvText = this.Localize("Export To CSV.ToolTip");

            foreach (GridColumn column in tableView.Columns)
            {
                column.HeaderText = this.Localize(column.HeaderText);
            }
        }

        private void LocalizeDetailGridTable(GridTableView detailTableView)
        {
            this.LocalizeGridTable(detailTableView);
            detailTableView.PagerStyle.PagerTextFormat = this.Localize("DetailPager.Format");
        }

        /// <summary>
        /// Whether to show the user status drop down for any user
        /// </summary>
        /// <returns>
        /// <c>true</c> if the user status drop down should be shown; otherwise, <c>false</c>
        /// </returns>
        private bool ShowUserStatuses()
        {
            return this.userStatuses.Any();
        }

        /// <summary>
        /// Sets the filename of the exported file.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        private void SetExportFileName(string fileName)
        {
            const int MaxFileNameLength = 150;
            if (fileName.Length > MaxFileNameLength)
            {
                fileName = fileName.Substring(0, MaxFileNameLength);
            }

            this.JobsGrid.ExportSettings.FileName = fileName;
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                this.RegisterDatePickerBehavior();
            }
        }

        /// <summary>
        /// Registers the jQuery date picker plugin on the page.
        /// </summary>
        private void RegisterDatePickerBehavior()
        {
            this.AddJQueryReference();
            this.Page.ClientScript.RegisterClientScriptResource(typeof(JobEdit), "Engage.Dnn.Employment.JavaScript.jquery-ui.js");

            var datePickerOptions = new DatePickerOptions(CultureInfo.CurrentCulture, this.LocalSharedResourceFile);
            this.Page.ClientScript.RegisterClientScriptBlock(typeof(JobEdit), "datepicker options", "var datePickerOpts = " + datePickerOptions.Serialize() + ";", true);        
        }
    }
}