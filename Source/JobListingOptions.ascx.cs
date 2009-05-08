//Engage: Publish - http://www.engagesoftware.com
//Copyright (c) 2004-2009
//by Engage Software ( http://www.engagesoftware.com )

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
//TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
//CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
//DEALINGS IN THE SOFTWARE.

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
                chkLimit.CheckedChanged += chkLimit_CheckedChanged;
                chkLimit.AutoPostBack = true;
            }
            rvLimit.MaximumValue = int.MaxValue.ToString(CultureInfo.InvariantCulture);

            this.Load += Page_Load;
            btnUpdate.Click += btnUpdate_Click;
            btnCancel.Click += btnCancel_Click;
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
                    rblDisplayOption.SelectedValue = ShowOnlyHotJobs.ToString(CultureInfo.InvariantCulture);
                    chkLimit.Checked = MaximumNumberOfJobsDisplayed.HasValue;
                    SetLimitEnabled(chkLimit.Checked);
                    if (chkLimit.Checked)
                    {
                        txtLimit.Text = MaximumNumberOfJobsDisplayed.Value.ToString(CultureInfo.CurrentCulture);
                    }
                    rblLimitOption.SelectedValue = LimitJobsRandomly.ToString(CultureInfo.InvariantCulture);
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (Page.IsValid)
                {
                    ModuleController modules = new ModuleController();
                    modules.UpdateTabModuleSetting(this.TabModuleId, "ShowOnlyHotJobs", rblDisplayOption.SelectedValue);
                    modules.UpdateTabModuleSetting(this.TabModuleId, "MaximumNumberOfJobsDisplayed", chkLimit.Checked ? Convert.ToInt32(txtLimit.Text, CultureInfo.CurrentCulture).ToString(CultureInfo.InvariantCulture) : string.Empty);
                    modules.UpdateTabModuleSetting(this.TabModuleId, "LimitJobsRandomly", rblLimitOption.SelectedValue);

                    Response.Redirect(Globals.NavigateURL(TabId));
                }
            }
            catch (Exception exc)
            {
                DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        protected void btnCancel_Click(object sender, EventArgs e)
        {
            //return to the main page view
            Response.Redirect(Globals.NavigateURL(TabId), false);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Member")]
        protected void chkLimit_CheckedChanged(object sender, EventArgs e)
        {
            SetLimitEnabled(this.chkLimit.Checked);
        }
        #endregion

        #region Helper Methods

        private void SetLimitEnabled(bool enabled) {
            this.txtLimit.Enabled = this.rblLimitOption.Enabled = this.rfvLimit.Enabled = this.rvLimit.Enabled = enabled;
        }

        private void FillDisplayOptionList()
        {
            this.rblDisplayOption.Items.Clear();
            this.rblDisplayOption.Items.Add(new ListItem(Localization.GetString("ShowHotJobs", LocalResourceFile), true.ToString(CultureInfo.InvariantCulture)));
            this.rblDisplayOption.Items.Add(new ListItem(Localization.GetString("ShowAllJobs", LocalResourceFile), false.ToString(CultureInfo.InvariantCulture)));
        }

        private void FillLimitOptionList()
        {
            this.rblLimitOption.Items.Clear();
            this.rblLimitOption.Items.Add(new ListItem(Localization.GetString("Sorted", LocalResourceFile), false.ToString(CultureInfo.InvariantCulture)));
            this.rblLimitOption.Items.Add(new ListItem(Localization.GetString("Random", LocalResourceFile), true.ToString(CultureInfo.InvariantCulture)));
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

