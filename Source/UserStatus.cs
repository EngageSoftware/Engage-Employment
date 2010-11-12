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
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using Data;

    public class UserStatus
    {
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

        private static UserStatus FillUserStatus(DataRow row)
        {
            return new UserStatus((string)row["StatusName"], (int)row["UserStatusId"]);
        }
    }
}