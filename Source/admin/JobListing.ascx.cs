// <copyright file="JobListing.ascx.cs" company="Engage Software">
// Engage: Employment
// Copyright (c) 2004-2013
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
    using System.Globalization;
    using System.Linq;
    using System.Web.UI.WebControls;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.UI.Utilities;
    using Globals = DotNetNuke.Common.Globals;

    /// <summary>
    /// An administrative listing of jobs and job components
    /// </summary>
    public partial class JobListing : ModuleBase, IActionable
    {
        /// <summary>
        /// JavaScript to redirect to a location which can be supplied in the 0 index of a <see cref="string.Format(IFormatProvider,string,object[])"/> call.
        /// </summary>
        private const string RedirectionJavaScript = "location.href='{0}';return false;";

        /// <summary>
        /// A table with the number of applications with each status for each job.
        /// Has the columns Count, JobId, and StatusId
        /// </summary>
        private DataTable applicationStatusTable;

        /// <summary>
        /// A table mapping users to jobs.  Used to calculate the number of applications with each user status for each job.
        /// Has the columns JobId, UserId
        /// </summary>
        private DataTable userStatusTable;

        /// <summary>
        /// Maps from a status ID to an <see cref="ApplicationStatus"/>
        /// </summary>
        private Dictionary<int, ApplicationStatus> applicationStatusMap;

        /////// <summary>
        /////// Maps from a status ID to a <see cref="UserStatus"/>
        /////// </summary>
        ////private Dictionary<int, UserStatus> userStatusMap;

        /// <summary>
        /// Gets the list of <see cref="ModuleAction"/>s to be displayed for this control.
        /// </summary>
        /// <value>The list of <see cref="ModuleAction"/>s to be displayed for this control.</value>
        public ModuleActionCollection ModuleActions
        {
            get
            {
                return new ModuleActionCollection(new[]
                {
                    new ModuleAction(
                        this.GetNextActionID(),
                        this.Localize("ManageStates.Text"),
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        this.EditUrl(ControlKey.ManageStates.ToString()),
                        string.Empty,
                        false,
                        SecurityAccessLevel.Edit,
                        true,
                        false),
                    new ModuleAction(
                        this.GetNextActionID(),
                        this.Localize("ManageLocations.Text"),
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        this.EditUrl(ControlKey.ManageLocations.ToString()),
                        string.Empty,
                        false,
                        SecurityAccessLevel.Edit,
                        Location.CanCreateLocation(this.PortalId),
                        false),
                    new ModuleAction(
                        this.GetNextActionID(),
                        this.Localize("ManageCategories.Text"),
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        this.EditUrl(ControlKey.ManageCategories.ToString()),
                        string.Empty,
                        false,
                        SecurityAccessLevel.Edit,
                        true,
                        false),
                    new ModuleAction(
                        this.GetNextActionID(),
                        this.Localize("ManagePositions.Text"),
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        this.EditUrl(ControlKey.ManagePositions.ToString()),
                        string.Empty,
                        false,
                        SecurityAccessLevel.Edit,
                        true,
                        false),
                    new ModuleAction(
                        this.GetNextActionID(),
                        this.Localize("AddNewJob.Text"),
                        string.Empty,
                        string.Empty,
                        string.Empty,
                        this.EditUrl(ControlKey.EditJob.ToString()),
                        string.Empty,
                        false,
                        SecurityAccessLevel.Edit,
                        Job.CanCreateJob(this.PortalId),
                        false)
                });
            }
        }

        /// <summary>
        /// Gets the name of the location, including the state name and/or abbreviation (depending on the "Location" resource key).
        /// </summary>
        /// <param name="locationId">The location ID.</param>
        /// <param name="locationName">Name of the location.</param>
        /// <param name="stateName">Name of the state.</param>
        /// <param name="stateAbbreviation">The state abbreviation.</param>
        /// <returns>A formatted version of the location and state</returns>
        protected string GetLocationName(object locationId, object locationName, object stateName, object stateAbbreviation)
        {
            if (locationId is DBNull)
            {
                // empty row for categories or positions not assigned to a job
                return locationName is DBNull ? this.Localize("UnassignedCategories") : this.Localize("UnassignedPositions");
            }

            return string.Format(CultureInfo.CurrentCulture, this.Localize("Location"), locationName, stateName, stateAbbreviation);
        }

        /// <summary>
        /// Gets a link to the applications of an opening represented by the given data row.
        /// </summary>
        /// <param name="row">A <see cref="DataRowView"/> representing a job opening.</param>
        /// <returns>A link to the applications of the given job opening</returns>
        protected string GetApplicationsLink(object row)
        {
            var jobRow = (DataRowView)row;

            if (jobRow.DataView.Table.Columns.Contains("JobId"))
            {
                var jobId = (int)jobRow["JobId"];
                var applicationCount = (int)jobRow["ApplicationCount"];
                string applicationText = applicationCount != 1
                                  ? this.Localize("Applications")
                                  : this.Localize("Application");

                return string.Format(
                    CultureInfo.CurrentCulture,
                    "<a href=\"{0}\">{1} {2}</a>",
                    this.EditUrl("jobId", jobId.ToString(CultureInfo.InvariantCulture), ControlKey.ManageApplications.ToString()),
                    applicationCount,
                    applicationText);
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets a sequence of objects with information about applications grouped by both application and user status.
        /// </summary>
        /// <param name="jobId">The job ID.</param>
        /// <returns>A sequence of anonymous objects with four properties (<c>IsUserStatus</c>, <c>Url</c>, <c>Count</c>, and <c>Status</c>)</returns>
        protected IEnumerable<object> GetApplicationStatusLinks(int jobId)
        {
            foreach (var statusRow in from DataRow row in this.applicationStatusTable.Rows
                                      where (int)row["JobId"] == jobId
                                      select new
                                      {
                                          Count = (int)row["Count"],
                                          JobId = (int)row["JobId"],
                                          StatusId = (int)row["StatusId"]
                                      })
            {
                ApplicationStatus status;
                if (!this.applicationStatusMap.TryGetValue(statusRow.StatusId, out status))
                {
                    continue;
                }

                yield return new
                {
                    IsUserStatus = false,
                    Url = this.EditUrl(
                            "jobId",
                            jobId.ToString(CultureInfo.InvariantCulture),
                            ControlKey.ManageApplications.ToString(),
                            "statusId=" + statusRow.StatusId.ToString(CultureInfo.InvariantCulture)),
                    Count = statusRow.Count,
                    Status = status.StatusName
                };
            }

            foreach (var row in this.userStatusTable.Rows.Cast<DataRow>()
                                            .Where(row => (int)row["JobId"] == jobId)
                                            .Select(row => row))
            {
                yield return new
                {
                    IsUserStatus = true,
                    Url =
                        this.EditUrl(
                            "jobId",
                            jobId.ToString(CultureInfo.InvariantCulture),
                            ControlKey.ManageApplications.ToString(),
                            "userStatusId=" + ((int)row["UserStatusId"]).ToString(CultureInfo.InvariantCulture)),
                    Count = (int)row["Count"],
                    Status = (string)row["Status"]
                };
            }
        }

        /// <summary>
        /// Gets the URL to navigate to in order to edit the given job.
        /// </summary>
        /// <param name="row">A <see cref="DataRowView"/> representing a job opening.</param>
        /// <returns>The URL to navigate to in order to edit the given job</returns>
        protected string GetEditUrl(object row)
        {
            var jobRow = (DataRowView)row;
            if (jobRow.DataView.Table.Columns.Contains("JobId"))
            {
                return this.EditUrl("JobId", ((int)jobRow["JobId"]).ToString(CultureInfo.InvariantCulture), ControlKey.EditJob.ToString());
            }

            return string.Empty;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            if (!PermissionController.CanManageJobs(this))
            {
                this.DenyAccess();
                return;
            }

            base.OnInit(e);
            this.Load += this.Page_Load;
            this.JobsGrid.RowDataBound += this.JobsGrid_RowDataBound;
        }

        /// <summary>
        /// Adds behavior to the given <paramref name="control"/> so that it redirects to the given <paramref name="url"/> when clicked
        /// </summary>
        /// <param name="control">The control to which the behavior should be added.</param>
        /// <param name="url">The URL that the user should be redirected to after clicking on <see cref="control"/>.</param>
        private static void AddRedirectionBehavior(WebControl control, string url)
        {
            control.Attributes["onclick"] = string.Format(CultureInfo.InvariantCulture, RedirectionJavaScript, ClientAPI.GetSafeJSString(url));
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!this.IsPostBack)
                {
                    this.BindData();
                    this.BindFooterButton();
                    this.SetupAddJobButton();
                }
            }
            catch (Exception exc)
            {
                // Module failed to load
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>
        /// Handles the RowDataBound event of the <see cref="JobsGrid"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewRowEventArgs"/> instance containing the event data.</param>
        private void JobsGrid_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.Header)
            {
                this.BindHeaderButtons(e);
            }
        }

        /// <summary>
        /// Binds the grids on this page with their data
        /// </summary>
        private void BindData()
        {
            var adminData = Job.GetAdminData(this.JobGroupId, this.PortalId);
            this.applicationStatusTable = adminData.Tables["ApplicationStatuses"];
            this.userStatusTable = adminData.Tables["UserStatuses"];

            this.InitializeStatusMaps();

            this.JobsGrid.DataSource = adminData.Tables["Jobs"];
            this.JobsGrid.DataBind();

            DataSet unusedData = Job.GetUnusedAdminData(this.JobGroupId, this.PortalId);
            this.EmptyStateRepeater.DataSource = unusedData.Tables["States"];
            this.EmptyStateRepeater.DataBind();
            this.EmptyCategoryRepeater.DataSource = unusedData.Tables["Categories"];
            this.EmptyCategoryRepeater.DataBind();
            this.EmptyPositionRepeater.DataSource = unusedData.Tables["Positions"];
            this.EmptyPositionRepeater.DataBind();

            this.EmptyLocationRepeater.Visible = Location.CanCreateLocation(this.PortalId);
            if (this.EmptyLocationRepeater.Visible)
            {
                this.EmptyLocationRepeater.DataSource = unusedData.Tables["Locations"];
                this.EmptyLocationRepeater.DataBind();
            }
        }

        /// <summary>
        /// Creates the <see cref="applicationStatusMap"/> and <see cref="userStatusMap"/>.
        /// </summary>
        private void InitializeStatusMaps()
        {
            this.applicationStatusMap = ApplicationStatus.GetStatuses(this.PortalId).ToDictionary(status => status.StatusId);

            ////var statusMap = UserStatus.LoadStatuses(this.PortalId).ToDictionary(status => status.StatusId);
            ////this.userStatusMap = UserStatus.GetUsersWithStatus(this.PortalSettings).ToDictionary(
            ////    user => user.UserID,
            ////    user =>
            ////    {
            ////        var status = user.Profile.GetPropertyValue(Utility.UserStatusPropertyName);
            ////        int statusId;
            ////        if (int.TryParse(status, out statusId))
            ////        {
            ////            UserStatus userStatus;
            ////            if (statusMap.TryGetValue(statusId, out userStatus))
            ////            {
            ////                return userStatus;
            ////            }
            ////        }

            ////        return null;
            ////    });

            ////this.userStatusMap = this.applicationUsersTable.Rows.Cast<DataRow>()
            ////    .Select(row => (int)row["UserId"])
            ////    .Distinct()
            ////    .ToDictionary(
            ////        userId => userId,
            ////        userId =>
            ////            {
            ////                UserStatus status;
            ////                var statusId = UserStatus.LoadUserStatus(this.PortalSettings, userId);
            ////                if (statusId.HasValue && statusMap.TryGetValue(statusId.Value, out status))
            ////                {
            ////                    return status;
            ////                }

            ////                return null;
            ////            });
        }

        /// <summary>
        /// Controls visibility of the AddJob button in the <see cref="JobsGrid"/>, and sets its redirection behavior
        /// </summary>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewRowEventArgs"/> instance containing the event data.</param>
        private void BindHeaderButtons(GridViewRowEventArgs e)
        {
            var addJobButton = (Button)e.Row.FindControl("AddJobButton");
            addJobButton.Visible = Job.CanCreateJob(this.PortalId);
            AddRedirectionBehavior(addJobButton, this.EditUrl(ControlKey.EditJob.ToString()));
        }

        /// <summary>
        /// Adds redirection behavior to the <see cref="BackButton"/>
        /// </summary>
        private void BindFooterButton()
        {
            ModuleInfo jobListingModule = Utility.GetCurrentModuleByDefinition(this.PortalSettings, ModuleDefinition.JobListing, this.JobGroupId);

            AddRedirectionBehavior(this.BackButton, jobListingModule != null ? Globals.NavigateURL(jobListingModule.TabID) : Globals.NavigateURL());
        }

        /// <summary>
        /// Sets up the <see cref="AddJobButton"/> control to only display when the grid isn't displayed
        /// </summary>
        private void SetupAddJobButton()
        {
            this.AddJobButton.Visible = this.JobsGrid.Rows.Count == 0 && Job.CanCreateJob(this.PortalId);
            if (this.AddJobButton.Visible)
            {
                AddRedirectionBehavior(this.AddJobButton, this.EditUrl(ControlKey.EditJob.ToString()));
            }
        }
    }
}