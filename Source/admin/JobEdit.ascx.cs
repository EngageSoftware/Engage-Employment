// <copyright file="JobEdit.ascx.cs" company="Engage Software">
// Engage: Employment - http://www.engagesoftware.com
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
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;
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
                    Hashtable settings = new ModuleController().GetTabModuleSettings(jobDetailsModule.TabModuleID);
                    return Dnn.Utility.GetStringSetting(settings, "ApplicationEmailAddress", this.PortalSettings.Email);
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
            base.OnInit(e);
            this.Load += this.Page_Load;
            this.PreRender += this.Page_PreRender;
            this.UpdateButton.Click += this.UpdateButton_Click;
            this.DeleteButton.Click += this.DeleteButton_Click;
            this.CancelButton.Click += this.CancelButton_Click;
            this.RequiredQualificationsRequiredValidator.ServerValidate += this.RequiredQualificationsRequiredValidator_ServerValidate;
            this.DesiredQualificationsRequiredValidator.ServerValidate += this.DesiredQualificationsRequiredValidator_ServerValidate;
            this.UniquePositionLocationValidator.ServerValidate += this.UniquePositionLocationValidator_ServerValidate;
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

                    this.EmailAddressTextBox.Text = this.ApplicationEmailAddress;
                    this.EmailAddressRegexValidator.ValidationExpression = Engage.Utility.EmailRegEx;

                    this.PositionDropDownList.Focus();
                    int jobId;
                    if (int.TryParse(this.Request.QueryString["jobId"], NumberStyles.Integer, CultureInfo.InvariantCulture, out jobId))
                    {
                        // updating
                        this.UpdateFields(jobId);
                        this.DeleteButton.Visible = true;
                        ClientAPI.AddButtonConfirm(this.DeleteButton, Localization.GetString("DeleteConfirm", this.LocalResourceFile));
                    }
                    else
                    {
                        // adding new
                        this.DeleteButton.Visible = false;
                        this.SortOrderTextBox.Text = DefaultSortOrder.ToString(CultureInfo.InvariantCulture);
                        this.StartDateTextBox.Text = DateTime.Now.ToShortDateString();
                    }
                }

                this.RegisterDatePickerBehavior();
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
            Job newJob;
            if (int.TryParse(this.Request.QueryString["jobId"], NumberStyles.Integer, CultureInfo.InvariantCulture, out jobId))
            {
                newJob = Job.Load(jobId);
            }
            else
            {
                newJob = Job.CreateJob();
            }

            newJob.CategoryId = int.Parse(this.CategoryDropDownList.SelectedValue, CultureInfo.InvariantCulture);
            newJob.DesiredQualifications = this.DesiredQualificationsTextEditor.Text;
            newJob.IsFilled = this.IsFilledCheckBox.Checked;
            newJob.IsHot = this.IsHotCheckBox.Checked;
            newJob.LocationId = int.Parse(this.LocationDropDownList.SelectedValue, CultureInfo.InvariantCulture);
            newJob.RequiredQualifications = this.RequiredQualificationsTextEditor.Text;
            newJob.PositionId = int.Parse(this.PositionDropDownList.SelectedValue, CultureInfo.InvariantCulture);
            newJob.SortOrder = Convert.ToInt32(this.SortOrderTextBox.Text, CultureInfo.CurrentCulture);
            newJob.NotificationEmailAddress = this.EmailAddressTextBox.Text;
            newJob.StartDate = DateTime.Parse(this.StartDateTextBox.Text, CultureInfo.CurrentCulture);
            newJob.ExpireDate = Engage.Utility.ParseNullableDateTime(this.ExpireDateTextBox.Text, CultureInfo.CurrentCulture);
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
        /// Handles the ServerValidate event of the <see cref="RequiredQualificationsRequiredValidator"/> control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="System.Web.UI.WebControls.ServerValidateEventArgs"/> instance containing the event data.</param>
        private void RequiredQualificationsRequiredValidator_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (args != null)
            {
                int length = this.RequiredQualificationsTextEditor.Text.Length;
                if (length == 0)
                {
                    this.RequiredQualificationsRequiredValidator.ErrorMessage = Localization.GetString("RequiredQualificationsRequired", this.LocalResourceFile);
                    args.IsValid = false;
                }
                ////else if (length > MaximumQualificationLength)
                ////{
                ////    this.RequiredQualificationsRequiredValidator.ErrorMessage = String.Format(CultureInfo.CurrentCulture, Localization.GetString("RequiredQualificationsMaxLength", LocalResourceFile), MaximumQualificationLength, length);
                ////    args.IsValid = false;
                ////}
            }
        }

        /// <summary>
        /// Handles the ServerValidate event of the <see cref="DesiredQualificationsRequiredValidator"/> control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="System.Web.UI.WebControls.ServerValidateEventArgs"/> instance containing the event data.</param>
        private void DesiredQualificationsRequiredValidator_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (args != null)
            {
                int length = this.DesiredQualificationsTextEditor.Text.Length;
                if (length == 0)
                {
                    this.DesiredQualificationsRequiredValidator.ErrorMessage = Localization.GetString("DesiredQualificationsRequired", this.LocalResourceFile);
                    args.IsValid = false;
                }
                ////else if (length > MaximumQualificationLength)
                ////{
                ////    this.DesiredQualificationsRequiredValidator.ErrorMessage = String.Format(CultureInfo.CurrentCulture, Localization.GetString("DesiredQualificationsMaxLength", LocalResourceFile), MaximumQualificationLength, length);
                ////    args.IsValid = false;
                ////}
            }
        }

        /// <summary>
        /// Handles the ServerValidate event of the <see cref="UniquePositionLocationValidator"/> control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="args">The <see cref="System.Web.UI.WebControls.ServerValidateEventArgs"/> instance containing the event data.</param>
        private void UniquePositionLocationValidator_ServerValidate(object source, ServerValidateEventArgs args)
        {
            if (args != null)
            {
                int positionId = int.Parse(this.PositionDropDownList.SelectedValue, CultureInfo.InvariantCulture);
                int locationId = int.Parse(this.LocationDropDownList.SelectedValue, CultureInfo.InvariantCulture);

                int? id = Job.GetJobId(locationId, positionId);
                int jobId;
                if (int.TryParse(this.Request.QueryString["jobId"], NumberStyles.Integer, CultureInfo.InvariantCulture, out jobId))
                {
                    // editing
                    args.IsValid = !id.HasValue || id.Value == jobId;
                }
                else
                {
                    // inserting
                    args.IsValid = !id.HasValue;
                }

                this.UniquePositionLocationValidator.ErrorMessage = Localization.GetString("cvUniquePositionLocation.Text", this.LocalResourceFile);
            }
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

            this.CategoryDropDownList.Items.Insert(0, new ListItem(Localization.GetString("SelectACategory", this.LocalResourceFile), "-1"));
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

            this.PositionDropDownList.Items.Insert(0, new ListItem(Localization.GetString("SelectAPosition", this.LocalResourceFile), "-1"));
        }

        /// <summary>
        /// Loads the <see cref="LocationDropDownList"/> with the list of <see cref="Location"/>s for this portal.
        /// </summary>
        private void LoadLocations()
        {
            foreach (Location location in Location.LoadLocations(null, this.PortalId))
            {
                this.LocationDropDownList.Items.Add(
                    new ListItem(location.LocationName + ", " + location.StateName, location.LocationId.Value.ToString(CultureInfo.InvariantCulture)));
            }

            this.LocationDropDownList.Items.Insert(0, new ListItem(Localization.GetString("SelectALocation", this.LocalResourceFile), "-1"));
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

                this.StartDateTextBox.Text = j.StartDate.ToShortDateString();
                this.ExpireDateTextBox.Text = j.ExpireDate.HasValue ? j.ExpireDate.Value.ToShortDateString() : string.Empty;

                this.EmailAddressTextBox.Text = Engage.Utility.HasValue(j.NotificationEmailAddress) ? j.NotificationEmailAddress : this.ApplicationEmailAddress;
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