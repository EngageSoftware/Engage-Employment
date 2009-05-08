// <copyright file="JobListing.ascx.cs" company="Engage Software">
// Engage: Employment
// Copyright (c) 2004-2009
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
    using System.Data;
    using System.Globalization;
    using System.Web.UI.WebControls;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
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

        #region IActionable Members

        /// <summary>
        /// Gets the list of <see cref="ModuleAction"/>s to be displayed for this control.
        /// </summary>
        /// <value>The list of <see cref="ModuleAction"/>s to be displayed for this control.</value>
        public ModuleActionCollection ModuleActions
        {
            get
            {
                return new ModuleActionCollection(new ModuleAction[]
                {
                    new ModuleAction(
                        this.GetNextActionID(),
                        Localization.GetString("ManageStates.Text", this.LocalResourceFile),
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
                        Localization.GetString("ManageLocations.Text", this.LocalResourceFile),
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
                        Localization.GetString("ManageCategories.Text", this.LocalResourceFile),
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
                        Localization.GetString("ManagePositions.Text", this.LocalResourceFile),
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
                        Localization.GetString("AddNewJob.Text", this.LocalResourceFile),
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

        #endregion

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
                if (locationName is DBNull)
                {
                    // position row has a non-null LocationName
                    return Localization.GetString("UnassignedCategories", this.LocalResourceFile);
                }
                else
                {
                    return Localization.GetString("UnassignedPositions", this.LocalResourceFile);
                }
            }
            else
            {
                return string.Format(
                    CultureInfo.CurrentCulture, Localization.GetString("Location", this.LocalResourceFile), locationName, stateName, stateAbbreviation);
            }
        }

        /// <summary>
        /// Gets a link to the applications of an opening represented by the given data row.
        /// </summary>
        /// <param name="row">A <see cref="DataRowView"/> representing a job opening.</param>
        /// <returns>A link to the applications of the given job opening</returns>
        protected string GetApplicationsLink(object row)
        {
            DataRowView jobRow = (DataRowView)row;

            if (jobRow.DataView.Table.Columns.Contains("JobId"))
            {
                int jobId = (int)jobRow["JobId"];
                int applicationCount = (int)jobRow["ApplicationCount"];
                string applicationText = applicationCount != 1
                                  ? Localization.GetString("Applications", this.LocalResourceFile)
                                  : Localization.GetString("Application", this.LocalResourceFile);

                return string.Format(
                    CultureInfo.CurrentCulture,
                    "<a href=\"{0}\">{1} {2}</a>",
                    this.EditUrl(ControlKey.ManageApplications.ToString()) + "#" + jobId.ToString(CultureInfo.InvariantCulture),
                    applicationCount,
                    applicationText);
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets the URL to navigate to in order to edit the given job.
        /// </summary>
        /// <param name="row">A <see cref="DataRowView"/> representing a job opening.</param>
        /// <returns>The URL to navigate to in order to edit the given job</returns>
        protected string GetEditUrl(object row)
        {
            DataRowView jobRow = (DataRowView)row;
            if (jobRow.DataView.Table.Columns.Contains("JobId"))
            {
                return this.EditUrl("JobId", ((int)jobRow["JobId"]).ToString(CultureInfo.InvariantCulture), ControlKey.EditJob.ToString());
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
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
            this.JobsGrid.DataSource = Job.GetAdminData(this.JobGroupId, this.PortalId);
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
        /// Controls visibility of the AddJob button in the <see cref="JobsGrid"/>, and sets its redirection behavior
        /// </summary>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewRowEventArgs"/> instance containing the event data.</param>
        private void BindHeaderButtons(GridViewRowEventArgs e)
        {
            Button addJobButton = (Button)e.Row.FindControl("AddJobButton");
            addJobButton.Visible = Job.CanCreateJob(this.PortalId);
            AddRedirectionBehavior(addJobButton, this.EditUrl(ControlKey.EditJob.ToString()));
        }

        /// <summary>
        /// Adds redirection behavior to the <see cref="BackButton"/>
        /// </summary>
        private void BindFooterButton()
        {
            ModuleInfo jobListingModule = Utility.GetCurrentModuleByDefinition(this.PortalSettings, ModuleDefinition.JobListing, this.JobGroupId);

            if (jobListingModule != null)
            {
                AddRedirectionBehavior(this.BackButton, Globals.NavigateURL(jobListingModule.TabID));
            }
            else
            {
                AddRedirectionBehavior(this.BackButton, Globals.NavigateURL());
            }
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