// <copyright file="ApplicationListing.ascx.cs" company="Engage Software">
// Engage: Employment - http://www.engagesoftware.com
// Copyright (c) 2004-2010
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
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web.UI.WebControls;
    using Data;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using Utility=Engage.Utility;

    /// <summary>
    /// Lists applications for the available jobs in the portal.
    /// </summary>
    public partial class ApplicationListing : ModuleBase, IActionable
    {
        #region IActionable Members

        /// <summary>
        /// Gets the module actions.
        /// </summary>
        /// <value>The module actions.</value>
        public ModuleActionCollection ModuleActions
        {
            get
            {
                return new ModuleActionCollection(new ModuleAction[]
                    {
                        new ModuleAction(
                            this.GetNextActionID(),
                            Localization.GetString("ManageStatuses", this.LocalResourceFile),
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
                            Localization.GetString("ManageApplicationStatuses", this.LocalResourceFile),
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

        #endregion

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
        /// Gets the job detail URL.
        /// </summary>
        /// <param name="jobId">The job id.</param>
        /// <returns>A URL to the job details page for the job with JobId of <paramref name="jobId"/>.</returns>
        protected string GetJobDetailUrl(object jobId)
        {
            return Employment.Utility.GetJobDetailUrl(jobId, this.JobGroupId, this.PortalSettings);
        }

        /// <summary>
        /// Gets the name of the user, formatted based on the "UserName" localization key.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <returns></returns>
        protected string GetUserName(int? userId)
        {
            if (userId.HasValue)
            {
                UserInfo applicationUser = new UserController().GetUser(this.PortalId, userId.Value);

                if (applicationUser != null)
                {
                    return string.Format(
                        CultureInfo.CurrentCulture,
                        Localization.GetString("UserName", this.LocalSharedResourceFile),
                        applicationUser.DisplayName,
                        applicationUser.FirstName,
                        applicationUser.LastName);
                }
            }

            return Localization.GetString("AnonymousUser", this.LocalResourceFile);
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.JobsRepeater.ItemDataBound += this.JobsRepeater_ItemDataBound;
            this.BackButton.Click += this.BackButton_Click;

            try
            {
                this.LoadApplications();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the JobsRepeater control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        private void JobsRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e != null && (e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.Item))
            {
                var applicationsRepeater = (Repeater)e.Item.FindControl("ApplicationsRepeater");
                var job = (Job)e.Item.DataItem;

                if (job != null)
                {
                    if (applicationsRepeater != null)
                    {
                        applicationsRepeater.ItemDataBound += this.ApplicationsRepeater_ItemDataBound;
                        ReadOnlyCollection<JobApplication> applications = JobApplication.LoadApplicationsForJob(job.JobId, this.JobGroupId);
                        if (applications != null && applications.Count > 0)
                        {
                            applicationsRepeater.DataSource = applications;
                            applicationsRepeater.DataBind();
                        }
                        else
                        {
                            e.Item.Visible = false;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the ApplicationsRepeater control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Compiler doesn't see validation")]
        private void ApplicationsRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e != null && (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem))
            {
                var userStatusDropDownList = (DropDownList)e.Item.FindControl("UserStatusDropDownList");
                var applicationStatusDropDownList = (DropDownList)e.Item.FindControl("ApplicationStatusDropDownList");
                var documentsRepeater = e.Item.FindControl("DocumentsRepeater") as Repeater;
                var propertiesRepeater = e.Item.FindControl("PropertiesRepeater") as Repeater;
                var application = (JobApplication)e.Item.DataItem;

                if (application != null)
                {
                    if (application.UserId.HasValue)
                    {
                        try
                        {
                            userStatusDropDownList.SelectedIndexChanged += this.UserStatusDropDownList_SelectedIndexChanged;
                            if (!this.IsPostBack)
                            {
                                this.BindUserStatusDropDownList(userStatusDropDownList, UserStatus.LoadUserStatus(this.PortalSettings, application.UserId.Value));
                            }
                        }
                        catch (NullReferenceException)
                        {
                            // thrown from LoadUserStatus if user doesn't exist (has been deleted after applying).  BD
                            userStatusDropDownList.Visible = false;
                        }
                    }
                    else
                    {
                        userStatusDropDownList.Visible = false;
                    }

                    applicationStatusDropDownList.SelectedIndexChanged += this.ApplicationStatusDropDownList_SelectedIndexChanged;
                    if (!this.IsPostBack)
                    {
                        this.BindApplicationStatusDropDownList(applicationStatusDropDownList, application.StatusId);
                    }

                    if (documentsRepeater != null && propertiesRepeater != null)
                    {
                        documentsRepeater.ItemDataBound += this.DocumentsRepeater_ItemDataBound;
                        documentsRepeater.DataSource = DataProvider.Instance().GetApplicationDocuments(application.ApplicationId);
                        documentsRepeater.DataBind();

                        propertiesRepeater.ItemDataBound += PropertiesRepeater_ItemDataBound;
                        propertiesRepeater.DataSource = DataProvider.Instance().GetApplicationProperties(application.ApplicationId);
                        propertiesRepeater.DataBind();
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the DocumentsRepeater control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Compiler doesn't see validation")]
        private void DocumentsRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e != null && (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem))
            {
                var documentLink = (HyperLink)e.Item.FindControl("DocumentLink");
                var row = (DataRowView)e.Item.DataItem;

                if (row != null)
                {
                    object documentIdObj = row["DocumentId"];
                    object documentTypeIdObj = row["DocumentTypeId"];
                    if (documentLink != null && documentIdObj is int && documentTypeIdObj is int)
                    {
                        var resumeId = (int)documentIdObj;

                        documentLink.NavigateUrl = Employment.Utility.GetDocumentUrl(this.Request, resumeId);

                        var documentTypeId = (int)documentTypeIdObj;
                        documentLink.Text = Localization.GetString(DocumentType.GetDocumentType(documentTypeId).Description, this.LocalResourceFile);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the PropertiesRepeater control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> instance containing the event data.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Compiler doesn't see validation")]
        private static void PropertiesRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e != null && (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem))
            {
                var applicationPropertyLabel = (Label)e.Item.FindControl("ApplicationPropertyLabel");
                var row = (DataRowView)e.Item.DataItem;

                if (row != null && row["PropertyValue"] != DBNull.Value)
                {
                    int leadId = Convert.ToInt32(row["PropertyValue"], CultureInfo.InvariantCulture);
                    if (applicationPropertyLabel != null)
                    {
                        ListEntryInfo leadListEntry = (new ListController()).GetListEntryInfo(leadId);
                        applicationPropertyLabel.Text = leadListEntry.Text;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the UserStatusDropDownList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "StatusId", Justification = "StatusId is understandable."), 
        SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "UserId", Justification = "UserId is understandable.")]
        private void UserStatusDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var userStatusDropDownList = (DropDownList)sender;
            var userIdHiddenField = (HiddenField)Utility.FindParentControl<RepeaterItem>(userStatusDropDownList).FindControl("UserIdHiddenField");

            int userId;
            if (int.TryParse(userIdHiddenField.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out userId))
            {
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

                UserStatus.UpdateUserStatus(this.PortalSettings, userId, statusIdValue);
                ////this.BindUserStatusDropDownList(userStatusDropDownList, statusIdValue);
                
                // reload page to make sure that all other drop downs for this user are updated
                this.Response.Redirect(this.EditUrl(ControlKey.ManageApplications.ToString()));
            }
            else
            {
                Exceptions.LogException(new InvalidOperationException("During update of application status, UserId could not be parsed."));
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ApplicationStatusDropDown control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ApplicationId", Justification = "ApplicationId is understandable."), 
        SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "StatusId", Justification = "StatusId is understandable.")]
        private void ApplicationStatusDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            var applicationStatusDropDownList = (DropDownList)sender;
            var applicationIdHiddenField = (HiddenField)Utility.FindParentControl<RepeaterItem>(applicationStatusDropDownList).FindControl("ApplicationIdHiddenField");

            int applicationId;
            if (int.TryParse(applicationIdHiddenField.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out applicationId))
            {
                JobApplication application = JobApplication.Load(applicationId);

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
                this.BindApplicationStatusDropDownList(applicationStatusDropDownList, application.StatusId);
            }
            else
            {
                Exceptions.LogException(new InvalidOperationException("During update of application status, ApplicationId could not be parsed."));
            }
        }

        /// <summary>
        /// Handles the Click event of the BackButton control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void BackButton_Click(object source, EventArgs e)
        {
            var mi = Employment.Utility.GetCurrentModuleByDefinition(this.PortalSettings, ModuleDefinition.JobListing, this.JobGroupId);

            this.Response.Redirect(mi != null ? Globals.NavigateURL(mi.TabID) : Globals.NavigateURL());
        }

        /// <summary>
        /// Loads the applications.
        /// </summary>
        private void LoadApplications()
        {
            this.JobsRepeater.DataSource = Job.LoadAll(this.JobGroupId, this.PortalId);
            this.JobsRepeater.DataBind();
        }

        /// <summary>
        /// Load the given list with the list of <see cref="UserStatus"/>es for this portal and sets the selected value to the given <paramref name="statusId"/>.
        /// </summary>
        /// <param name="userStatusDropDownList">The <see cref="ListControl"/> of user statuses.</param>
        /// <param name="statusId">The status ID.</param>
        private void BindUserStatusDropDownList(ListControl userStatusDropDownList, int? statusId)
        {
            List<UserStatus> statuses = UserStatus.LoadStatuses(this.PortalId);
            if (statuses.Count > 0)
            {
                userStatusDropDownList.DataSource = statuses;
                userStatusDropDownList.DataTextField = "Status";
                userStatusDropDownList.DataValueField = "StatusId";
                userStatusDropDownList.DataBind();

                userStatusDropDownList.Items.Insert(0, new ListItem(Localization.GetString("NoStatus", this.LocalResourceFile), string.Empty));
            }
            else
            {
                userStatusDropDownList.Visible = false;
            }

            userStatusDropDownList.SelectedValue = statusId.HasValue ? statusId.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
        }

        /// <summary>
        /// Load the given list with the list of <see cref="ApplicationStatus"/>es for this portal and sets the selected value to the given <paramref name="statusId"/>.
        /// </summary>
        /// <param name="applicationStatusList">The <see cref="ListControl"/> of application statuses.</param>
        /// <param name="statusId">The status ID.</param>
        private void BindApplicationStatusDropDownList(ListControl applicationStatusList, int? statusId)
        {
            List<ApplicationStatus> statuses = ApplicationStatus.GetStatuses(this.PortalId);
            if (statuses.Count > 0)
            {
                applicationStatusList.DataSource = statuses;
                applicationStatusList.DataTextField = "StatusName";
                applicationStatusList.DataValueField = "StatusId";
                applicationStatusList.DataBind();

                applicationStatusList.Items.Insert(0, new ListItem(Localization.GetString("NoApplicationStatus", this.LocalResourceFile), string.Empty));
            }
            else
            {
                applicationStatusList.Visible = false;
            }

            applicationStatusList.SelectedValue = statusId.HasValue ? statusId.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
        }
    }
}