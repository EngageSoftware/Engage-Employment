// <copyright file="JobDetail.ascx.cs" company="Engage Software">
// Engage: Employment - http://www.engagesoftware.com
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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net.Mail;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using Data;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Mail;

    /// <summary>
    /// A control which displays detailed information about a job opening.  Allows users to email the job opening to a friend, and to apply for the job opening.
    /// </summary>
    public partial class JobDetail : ModuleBase, IActionable
    {
        /// <summary>
        /// Backing field for <see cref="CurrentJob"/>
        /// </summary>
        private Job currentJob;

        public ModuleActionCollection ModuleActions
        {
            get
            {
                return new ModuleActionCollection(new[]
                {
                    new ModuleAction(
                        this.GetNextActionID(), 
                        Localization.GetString("JobDetailOptions", this.LocalResourceFile), 
                        ModuleActionType.ContentOptions, 
                        string.Empty, 
                        string.Empty, 
                        this.EditUrl(ControlKey.Options.ToString()), 
                        string.Empty, 
                        false, 
                        SecurityAccessLevel.Edit, 
                        true, 
                        false)
                });
            }
        }

        protected int? ApplicationId
        {
            get
            {
                int applicationId;
                string applicationIdFromQueryString = this.Request.QueryString["applicationId"];
                if (Engage.Utility.HasValue(applicationIdFromQueryString) && int.TryParse(applicationIdFromQueryString, NumberStyles.Integer, CultureInfo.InvariantCulture, out applicationId))
                {
                    return applicationId;
                }

                return null;
            }
        }

        protected Job CurrentJob
        {
            get
            {
                if (this.currentJob == null)
                {
                    this.currentJob = (Job)this.Context.Items["Job"];
                    if (this.currentJob == null)
                    {
                        int id = Job.CurrentJobId;
                        if (id != -1 && (!this.JobGroupId.HasValue || DataProvider.Instance().IsJobInGroup(id, this.JobGroupId.Value)))
                        {
                            this.currentJob = Job.Load(id);
                        }

                        if (this.currentJob == null)
                        {
                            this.currentJob = Job.CreateJob();
                        }

                        this.Context.Items["Job"] = this.currentJob;
                    }
                }

                return this.currentJob;
            }

            set
            {
                this.Context.Items["Job"] = this.currentJob = value;
            }
        }

        private string DefaultNotificationEmailAddress
        {
            get
            {
                return Dnn.Utility.GetStringSetting(this.Settings, "ApplicationEmailAddress", this.PortalSettings.Email);
            }
        }

        private Visibility DisplayCoverLetter
        {
            get
            {
                return Dnn.Utility.GetEnumSetting(this.Settings, "DisplayCoverLetter", Visibility.Hidden);
            }
        }

        private Visibility DisplayLead
        {
            get
            {
                return Dnn.Utility.GetEnumSetting(this.Settings, "DisplayLead", Visibility.Hidden);
            }
        }

        private Visibility DisplayMessage
        {
            get
            {
                return Dnn.Utility.GetEnumSetting(this.Settings, "DisplayMessage", Visibility.Optional);
            }
        }

        private Visibility DisplaySalaryRequirement
        {
            get
            {
                return Dnn.Utility.GetEnumSetting(this.Settings, "DisplaySalaryRequirement", Visibility.Optional);
            }
        }

        private string FriendEmailAddress
        {
            get
            {
                return Dnn.Utility.GetStringSetting(this.Settings, "FriendEmailAddress", this.PortalSettings.Email);
            }
        }

        private bool RequireRegistration
        {
            get
            {
                return Dnn.Utility.GetBoolSetting(this.Settings, "RequireRegistration", true);
            }
        }

        /// <summary>
        /// Raises the <see cref="ModuleBase.Init"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.Load += this.Page_Load;
            this.EmailFriendButton.Click += this.EmailFriendButton_Click;
            this.ApplyButton.Click += this.ApplyButton_Click;
            this.SendToFriendButton.Click += this.SendToFriendButton_Click;
            this.BackButton.Click += this.BackButton_Click;
        }

        /// <summary>
        /// Builds a regular expression to validate the a given filename ends with a file extension in the provided (comma-delimited) list.
        /// </summary>
        /// <remarks>            
        ///  A description of the regular expression: .*\.(?:[Pp][Dd][Ff]|[Dd][Oo][Cc][Xx]|[Ee][Tt][Cc])$
        ///  .*\.
        ///      Any character, any number of repetitions
        ///      Literal .
        ///  Match expression but don't capture it. [Pp][Dd][Ff]|[Dd][Oo][Cc][Xx]|[Ee][Tt][Cc]
        ///      Select from alternatives
        ///          [Pp][Dd][Ff]
        ///              Any character in this class: [Pp]
        ///              Any character in this class: [Dd]
        ///              Any character in this class: [Ff]
        ///          [Dd][Oo][Cc][Xx]
        ///              Any character in this class: [Dd]
        ///              Any character in this class: [Oo]
        ///              Any character in this class: [Cc]
        ///              Any character in this class: [Xx]
        ///          [Ee][Tt][Cc]
        ///              Any character in this class: [Ee]
        ///              Any character in this class: [Tt]
        ///              Any character in this class: [Cc]            
        ///  End of line or string
        /// </remarks>
        /// <param name="fileExtensionsList">The comma-delimited list of acceptable file extensions.</param>
        /// <returns>A regular expression which only matches filenames with a file extension in the given list</returns>
        private static string BuildFileExtensionValidationExpression(string fileExtensionsList)
        {
            var fileExtensionsBuilder = new StringBuilder(fileExtensionsList.Length);
            foreach (char c in fileExtensionsList)
            {
                if (c != ',')
                {
                    fileExtensionsBuilder.AppendFormat(CultureInfo.InvariantCulture, "[{0}{1}]", char.ToUpperInvariant(c), char.ToLowerInvariant(c));
                }
                else
                {
                    fileExtensionsBuilder.Append('|');
                }
            }

            return string.Format(CultureInfo.InvariantCulture, @".*\.(?:{0})$", fileExtensionsBuilder);
        }

        /// <summary>
        /// Fills in the information about the application, if one is specified and it belongs to this user.
        /// </summary>
        /// <returns>Whether the specified application was filled in</returns>
        private bool FillApplication()
        {
            JobApplication jobApplication = JobApplication.Load(this.ApplicationId.Value);
            if (this.UserId == jobApplication.UserId && !Null.IsNull(this.UserId))
            {
                List<Document> documents = jobApplication.GetDocuments();
                Dictionary<string, string> properties = jobApplication.GetApplicationProperties();
                this.InitializeApplicantInfoSection();

                this.ApplicationMessageTextBox.Text = jobApplication.Message;
                this.SalaryTextBox.Text = jobApplication.SalaryRequirement;

                if (this.LeadDropDownList.Items.Count < 1)
                {
                    this.FillLeadDropDown();
                }

                foreach (KeyValuePair<string, string> pair in properties)
                {
                    if (pair.Key.Equals(ApplicationPropertyDefinition.Lead.GetName(), StringComparison.Ordinal))
                    {
                        this.LeadDropDownList.SelectedValue = pair.Value;
                        break;
                    }
                }

                foreach (Document document in documents)
                {
                    HyperLink lnkDocument = null;
                    if (document.DocumentTypeId == DocumentType.Resume.GetId())
                    {
                        lnkDocument = this.ResumeLink;
                    }
                    else if (document.DocumentTypeId == DocumentType.CoverLetter.GetId())
                    {
                        lnkDocument = this.CoverLetterLink;
                    }

                    if (lnkDocument != null)
                    {
                        lnkDocument.Parent.Visible = true; // should be the panel around the link
                        lnkDocument.Text = document.FileName;
                        lnkDocument.NavigateUrl = Utility.GetDocumentUrl(this.Request, document.DocumentId);
                    }
                }

                return true;
            }

            return false;
        }

        private void FillLeadDropDown()
        {
            this.LeadRow.Visible = this.DisplayLead != Visibility.Hidden;
            this.LeadRequiredLabel.Visible = this.DisplayLead == Visibility.Required;

            if (this.LeadRow.Visible && this.LeadDropDownList.Items.Count < 1)
            {
                ListEntryInfoCollection leadList = new ListController().GetListEntryInfoCollection(Utility.LeadListName);

                if (leadList.Count > 0)
                {
                    this.LeadDropDownList.DataSource = leadList;
                    this.LeadDropDownList.DataTextField = "Text";
                    this.LeadDropDownList.DataValueField = "EntryID";
                    this.LeadDropDownList.DataBind();

                    if (this.DisplayLead == Visibility.Optional)
                    {
                        this.LeadDropDownList.Items.Insert(0, new ListItem(Localization.GetString("ChooseLead", this.LocalResourceFile), string.Empty));
                    }
                }
                else
                {
                    this.LeadRow.Visible = false;
                }
            }
        }

        private string GetMessageBody()
        {
            return this.GetMessageBody(null);
        }

        // TODO: This and GetSendToAFriendMessageBody need some serious refactoring (or replacement).  Ugly code, ugly output.
        private string GetMessageBody(int? resumeId)
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                using (var textWriter = new HtmlTextWriter(stringWriter))
                {
                    var table = new Table();

                    var row = new TableRow();
                    table.Rows.Add(row);

                    var cell = new TableCell();
                    row.Cells.Add(cell);

                    ModuleInfo jobDetailModule = Utility.GetCurrentModuleByDefinition(this.PortalSettings, ModuleDefinition.JobDetail, this.JobGroupId);

                    var jobDetailLink = new HyperLink
                                            {
                                                    Text = Localization.GetString("ApplicationEmailLink", this.LocalResourceFile),
                                                    NavigateUrl = this.MakeUrlAbsolute(
                                                        Globals.NavigateURL(
                                                            jobDetailModule == null ? -1 : jobDetailModule.TabID,
                                                            string.Empty,
                                                            "jobId=" + Job.CurrentJobId.ToString(CultureInfo.InvariantCulture)))
                                            };

                    cell.Controls.Add(jobDetailLink);

                    if (this.SalaryTextBox.Text.Length > 0)
                    {
                        row = new TableRow();
                        table.Rows.Add(row);
                        row.Cells.Add(new TableCell
                                          {
                                                  Text = Localization.GetString("ApplicationEmailSalaryLabel", this.LocalResourceFile) + this.SalaryTextBox.Text
                                          });
                    }

                    if (resumeId.HasValue)
                    {
                        row = new TableRow();
                        table.Rows.Add(row);

                        cell = new TableCell();
                        row.Cells.Add(cell);
                        var resumeLink = new HyperLink
                                             {
                                                     Text = Localization.GetString("ApplicationEmailResumeLink", this.LocalResourceFile),
                                                     NavigateUrl = Utility.GetDocumentUrl(this.Request, resumeId.Value)
                                             };
                        cell.Controls.Add(resumeLink);
                    }

                    row = new TableRow();
                    table.Rows.Add(row);

                    row.Cells.Add(new TableCell
                                     {
                                             Text = Localization.GetString("ApplicationEmailMessageLabel", this.LocalResourceFile) + this.ApplicationMessageTextBox.Text
                                     });

                    table.RenderControl(textWriter);
                    return stringWriter.ToString();
                }
            }
        }

        private string GetSendToAFriendMessageBody()
        {
            using (var stringWriter = new StringWriter(CultureInfo.InvariantCulture))
            {
                using (var textWriter = new HtmlTextWriter(stringWriter))
                {
                    var table = new Table();

                    var row = new TableRow();
                    table.Rows.Add(row);

                    var cell = new TableCell();
                    row.Cells.Add(cell);

                    ModuleInfo jobDetailModule = Utility.GetCurrentModuleByDefinition(this.PortalSettings, ModuleDefinition.JobDetail, this.JobGroupId);

                    var jobDetailLink = new HyperLink
                                             {
                                                     Text = Localization.GetString("FriendEmailLink", this.LocalResourceFile),
                                                     NavigateUrl = this.MakeUrlAbsolute(
                                                        Globals.NavigateURL(
                                                            jobDetailModule == null ? -1 : jobDetailModule.TabID,
                                                            string.Empty,
                                                            "jobId=" + Job.CurrentJobId.ToString(CultureInfo.InvariantCulture)))
                                             };

                    cell.Controls.Add(jobDetailLink);
                    row = new TableRow();
                    table.Rows.Add(row);

                    row.Cells.Add(new TableCell
                                      {
                                              Text = Localization.GetString("ApplicationEmailMessageLabel", this.LocalResourceFile) + this.FriendEmailMessageTextBox.Text
                                      });

                    table.RenderControl(textWriter);
                    return stringWriter.ToString();
                }
            }
        }

        /// <summary>
        /// Initializes the ApplicantInfoSection control, and hides the EmailFriendSection control.
        /// </summary>
        private void InitializeApplicantInfoSection()
        {
            this.ApplicantInfoSection.Visible = true;
            this.EmailFriendSection.Visible = false;

            this.FillLeadDropDown();

            this.ResumeFileRequiredValidator.Enabled = this.ResumeRequiredLabel.Visible = this.UserId == -1 || JobApplication.GetResumeId(this.UserId) == -1;
            this.ResumeMessageRow.Visible = this.UserId != -1 && JobApplication.GetResumeId(this.UserId) != -1;

            string fileExtensionsList = (string)Globals.HostSettings["FileExtensions"] ?? string.Empty;
            string fileExtensionValidationExpression = BuildFileExtensionValidationExpression(fileExtensionsList);
            this.ResumeFileExtensionValidator.ValidationExpression = this.CoverLetterFileExtensionValidator.ValidationExpression = fileExtensionValidationExpression;
            this.ResumeFileExtensionValidator.ErrorMessage = string.Format(CultureInfo.CurrentCulture, Localization.GetString("regexResumeFile.Text", this.LocalResourceFile), fileExtensionsList);
            this.CoverLetterFileExtensionValidator.ErrorMessage = string.Format(CultureInfo.CurrentCulture, Localization.GetString("regexCoverLetterFile.Text", this.LocalResourceFile), fileExtensionsList);

            this.CoverLetterRow.Visible = this.DisplayCoverLetter != Visibility.Hidden;
            this.CoverLetterFileRequiredValidator.Enabled = this.CoverLetterRequiredLabel.Visible = this.DisplayCoverLetter == Visibility.Required;

            this.SalaryRow.Visible = this.DisplaySalaryRequirement != Visibility.Hidden;
            this.SalaryMessageRow.Visible = this.DisplaySalaryRequirement == Visibility.Optional;
            this.SalaryRequiredTextBox.Enabled = this.SalaryRequiredLabel.Visible = this.DisplaySalaryRequirement == Visibility.Required;

            this.ApplicationMessageRow.Visible = this.DisplayMessage != Visibility.Hidden;
            this.ApplicationMessageRequiredValidator.Enabled = this.MessageRequiredLabel.Visible = this.DisplayMessage == Visibility.Required;
        }

        /// <summary>
        /// Transforms the given <paramref name="url"/> into an absolute URL.
        /// </summary>
        /// <param name="url">The URL to transform.</param>
        /// <returns>An absolute URL</returns>
        private string MakeUrlAbsolute(string url)
        {
            if (url.StartsWith("~", StringComparison.Ordinal))
            {
                url = this.ResolveUrl(url);
            }

            if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return url;
            }

            return new Uri(this.Request.Url, url).AbsoluteUri;
        }

        private void SendNotificationEmail(int resumeId)
        {
            try
            {
                Debug.Assert(this.CurrentJob != null, "this.CurrentJob must not be null when sending notification about a new application");
                string fromAddress = Engage.Utility.HasValue(this.UserInfo.Email) ? this.UserInfo.Email : this.DefaultNotificationEmailAddress;
                string toAddress = Engage.Utility.HasValue(this.CurrentJob.NotificationEmailAddress)
                                           ? this.CurrentJob.NotificationEmailAddress
                                           : this.DefaultNotificationEmailAddress;
                string subject = String.Format(
                        CultureInfo.CurrentCulture, 
                        Localization.GetString("ApplicationSubject", this.LocalResourceFile), 
                        this.UserInfo.DisplayName, 
                        this.CurrentJob.Title);
                string message = this.GetMessageBody(resumeId);
                Mail.SendMail(
                        fromAddress, 
                        toAddress, 
                        string.Empty, 
                        subject, 
                        message, 
                        string.Empty, 
                        "HTML", 
                        string.Empty, 
                        string.Empty, 
                        string.Empty, 
                        string.Empty);
            }
            catch (SmtpException exc)
            {
                this.EmailErrorLabel.Text = Localization.GetString("SmtpError", this.LocalResourceFile);
                Exceptions.LogException(exc);
            }
        }

        private void SendRecieptEmail()
        {
            if (Engage.Utility.IsLoggedIn)
            {
                string fromAddress = this.DefaultNotificationEmailAddress;
                string toAddress = this.UserInfo.Email;
                string subject = Localization.GetString("ApplicationAutoRespondSubject", this.LocalResourceFile);
                string message = this.GetMessageBody();
                Mail.SendMail(
                        fromAddress, 
                        toAddress, 
                        string.Empty, 
                        subject, 
                        message, 
                        string.Empty, 
                        "HTML", 
                        string.Empty, 
                        string.Empty, 
                        string.Empty, 
                        string.Empty);
            }
        }

        private void SetNextAction()
        {
            if (Engage.Utility.IsLoggedIn || !this.RequireRegistration)
            {
                if (Job.CurrentJobId != -1)
                {
                    if (Engage.Utility.IsLoggedIn && JobApplication.HasAppliedForJob(Job.CurrentJobId, this.UserId))
                    {
                        this.NextActionButton.Text = Localization.GetString("AlreadyApplied", this.LocalResourceFile);
                        this.NextActionButton.Enabled = false;
                    }
                    else if (this.CurrentJob.IsFilled)
                    {
                        this.NextActionButton.Text = Localization.GetString("JobIsFilled", this.LocalResourceFile);
                        this.NextActionButton.Enabled = false;
                    }
                    else if (Engage.Utility.IsLoggedIn && !Engage.Utility.ValidateEmailAddress(this.UserInfo.Email))
                    {
                        this.EmailErrorLabel.Visible = true;
                        this.NextActionButton.Enabled = false;
                    }
                    else if (!string.IsNullOrEmpty(this.CurrentJob.ApplicationUrl))
                    {
                        this.NextActionButton.Text = Localization.GetString("RemoteApply", this.LocalResourceFile);
                        this.NextActionButton.Click += this.NextActionButtonRedirect_Click;
                    }
                    else
                    {
                        this.NextActionButton.Text = Localization.GetString("Apply", this.LocalResourceFile);
                        this.NextActionButton.Click += this.NextActionButtonApply_Click;
                    }
                }
            }
            else
            {
                this.NextActionButton.Text = Localization.GetString("Register", this.LocalResourceFile);
                this.NextActionButton.Click += this.NextActionButtonLogOn_Click;
            }
        }

        /// <summary>
        /// Handles the Click event of the ApplyButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ApplyButton_Click(object sender, EventArgs e)
        {
            this.InitializeApplicantInfoSection();

            this.Page.Validate("apply");
            if (!this.Page.IsValid)
            {
                return;
            }

            JobApplication jobApplication = null;
            if (this.ApplicationId.HasValue)
            {
                jobApplication = JobApplication.Load(this.ApplicationId.Value);
            }

            if (Job.CurrentJobId != -1 && (this.UserId == -1 || (jobApplication != null && jobApplication.UserId == this.UserId) || !JobApplication.HasAppliedForJob(Job.CurrentJobId, this.UserId)))
            {
                string resumeFile = string.Empty;
                string resumeContentType = string.Empty;
                if (this.ResumeUpload.PostedFile != null)
                {
                    resumeFile = Path.GetFileName(this.ResumeUpload.PostedFile.FileName);
                    resumeContentType = this.ResumeUpload.PostedFile.ContentType;
                }

                string coverLetterFile = string.Empty;
                string coverLetterContentType = string.Empty;
                if (this.CoverLetterUpload.PostedFile != null)
                {
                    coverLetterFile = Path.GetFileName(this.CoverLetterUpload.PostedFile.FileName);
                    coverLetterContentType = this.CoverLetterUpload.PostedFile.ContentType;
                }

                int? leadId = null;
                int value;
                if (int.TryParse(this.LeadDropDownList.SelectedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                {
                    leadId = value;
                }

                int? userId = (this.UserId == -1) ? (int?)null : this.UserId;
                int resumeId = this.ApplicationId.HasValue
                                       ? JobApplication.UpdateApplication(
                                                 this.ApplicationId.Value, 
                                                 userId, 
                                                 resumeFile, 
                                                 resumeContentType, 
                                                 this.ResumeUpload.FileBytes, 
                                                 coverLetterFile, 
                                                 coverLetterContentType, 
                                                 this.CoverLetterUpload.FileBytes, 
                                                 this.SalaryTextBox.Text, 
                                                 this.ApplicationMessageTextBox.Text, 
                                                 leadId)
                                       : JobApplication.Apply(
                                                 Job.CurrentJobId, 
                                                 userId, 
                                                 resumeFile, 
                                                 resumeContentType, 
                                                 this.ResumeUpload.FileBytes, 
                                                 coverLetterFile, 
                                                 coverLetterContentType, 
                                                 this.CoverLetterUpload.FileBytes, 
                                                 this.SalaryTextBox.Text, 
                                                 this.ApplicationMessageTextBox.Text, 
                                                 leadId);

                this.SendNotificationEmail(resumeId);

                try
                {
                    this.SendRecieptEmail();
                }
                catch (SmtpException exc)
                {
                    this.EmailErrorLabel.Text = Localization.GetString("SmtpError", this.LocalResourceFile);
                    Exceptions.LogException(exc);
                    return;
                }
            }

            this.SuccessLabel.Visible = true;
            this.SuccessLabel.Text = Localization.GetString("ApplicationSent", this.LocalResourceFile);
            this.ApplicantInfoSection.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the BackButton control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void BackButton_Click(object source, EventArgs e)
        {
            this.Response.Redirect(Globals.NavigateURL(Utility.GetJobListingTabId(this.JobGroupId, this.PortalSettings) ?? this.TabId));
        }

        /// <summary>
        /// Handles the Click event of the EmailFriendButton control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void EmailFriendButton_Click(object source, EventArgs e)
        {
            this.EmailFriendSection.Visible = true;
            this.ApplicantInfoSection.Visible = false;
            this.FriendEmailRegexValidator.ValidationExpression = Engage.Utility.EmailsRegEx;
        }

        /// <summary>
        /// Handles the Click event of the NextActionButton control, displaying the application panel.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void NextActionButtonApply_Click(object sender, EventArgs e)
        {
            this.InitializeApplicantInfoSection();
        }

        /// <summary>
        /// Handles the Click event of the NextActionButton control, redirecting the user to the login.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void NextActionButtonLogOn_Click(object sender, EventArgs e)
        {
            this.Response.Redirect(Dnn.Utility.GetLoginUrl(this.PortalSettings, this.Request));
        }

        /// <summary>
        /// Handles the Click event of the NextActionButton control, redirecting the user to the job's <see cref="Job.ApplicationUrl"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void NextActionButtonRedirect_Click(object sender, EventArgs e)
        {
            this.Response.Redirect(this.CurrentJob.ApplicationUrl);
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
                    bool editingApplication = false;
                    if (this.ApplicationId.HasValue)
                    {
                        editingApplication = this.FillApplication();
                    }

                    int jobId;
                    if (int.TryParse(this.Request.QueryString["jobid"], NumberStyles.Integer, CultureInfo.InvariantCulture, out jobId))
                    {
                        Job.CurrentJobId = jobId;

                        if (!this.CurrentJob.IsActive && !editingApplication)
                        {
                            Job.CurrentJobId = Null.NullInteger;
                            this.CurrentJob = null;
                        }
                    }
                }

                this.FillLeadDropDown();
                this.SetNextAction();
                this.BackButton.Visible = Utility.GetJobListingTabId(this.JobGroupId, this.PortalSettings) != this.TabId;
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>
        /// Handles the Click event of the SendToFriendButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void SendToFriendButton_Click(object sender, EventArgs e)
        {
            this.Page.Validate("email");
            if (!this.Page.IsValid)
            {
                return;
            }

            if (Job.CurrentJobId != -1)
            {
                // send email to list
                string toAddress = this.SendToAddressTextBox.Text;
                string subject = String.Format(
                        CultureInfo.CurrentCulture, 
                        Localization.GetString("FriendEmailSubject", this.LocalResourceFile), 
                        this.FromNameTextBox.Text, 
                        this.PortalSettings.PortalName);
                string message = this.GetSendToAFriendMessageBody();

                try
                {
                    Mail.SendMail(
                            this.FriendEmailAddress, 
                            toAddress, 
                            string.Empty, 
                            subject, 
                            message, 
                            string.Empty, 
                            "HTML", 
                            string.Empty, 
                            string.Empty, 
                            string.Empty, 
                            string.Empty);
                }
                catch (SmtpException exc)
                {
                    this.EmailErrorLabel.Text = Localization.GetString("SmtpError", this.LocalResourceFile);
                    Exceptions.LogException(exc);
                    return;
                }
            }

            this.SuccessLabel.Visible = true;
            this.SuccessLabel.Text = Localization.GetString("EmailToFriendSent", this.LocalResourceFile);
            this.EmailFriendSection.Visible = false;
        }
    }
}