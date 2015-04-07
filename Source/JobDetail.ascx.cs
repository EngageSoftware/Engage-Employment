// <copyright file="JobDetail.ascx.cs" company="Engage Software">
// Engage: Employment
// Copyright (c) 2004-2014
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
    using System.Linq;
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

    using Engage.Annotations;

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
        /// Backing field for <see cref="JobApplication"/>
        /// </summary>
        private JobApplication jobApplication;

        /// <summary>
        /// Backing field for <see cref="JobId"/>
        /// </summary>
        private int? jobId;

        /// <summary>
        /// Backing field for <see cref="ShowCloseDate"/>
        /// </summary>
        private bool? showCloseDate;

        /// <summary>
        /// Gets the actions that this module performs.
        /// </summary>
        public ModuleActionCollection ModuleActions
        {
            get
            {
                var actions = new ModuleActionCollection();
                if (PermissionController.CanManageJobDetailOptions(this))
                {
                    actions.Add(new ModuleAction(
                        this.GetNextActionID(), 
                        this.Localize("JobDetailOptions"), 
                        ModuleActionType.ContentOptions, 
                        string.Empty, 
                        string.Empty, 
                        this.EditUrl(ControlKey.Options.ToString()), 
                        string.Empty, 
                        false, 
                        SecurityAccessLevel.View, 
                        true, 
                        false));
                }

                return actions;
            }
        }

        /// <summary>
        /// Gets a value indicating whether to display the job's close date.
        /// </summary>
        /// <value>
        /// <c>true</c> if the close date is shown; otherwise, <c>false</c>.
        /// </value>
        protected bool ShowCloseDate
        {
            get
            {
                if (!this.showCloseDate.HasValue)
                {
                    this.showCloseDate = ModuleSettings.JobDetailShowCloseDate.GetValueAsBooleanFor(this).Value;
                }

                return this.showCloseDate.Value;
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
        /// Gets the current job being viewed.
        /// </summary>
        /// <value>
        /// The current job.
        /// </value>
        protected Job CurrentJob
        {
            get
            {
                if (this.currentJob == null && this.JobId.HasValue)
                {
                    if (!this.JobGroupId.HasValue || DataProvider.Instance().IsJobInGroup(this.JobId.Value, this.JobGroupId.Value))
                    {
                        this.currentJob = Job.Load(this.JobId.Value);
                    }

                    if (this.currentJob == null)
                    {
                        this.currentJob = Job.CreateJob();
                    }
                }

                return this.currentJob;
            }
        }

        /// <summary>Gets the job application being edited.</summary>
        /// <value>The job application, or <c>null</c>.</value>
        private JobApplication JobApplication
        {
            get
            {
                if (this.jobApplication == null && this.ApplicationId != null)
                {
                    this.jobApplication = JobApplication.Load(this.ApplicationId.Value);
                }

                return this.jobApplication;
            }
        }

        /// <summary>
        /// Gets the ID of the application being edited (or <c>null</c> if no application is being edited).
        /// </summary>
        private int? JobId
        {
            get
            {
                if (!this.jobId.HasValue)
                {
                    int id;
                    if (int.TryParse(this.Request.QueryString["jobId"], NumberStyles.Integer, CultureInfo.InvariantCulture, out id))
                    {
                        this.jobId = id;
                    }
                }

                return this.jobId;
            }
        }

        /// <summary>
        /// Gets list of default email addresses to which notifications should be sent, when the job does not have any defined, for this instance of the module.
        /// </summary>
        private string DefaultNotificationEmailAddresses
        {
            get { return ModuleSettings.JobDetailApplicationEmailAddresses.GetValueAsStringFor(this) ?? PortalController.GetCurrentPortalSettings().Email; }
        }

        /// <summary>
        /// Gets the setting for whether to display the name field.
        /// </summary>
        private Visibility DisplayName
        {
            get { return ModuleSettings.JobDetailDisplayName.GetValueAsEnumFor<Visibility>(this).Value; }
        }

        /// <summary>
        /// Gets the setting for whether to display the email field.
        /// </summary>
        private Visibility DisplayEmail
        {
            get { return ModuleSettings.JobDetailDisplayEmail.GetValueAsEnumFor<Visibility>(this).Value; }
        }

        /// <summary>
        /// Gets the setting for whether to display the phone field.
        /// </summary>
        private Visibility DisplayPhone
        {
            get { return ModuleSettings.JobDetailDisplayPhone.GetValueAsEnumFor<Visibility>(this).Value; }
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
        /// Gets the setting for whether to display the cover letter field.
        /// </summary>
        private Visibility DisplayCoverLetter
        {
            get { return ModuleSettings.JobDetailDisplayCoverLetter.GetValueAsEnumFor<Visibility>(this).Value; }
        }

        /// <summary>
        /// Gets the setting for whether to display the resume field.
        /// </summary>
        private Visibility DisplayResume
        {
            get { return ModuleSettings.JobDetailDisplayResume.GetValueAsEnumFor<Visibility>(this).Value; }
        }

        /// <summary>
        /// Gets the setting for whether to display the lead (How did you hear?) field.
        /// </summary>
        private Visibility DisplayLead
        {
            get { return ModuleSettings.JobDetailDisplayLead.GetValueAsEnumFor<Visibility>(this).Value; }
        }

        /// <summary>
        /// Gets the email address from which "send to a friend" and notification emails come.
        /// </summary>
        private string FromEmailAddress
        {
            get { return ModuleSettings.JobDetailFromEmailAddress.GetValueAsStringFor(this) ?? PortalSettings.Email; }
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
            if (this.JobId == null)
            {
                this.Visible = false;
            }

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
        ///  A description of the regular expression: <c>.*\.(?:[Pp][Dd][Ff]|[Dd][Oo][Cc][Xx]|[Ee][Tt][Cc])$</c>
        ///  <c>.*\.</c>
        ///      Any character, any number of repetitions
        ///      Literal <c>.</c>
        ///  Match expression but don't capture it. <c>[Pp][Dd][Ff]|[Dd][Oo][Cc][Xx]|[Ee][Tt][Cc]</c>
        ///      Select from alternatives
        ///          <c>[Pp][Dd][Ff]</c>
        ///              Any character in this class: <c>[Pp]</c>
        ///              Any character in this class: <c>[Dd]</c>
        ///              Any character in this class: <c>[Ff]</c>
        ///          <c>[Dd][Oo][Cc][Xx]</c>
        ///              Any character in this class: <c>[Dd]</c>
        ///              Any character in this class: <c>[Oo]</c>
        ///              Any character in this class: <c>[Cc]</c>
        ///              Any character in this class: <c>[Xx]</c>
        ///          <c>[Ee][Tt][Cc]</c>
        ///              Any character in this class: <c>[Ee]</c>
        ///              Any character in this class: <c>[Tt]</c>
        ///              Any character in this class: <c>[Cc]</c>     
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

        /// <summary>Sets the field visibility.</summary>
        /// <param name="fieldRow">The field row.</param>
        /// <param name="requiredFieldValidator">The required field validator.</param>
        /// <param name="requiredLabel">The required label.</param>
        /// <param name="displayField">The display field.</param>
        private static void SetFieldVisibility(Control fieldRow, WebControl requiredFieldValidator, Control requiredLabel, Visibility displayField)
        {
            fieldRow.Visible = displayField != Visibility.Hidden;
            requiredFieldValidator.Enabled = requiredLabel.Visible = displayField == Visibility.Required;
        }

        /// <summary>Gets the <see cref="TextBox.Text"/> if the <paramref name="textBox"/> is <see cref="Control.Visible"/>.</summary>
        /// <param name="textBox">The text box from which to get the text.</param>
        /// <returns>The text, or <c>null</c>.</returns>
        private static string GetTextIfVisible(TextBox textBox)
        {
            return textBox.Visible ? textBox.Text : null;
        }

        /// <summary>Gets the document.</summary>
        /// <param name="documentId">The ID of the document, or <c>null</c>.</param>
        /// <param name="documentType">Type of the document.</param>
        /// <param name="documentFileName">File name of the document.</param>
        /// <param name="documentContentType">Content type of the document.</param>
        /// <param name="documentBytes">The document bytes.</param>
        /// <returns>A <see cref="Document" /> instance, or <c>null</c>.</returns>
        [CanBeNull]
        private static Document GetDocument(int? documentId, DocumentType documentType, string documentFileName, string documentContentType, byte[] documentBytes)
        {
            if (documentId == null)
            {
                return null;
            }

            if (documentBytes.Length != 0)
            {
                return new Document(documentId.Value, documentType, documentFileName, documentContentType, documentBytes);
            }

            using (var documentReader = JobApplication.GetDocument(documentId.Value))
            {
                if (documentReader.Read())
                {
                    return new Document(
                        documentId.Value,
                        documentType,
                        (string)documentReader["filename"],
                        (string)documentReader["ContentType"],
                        (byte[])documentReader["ResumeData"]);
                }
                
                return new Document(documentId.Value, documentType, null, null, null);
            }
        }

        /// <summary>Shows the document link.</summary>
        /// <param name="document">The document.</param>
        /// <param name="documentLinkWrap">The control around the <paramref name="documentLink"/>.</param>
        /// <param name="documentLink">The document link.</param>
        private static void ShowDocumentLink(Document document, Control documentLinkWrap, HyperLink documentLink)
        {
            documentLinkWrap.Visible = true;
            documentLink.Text = document.FileName;
            documentLink.NavigateUrl = Utility.GetDocumentUrl(document.DocumentId);
        }

        /// <summary>
        /// Fills in the information about the application, if one is specified and it belongs to this user.
        /// </summary>
        /// <returns>Whether the specified application was filled in</returns>
        private bool FillApplication()
        {
            if (!Null.IsNull(this.UserId) && this.UserId == this.JobApplication.UserId)
            {
                var documents = this.JobApplication.GetDocuments();
                var properties = this.JobApplication.GetApplicationProperties();
                this.InitializeApplicantInfoSection();

                this.ApplicantNameTextBox.Text = this.JobApplication.ApplicantName;
                this.ApplicantEmailTextBox.Text = this.JobApplication.ApplicantEmail;
                this.ApplicantPhoneTextBox.Text = this.JobApplication.ApplicantPhone;
                this.ApplicationMessageTextBox.Text = this.JobApplication.Message;
                this.SalaryTextBox.Text = this.JobApplication.SalaryRequirement;

                if (this.LeadDropDownList.Items.Count < 1)
                {
                    this.FillLeadDropDown();
                }

                foreach (var pair in properties)
                {
                    if (pair.Key.Equals(ApplicationPropertyDefinition.Lead.GetName(), StringComparison.Ordinal))
                    {
                        this.LeadDropDownList.SelectedValue = pair.Value;
                        break;
                    }
                }

                foreach (var document in documents)
                {
                    if (document.DocumentTypeId == DocumentType.Resume.GetId())
                    {
                        ShowDocumentLink(document, this.ResumeLinkPanel, this.ResumeLink);
                    }
                    else if (document.DocumentTypeId == DocumentType.CoverLetter.GetId())
                    {
                        ShowDocumentLink(document, this.CoverLetterLinkPanel, this.CoverLetterLink);
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

            if (!this.LeadRow.Visible || this.LeadDropDownList.Items.Count >= 1)
            {
                return;
            }

            var leadList = new ListController().GetListEntryInfoCollection(Utility.LeadListName, Null.NullString, this.PortalId);
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

        /// <summary>
        /// Gets the message body.
        /// </summary>
        /// <param name="resumeId">The resume id.</param>
        /// <param name="emailBodyResourceKey">The resource key of the email body.</param>
        /// <returns>A formatted email message body</returns>
        private string GetMessageBody(int? resumeId, string emailBodyResourceKey)
        {
            var jobUrl = Globals.NavigateURL(this.TabId, string.Empty, "jobId=" + this.JobId.Value.ToString(CultureInfo.InvariantCulture));
            var salaryText = !string.IsNullOrEmpty(this.SalaryTextBox.Text) 
                                ? this.SalaryTextBox.Text 
                                : this.Localize("EmailSalaryBlank");
            var messageText = !string.IsNullOrEmpty(this.ApplicationMessageTextBox.Text)
                                ? this.ApplicationMessageTextBox.Text
                                : this.Localize("EmailMessageBlank");
            var displayNameText = this.GetTextWithFallback(this.ApplicantNameTextBox, this.UserInfo.DisplayName, "DisplayNameBlank");
            var emailText = this.GetTextWithFallback(this.ApplicantEmailTextBox, this.UserInfo.Email, "EmailBlank");
            var phoneText = this.GetTextWithFallback(this.ApplicantPhoneTextBox, this.UserInfo.Profile.Telephone, "PhoneBlank");
            var usernameText = !string.IsNullOrEmpty(this.UserInfo.Username)
                                    ? this.UserInfo.Username
                                    : this.Localize("UsernameBlank");
            return string.Format(
                CultureInfo.CurrentCulture,
                this.Localize(emailBodyResourceKey),
                HttpUtility.HtmlEncode(Engage.Utility.MakeUrlAbsolute(this.Page, jobUrl)),
                HttpUtility.HtmlEncode(this.Localize("ApplicationEmailLink")),
                HttpUtility.HtmlEncode(this.Localize("ApplicationEmailSalaryLabel")),
                HttpUtility.HtmlEncode(salaryText),
                resumeId.HasValue ? HttpUtility.HtmlEncode(Engage.Utility.MakeUrlAbsolute(this.Page, Utility.GetDocumentUrl(resumeId.Value))) : null,
                HttpUtility.HtmlEncode(this.Localize("ApplicationEmailResumeLink")),
                HttpUtility.HtmlEncode(this.Localize("ApplicationEmailMessageLabel")),
                HttpUtility.HtmlEncode(messageText),
                HttpUtility.HtmlEncode(displayNameText),
                HttpUtility.HtmlEncode(usernameText),
                HttpUtility.HtmlEncode(emailText),
                HttpUtility.HtmlEncode(phoneText));
        }

        /// <summary>Gets the text with a fallback to a user property.</summary>
        /// <param name="textBox">The text box.</param>
        /// <param name="fallbackValue">The fallback value.</param>
        /// <param name="noValueResourceKey">The resource key for the localized text if no value is available.</param>
        /// <returns>The text representing the value.</returns>
        private string GetTextWithFallback(TextBox textBox, string fallbackValue, string noValueResourceKey = "")
        {
            // fallback to profile if the field isn't visible
            var textValue = GetTextIfVisible(textBox) ?? fallbackValue;

            // if they didn't provide a value, show "the applicant didn't provide a value" message
            return !string.IsNullOrEmpty(textValue) ? textValue : this.Localize(noValueResourceKey);
        }

        /// <summary>
        /// Gets the send to A friend message body.
        /// </summary>
        /// <returns>A formatted email message body</returns>
        private string GetSendToAFriendMessageBody()
        {
            var jobUrl = Globals.NavigateURL(this.TabId, string.Empty, "jobId=" + this.JobId.Value.ToString(CultureInfo.InvariantCulture));
            var messageText = !string.IsNullOrEmpty(this.FriendEmailMessageTextBox.Text)
                                  ? HttpUtility.HtmlEncode(this.FriendEmailMessageTextBox.Text).Replace(Environment.NewLine, "<br />")
                                  : this.Localize("FriendEmailMessageBlank");
            
            return string.Format(
                CultureInfo.CurrentCulture,
                this.Localize("FriendEmailBody.Format"),
                Engage.Utility.MakeUrlAbsolute(this.Page, jobUrl),
                HttpUtility.HtmlEncode(this.Localize("FriendEmailLink")),
                HttpUtility.HtmlEncode(this.Localize("ApplicationEmailMessageLabel")),
                messageText);
        }

        /// <summary>
        /// Sends the user back to the Job Listing page.
        /// </summary>
        private void GoBack()
        {
            this.Response.Redirect(Globals.NavigateURL(Utility.GetJobListingTabId(this.JobGroupId, this.PortalSettings) ?? this.TabId));
        }

        /// <summary>Initializes the ApplicantInfoSection control, and hides the EmailFriendSection control.</summary>
        /// <param name="setInitialInfo">if set to <c>true</c> show user info in the user text boxes; otherwise, leave them alone.</param>
        private void InitializeApplicantInfoSection(bool setInitialInfo = false)
        {
            this.ApplicantInfoSection.Visible = true;
            this.EmailFriendSection.Visible = false;

            this.BasePage.ScrollToControl(this.ApplicantInfoSection);

            this.FillLeadDropDown();

            string fileExtensionsList = Host.FileExtensions ?? string.Empty;
            string fileExtensionValidationExpression = BuildFileExtensionValidationExpression(fileExtensionsList);
            this.ResumeFileExtensionValidator.ValidationExpression = this.CoverLetterFileExtensionValidator.ValidationExpression = fileExtensionValidationExpression;
            this.ResumeFileExtensionValidator.ErrorMessage = string.Format(CultureInfo.CurrentCulture, this.Localize("regexResumeFile.Text"), fileExtensionsList);
            this.CoverLetterFileExtensionValidator.ErrorMessage = string.Format(CultureInfo.CurrentCulture, this.Localize("regexCoverLetterFile.Text"), fileExtensionsList);

            SetFieldVisibility(this.ApplicantNameRow, this.ApplicantNameRequiredValidator, this.ApplicantNameRequiredLabel, this.DisplayName);
            SetFieldVisibility(this.ApplicantEmailRow, this.ApplicantEmailRequiredValidator, this.ApplicantEmailRequiredLabel, this.DisplayEmail);
            SetFieldVisibility(this.ApplicantPhoneRow, this.ApplicantPhoneRequiredValidator, this.ApplicantPhoneRequiredLabel, this.DisplayPhone);
            SetFieldVisibility(this.ApplicationMessageRow, this.ApplicationMessageRequiredValidator, this.MessageRequiredLabel, this.DisplayMessage);
            SetFieldVisibility(this.SalaryRow, this.SalaryRequiredFieldValidator, this.SalaryRequiredLabel, this.DisplaySalaryRequirement);
            this.SalaryMessageRow.Visible = this.DisplaySalaryRequirement == Visibility.Optional;
            this.ApplicantEmailRegexValidator.ValidationExpression = Engage.Utility.EmailRegEx;

            var alreadyHasCoverLetter = this.JobApplication != null && this.JobApplication.GetDocuments().Any(d => d.DocumentTypeId == DocumentType.CoverLetter.GetId());
            var coverLetterVisibility = this.DisplayCoverLetter == Visibility.Required && alreadyHasCoverLetter ? Visibility.Optional : this.DisplayCoverLetter;
            SetFieldVisibility(this.CoverLetterRow, this.CoverLetterFileRequiredValidator, this.CoverLetterRequiredLabel, coverLetterVisibility);

            var alreadyHasResume = this.UserId != -1 && JobApplication.GetResumeId(this.UserId) != null;
            var resumeVisibility = this.DisplayResume == Visibility.Required && alreadyHasResume ? Visibility.Optional : this.DisplayResume;
            SetFieldVisibility(this.ResumeRow, this.ResumeFileRequiredValidator, this.ResumeRequiredLabel, resumeVisibility);
            this.ResumeMessageRow.Visible = alreadyHasResume;

            if (setInitialInfo)
            {
                this.ApplicantNameTextBox.Text = this.UserInfo.DisplayName;
                this.ApplicantEmailTextBox.Text = this.UserInfo.Email;
                this.ApplicantPhoneTextBox.Text = this.UserInfo.Profile.Telephone;
            }
        }

        /// <summary>
        /// Sends an notification email about a new application.
        /// </summary>
        /// <param name="resumeId">The ID of the resume.</param>
        /// <param name="isNewApplication">if set to <c>true</c> it's a new application, otherwise it's an application edit.</param>
        /// <param name="toAddress">The email address to which the notification should be sent.</param>
        /// <param name="replyToApplicant">if set to <c>true</c> sets the reply-to to the applicant's email address (if they're logged in), otherwise leaves it as the "from" address.</param>
        /// <param name="newSubjectResourceKey">The resource key to use to retrieve the localized email subject for new applications.</param>
        /// <param name="updateSubjectResourceKey">The resource key to use to retrieve the localized email subject for updated applications.</param>
        /// <param name="messageResourceKey">The resource key to use to retrieve the localized email message (with format placeholders).</param>
        private void SendNotificationEmail(int? resumeId, bool isNewApplication, string toAddress, bool replyToApplicant, string newSubjectResourceKey, string updateSubjectResourceKey, string messageResourceKey)
        {
            this.SendNotificationEmail(resumeId != null ? new Document(resumeId.Value, DocumentType.Resume, null, null, null) : null, null, isNewApplication, toAddress, replyToApplicant, newSubjectResourceKey, newSubjectResourceKey, updateSubjectResourceKey, messageResourceKey);
        }

        /// <summary>Sends an notification email about a new application.</summary>
        /// <param name="resume">The resume document</param>
        /// <param name="coverLetter">The cover letter document, or <c>null</c>.</param>
        /// <param name="isNewApplication">if set to <c>true</c> it's a new application, otherwise it's an application edit.</param>
        /// <param name="toAddress">The email address to which the notification should be sent.</param>
        /// <param name="replyToApplicant">if set to <c>true</c> sets the reply-to to the applicant's email address (if they're logged in), otherwise leaves it as the "from" address.</param>
        /// <param name="newSubjectResourceKey">The resource key to use to retrieve the localized email subject for new applications.</param>
        /// <param name="newAnonymousSubjectResourceKey">The resource key to use to retrieve the localized email subject for new applications from users who aren't logged in.</param>
        /// <param name="updateSubjectResourceKey">The resource key to use to retrieve the localized email subject for updated applications.</param>
        /// <param name="messageResourceKey">The resource key to use to retrieve the localized email message (with format placeholders).</param>
        private void SendNotificationEmail(Document resume, Document coverLetter, bool isNewApplication, string toAddress, bool replyToApplicant, string newSubjectResourceKey, string newAnonymousSubjectResourceKey, string updateSubjectResourceKey, string messageResourceKey)
        {
            try
            {
                var fromAddress = this.FromEmailAddress;
                var replyTo = replyToApplicant
                                  ? this.GetTextWithFallback(this.ApplicantEmailTextBox, this.UserInfo.Email) ?? fromAddress
                                  : fromAddress;

                var subjectResourceKey = isNewApplication
                                             ? Framework.ModuleBase.IsLoggedIn
                                                   ? newSubjectResourceKey
                                                   : newAnonymousSubjectResourceKey
                                             : updateSubjectResourceKey;
                var subject = string.Format(
                    CultureInfo.CurrentCulture,
                    this.Localize(subjectResourceKey),
                    this.GetTextWithFallback(this.ApplicantNameTextBox, this.UserInfo.DisplayName, "DisplayNameBlank"),
                    this.CurrentJob.Title,
                    this.GetTextWithFallback(this.ApplicantEmailTextBox, this.UserInfo.Email, "EmailBlank"));

                MemoryStream resumeStream = null;
                MemoryStream coverLetterStream = null;
                var attachments = new List<Attachment>(2);
                try
                {
                    if (resume != null && resume.FileData != null && resume.FileData.Length > 0)
                    {
                        resumeStream = new MemoryStream(resume.FileData);
                        attachments.Add(new Attachment(resumeStream, resume.FileName, resume.ContentType));
                    }

                    if (coverLetter != null && coverLetter.FileData != null && coverLetter.FileData.Length > 0)
                    {
                        coverLetterStream = new MemoryStream(coverLetter.FileData);
                        attachments.Add(new Attachment(coverLetterStream, coverLetter.FileName, coverLetter.ContentType));
                    }

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
                        this.GetMessageBody(resume != null ? resume.DocumentId : (int?)null, messageResourceKey),
                        attachments,
                        Host.SMTPServer,
                        Host.SMTPAuthentication, 
                        Host.SMTPUsername, 
                        Host.SMTPPassword,
                        Host.EnableSMTPSSL);
                }
                finally
                {
                    if (resumeStream != null)
                    {
                        resumeStream.Dispose();
                    }

                    if (coverLetterStream != null)
                    {
                        coverLetterStream.Dispose();
                    }
                }
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
                if (this.JobId != null)
                {
                    if (Engage.Utility.IsLoggedIn && JobApplication.HasAppliedForJob(this.JobId.Value, this.UserId))
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

            var isNewApplication = this.JobApplication == null;
            if (this.JobId != null && (this.UserId == -1 || (!isNewApplication && this.JobApplication.UserId == this.UserId) || !JobApplication.HasAppliedForJob(this.JobId.Value, this.UserId)))
            {
                string resumeFileName = string.Empty;
                string resumeContentType = string.Empty;
                if (this.ResumeUpload.PostedFile != null)
                {
                    resumeFileName = Path.GetFileName(this.ResumeUpload.PostedFile.FileName);
                    resumeContentType = this.ResumeUpload.PostedFile.ContentType;
                }

                string coverLetterFileName = string.Empty;
                string coverLetterContentType = string.Empty;
                if (this.CoverLetterUpload.PostedFile != null)
                {
                    coverLetterFileName = Path.GetFileName(this.CoverLetterUpload.PostedFile.FileName);
                    coverLetterContentType = this.CoverLetterUpload.PostedFile.ContentType;
                }

                int? leadId = null;
                int value;
                if (int.TryParse(this.LeadDropDownList.SelectedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                {
                    leadId = value;
                }

                int? userId = (this.UserId == -1) ? (int?)null : this.UserId;
                var resumeBytes = this.ResumeUpload.FileBytes;
                var coverLetterBytes = this.CoverLetterUpload.FileBytes;
                var documentIds = isNewApplication
                                   ? JobApplication.Apply(
                                       this.JobId.Value,
                                       userId,
                                       resumeFileName,
                                       resumeContentType,
                                       resumeBytes,
                                       coverLetterFileName,
                                       coverLetterContentType,
                                       coverLetterBytes,
                                       GetTextIfVisible(this.SalaryTextBox),
                                       GetTextIfVisible(this.ApplicationMessageTextBox),
                                       GetTextIfVisible(this.ApplicantNameTextBox),
                                       GetTextIfVisible(this.ApplicantEmailTextBox),
                                       GetTextIfVisible(this.ApplicantPhoneTextBox),
                                       leadId)
                                   : JobApplication.UpdateApplication(
                                       this.ApplicationId.Value,
                                       userId,
                                       resumeFileName,
                                       resumeContentType,
                                       resumeBytes,
                                       coverLetterFileName,
                                       coverLetterContentType,
                                       coverLetterBytes,
                                       GetTextIfVisible(this.SalaryTextBox),
                                       GetTextIfVisible(this.ApplicationMessageTextBox), 
                                       GetTextIfVisible(this.ApplicantNameTextBox), 
                                       GetTextIfVisible(this.ApplicantEmailTextBox), 
                                       GetTextIfVisible(this.ApplicantPhoneTextBox), 
                                       leadId);

                Debug.Assert(this.CurrentJob != null, "this.CurrentJob must not be null when sending notification about a new application");
                string notificationEmailAddress = Engage.Utility.HasValue(this.CurrentJob.NotificationEmailAddress)
                                                      ? this.CurrentJob.NotificationEmailAddress
                                                      : this.DefaultNotificationEmailAddresses;

                this.SendNotificationEmail(
                    GetDocument(documentIds.First, DocumentType.Resume, resumeFileName, resumeContentType, resumeBytes), 
                    GetDocument(documentIds.Second, DocumentType.CoverLetter, coverLetterFileName, coverLetterContentType, coverLetterBytes),
                    isNewApplication,
                    notificationEmailAddress, 
                    true,
                    "ApplicationSubject",
                    "AnonymousApplicationSubject",
                    "ApplicationUpdateSubject",
                    "NotificationEmailBody.Format");

                var applicantEmail = this.GetTextWithFallback(this.ApplicantEmailTextBox, this.UserInfo.Email);
                if (applicantEmail != null)
                {
                    this.SendNotificationEmail(
                        documentIds.First,
                        isNewApplication,
                        applicantEmail, 
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
            this.GoBack();
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
            this.InitializeApplicantInfoSection(true);
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
                    var editingApplication = false;
                    if (this.ApplicationId.HasValue)
                    {
                        editingApplication = this.FillApplication();
                    }

                    if (this.CurrentJob == null || (!this.CurrentJob.IsActive && !editingApplication))
                    {
                        this.Visible = false;
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

            if (this.JobId != null)
            {
                // send email to list
                var toAddress = this.SendToAddressTextBox.Text;
                var replyTo = !string.IsNullOrEmpty(this.FromAddressTextBox.Text) 
                    ? this.FromAddressTextBox.Text 
                    : this.FromEmailAddress;
                var subject = string.Format(
                    CultureInfo.CurrentCulture, 
                    this.Localize("FriendEmailSubject"), 
                    this.FromNameTextBox.Text, 
                    this.PortalSettings.PortalName);
                var message = this.GetSendToAFriendMessageBody();

                try
                {
                    Mail.SendMail(
                            this.FromEmailAddress, 
                            toAddress, 
                            string.Empty,
                            string.Empty,
                            replyTo,
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