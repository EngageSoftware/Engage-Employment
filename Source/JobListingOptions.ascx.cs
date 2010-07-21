// <copyright file="JobListingOptions.ascx.cs" company="Engage Software">
// Engage: Employment
// Copyright (c) 2004-2010
// by Engage Software ( http://www.engagesoftware.com )
// </copyright>

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

using System;
using System.Globalization;
using System.Web.UI.WebControls;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;

namespace Engage.Dnn.Employment
{
    public partial class JobListingOptions : ModuleBase
    {
        #region Event Handlers
        protected override void OnInit(EventArgs e)
        {
            if (AJAX.IsInstalled())
            {
                //AJAX.AddScriptManager(Page);
                AJAX.WrapUpdatePanelControl(phLimitOption, false);
                this.LimitCheckBox.CheckedChanged += this.LimitCheckBox_CheckedChanged;
                this.LimitCheckBox.AutoPostBack = true;
            }
            rvLimit.MaximumValue = int.MaxValue.ToString(CultureInfo.InvariantCulture);

            this.Load += Page_Load;
            this.UpdateButton.Click += this.UpdateButton_Click;
            this.CancelButton.Click += this.CancelButton_Click;
            base.OnInit(e);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    FillDisplayOptionList();
                    FillLimitOptionList();
                    this.DisplayOptionRadioButtonList.SelectedValue = ShowOnlyHotJobs.ToString(CultureInfo.InvariantCulture);
                    this.LimitCheckBox.Checked = MaximumNumberOfJobsDisplayed.HasValue;
                    SetLimitEnabled(this.LimitCheckBox.Checked);
                    if (this.LimitCheckBox.Checked)
                    {
                        txtLimit.Text = MaximumNumberOfJobsDisplayed.Value.ToString(CultureInfo.CurrentCulture);
                    }
                    this.LimitOptionRadioButtonList.SelectedValue = LimitJobsRandomly.ToString(CultureInfo.InvariantCulture);
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        protected void UpdateButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (Page.IsValid)
                {
                    ModuleController modules = new ModuleController();
                    modules.UpdateTabModuleSetting(this.TabModuleId, "ShowOnlyHotJobs", this.DisplayOptionRadioButtonList.SelectedValue);
                    modules.UpdateTabModuleSetting(this.TabModuleId, "MaximumNumberOfJobsDisplayed", this.LimitCheckBox.Checked ? Convert.ToInt32(txtLimit.Text, CultureInfo.CurrentCulture).ToString(CultureInfo.InvariantCulture) : string.Empty);
                    modules.UpdateTabModuleSetting(this.TabModuleId, "LimitJobsRandomly", this.LimitOptionRadioButtonList.SelectedValue);

                    Response.Redirect(Globals.NavigateURL(TabId));
                }
            }
            catch (Exception exc)
            {
                DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        protected void CancelButton_Click(object sender, EventArgs e)
        {
            //return to the main page view
            Response.Redirect(Globals.NavigateURL(TabId), false);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        protected void LimitCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            SetLimitEnabled(this.LimitCheckBox.Checked);
        }
        #endregion

        #region Helper Methods

        private void SetLimitEnabled(bool enabled) {
            this.txtLimit.Enabled = this.LimitOptionRadioButtonList.Enabled = this.LimitRequiredFieldValidator.Enabled = this.rvLimit.Enabled = enabled;
        }

        private void FillDisplayOptionList()
        {
            this.DisplayOptionRadioButtonList.Items.Clear();
            this.DisplayOptionRadioButtonList.Items.Add(new ListItem(Localization.GetString("ShowHotJobs", LocalResourceFile), true.ToString(CultureInfo.InvariantCulture)));
            this.DisplayOptionRadioButtonList.Items.Add(new ListItem(Localization.GetString("ShowAllJobs", LocalResourceFile), false.ToString(CultureInfo.InvariantCulture)));
        }

        private void FillLimitOptionList()
        {
            this.LimitOptionRadioButtonList.Items.Clear();
            this.LimitOptionRadioButtonList.Items.Add(new ListItem(Localization.GetString("Sorted", LocalResourceFile), false.ToString(CultureInfo.InvariantCulture)));
            this.LimitOptionRadioButtonList.Items.Add(new ListItem(Localization.GetString("Random", LocalResourceFile), true.ToString(CultureInfo.InvariantCulture)));
        }

        #endregion

        #region Module Settings
        private bool ShowOnlyHotJobs
        {
            get
            {
                return Engage.Dnn.Utility.GetBoolSetting(Settings, "ShowOnlyHotJobs", true);
            }
        }

        private int? MaximumNumberOfJobsDisplayed
        {
            get
            {
                int? value = Engage.Dnn.Utility.GetIntSetting(Settings, "MaximumNumberOfJobsDisplayed");
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
                return Engage.Dnn.Utility.GetBoolSetting(Settings, "LimitJobsRandomly", true);
            }
        }

        #endregion
    }
}

