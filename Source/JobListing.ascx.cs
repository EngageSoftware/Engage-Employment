//Engage: Employment - http://www.engagesoftware.com
//Copyright (c) 2004-2009
//by Engage Software ( http://www.engagesoftware.com )

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
//TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
//CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
//DEALINGS IN THE SOFTWARE.

namespace Engage.Dnn.Employment
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web.UI.WebControls;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Framework;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Utilities;
    using Globals=DotNetNuke.Common.Globals;

    partial class JobListing : ModuleBase, IActionable
    {
        private const string EditCommandName = "Edit";

        #region Module Settings
        private bool ShowOnlyHotJobs
        {
            get
            {
                return Dnn.Utility.GetBoolSetting(Settings, "ShowOnlyHotJobs", true);
            }
        }

        private int? MaximumNumberOfJobsDisplayed
        {
            get
            {
                int? value = Dnn.Utility.GetIntSetting(Settings, "MaximumNumberOfJobsDisplayed");
                if (!value.HasValue)
                {
                    //if the settings has never been set, keep the setting from the last version (5), otherwise the settings has been set to null
                    value = Settings.ContainsKey("MaximumNumberOfJobsDisplayed") ? (int?)null : 5;
                }
                return value;
            }
        }

        private bool LimitJobsRandomly
        {
            get
            {
                return Dnn.Utility.GetBoolSetting(Settings, "LimitJobsRandomly", true);
            }
        }
        #endregion

        #region Event Handlers

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void Page_Init(Object sender, EventArgs e)
        {
            try
            {
                if (AJAX.IsInstalled())
                {
                    //AJAX.RegisterScriptManager();
                    AJAX.WrapUpdatePanelControl(rpSavedSearches, true);
                }
                rpSavedSearches.ItemDataBound += rpSavedSearches_ItemDataBound;
                rpAppliedJobs.ItemDataBound += rpAppliedJobs_ItemDataBound;
                rpAppliedJobs.ItemCommand += rpAppliedJobs_ItemCommand;
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void Page_Load(Object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    LoadJobListing();
                    LoadJobsAppliedFor();
                    LoadSavedSearches();
                }

                SetLinkUrls();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

//        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
//        protected void rpJobListing_ItemDataBound(object sender, RepeaterItemEventArgs e)
//        {
//            if (e != null)
//            {
//                Job j = (Job)e.Item.DataItem;
//
//                this.currentCategory = (j == null ? null : j.CategoryName);
//            }
//        }

        [SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member"), SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void rpSavedSearches_ItemDataBound(object source, RepeaterItemEventArgs e)
        {
            if (e != null && (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem))
            {
                JobSearchQuery query = e.Item.DataItem as JobSearchQuery;
                Button btnDelete = e.Item.FindControl("btnDelete") as Button;
                HyperLink lnkSearch = e.Item.FindControl("lnkSearch") as HyperLink;

                if (query != null)
                {
                    if (btnDelete != null)
                    {
                        ClientAPI.AddButtonConfirm(btnDelete, Localization.GetString("DeleteConfirm.Text", LocalResourceFile));
                        btnDelete.CommandArgument = query.Id.ToString(CultureInfo.InvariantCulture);
                    }
                    if (lnkSearch != null)
                    {
                        lnkSearch.NavigateUrl = Globals.NavigateURL(Utility.GetSearchResultsTabId(this.JobGroupId, PortalSettings), string.Empty, "usid=" + query.Id);
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member"), SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void rpAppliedJobs_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e != null && (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem))
            {
                JobApplication jobApplication = e.Item.DataItem as JobApplication;
                Button btnEditApplication = (Button)e.Item.FindControl("btnEditApplication");

                if (jobApplication != null)
                {
                    if (btnEditApplication != null)
                    {
                        btnEditApplication.CommandName = EditCommandName;
                        btnEditApplication.CommandArgument = GetApplicationEditUrl(jobApplication.ApplicationId, jobApplication.JobId);
                    }
                }
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1706:ShortAcronymsShouldBeUppercase", MessageId = "Member"), SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        private void rpAppliedJobs_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e != null && e.CommandName == EditCommandName)
            {
                Response.Redirect((string)e.CommandArgument);
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        protected void btnDelete_Command(object sender, CommandEventArgs e)
        {
            int queryId;
            if (e != null && e.CommandArgument != null && int.TryParse(e.CommandArgument.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out queryId))
            {
                JobSearchQuery.Delete(queryId);
                LoadSavedSearches();
            }
            LoadSavedSearches();
        }
        #endregion

        private void LoadJobListing()
        {
            this.rpJobListing.DataSource = Job.Load(this.MaximumNumberOfJobsDisplayed, this.LimitJobsRandomly, this.ShowOnlyHotJobs, this.JobGroupId, this.PortalId);
            this.rpJobListing.DataBind();
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected string GetJobListingHeader() 
        {
            if (MaximumNumberOfJobsDisplayed.HasValue)
            {
                if (LimitJobsRandomly)
                {
                    if (ShowOnlyHotJobs)
                    {
                        return string.Format(CultureInfo.CurrentCulture, Localization.GetString("HotJobs", LocalResourceFile),
                            MaximumNumberOfJobsDisplayed.Value);
                    }
                    else
                    {
                        return string.Format(CultureInfo.CurrentCulture, Localization.GetString("Jobs", LocalResourceFile),
                            MaximumNumberOfJobsDisplayed.Value);
                    }
                }
                else
                {
                    if (ShowOnlyHotJobs)
                    {
                        return string.Format(CultureInfo.CurrentCulture, Localization.GetString("TopHotJobs", LocalResourceFile),
                            MaximumNumberOfJobsDisplayed.Value);
                    }
                    else
                    {
                        return string.Format(CultureInfo.CurrentCulture, Localization.GetString("TopJobs", LocalResourceFile),
                            MaximumNumberOfJobsDisplayed.Value);
                    }
                }
            }
            else
            {
                if (ShowOnlyHotJobs)
                {
                    return Localization.GetString("AllHotJobs", LocalResourceFile);
                }
                else
                {
                    return Localization.GetString("AllJobs", LocalResourceFile);
                }
            }
        }

        private void LoadJobsAppliedFor()
        {
            if (Engage.Utility.IsLoggedIn == false) return;

            ReadOnlyCollection<JobApplication> applications = JobApplication.GetAppliedFor(UserId, this.JobGroupId, PortalId);
            if (applications != null && applications.Count > 0)
            {
                this.rpAppliedJobs.DataSource = applications;
                this.rpAppliedJobs.DataBind();
            }
        }

        private void LoadSavedSearches()
        {
            rpSavedSearches.Controls.Clear();
            if (!Null.IsNull(UserId))
            {
                ReadOnlyCollection<JobSearchQuery> queries = JobSearchQuery.LoadSearches(UserId, this.JobGroupId);

                if (queries.Count > 0)
                {
                    rpSavedSearches.DataSource = queries;
                    rpSavedSearches.DataBind();
                }
            }
        }

        private void SetLinkUrls()
        {
            //this.hlAllHotJobs.NavigateUrl = Globals.NavigateURL(TabId, string.Empty, "jobType=" + Convert.ToInt32(JobListingType.AllHot, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture));
            int searchResultsTabId = Utility.GetSearchResultsTabId(this.JobGroupId, PortalSettings);
            this.hlSearchJobs.Visible = searchResultsTabId != TabId;
            if (this.hlSearchJobs.Visible)
            {
                this.hlSearchJobs.NavigateUrl = Globals.NavigateURL(searchResultsTabId);
            }
        }

//        protected string GetCategory(object o)
//        {
//            if (o != null && string.Equals(this.currentCategory, o.ToString(), StringComparison.OrdinalIgnoreCase))
//            {
//                return string.Empty;
//            }
//            else
//            {
//                //return "<h3>" + o + "</h3><br />";
//                return "<br /><strong>" + o + "</strong><br />";
//            }
//        }

// ReSharper disable SuggestBaseTypeForParameter
        private string GetApplicationEditUrl(int applicationId, int jobId)
// ReSharper restore SuggestBaseTypeForParameter
        {
            return Globals.NavigateURL(Utility.GetJobDetailTabId(this.JobGroupId, PortalSettings), string.Empty, "jobId=" + jobId.ToString(CultureInfo.InvariantCulture), "applicationId=" + applicationId.ToString(CultureInfo.InvariantCulture));
        }

        #region IActionable Members

        public ModuleActionCollection ModuleActions
        {
            get
            {
                ModuleActionCollection actions = new ModuleActionCollection();

                actions.Add(GetNextActionID(), Localization.GetString("ManageApplications", LocalResourceFile), ModuleActionType.AddContent, "", "", EditUrl(ControlKey.ManageApplications.ToString()), false, SecurityAccessLevel.Edit, true, false);
                actions.Add(GetNextActionID(), Localization.GetString("ManageJobs", LocalResourceFile), ModuleActionType.AddContent, "", "", EditUrl(ControlKey.Edit.ToString()), false, SecurityAccessLevel.Edit, true, false);
                actions.Add(GetNextActionID(), Localization.GetString("JobListingOptions", LocalResourceFile), ModuleActionType.AddContent, "", "", EditUrl(ControlKey.Options.ToString()), false, SecurityAccessLevel.Edit, true, false);

                return actions;
            }
        }

        #endregion

        protected string GetJobDetailUrl(object jobId)
        {
            return Utility.GetJobDetailUrl(jobId, this.JobGroupId, PortalSettings);
        }
    }
}