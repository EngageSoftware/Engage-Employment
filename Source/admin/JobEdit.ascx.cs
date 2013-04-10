// <copyright file="JobEdit.ascx.cs" company="Engage Software">
// Engage: Employment - http://www.engagesoftware.com
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
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.UI.Utilities;

    /// <summary>
    /// A form to create and edit job openings
    /// </summary>
    public partial class JobEdit : ModuleBase
    {
        /// <summary>
        /// The default sort order for a new job
        /// </summary>
        private const int DefaultSortOrder = 5;

        /// <summary>
        /// Gets the setting for the default email address to use for applications in this instance.
        /// </summary>
        /// <value>The default email address to use for applications in this instance</value>
        private string ApplicationEmailAddress
        {
            get
            {
                ModuleInfo jobDetailsModule = Utility.GetCurrentModuleByDefinition(this.PortalSettings, ModuleDefinition.JobDetail, this.JobGroupId);
                if (jobDetailsModule != null)
                {
                    return ModuleSettings.JobDetailApplicationEmailAddress.GetValueAsStringFor(this.DesktopModuleName, jobDetailsModule, PortalSettings.Email);
                }

                return string.Empty;
            }
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
            this.PreRender += this.Page_PreRender;
            this.UpdateButton.Click += this.UpdateButton_Click;
            this.DeleteButton.Click += this.DeleteButton_Click;
            this.CancelButton.Click += this.CancelButton_Click;
        }

        /// <summary>
        /// Handles the Load event of this control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!this.IsPostBack)
                {
                    this.LoadCategories();
                    this.LoadPositions();
                    this.LoadLocations();
                    this.SetMaxLengths();

                    // if a number that's too large is entered, SortOrderIntegerValidator is invalid after a postback, but its ErrorMessage is shown, instead of its Text
                    this.SortOrderIntegerValidator.ErrorMessage = this.Localize("cvSortOrder");
                    this.EmailAddressTextBox.Text = this.ApplicationEmailAddress;
                    this.EmailAddressRegexValidator.ValidationExpression = Engage.Utility.EmailsRegEx;

                    this.PositionDropDownList.Focus();
                    int jobId;
                    if (int.TryParse(this.Request.QueryString["jobId"], NumberStyles.Integer, CultureInfo.InvariantCulture, out jobId))
                    {
                        // updating
                        this.UpdateFields(jobId);
                        this.DeleteButton.Visible = true;
                        ClientAPI.AddButtonConfirm(this.DeleteButton, this.Localize("DeleteConfirm"));
                    }
                    else
                    {
                        // adding new
                        this.DeleteButton.Visible = false;
                        this.SortOrderTextBox.Text = DefaultSortOrder.ToString(CultureInfo.InvariantCulture);
                        this.StartDateTextBox.SelectedDate = DateTime.Now;
                    }
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.PreRender"/> event of this control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Page_PreRender(object sender, EventArgs e)
        {
            try
            {
                Engage.Utility.RegisterServerValidationMessageScript(this);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>
        /// Handles the Click event of the <see cref="UpdateButton"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void UpdateButton_Click(object sender, EventArgs e)
        {
            if (!this.Page.IsValid)
            {
                return;
            }

            int jobId;

            var newJob = int.TryParse(this.Request.QueryString["jobId"], NumberStyles.Integer, CultureInfo.InvariantCulture, out jobId)
                             ? Job.Load(jobId)
                             : Job.CreateJob();

            newJob.PositionId = int.Parse(this.PositionDropDownList.SelectedValue, CultureInfo.InvariantCulture);
            newJob.CategoryId = int.Parse(this.CategoryDropDownList.SelectedValue, CultureInfo.InvariantCulture);
            newJob.LocationId = int.Parse(this.LocationDropDownList.SelectedValue, CultureInfo.InvariantCulture);
            newJob.SortOrder = Convert.ToInt32(this.SortOrderTextBox.Text, CultureInfo.CurrentCulture);
            newJob.IsHot = this.IsHotCheckBox.Checked;
            newJob.IsFilled = this.IsFilledCheckBox.Checked;
            newJob.StartDate = this.StartDateTextBox.SelectedDate.Value;
            newJob.ExpireDate = this.ExpireDateTextBox.SelectedDate;
            newJob.RequiredQualifications = this.FilterHtml(this.RequiredQualificationsTextEditor.Text);
            newJob.DesiredQualifications = this.FilterHtml(this.DesiredQualificationsTextEditor.Text);
            newJob.NotificationEmailAddress = this.EmailAddressTextBox.Text;
            newJob.ApplicationUrl = this.ApplicationUrlTextBox.Text.Trim();
            newJob.Save(this.UserId, this.JobGroupId, this.PortalId);

            this.Response.Redirect(this.EditUrl(ControlKey.Edit.ToString()));
        }

        /// <summary>
        /// Handles the Click event of the <see cref="DeleteButton"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            int jobId;
            if (int.TryParse(this.Request.QueryString["jobId"], NumberStyles.Integer, CultureInfo.InvariantCulture, out jobId))
            {
                Job.Delete(jobId);
            }

            this.Response.Redirect(this.EditUrl(ControlKey.Edit.ToString()));
        }

        /// <summary>
        /// Handles the Click event of the <see cref="CancelButton"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Response.Redirect(this.EditUrl(ControlKey.Edit.ToString()));
        }

        /// <summary>
        /// Loads the <see cref="CategoryDropDownList"/> with the list of <see cref="Category"/>s for this portal.
        /// </summary>
        private void LoadCategories()
        {
            this.CategoryDropDownList.DataSource = Category.LoadCategories(null, this.PortalId);
            this.CategoryDropDownList.DataTextField = "CategoryName";
            this.CategoryDropDownList.DataValueField = "CategoryId";
            this.CategoryDropDownList.DataBind();

            this.CategoryDropDownList.Items.Insert(0, new ListItem(this.Localize("SelectACategory"), "-1"));
        }

        /// <summary>
        /// Loads the <see cref="PositionDropDownList"/> with the list of <see cref="Position"/>s for this portal.
        /// </summary>
        private void LoadPositions()
        {
            this.PositionDropDownList.DataSource = Position.LoadPositions(null, this.PortalId);
            this.PositionDropDownList.DataTextField = "JobTitle";
            this.PositionDropDownList.DataValueField = "PositionId";
            this.PositionDropDownList.DataBind();

            this.PositionDropDownList.Items.Insert(0, new ListItem(this.Localize("SelectAPosition"), "-1"));
        }

        /// <summary>
        /// Loads the <see cref="LocationDropDownList"/> with the list of <see cref="Location"/>s for this portal.
        /// </summary>
        private void LoadLocations()
        {
            foreach (Location location in Location.LoadLocations(null, this.PortalId))
            {
                this.LocationDropDownList.Items.Add(
                    new ListItem(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            this.Localize("Location"),
                            location.LocationName,
                            location.StateName,
                            location.StateAbbreviation),
                    location.LocationId.Value.ToString(CultureInfo.InvariantCulture)));
            }

            this.LocationDropDownList.Items.Insert(0, new ListItem(this.Localize("SelectALocation"), "-1"));
        }

        /// <summary>
        /// Sets the <see cref="TextBox.MaxLength"/> property for fields with a maximum length defined.
        /// </summary>
        private void SetMaxLengths()
        {
            this.ApplicationUrlTextBox.MaxLength = DataProvider.MaxUrlLength;
        }

        /// <summary>
        /// Sets up the form with the information for the specified job
        /// </summary>
        /// <param name="jobId">The job ID.</param>
        private void UpdateFields(int jobId)
        {
            Job j = Job.Load(jobId);
            if (j != null)
            {
                ListItem li = this.CategoryDropDownList.Items.FindByValue(j.CategoryId.ToString(CultureInfo.InvariantCulture));
                this.CategoryDropDownList.SelectedValue = li.Value;

                li = this.LocationDropDownList.Items.FindByValue(j.LocationId.ToString(CultureInfo.InvariantCulture));
                this.LocationDropDownList.SelectedValue = li.Value;

                li = this.PositionDropDownList.Items.FindByValue(j.PositionId.ToString(CultureInfo.InvariantCulture));
                this.PositionDropDownList.SelectedValue = li.Value;

                this.IsFilledCheckBox.Checked = j.IsFilled;
                this.IsHotCheckBox.Checked = j.IsHot;

                this.DesiredQualificationsTextEditor.Text = j.DesiredQualifications;
                this.RequiredQualificationsTextEditor.Text = j.RequiredQualifications;

                this.SortOrderTextBox.Text = j.SortOrder.ToString(CultureInfo.CurrentCulture);

                this.StartDateTextBox.SelectedDate = j.StartDate;
                this.ExpireDateTextBox.SelectedDate = j.ExpireDate;

                this.EmailAddressTextBox.Text = Engage.Utility.HasValue(j.NotificationEmailAddress) ? j.NotificationEmailAddress : this.ApplicationEmailAddress;
                this.ApplicationUrlTextBox.Text = j.ApplicationUrl;
            }
        }
    }
}