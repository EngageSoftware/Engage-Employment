// <copyright file="UserStatus.cs" company="Engage Software">
// Engage: Employment - http://www.engagesoftware.com
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
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;

    using Data;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;

    public class UserStatus
    {
        /// <summary>
        /// The cache key for <see cref="GetStatusForUser"/> and <see cref="LoadUserStatus"/>, taking a portalId and userId
        /// </summary>
        private const string UserStatusCacheKeyFormat = "UserStatus.GetStatusForUser({0}, {1})";

        public UserStatus(string status, int statusId)
        {
            this.Status = status;
            this.StatusId = statusId;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Only used via reflection (i.e. databinding)")]
        public int StatusId { get; private set; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Only used via reflection (i.e. databinding)")]
        public string Status { get; private set; }

        public static IEnumerable<UserStatus> LoadStatuses(int portalId)
        {
            DataTable statusTable = DataProvider.Instance().GetUserStatuses(portalId);
            
            return from DataRow row in statusTable.Rows
                   select FillUserStatus(row);
        }

        public static UserStatus LoadStatus(int statusId)
        {
            DataTable statusTable = DataProvider.Instance().GetUserStatus(statusId);
            if (statusTable.Rows.Count > 0)
            { 
                return FillUserStatus(statusTable.Rows[0]);
            }

            return null;
        }

        public static void UpdateStatus(int statusId, string status)
        {
            DataProvider.Instance().UpdateUserStatus(statusId, status);
        }

        public static void InsertStatus(string status, int portalId)
        {
            DataProvider.Instance().InsertUserStatus(status, portalId);
        }

        public static bool IsStatusUsed(int statusId)
        {
            return DataProvider.Instance().IsUserStatusUsed(statusId);
        }

        public static void DeleteStatus(int statusId)
        {
            DataProvider.Instance().DeleteUserStatus(statusId);
        }

        public static int? GetStatusId(string statusName, int portalId)
        {
            return DataProvider.Instance().GetUserStatusId(statusName, portalId);
        }

        /// <summary>
        /// Gets the list of users who have a given status.
        /// </summary>
        /// <param name="portalSettings">The settings of the portal in which the module is currently operating.</param>
        /// <param name="statusId">The ID of the status we're looking for.</param>
        /// <returns>A sequence of all users in the given portal.</returns>
        public static IEnumerable<UserInfo> GetUsersWithStatus(PortalSettings portalSettings, int statusId)
        {
            if (portalSettings == null)
            {
                throw new ArgumentNullException("portalSettings");
            }

            return UserController.GetUsers(portalSettings.PortalId).Cast<UserInfo>()
                    .Where(user => GetStatusForUser(portalSettings, user) == statusId);
        }

        /// <summary>
        /// Loads the status ID of a user.
        /// </summary>
        /// <param name="portalSettings">The settings of the portal in which the module is currently operating.</param>
        /// <param name="userId">The ID of the user whom we are looking up.</param>
        /// <returns>The ID of the current status of the given user, or <c>null</c> if the user has no status.</returns>
        /// <exception cref="NullReferenceException">If the user does not exist</exception>
        public static int? LoadUserStatus(PortalSettings portalSettings, int userId)
        {
            if (portalSettings == null)
            {
                throw new ArgumentNullException("portalSettings");
            }

            return DataCache.GetCachedData<int?>(
                new CacheItemArgs(string.Format(CultureInfo.InvariantCulture, UserStatusCacheKeyFormat, portalSettings.PortalId, userId)),
                args =>
                    {
                        var user = (new UserController()).GetUser(portalSettings.PortalId, userId);
                        return GetStatusForUser(portalSettings, user);
                    });
        }

        /// <summary>
        /// Updates the status of the given user.
        /// </summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="userId">The user ID.</param>
        /// <param name="statusId">The status ID.</param>
        public static void UpdateUserStatus(PortalSettings portalSettings, int userId, int? statusId)
        {
            if (portalSettings == null)
            {
                throw new ArgumentNullException("portalSettings");
            }

            CheckUserStatusPropertyExists(portalSettings);
            UserInfo user = (new UserController()).GetUser(portalSettings.PortalId, userId);
            ProfileController.GetUserProfile(ref user);

            string statusValue = string.Empty;
            if (statusId.HasValue)
            {
                statusValue = statusId.Value.ToString(CultureInfo.InvariantCulture);
            }
            
            user.Profile.SetProfileProperty(Utility.UserStatusPropertyName, statusValue);
            UserController.UpdateUser(portalSettings.PortalId, user);
            DataCache.RemoveCache(string.Format(CultureInfo.InvariantCulture, UserStatusCacheKeyFormat, portalSettings.PortalId, userId));
        }

        /// <summary>
        /// Gets the ID of the status for the given user.
        /// </summary>
        /// <param name="portalSettings">The settings of the portal in which the module is currently operating.</param>
        /// <param name="user">The user for whom to get the status.</param>
        /// <returns>The ID of the user's status, or <c>null</c> if the user doesn't have a status set</returns>
        private static int? GetStatusForUser(PortalSettings portalSettings, UserInfo user)
        {
            return DataCache.GetCachedData<int?>(
                new CacheItemArgs(string.Format(CultureInfo.InvariantCulture, UserStatusCacheKeyFormat, portalSettings.PortalId, user.UserID)),
                args =>
                    {
                        CheckUserStatusPropertyExists(portalSettings);
                        ProfileController.GetUserProfile(ref user);
                        var status = user.Profile.GetPropertyValue(Utility.UserStatusPropertyName);

                        int statusId;
                        if (int.TryParse(status, NumberStyles.Integer, CultureInfo.InvariantCulture, out statusId))
                        {
                            return statusId;
                        }

                        return null;
                    });
        }

        private static void CheckUserStatusPropertyExists(PortalSettings portalSettings)
        {
            try
            {
                if (ProfileController.GetPropertyDefinitionByName(portalSettings.PortalId, Utility.UserStatusPropertyName) == null)
                {
                    ProfileController.AddPropertyDefinition(GetUserStatusProfilePropertyDefinition(portalSettings));
                }
            }
            catch (InvalidOperationException)
            {
                ProfileController.AddPropertyDefinition(GetUserStatusProfilePropertyDefinition(portalSettings));
            }
        }

        private static ProfilePropertyDefinition GetUserStatusProfilePropertyDefinition(PortalSettings portalSettings)
        {
            var property = new ProfilePropertyDefinition
                {
                    PortalId = portalSettings.PortalId,
                    ModuleDefId =
                        Utility.GetCurrentModuleByDefinition(
                            portalSettings, ModuleDefinition.JobListing, null).ModuleDefID,
                    DataType = (new ListController()).GetListEntryInfo("DataType", "Integer").EntryID,
                    PropertyCategory = "Engage: Employment",
                    PropertyName = Utility.UserStatusPropertyName,
                    Required = false,
                    ViewOrder = 0,
                    Visibility = UserVisibilityMode.AdminOnly,
                    Length = 0
                };

            return property;
        }

        private static UserStatus FillUserStatus(DataRow row)
        {
            return new UserStatus((string)row["StatusName"], (int)row["UserStatusId"]);
        }
    }
}