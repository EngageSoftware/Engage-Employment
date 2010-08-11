// <copyright file="JobListing.ascx.cs" company="Engage Software">
// Engage: Employment
// Copyright (c) 2004-2010
// by Engage Software ( http://www.engagesoftware.com )
// </copyright>
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

namespace Engage.Dnn.Employment
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Framework;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;
    using Globals = DotNetNuke.Common.Globals;

    /// <summary>
    /// Display a listing of jobs (determined by the settings).  Also, when the user is logged in, shows lists of saved searches and the jobs they've applied for.  
    /// Includes a link to the search module, and action menu access to <see cref="Admin.ApplicationListing"/>, <see cref="Admin.JobListing"/>, and <see cref="JobListingOptions"/>
    /// </summary>
    public partial class JobListing : ModuleBase, IActionable
    {
        private const string EditCommandName = "Edit";

        public ModuleActionCollection ModuleActions
        {
            get
            {
                return new ModuleActionCollection
                           {
                                   {
                                           this.GetNextActionID(), 
                                           Localization.GetString("ManageApplications", this.LocalResourceFile), 
                                           ModuleActionType.AddContent, 
                                           string.Empty, 
                                           string.Empty, 
                                           this.EditUrl(ControlKey.ManageApplications.ToString()), 
                                           false, 
                                           SecurityAccessLevel.Edit, 
                                           true, 
                                           false
                                   }, 
                                   {
                                           this.GetNextActionID(), 
                                           Localization.GetString("ManageJobs", this.LocalResourceFile), 
                                           ModuleActionType.AddContent, 
                                           string.Empty, 
                                           string.Empty, 
                                           this.EditUrl(ControlKey.Edit.ToString()), 
                                           false, 
                                           SecurityAccessLevel.Edit, 
                                           true, 
                                           false
                                   }, 
                                   {
                                           this.GetNextActionID(), 
                                           Localization.GetString("JobListingOptions", this.LocalResourceFile), 
                                           ModuleActionType.AddContent, 
                                           string.Empty, 
                                           string.Empty, 
                                           this.EditUrl(ControlKey.Options.ToString()), 
                                           false, 
                                           SecurityAccessLevel.Edit, 
                                           true, 
                                           false
                                   }
                           };
            }
        }

        private bool LimitJobsRandomly
        {
            get { return ModuleSettings.JobListingLimitJobsRandomly.GetValueAsBooleanFor(this).Value; }
        }

        private int? MaximumNumberOfJobsDisplayed
        {
            get
            {
                    // If the maximum number of jobs is set as no maximum, it is stored as an empty string.  
                    // Since this isn't an int value, getting it as an int gives us the default, instead of null
                    // So, if we get the default, we need to doublecheck that it's the default and not the "no maximum" value
                    int? maximumNumberOfJobs = ModuleSettings.JobListingMaximumNumberOfJobsDisplayed.GetValueAsInt32For(this);
                    if (maximumNumberOfJobs == ModuleSettings.JobListingMaximumNumberOfJobsDisplayed.DefaultValue 
                        && string.IsNullOrEmpty(ModuleSettings.JobListingMaximumNumberOfJobsDisplayed.GetValueAsStringFor(this)))
                    {
                        maximumNumberOfJobs = null;
                    }

                    return maximumNumberOfJobs;
            }
        }

        private bool ShowOnlyHotJobs
        {
            get { return ModuleSettings.JobListingShowOnlyHotJobs.GetValueAsBooleanFor(this).Value; }
        }

        /// <summary>
        /// Handles the <see cref="Button.Command"/> event of the <c>btnDelete</c> control in the <see cref="SavedSearchesRepeater"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.CommandEventArgs"/> instance containing the event data.</param>
        protected void DeleteButton_Command(object sender, CommandEventArgs e)
        {
            int queryId;
            if (e != null && e.CommandArgument != null && int.TryParse(e.CommandArgument.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out queryId))
            {
                JobSearchQuery.Delete(queryId);
                this.LoadSavedSearches();
            }

            this.LoadSavedSearches();
        }

        protected string GetJobDetailUrl(object jobId)
        {
            return Utility.GetJobDetailUrl(jobId, this.JobGroupId, this.PortalSettings);
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Does not represent object state")]
        protected string GetJobListingHeader()
        {
            if (this.MaximumNumberOfJobsDisplayed.HasValue)
            {
                if (this.LimitJobsRandomly)
                {
                    return this.ShowOnlyHotJobs
                                   ? string.Format(
                                             CultureInfo.CurrentCulture, 
                                             Localization.GetString("HotJobs", this.LocalResourceFile), 
                                             this.MaximumNumberOfJobsDisplayed.Value)
                                   : string.Format(
                                             CultureInfo.CurrentCulture, 
                                             Localization.GetString("Jobs", this.LocalResourceFile), 
                                             this.MaximumNumberOfJobsDisplayed.Value);
                }

                return this.ShowOnlyHotJobs
                               ? string.Format(
                                         CultureInfo.CurrentCulture, 
                                         Localization.GetString("TopHotJobs", this.LocalResourceFile), 
                                         this.MaximumNumberOfJobsDisplayed.Value)
                               : string.Format(
                                         CultureInfo.CurrentCulture, 
                                         Localization.GetString("TopJobs", this.LocalResourceFile), 
                                         this.MaximumNumberOfJobsDisplayed.Value);
            }

            return this.ShowOnlyHotJobs
                           ? Localization.GetString("AllHotJobs", this.LocalResourceFile)
                           : Localization.GetString("AllJobs", this.LocalResourceFile);
        }

        /// <summary>
        /// Raises the <see cref="Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            try
            {
                if (AJAX.IsInstalled())
                {
                    // AJAX.RegisterScriptManager();
                    AJAX.WrapUpdatePanelControl(this.SavedSearchesRepeater, true);
                }

                this.Load += this.Page_Load;
                this.SavedSearchesRepeater.ItemDataBound += this.SavedSearchesRepeater_ItemDataBound;
                this.AppliedJobsRepeater.ItemDataBound += this.AppliedJobsRepeater_ItemDataBound;
                this.AppliedJobsRepeater.ItemCommand += this.AppliedJobsRepeater_ItemCommand;
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }

            base.OnInit(e);
        }

        private string GetApplicationEditUrl(int applicationId, int jobId)
        {
            return Globals.NavigateURL(
                    Utility.GetJobDetailTabId(this.JobGroupId, this.PortalSettings), 
                    string.Empty, 
                    "jobId=" + jobId.ToString(CultureInfo.InvariantCulture), 
                    "applicationId=" + applicationId.ToString(CultureInfo.InvariantCulture));
        }

        private void LoadJobListing()
        {
            this.JobListingRepeater.DataSource = Job.LoadActiveJobs(this.MaximumNumberOfJobsDisplayed, this.LimitJobsRandomly, this.ShowOnlyHotJobs, this.JobGroupId, this.PortalId);
            this.JobListingRepeater.DataBind();
        }

        private void LoadJobsAppliedFor()
        {
            if (Engage.Utility.IsLoggedIn == false)
            {
                return;
            }

            ReadOnlyCollection<JobApplication> applications = JobApplication.GetAppliedFor(this.UserId, this.JobGroupId, this.PortalId);
            if (applications != null && applications.Count > 0)
            {
                this.AppliedJobsRepeater.DataSource = applications;
                this.AppliedJobsRepeater.DataBind();
            }
        }

        private void LoadSavedSearches()
        {
            this.SavedSearchesRepeater.Controls.Clear();
            if (!Null.IsNull(this.UserId))
            {
                ReadOnlyCollection<JobSearchQuery> queries = JobSearchQuery.LoadSearches(this.UserId, this.JobGroupId);

                if (queries.Count > 0)
                {
                    this.SavedSearchesRepeater.DataSource = queries;
                    this.SavedSearchesRepeater.DataBind();
                }
            }
        }

        private void SetLinkUrls()
        {
            int searchResultsTabId = Utility.GetSearchResultsTabId(this.JobGroupId, this.PortalSettings);
            this.SearchJobsLink.Visible = searchResultsTabId != this.TabId;
            if (this.SearchJobsLink.Visible)
            {
                this.SearchJobsLink.NavigateUrl = Globals.NavigateURL(searchResultsTabId);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Load"/> event of this control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!this.IsPostBack)
                {
                    this.LoadJobListing();
                    this.LoadJobsAppliedFor();
                    this.LoadSavedSearches();
                }

                this.SetLinkUrls();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>
        /// Handles the <see cref="Repeater.ItemCommand"/> event of the <see cref="AppliedJobsRepeater"/> control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        private void AppliedJobsRepeater_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e != null && e.CommandName == EditCommandName)
            {
                this.Response.Redirect((string)e.CommandArgument);
            }
        }

        /// <summary>
        /// Handles the <see cref="Repeater.ItemDataBound"/> event of the <see cref="AppliedJobsRepeater"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        private void AppliedJobsRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e != null && (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem))
            {
                var jobApplication = e.Item.DataItem as JobApplication;
                var editApplicationButton = (Button)e.Item.FindControl("EditApplicationButton");

                if (jobApplication != null)
                {
                    if (editApplicationButton != null)
                    {
                        editApplicationButton.CommandName = EditCommandName;
                        editApplicationButton.CommandArgument = this.GetApplicationEditUrl(jobApplication.ApplicationId, jobApplication.JobId);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="Repeater.ItemDataBound"/> event of the <see cref="SavedSearchesRepeater"/> control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        private void SavedSearchesRepeater_ItemDataBound(object source, RepeaterItemEventArgs e)
        {
            if (e != null && (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem))
            {
                var query = e.Item.DataItem as JobSearchQuery;
                var deleteButton = e.Item.FindControl("DeleteButton") as Button;
                var searchLink = e.Item.FindControl("SearchLink") as HyperLink;

                if (query != null)
                {
                    if (deleteButton != null)
                    {
                        ClientAPI.AddButtonConfirm(deleteButton, Localization.GetString("DeleteConfirm.Text", this.LocalResourceFile));
                        deleteButton.CommandArgument = query.Id.ToString(CultureInfo.InvariantCulture);
                    }

                    if (searchLink != null)
                    {
                        searchLink.NavigateUrl = Globals.NavigateURL(Utility.GetSearchResultsTabId(this.JobGroupId, this.PortalSettings), string.Empty, "usid=" + query.Id);
                    }
                }
            }
        }
    }
}