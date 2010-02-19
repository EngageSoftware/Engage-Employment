//Engage: Publish - http://www.engagesoftware.com
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
    using System.Data;
    using System.Globalization;
    using System.Web.UI.WebControls;
    using Data;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Localization;

    /// <summary>
    /// Displays settings for the Job Listing module
    /// </summary>
    public partial class JobListingSettings : EmploymentModuleSettingsBase
    {
        /// <summary>
        /// Called the the settings page loads.
        /// </summary>
        public override void LoadSettings()
        {
            try
            {
                if (!IsPostBack)
                {
                    DataTable jobGroups = DataProvider.Instance().GetJobGroups(PortalId);
                    this.JobGroupDropDownList.DataSource = jobGroups;
                    this.JobGroupDropDownList.DataValueField = "JobGroupId";
                    this.JobGroupDropDownList.DataTextField = "Name";
                    this.JobGroupDropDownList.DataBind();

                    string helpTextResourceKey = "lblJobGroup.Help";
                    if (jobGroups.Rows.Count > 0)
                    {
                        this.JobGroupDropDownList.Items.Insert(0, new ListItem(Localization.GetString("All", LocalResourceFile), string.Empty));
                    }
                    else
                    {
                        this.JobGroupDropDownList.Enabled = false;
                        helpTextResourceKey = "NoJobGroups.Help";
                    }

                    this.HelpTextLabel.Text = this.Localize(helpTextResourceKey);
                    this.JobGroupDropDownList.SelectedValue = JobGroupId.HasValue ? JobGroupId.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
                }

                base.LoadSettings();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <summary>
        /// Called when the user clicks the Update button on the settings page.
        /// </summary>
        public override void UpdateSettings()
        {
            try
            {
                new ModuleController().UpdateTabModuleSetting(this.TabModuleId, Utility.JobGroupIdSetting, this.JobGroupDropDownList.SelectedValue);
                base.UpdateSettings();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}

