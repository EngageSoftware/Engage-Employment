// <copyright file="EmploymentModuleSettingsBase.cs" company="Engage Software">
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
    using System.Globalization;
    using DotNetNuke.Entities.Modules;
    using Framework;

    /// <summary>
    /// Base functionality for controls integrating with DNN settings
    /// </summary>
    public abstract class EmploymentModuleSettingsBase : SettingsBase
    {
        /// <summary>
        /// Gets the name of the this module's desktop module record in DNN.
        /// </summary>
        /// <value>The name of this module's desktop module record in DNN.</value>
        public override string DesktopModuleName
        {
            get
            {
                return "Engage: Employment";
            }
        }

        /// <summary>
        /// Gets or sets the job group ID setting for this module instance.
        /// </summary>
        /// <value>The job group ID.</value>
        protected int? JobGroupId
        {
            get
            {
                return Employment.ModuleSettings.JobGroupId.GetValueAsInt32For(this);
            }

            set
            {
                string settingValue = string.Empty;
                if (value.HasValue)
                {
                    settingValue = value.Value.ToString(CultureInfo.InvariantCulture);
                }

                (new ModuleController()).UpdateTabModuleSetting(this.TabModuleId, Utility.JobGroupIdSetting, settingValue);
            }
        }
    }
}
