//Engage: Employment - http://www.engagesoftware.com
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
using System.Globalization;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Services.Search;
using Engage.Dnn.Employment.Data;

namespace Engage.Dnn.Employment
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Called by DNN through reflection")]
    internal class EmploymentController : ISearchable
    {
        private const string LocalResourceFile = Utility.DesktopModuleRelativePath + "App_LocalResources/EmploymentController.resx";

        #region ISearchable Members

        public SearchItemInfoCollection GetSearchItems(ModuleInfo ModInfo)
        {
            SearchItemInfoCollection searchItems = new SearchItemInfoCollection();
            Hashtable moduleSettings = (new ModuleController()).GetTabModuleSettings(ModInfo.TabModuleID);

            //only index the JobDetail module definition (since most of this information is only viewable there,
            //and because the Guid parameter on the SearchItemInfo ("jobid=" + jobid) gets put on the querystring to make it work all automagically).  BD
            if (Engage.Dnn.Utility.GetBoolSetting(moduleSettings, Utility.EnableDnnSearchSetting, true) && (new ModuleDefinitionController()).GetModuleDefinitionByName(ModInfo.DesktopModuleID, ModuleDefinition.JobDetail.ToString()).ModuleDefID == ModInfo.ModuleDefID)
            {
                int? jobGroupId = Engage.Dnn.Utility.GetIntSetting(moduleSettings, Utility.JobGroupIdSetting);

                using (IDataReader jobs = DataProvider.Instance().GetJobs(jobGroupId, ModInfo.PortalID))
                {
                    while (jobs.Read())
                    {
                        if (!(bool)jobs["IsFilled"])
                        {
                            string jobId = ((int)jobs["JobId"]).ToString(CultureInfo.InvariantCulture);
                            string searchItemTitle = string.Format(CultureInfo.CurrentCulture, Utility.GetString("JobInLocation", LocalResourceFile, ModInfo.PortalID), (string)jobs["JobTitle"], (string)jobs["LocationName"], (string)jobs["StateName"]);
                            string searchedContent = HtmlUtils.StripWhiteSpace(HtmlUtils.Clean((string)jobs["JobTitle"] + " " + (string)jobs["PositionDescription"] + " " + (string)jobs["RequiredQualifications"] + " " + (string)jobs["DesiredQualifications"], false), true);
                            string searchDescription = HtmlUtils.StripWhiteSpace(HtmlUtils.Clean((string)jobs["PositionDescription"], false), true);

                            searchItems.Add(new SearchItemInfo(searchItemTitle, searchDescription, (int)jobs["RevisingUser"], (DateTime)jobs["PostedDate"], ModInfo.ModuleID, jobId, searchedContent, "jobid=" + jobId));
                        }
                    }
                }
            }

            return searchItems;
        }

        #endregion
    }
}
