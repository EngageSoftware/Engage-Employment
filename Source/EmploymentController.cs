// <copyright file="EmploymentController.cs" company="Engage Software">
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
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;

    using Data;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Search;

    /// <summary>
    /// Exposes the capabilities of this module to DNN.  Implements the method to allow searching and syndication integration.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses", Justification = "Called by DNN through reflection")]
    internal class EmploymentController : ISearchable, IUpgradeable
    {
        /// <summary>
        /// The name of this module's desktop module record in DNN
        /// </summary>
        public const string DesktopModuleName = "Engage: Employment";

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
                throw new ArgumentNullException("modInfo", @"modInfo must not be null.");
            }

            var searchItems = new SearchItemInfoCollection();

            // only index the JobDetail module definition (since most of this information is only viewable there,
            // and because the Guid parameter on the SearchItemInfo ("jobid=" + jobid) gets put on the querystring to make it work all automagically).  BD
            if (ModuleDefinitionController.GetModuleDefinitionByFriendlyName(ModuleDefinition.JobDetail.ToString(), modInfo.DesktopModuleID).ModuleDefID == modInfo.ModuleDefID 
                && ModuleSettings.JobDetailEnableDnnSearch.GetValueAsBooleanFor(DesktopModuleName, modInfo, ModuleSettings.JobDetailEnableDnnSearch.DefaultValue))
            {
                int? jobGroupId = ModuleSettings.JobGroupId.GetValueAsInt32For(DesktopModuleName, modInfo, ModuleSettings.JobGroupId.DefaultValue);

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

                            string searchedContent =
                                HtmlUtils.StripWhiteSpace(
                                    HtmlUtils.Clean(
                                        (string)jobs["JobTitle"] + " " + (string)jobs["JobDescription"] + " " + (string)jobs["RequiredQualifications"] +
                                        " " + (string)jobs["DesiredQualifications"],
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

        public string UpgradeModule(string version)
        {
            var v = new Version(version);
            if (v >= new Version(1,8,7))
            {
                // Migrate UserStatus from UserProfile to EngageEmployment_UserStatus table.
                var portalController = new PortalController();
                var portals = portalController.GetPortals().Cast<PortalInfo>();

                foreach (var portal in portals)
                {
                    var maxSize = 0;
                    var users = UserController.GetUsersByProfileProperty(
                        portal.PortalID, Utility.UserStatusPropertyName, "%", 0, int.MaxValue, ref maxSize).Cast<UserInfo>();

                    foreach (var userInfo in users)
                    {
                        var userId = userInfo.UserID;
                        var statusIdStr = userInfo.Profile.GetPropertyValue(Utility.UserStatusPropertyName);
                        int statusId;
                        int.TryParse(statusIdStr, out statusId);

                        DataProvider.Instance().UpdateUserStatus(portal.PortalID, userId, statusId);
                    }

                    ProfileController.DeletePropertyDefinition(ProfileController.GetPropertyDefinitionByName(portal.PortalID, Utility.UserStatusPropertyName));
                }
            }

            return "No upgrade action required for this version";
        }
    }
}
