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
    using System.Diagnostics;
    using System.Globalization;
    using Data;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;

    internal class UserStatus
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _status;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int _statusId;

        public UserStatus(string status, int statusId)
        {
            this._status = status;
            this._statusId = statusId;
        }

        public int StatusId
        {
            [DebuggerStepThrough]
            get { return this._statusId; }
        }

        public string Status
        {
            [DebuggerStepThrough]
            get { return this._status; }
        }

        public static List<UserStatus> LoadStatuses(int portalId)
        {
            DataTable statusTable = DataProvider.Instance().GetUserStatuses(portalId);
            List<UserStatus> statuses = new List<UserStatus>(statusTable.Rows.Count);
            foreach (DataRow row in statusTable.Rows)
            {
                statuses.Add(FillUserStatus(row));
            }

            return statuses;
        }

        public static UserStatus LoadStatus(int statusId)
        {
            UserStatus status = null;
            DataTable statusTable = DataProvider.Instance().GetUserStatus(statusId);
            if (statusTable != null && statusTable.Rows.Count > 0)
            {
                status = FillUserStatus(statusTable.Rows[0]);
            }

            return status;
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
        /// Loads the status ID of a user.
        /// </summary>
        /// <param name="portalSettings">The ID of the portal in which the module is currently operating.</param>
        /// <param name="userId">The ID of the user whom we are looking up.</param>
        /// <returns>The ID of the current status of the given user, or <c>null</c> if the user has no status.</returns>
        /// <exception cref="NullReferenceException">If the user does not exist</exception>
        public static int? LoadUserStatus(PortalSettings portalSettings, int userId)
        {
            CheckUserStatusPropertyExists(portalSettings);
            UserInfo user = (new UserController()).GetUser(portalSettings.PortalId, userId);
            ProfileController.GetUserProfile(ref user);
            string status = user.Profile.GetPropertyValue(Utility.UserStatusPropertyName);

            int statusId;
            if (int.TryParse(status, NumberStyles.Integer, CultureInfo.InvariantCulture, out statusId))
            {
                return statusId;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Updates the status of the given user.
        /// </summary>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="userId">The user ID.</param>
        /// <param name="statusId">The status ID.</param>
        public static void UpdateUserStatus(PortalSettings portalSettings, int userId, int? statusId)
        {
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
            ProfilePropertyDefinition property = new ProfilePropertyDefinition();
            property.PortalId = portalSettings.PortalId;
            property.ModuleDefId = Utility.GetCurrentModuleByDefinition(portalSettings, ModuleDefinition.JobListing, null).ModuleDefID;
            property.DataType = (new ListController()).GetListEntryInfo("DataType", "Integer").EntryID;
            property.PropertyCategory = "Engage: Employment";
            property.PropertyName = Utility.UserStatusPropertyName;
            property.Required = false;
            property.ViewOrder = 0;
            property.Visibility = UserVisibilityMode.AdminOnly;
            property.Length = 0;

            return property;
        }

        private static UserStatus FillUserStatus(DataRow row)
        {
            return new UserStatus((string)row["StatusName"], (int)row["UserStatusId"]);
        }
    }
}