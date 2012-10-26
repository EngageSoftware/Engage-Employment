// <copyright file="UserStatusInfo.cs" company="Engage Software">
// Engage: Employment - http://www.engagesoftware.com
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
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using DotNetNuke.Entities.Portals;

    using Engage.Dnn.Employment.Data;

    public class UserStatusInfo
    {
        /// <summary>
        /// The cache key for <see cref="LoadUserStatus"/>, taking a portalId and userId
        /// </summary>
        private const string UserStatusCacheKeyFormat = "UserStatus.GetStatusForUser({0}, {1})";

        public UserStatusInfo(int userId, int portalId, int statusId)
        {
            this.StatusId = statusId;
            this.UserId = userId;
            this.PortalId = portalId;
        }

        public int UserId { get; private set; }

        public int PortalId { get; private set; }

        public int StatusId { get; private set; }

        /// <summary>
        /// Gets the list of users who have a given status.
        /// </summary>
        /// <param name="portalSettings">The settings of the portal in which the module is currently operating.</param>
        /// <param name="statusId">The ID of the status we're looking for.</param>
        /// <returns>A sequence of all users in the given portal.</returns>
        public static IEnumerable<UserStatusInfo> GetUsersWithStatus(PortalSettings portalSettings, int statusId)
        {
            if (portalSettings == null)
            {
                throw new ArgumentNullException("portalSettings");
            }

            var userStatusTable = DataProvider.Instance().GetUsersWithStatus(portalSettings.PortalId, statusId);
            
            return from DataRow row in userStatusTable.Rows
                   select FillUserInfo(row);
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

            DataProvider.Instance().UpdateUserStatus(portalSettings.PortalId, userId, statusId);
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

            return DataProvider.Instance().GetUserStatus(portalSettings.PortalId, userId);
        }

        private static UserStatusInfo FillUserInfo(DataRow row)
        {
            return new UserStatusInfo((int)row["UserId"], (int)row["PortalId"], (int)row["UserStatusId"]);
        }
    }
}