//Engage: Publish - http://www.engagesoftware.com
//Copyright (c) 2004-2009
//by Engage Software ( http://www.engagesoftware.com )

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
//TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
//CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
//DEALINGS IN THE SOFTWARE.

using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.UI.UserControls;
using Engage.Dnn.Employment.Data;

namespace Engage.Dnn.Employment
{
    public partial class JobDetailSettings : EmploymentModuleSettingsBase
    {
        public override void LoadSettings()
        {
            try
            {
                if (!IsPostBack)
                {
                    DataTable jobGroups = DataProvider.Instance().GetJobGroups(PortalId);
                    ddlJobGroup.DataSource = jobGroups;
                    ddlJobGroup.DataValueField = "JobGroupId";
                    ddlJobGroup.DataTextField = "Name";
                    ddlJobGroup.DataBind();
                    if (jobGroups.Rows.Count > 1)
                    {
                        ddlJobGroup.Items.Insert(0, new ListItem(Localization.GetString("All", LocalResourceFile), string.Empty));
                    }
                    else
                    {
                        ddlJobGroup.Enabled = false;
                    }

                    ddlJobGroup.SelectedValue = JobGroupId.HasValue ? JobGroupId.Value.ToString(CultureInfo.InvariantCulture) : string.Empty;
                }
                base.LoadSettings();
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        public override void UpdateSettings()
        {
            try
            {
                (new ModuleController()).UpdateTabModuleSetting(this.TabModuleId, Utility.JobGroupIdSetting, ddlJobGroup.SelectedValue);
                base.UpdateSettings();
            }
            catch (Exception exc)
            {
                DotNetNuke.Services.Exceptions.Exceptions.ProcessModuleLoadException(this, exc);
            }
        }
    }
}

