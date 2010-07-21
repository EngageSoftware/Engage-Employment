// <copyright file="EmploymentController.cs" company="Engage Software">
// Engage: Employment
// Copyright (c) 2004-2010
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
    using System.Collections;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Data;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Services.Search;

    /// <summary>
    /// Exposes the capabilities of this module to DNN.  Implements the method to allow searching and syndication integration.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Called by DNN through reflection")]
    internal class EmploymentController : ISearchable
    {
        /// <summary>
        /// The file from which to retrieve localized text for this type
        /// </summary>
        private const string LocalResourceFile = Utility.DesktopModuleRelativePath + "App_LocalResources/EmploymentController.resx";

#if TRIAL
        /// <summary>
        /// The license key for this module
        /// </summary>
        public static readonly Guid ModuleLicenseKey = new Guid("FF8492A1-C02B-4A8F-979C-7CCB9EAAC31A");
#endif

        #region ISearchable Members

        /// <summary>
        /// Gets a collection of <see cref="SearchItemInfo"/> instances, describing the job openings that can be viewed in the given Job Details module.
        /// </summary>
        /// <param name="modInfo">Information about the module instance for which jobs should be returned.</param>
        /// <returns>A collection of <see cref="SearchItemInfo"/> instances</returns>
        public SearchItemInfoCollection GetSearchItems(ModuleInfo modInfo)
        {
            if (modInfo == null)
            {
                throw new ArgumentNullException("modInfo", "modInfo must not be null.");
            }

            SearchItemInfoCollection searchItems = new SearchItemInfoCollection();
            Hashtable moduleSettings = (new ModuleController()).GetTabModuleSettings(modInfo.TabModuleID);

            // only index the JobDetail module definition (since most of this information is only viewable there,
            // and because the Guid parameter on the SearchItemInfo ("jobid=" + jobid) gets put on the querystring to make it work all automagically).  BD
            if (Dnn.Utility.GetBoolSetting(moduleSettings, Utility.EnableDnnSearchSetting, true) && (new ModuleDefinitionController()).GetModuleDefinitionByName(modInfo.DesktopModuleID, ModuleDefinition.JobDetail.ToString()).ModuleDefID == modInfo.ModuleDefID)
            {
                int? jobGroupId = Dnn.Utility.GetIntSetting(moduleSettings, Utility.JobGroupIdSetting);

                using (IDataReader jobs = DataProvider.Instance().GetJobs(jobGroupId, modInfo.PortalID))
                {
                    while (jobs.Read())
                    {
                        if (!(bool)jobs["IsFilled"])
                        {
                            string jobId = ((int)jobs["JobId"]).ToString(CultureInfo.InvariantCulture);
                            string searchDescription = HtmlUtils.StripWhiteSpace(HtmlUtils.Clean((string)jobs["JobDescription"], false), true);
                            string searchItemTitle = string.Format(
                                CultureInfo.CurrentCulture,
                                Utility.GetString("JobInLocation", LocalResourceFile, modInfo.PortalID),
                                (string)jobs["JobTitle"],
                                (string)jobs["LocationName"],
                                (string)jobs["StateName"]);

                            string searchedContent = HtmlUtils.StripWhiteSpace(
                                HtmlUtils.Clean(
                                    (string)jobs["JobTitle"] + " " + (string)jobs["JobDescription"] + " " + (string)jobs["RequiredQualifications"] + " " + (string)jobs["DesiredQualifications"],
                                    false),
                                true);

                            searchItems.Add(
                                new SearchItemInfo(
                                    searchItemTitle,
                                    searchDescription,
                                    (int)jobs["RevisingUser"],
                                    (DateTime)jobs["RevisionDate"],
                                    modInfo.ModuleID,
                                    jobId,
                                    searchedContent,
                                    "jobid=" + jobId));
                        }
                    }
                }
            }

            return searchItems;
        }

        #endregion
    }
}
