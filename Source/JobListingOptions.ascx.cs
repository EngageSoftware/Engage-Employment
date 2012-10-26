// <copyright file="JobListingOptions.ascx.cs" company="Engage Software">
// Engage: Employment
// Copyright (c) 2004-2011
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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web.UI.WebControls;
    using DotNetNuke.Common;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Exceptions;

    public partial class JobListingOptions : ModuleBase
    {
        #region Event Handlers
        protected override void OnInit(EventArgs e)
        {
            if (!PermissionController.CanManageJobListingOptions(this))
            {
                DenyAccess();
                return;
            }

            if (AJAX.IsInstalled())
            {
                // AJAX.AddScriptManager(Page);
                AJAX.WrapUpdatePanelControl(this.LimitOptionPlaceholder, false);
                this.LimitCheckBox.CheckedChanged += this.LimitCheckBox_CheckedChanged;
                this.LimitCheckBox.AutoPostBack = true;
            }

            this.LimitRangeValidator.MaximumValue = int.MaxValue.ToString(CultureInfo.InvariantCulture);

            this.Load += this.Page_Load;
            this.UpdateButton.Click += this.UpdateButton_Click;
            this.CancelButton.Click += this.CancelButton_Click;
            base.OnInit(e);
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Better to keep to module, rather than take down the whole page")]
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    this.FillDisplayOptionList();
                    this.FillLimitOptionList();

                    // If the maximum number of jobs is set as no maximum, it is stored as an empty string.  
                    // Since this isn't an int value, getting it as an int gives us the default, instead of null
                    // So, if we get the default, we need to doublecheck that it's the default and not the "no maximum" value
                    int? maximumNumberOfjobs = ModuleSettings.JobListingMaximumNumberOfJobsDisplayed.GetValueAsInt32For(this);
                    if (maximumNumberOfjobs == ModuleSettings.JobListingMaximumNumberOfJobsDisplayed.DefaultValue 
                        && string.IsNullOrEmpty(ModuleSettings.JobListingMaximumNumberOfJobsDisplayed.GetValueAsStringFor(this)))
                    {
                        maximumNumberOfjobs = null;
                    }

                    this.DisplayOptionRadioButtonList.SelectedValue = ModuleSettings.JobListingShowOnlyHotJobs.GetValueAsBooleanFor(this).ToString();
                    this.LimitCheckBox.Checked = maximumNumberOfjobs.HasValue;
                    this.SetLimitEnabled(this.LimitCheckBox.Checked);
                    if (maximumNumberOfjobs.HasValue)
                    {
                        this.txtLimit.Text = maximumNumberOfjobs.Value.ToString(CultureInfo.CurrentCulture);
                    }

                    this.LimitOptionRadioButtonList.SelectedValue = ModuleSettings.JobListingLimitJobsRandomly.GetValueAsStringFor(this);
                    this.ShowCloseDateCheckBox.Checked = ModuleSettings.JobListingShowCloseDate.GetValueAsBooleanFor(this).Value;
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Better to catch at module level than take down the whole page")]
        protected void UpdateButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (Page.IsValid)
                {
                    ModuleSettings.JobListingShowOnlyHotJobs.Set(this, this.DisplayOptionRadioButtonList.SelectedValue);
                    ModuleSettings.JobListingMaximumNumberOfJobsDisplayed.Set(this, this.LimitCheckBox.Checked ? Convert.ToInt32(this.txtLimit.Text, CultureInfo.CurrentCulture).ToString(CultureInfo.InvariantCulture) : string.Empty);
                    ModuleSettings.JobListingLimitJobsRandomly.Set(this, this.LimitOptionRadioButtonList.SelectedValue);
                    ModuleSettings.JobListingShowCloseDate.Set(this, this.ShowCloseDateCheckBox.Checked);

                    Response.Redirect(Globals.NavigateURL(TabId));
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected void CancelButton_Click(object sender, EventArgs e)
        {
            // return to the main page view
            Response.Redirect(Globals.NavigateURL(TabId), false);
        }

        protected void LimitCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            this.SetLimitEnabled(this.LimitCheckBox.Checked);
        }
        #endregion

        #region Helper Methods

        private void SetLimitEnabled(bool enabled) 
        {
            this.txtLimit.Enabled = this.LimitOptionRadioButtonList.Enabled = this.LimitRequiredFieldValidator.Enabled = this.LimitRangeValidator.Enabled = enabled;
        }

        private void FillDisplayOptionList()
        {
            this.DisplayOptionRadioButtonList.Items.Clear();
            this.DisplayOptionRadioButtonList.Items.Add(new ListItem(this.Localize("ShowHotJobs"), true.ToString(CultureInfo.InvariantCulture)));
            this.DisplayOptionRadioButtonList.Items.Add(new ListItem(this.Localize("ShowAllJobs"), false.ToString(CultureInfo.InvariantCulture)));
        }

        private void FillLimitOptionList()
        {
            this.LimitOptionRadioButtonList.Items.Clear();
            this.LimitOptionRadioButtonList.Items.Add(new ListItem(this.Localize("Sorted"), false.ToString(CultureInfo.InvariantCulture)));
            this.LimitOptionRadioButtonList.Items.Add(new ListItem(this.Localize("Random"), true.ToString(CultureInfo.InvariantCulture)));
        }

        #endregion
    }
}

