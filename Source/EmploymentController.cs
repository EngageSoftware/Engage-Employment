// <copyright file="EmploymentController.cs" company="Engage Software">
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
    using System;
    using System.Collections.Generic;
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
    using DotNetNuke.Services.Search.Entities;

    /// <summary>
    /// Exposes the capabilities of this module to DNN.  Implements the method to allow searching and syndication integration.
    /// </summary>
    [SuppressMessage(
        "Microsoft.Performance",
        "CA1812:AvoidUninstantiatedInternalClasses",
        Justification = "Called by DNN through reflection")]
    internal class EmploymentController : ModuleSearchBase, IUpgradeable
    {
        /// <summary>
        /// The name of this module's desktop module record in DNN
        /// </summary>
        public const string DesktopModuleName = "Engage: Employment";

        /// <summary>
        /// The file from which to retrieve localized text for this type
        /// </summary>
        private const string LocalResourceFile =
            Utility.DesktopModuleRelativePath + "App_LocalResources/EmploymentController.resx";

        /// <inheritdoc />
        public override IList<SearchDocument> GetModifiedSearchDocuments(ModuleInfo moduleInfo, DateTime beginDateUtc)
        {
            if (moduleInfo == null)
            {
                throw new ArgumentNullException("modInfo", @"modInfo must not be null.");
            }

            var searchItems = new List<SearchDocument>();

            // only index the JobDetail module definition (since most of this information is only viewable there,
            // and because the Guid parameter on the SearchItemInfo ("jobid=" + jobid) gets put on the querystring to make it work all automagically).  BD
            if (ModuleDefinitionController.GetModuleDefinitionByFriendlyName(
                        ModuleDefinition.JobDetail.ToString(),
                        moduleInfo.DesktopModuleID)
                    .ModuleDefID
                != moduleInfo.ModuleDefID
                || !ModuleSettings.JobDetailEnableDnnSearch.GetValueAsBooleanFor(
                    DesktopModuleName,
                    moduleInfo,
                    ModuleSettings.JobDetailEnableDnnSearch.DefaultValue))
            {
                return searchItems;
            }

            int? jobGroupId = ModuleSettings.JobGroupId.GetValueAsInt32For(
                DesktopModuleName,
                moduleInfo,
                ModuleSettings.JobGroupId.DefaultValue);

            // TODO: only retrieve jobs modified after beginDateUtc
            using (IDataReader jobs = DataProvider.Instance()
                .GetJobs(jobGroupId, moduleInfo.PortalID))
            {
                while (jobs.Read())
                {
                    var jobId = ((int)jobs["JobId"]).ToString(CultureInfo.InvariantCulture);
                    var revisionDate = (DateTime)jobs["RevisionDate"];
                    var searchDocument = new SearchDocument
                                             {
                                                 UniqueKey = $"Employment_Job_{jobId}",
                                                 ModifiedTimeUtc = revisionDate.ToUniversalTime(),
                                                 ModuleId = moduleInfo.ModuleID,
                                                 PortalId = moduleInfo.PortalID,
                                                 QueryString = "jobid=" + jobId,
                                             };
                    searchItems.Add(searchDocument);
                    if ((bool)jobs["IsFilled"])
                    {
                        searchDocument.IsActive = false;
                        continue;
                    }

                    var searchDescription = HtmlUtils.StripWhiteSpace(
                        HtmlUtils.Clean((string)jobs["JobDescription"], false),
                        true);
                    var searchItemTitle = string.Format(
                        CultureInfo.CurrentCulture,
                        Utility.GetString("JobInLocation", LocalResourceFile, moduleInfo.PortalID),
                        (string)jobs["JobTitle"],
                        (string)jobs["LocationName"],
                        (string)jobs["StateName"]);

                    var searchedContent = HtmlUtils.StripWhiteSpace(
                        HtmlUtils.Clean(
                            (string)jobs["JobTitle"]
                            + " "
                            + (string)jobs["JobDescription"]
                            + " "
                            + (string)jobs["RequiredQualifications"]
                            + " "
                            + (string)jobs["DesiredQualifications"],
                            false),
                        true);

                    searchDocument.Title = searchItemTitle;
                    searchDocument.Description = searchDescription;
                    searchDocument.AuthorUserId = (int)jobs["RevisingUser"];
                    searchDocument.Body = searchedContent;
                }
            }

            return searchItems;
        }



        public string UpgradeModule(string version)
        {
            var v = new Version(version);
            if (v == new Version(1, 8, 7))
            {
                // Migrate UserStatus from UserProfile to EngageEmployment_UserStatus table.
                var portalController = new PortalController();
                var portals = portalController.GetPortals();

                foreach (PortalInfo portal in portals)
                {
                    var maxSize = 0;
                    var users = UserController.GetUsersByProfileProperty(
                        portal.PortalID, 
                        propertyName: Utility.UserStatusPropertyName, 
                        propertyValue: "%", 
                        pageIndex: 0, 
                        pageSize: int.MaxValue, 
                        totalRecords: ref maxSize);

                    foreach (UserInfo userInfo in users)
                    {
                        var userId = userInfo.UserID;
                        var statusIdStr = userInfo.Profile.GetPropertyValue(Utility.UserStatusPropertyName);
                        int statusId;
                        if (int.TryParse(statusIdStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out statusId))
                        {
                            DataProvider.Instance().UpdateUserStatus(portal.PortalID, userId, statusId);
                        }
                    }

                    var userStatusPropertyDefinition = ProfileController.GetPropertyDefinitionByName(portal.PortalID, Utility.UserStatusPropertyName);
                    if (userStatusPropertyDefinition != null)
                    {
                        ProfileController.DeletePropertyDefinition(userStatusPropertyDefinition);
                    }
                }

                return "Migrated user statuses from DNN user profile to EngageEmployment_UserStatus table";
            }

            if (v == new Version(1, 9, 8)) 
            {
                new PermissionController().CreateCustomPermissions();
                return "Created custom module permissions";
            }

            return "No upgrade action required for this version";
        }
    }
}
