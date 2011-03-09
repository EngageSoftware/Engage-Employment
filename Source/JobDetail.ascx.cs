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
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using Data;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Mail;

    using MailPriority = DotNetNuke.Services.Mail.MailPriority;

    /// <summary>
    /// A control which displays detailed information about a job opening.  Allows users to email the job opening to a friend, and to apply for the job opening.
    /// </summary>
    public partial class JobDetail : ModuleBase, IActionable
    {
        /// <summary>
        /// Backing field for <see cref="CurrentJob"/>
        /// </summary>
        private Job currentJob;

        /// <summary>
        /// Gets the actions that this module performs.
        /// </summary>
        public ModuleActionCollection ModuleActions
        {
            get
            {
                return new ModuleActionCollection(new[]
                {
                    new ModuleAction(
                        this.GetNextActionID(), 
                        this.Localize("JobDetailOptions"), 
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

        /// <summary>
        /// Gets the ID of the application being edited (or <c>null</c> if no application is being edited).
        /// </summary>
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

        /// <summary>
        /// Gets or sets the current job being viewed.
        /// </summary>
        /// <value>
        /// The current job.
        /// </value>
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

        /// <summary>
        /// Gets the default notification email address for this instance of the module.
        /// </summary>
        private string DefaultNotificationEmailAddress
        {
            get { return ModuleSettings.JobDetailApplicationEmailAddress.GetValueAsStringFor(this) ?? PortalController.GetCurrentPortalSettings().Email; }
        }

        /// <summary>
        /// Gets the setting for whether to display the cover letter field.
        /// </summary>
        private Visibility DisplayCoverLetter
        {
            get { return ModuleSettings.JobDetailDisplayCoverLetter.GetValueAsEnumFor<Visibility>(this).Value; }
        }

        /// <summary>
        /// Gets the setting for whether to display the lead (How did you hear?) field.
        /// </summary>
        private Visibility DisplayLead
        {
            get { return ModuleSettings.JobDetailDisplayLead.GetValueAsEnumFor<Visibility>(this).Value; }
        }

        /// <summary>
        /// Gets the setting for whether to display the message field.
        /// </summary>
        private Visibility DisplayMessage
        {
            get { return ModuleSettings.JobDetailDisplayMessage.GetValueAsEnumFor<Visibility>(this).Value; }
        }

        /// <summary>
        /// Gets the setting for whether to display the salary requirement field.
        /// </summary>
        private Visibility DisplaySalaryRequirement
        {
            get { return ModuleSettings.JobDetailDisplaySalaryRequirement.GetValueAsEnumFor<Visibility>(this).Value; }
        }

        /// <summary>
        /// Gets the email address from which "send to a friend" email come.
        /// </summary>
        private string FriendEmailAddress
        {
            get { return ModuleSettings.JobDetailFriendEmailAddress.GetValueAsStringFor(this) ?? PortalSettings.Email; }
        }

        /// <summary>
        /// Gets a value indicating whether applying requires registration.
        /// </summary>
        /// <value>
        /// <c>true</c> if anonymous users can apply for jobs through this module; otherwise, <c>false</c>.
        /// </value>
        private bool RequireRegistration
        {
            get { return ModuleSettings.JobDetailRequireRegistration.GetValueAsBooleanFor(this).Value; }
        }

        /// <summary>
        /// Raises the <see cref="Control.Init"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
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
            var jobApplication = JobApplication.Load(this.ApplicationId.Value);
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
                        lnkDocument.NavigateUrl = Utility.GetDocumentUrl(document.DocumentId);
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Fills the <see cref="LeadDropDownList"/>.
        /// </summary>
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
                        this.LeadDropDownList.Items.Insert(0, new ListItem(this.Localize("ChooseLead"), string.Empty));
                    }
                }
                else
                {
                    this.LeadRow.Visible = false;
                }
            }
        }

        /// <summary>
        /// Gets the message body.
        /// </summary>
        /// <param name="resumeId">The resume id.</param>
        /// <param name="emailBodyResourceKey">The resource key of the email body.</param>
        /// <returns>A formatted email message body</returns>
        private string GetMessageBody(int resumeId, string emailBodyResourceKey)
        {
            var jobDetailModule = Utility.GetCurrentModuleByDefinition(this.PortalSettings, ModuleDefinition.JobDetail, this.JobGroupId);
            var salaryText = this.SalaryTextBox.Text;
            var messageText = this.ApplicationMessageTextBox.Text;
            var jobDetailsUrl = Globals.NavigateURL(
                jobDetailModule == null ? -1 : jobDetailModule.TabID, string.Empty, "jobId=" + Job.CurrentJobId.ToString(CultureInfo.InvariantCulture));

            if (string.IsNullOrEmpty(salaryText))
            {
                salaryText = this.Localize("EmailSalaryBlank");
            }

            if (string.IsNullOrEmpty(messageText))
            {
                messageText = this.Localize("EmailMessageBlank");
            }

            return string.Format(
                CultureInfo.CurrentCulture,
                this.Localize(emailBodyResourceKey),
                this.MakeUrlAbsolute(jobDetailsUrl),
                HttpUtility.HtmlEncode(this.Localize("ApplicationEmailLink")),
                HttpUtility.HtmlEncode(this.Localize("ApplicationEmailSalaryLabel")),
                HttpUtility.HtmlEncode(salaryText),
                this.MakeUrlAbsolute(Utility.GetDocumentUrl(resumeId)),
                HttpUtility.HtmlEncode(this.Localize("ApplicationEmailResumeLink")),
                HttpUtility.HtmlEncode(this.Localize("ApplicationEmailMessageLabel")),
                HttpUtility.HtmlEncode(messageText));
        }

        /// <summary>
        /// Gets the send to A friend message body.
        /// </summary>
        /// <returns>A formatted email message body</returns>
        private string GetSendToAFriendMessageBody()
        {
            var jobDetailModule = Utility.GetCurrentModuleByDefinition(this.PortalSettings, ModuleDefinition.JobDetail, this.JobGroupId);
            var jobDetailLink =
                this.MakeUrlAbsolute(
                    Globals.NavigateURL(
                        jobDetailModule == null ? -1 : jobDetailModule.TabID,
                        string.Empty,
                        "jobId=" + Job.CurrentJobId.ToString(CultureInfo.InvariantCulture)));
            var messageText = this.FriendEmailMessageTextBox.Text;
            
            if (string.IsNullOrEmpty(messageText))
            {
                messageText = this.Localize("FriendEmailMessageBlank");
            }

            return string.Format(
                CultureInfo.CurrentCulture,
                this.Localize("FriendEmailBody.Format"),
                jobDetailLink,
                HttpUtility.HtmlEncode(this.Localize("FriendEmailLink")),
                HttpUtility.HtmlEncode(this.Localize("ApplicationEmailMessageLabel")),
                HttpUtility.HtmlEncode(messageText));
        }

        /// <summary>
        /// Initializes the ApplicantInfoSection control, and hides the EmailFriendSection control.
        /// </summary>
        private void InitializeApplicantInfoSection()
        {
            this.ApplicantInfoSection.Visible = true;
            this.EmailFriendSection.Visible = false;

            this.BasePage.ScrollToControl(this.ApplicantInfoSection);

            this.FillLeadDropDown();

            this.ResumeFileRequiredValidator.Enabled = this.ResumeRequiredLabel.Visible = this.UserId == -1 || JobApplication.GetResumeId(this.UserId) == -1;
            this.ResumeMessageRow.Visible = this.UserId != -1 && JobApplication.GetResumeId(this.UserId) != -1;

            string fileExtensionsList = Host.FileExtensions ?? string.Empty;
            string fileExtensionValidationExpression = BuildFileExtensionValidationExpression(fileExtensionsList);
            this.ResumeFileExtensionValidator.ValidationExpression = this.CoverLetterFileExtensionValidator.ValidationExpression = fileExtensionValidationExpression;
            this.ResumeFileExtensionValidator.ErrorMessage = string.Format(CultureInfo.CurrentCulture, this.Localize("regexResumeFile.Text"), fileExtensionsList);
            this.CoverLetterFileExtensionValidator.ErrorMessage = string.Format(CultureInfo.CurrentCulture, this.Localize("regexCoverLetterFile.Text"), fileExtensionsList);

            this.ApplicationMessageRow.Visible = this.DisplayMessage != Visibility.Hidden;
            this.ApplicationMessageRequiredValidator.Enabled = this.MessageRequiredLabel.Visible = this.DisplayMessage == Visibility.Required;

            this.CoverLetterRow.Visible = this.DisplayCoverLetter != Visibility.Hidden;
            this.CoverLetterFileRequiredValidator.Enabled = this.CoverLetterRequiredLabel.Visible = this.DisplayCoverLetter == Visibility.Required;

            this.SalaryRow.Visible = this.DisplaySalaryRequirement != Visibility.Hidden;
            this.SalaryMessageRow.Visible = this.DisplaySalaryRequirement == Visibility.Optional;
            this.SalaryRequiredTextBox.Enabled = this.SalaryRequiredLabel.Visible = this.DisplaySalaryRequirement == Visibility.Required;

            var firstVisibleInputControl = this.ApplicationMessageTextBox.Visible
                                               ? this.ApplicationMessageTextBox
                                               : this.SalaryTextBox.Visible
                                                     ? this.SalaryTextBox
                                                     : this.LeadDropDownList.Visible
                                                           ? this.LeadDropDownList
                                                           : this.CoverLetterUpload.Visible 
                                                                ? (Control)this.CoverLetterUpload 
                                                                : this.ResumeUpload;
            firstVisibleInputControl.Focus();
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

        /// <summary>
        /// Sends an notifcation email about a new application.
        /// </summary>
        /// <param name="resumeId">The ID of the resumé.</param>
        /// <param name="isNewApplication">if set to <c>true</c> it's a new application, otherwise it's an application edit.</param>
        /// <param name="toAddress">The email address to which the notification should be sent.</param>
        /// <param name="replyToApplicant">if set to <c>true</c> sets the reply-to to the applicant's email address (if they're logged in), otherwise leaves it as the "from" address.</param>
        /// <param name="newSubjectResourceKey">The resource key to use to retreive the localized email subject for new applications.</param>
        /// <param name="updateSubjectResourceKey">The resource key to use to retreive the localized email subject for updated applications.</param>
        /// <param name="messageResourceKey">The resource key to use to retrieve the localized email message (with format placeholders).</param>
        private void SendNotificationEmail(int resumeId, bool isNewApplication, string toAddress, bool replyToApplicant, string newSubjectResourceKey, string updateSubjectResourceKey, string messageResourceKey)
        {
            try
            {
                var fromAddress = this.DefaultNotificationEmailAddress;
                var replyTo = replyToApplicant
                                  ? Engage.Utility.HasValue(this.UserInfo.Email) ? this.UserInfo.Email : fromAddress
                                  : this.DefaultNotificationEmailAddress;
                var subject = string.Format(
                    CultureInfo.CurrentCulture,
                    this.Localize(isNewApplication ? newSubjectResourceKey : updateSubjectResourceKey),
                    this.UserInfo.DisplayName,
                    this.CurrentJob.Title);
                var message = this.GetMessageBody(resumeId, messageResourceKey);

                Mail.SendMail(
                        fromAddress, 
                        toAddress, 
                        string.Empty,
                        string.Empty,
                        replyTo,
                        MailPriority.Normal, 
                        subject,
                        MailFormat.Html, 
                        Encoding.UTF8,
                        message,
                        new string[0],
                        Host.SMTPServer,
                        Host.SMTPAuthentication, 
                        Host.SMTPUsername, 
                        Host.SMTPPassword,
                        Host.EnableSMTPSSL);
            }
            catch (SmtpException exc)
            {
                this.EmailErrorLabel.Text = this.Localize("SmtpError");
                Exceptions.LogException(exc);
            }
        }

        /// <summary>
        /// Determines what happens when the <see cref="NextActionButton"/> is clicked (or whether it can be clicked)
        /// </summary>
        private void SetNextAction()
        {
            if (Engage.Utility.IsLoggedIn || !this.RequireRegistration)
            {
                if (Job.CurrentJobId != -1)
                {
                    if (Engage.Utility.IsLoggedIn && JobApplication.HasAppliedForJob(Job.CurrentJobId, this.UserId))
                    {
                        this.NextActionButton.Text = this.Localize("AlreadyApplied");
                        this.NextActionButton.Enabled = false;
                    }
                    else if (this.CurrentJob.IsFilled)
                    {
                        this.NextActionButton.Text = this.Localize("JobIsFilled");
                        this.NextActionButton.Enabled = false;
                    }
                    else if (Engage.Utility.IsLoggedIn && !Engage.Utility.ValidateEmailAddress(this.UserInfo.Email))
                    {
                        this.EmailErrorLabel.Visible = true;
                        this.NextActionButton.Enabled = false;
                    }
                    else if (!string.IsNullOrEmpty(this.CurrentJob.ApplicationUrl))
                    {
                        this.NextActionButton.Text = this.Localize("RemoteApply");
                        this.NextActionButton.Click += this.NextActionButtonRedirect_Click;
                    }
                    else
                    {
                        this.NextActionButton.Text = this.Localize("Apply");
                        this.NextActionButton.Click += this.NextActionButtonApply_Click;
                    }
                }
            }
            else
            {
                this.NextActionButton.Text = this.Localize("Register");
                this.NextActionButton.Click += this.NextActionButtonLogOn_Click;
            }
        }

        /// <summary>
        /// Handles the <see cref="LinkButton.Click"/> event of the <see cref="ApplyButton"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
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

            bool isNewApplication = jobApplication == null;
            if (Job.CurrentJobId != -1 && (this.UserId == -1 || (!isNewApplication && jobApplication.UserId == this.UserId) || !JobApplication.HasAppliedForJob(Job.CurrentJobId, this.UserId)))
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
                int resumeId = isNewApplication
                                   ? JobApplication.Apply(
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
                                       leadId)
                                   : JobApplication.UpdateApplication(
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
                                       leadId);

                Debug.Assert(this.CurrentJob != null, "this.CurrentJob must not be null when sending notification about a new application");
                string notificationEmailAddress = Engage.Utility.HasValue(this.CurrentJob.NotificationEmailAddress)
                                                      ? this.CurrentJob.NotificationEmailAddress
                                                      : this.DefaultNotificationEmailAddress;
                this.SendNotificationEmail(
                    resumeId,
                    isNewApplication,
                    notificationEmailAddress, 
                    true,
                    "ApplicationSubject",
                    "ApplicationUpdateSubject",
                    "NotificationEmailBody.Format");

                if (IsLoggedIn)
                {
                    this.SendNotificationEmail(
                        resumeId,
                        isNewApplication,
                        this.UserInfo.Email, 
                        false,
                        "ApplicationAutoRespondSubject",
                        "ApplicationUpdateAutoRespondSubject",
                        "ReceiptEmailBody.Format");
                }
            }

            this.SuccessLabel.Visible = true;
            this.SuccessLabel.Text = this.Localize("ApplicationSent");
            this.ApplicantInfoSection.Visible = false;
        }

        /// <summary>
        /// Handles the <see cref="LinkButton.Click"/> event of the <see cref="BackButton"/> control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void BackButton_Click(object source, EventArgs e)
        {
            this.Response.Redirect(Globals.NavigateURL(Utility.GetJobListingTabId(this.JobGroupId, this.PortalSettings) ?? this.TabId));
        }

        /// <summary>
        /// Handles the <see cref="LinkButton.Click"/> event of the <see cref="EmailFriendButton"/> control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void EmailFriendButton_Click(object source, EventArgs e)
        {
            this.EmailFriendSection.Visible = true;
            this.ApplicantInfoSection.Visible = false;
            this.FriendFromEmailRegexValidator.ValidationExpression = Engage.Utility.EmailRegEx;
            this.FriendEmailRegexValidator.ValidationExpression = Engage.Utility.EmailsRegEx;

            this.FromNameTextBox.Text = this.UserInfo.DisplayName;
            this.FromAddressTextBox.Text = this.UserInfo.Email;
        }

        /// <summary>
        /// Handles the <see cref="LinkButton.Click"/> event of the <see cref="NextActionButton"/> control, displaying the application panel.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NextActionButtonApply_Click(object sender, EventArgs e)
        {
            this.InitializeApplicantInfoSection();
        }

        /// <summary>
        /// Handles the <see cref="LinkButton.Click"/> event of the <see cref="NextActionButton"/> control, redirecting the user to the login.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NextActionButtonLogOn_Click(object sender, EventArgs e)
        {
            this.Response.Redirect(Dnn.Utility.GetLoginUrl(this.PortalSettings, this.Request));
        }

        /// <summary>
        /// Handles the <see cref="LinkButton.Click"/> event of the <see cref="NextActionButton"/> control, redirecting the user to the job's <see cref="Job.ApplicationUrl"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NextActionButtonRedirect_Click(object sender, EventArgs e)
        {
            this.Response.Redirect(this.CurrentJob.ApplicationUrl);
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
        /// Handles the <see cref="LinkButton.Click"/> event of the <see cref="SendToFriendButton"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
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
                var toAddress = this.SendToAddressTextBox.Text;
                var fromAddress = !string.IsNullOrEmpty(this.FromAddressTextBox.Text) 
                    ? this.FromAddressTextBox.Text 
                    : this.FriendEmailAddress;
                var subject = string.Format(
                    CultureInfo.CurrentCulture, 
                    this.Localize("FriendEmailSubject"), 
                    this.FromNameTextBox.Text, 
                    this.PortalSettings.PortalName);
                var message = this.GetSendToAFriendMessageBody();

                try
                {
                    Mail.SendMail(
                            this.FriendEmailAddress, 
                            toAddress, 
                            string.Empty,
                            string.Empty,
                            fromAddress,
                            MailPriority.Normal, 
                            subject, 
                            MailFormat.Html, 
                            Encoding.UTF8,
                            message, 
                            new string[] { }, 
                            Host.SMTPServer, 
                            Host.SMTPAuthentication, 
                            Host.SMTPUsername, 
                            Host.SMTPPassword,
                            Host.EnableSMTPSSL);
                }
                catch (SmtpException exc)
                {
                    this.EmailErrorLabel.Text = this.Localize("SmtpError");
                    Exceptions.LogException(exc);
                    return;
                }
            }

            this.SuccessLabel.Visible = true;
            this.SuccessLabel.Text = this.Localize("EmailToFriendSent");
            this.EmailFriendSection.Visible = false;
        }
    }
}