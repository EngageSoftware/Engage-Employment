//Engage: Employment - http://www.engagesoftware.com
//Copyright (c) 2004-2009
//by Engage Software ( http://www.engagesoftware.com )

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
//TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
//CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
//DEALINGS IN THE SOFTWARE.

using System.Globalization;
using DotNetNuke.Entities.Modules;

namespace Engage.Dnn.Employment
{
    public abstract class EmploymentModuleSettingsBase : ModuleSettingsBase
    {
        protected int? JobGroupId
        {
            get
            {
                return Engage.Dnn.Utility.GetIntSetting(Settings, Utility.JobGroupIdSetting);
            }
            set
            {
                string settingValue = string.Empty;
                if (value.HasValue)
                {
                    settingValue = value.Value.ToString(CultureInfo.InvariantCulture);
                }
                (new ModuleController()).UpdateTabModuleSetting(TabModuleId, Utility.JobGroupIdSetting, settingValue);
            }
        }
    }
}
