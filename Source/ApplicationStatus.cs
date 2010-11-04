// <copyright file="ApplicationStatus.cs" company="Engage Software">
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
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using Data;
    using DotNetNuke.Common.Lists;
    using DotNetNuke.Common.Utilities;

    /// <summary>
    /// The status of a <see cref="JobApplication"/>.
    /// </summary>
    public class ApplicationStatus
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationStatus"/> class.
        /// </summary>
        /// <param name="statusId">The ID of the status.</param>
        /// <param name="statusName">Name of the status.</param>
        private ApplicationStatus(int statusId, string statusName)
        {
            this.StatusId = statusId;
            this.StatusName = statusName;
        }

        /// <summary>
        /// Gets the ID of this status.
        /// </summary>
        /// <value>The ID of this status.</value>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called from data binding on ApplicationStatusListing.ascx")]
        public int StatusId { get; private set; }

        /// <summary>
        /// Gets the name of this status.
        /// </summary>
        /// <value>The name of this status.</value>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called from data binding on ApplicationStatusListing.ascx")]
        public string StatusName { get; private set; }

        /// <summary>
        /// Gets a list of all of the application statuses for this portal.
        /// </summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>A list of all of the application statuses for this portal</returns>
        public static IEnumerable<ApplicationStatus> GetStatuses(int portalId)
        {
            var statusList = (new ListController()).GetListEntryInfoCollection(Utility.ApplicationStatusListName);

            return from ListEntryInfo entry in statusList
                   where entry.PortalID == portalId
                   select new ApplicationStatus(entry.EntryID, entry.Text);
        }

        /// <summary>
        /// Gets the ID of the status with the given name in the given portal, or <c>null</c> if none exists.
        /// </summary>
        /// <param name="statusName">Name of the status.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>The ID of the status with the given name in the given portal, or <c>null</c> if none exists</returns>
        public static int? GetStatusId(string statusName, int portalId)
        {
            // TODO: test this on multiple portals
            var statusEntry = new ListController().GetListEntryInfo(Utility.ApplicationStatusListName, statusName);
            if (statusEntry != null && statusEntry.PortalID == portalId)
            {
                return statusEntry.EntryID;
            }

            return null;
        }

        /// <summary>
        /// Gets the name of the status with the given ID.
        /// </summary>
        /// <param name="statusId">The ID of the status.</param>
        /// <returns>The name of the status</returns>
        public static string GetStatusName(int statusId)
        {
            return new ListController().GetListEntryInfo(statusId).Text;
        }

        /// <summary>
        /// Determines whether the status with the given ID is used on any applications.
        /// </summary>
        /// <param name="statusId">The ID of the status.</param>
        /// <returns>
        /// <c>true</c> if the status with the given ID is used on any applications; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsStatusUsed(int statusId)
        {
            return DataProvider.Instance().IsApplicationStatusUsed(statusId);
        }

        /// <summary>
        /// Inserts a new status with the given name, in the given portal.
        /// </summary>
        /// <param name="statusName">Name of the status.</param>
        /// <param name="portalId">The portal ID.</param>
        public static void InsertStatus(string statusName, int portalId)
        {
            var listItem = new ListEntryInfo 
            {
                Text = statusName,
                DefinitionID = Null.NullInteger,
                PortalID = portalId,
                ListName = Utility.ApplicationStatusListName,
                Value = string.Empty
            };

            new ListController().AddListEntry(listItem);
        }

        /// <summary>
        /// Deletes the status with the given ID.
        /// </summary>
        /// <param name="statusId">The status ID.</param>
        public static void DeleteStatus(int statusId)
        {
            new ListController().DeleteListEntryByID(statusId, true);
        }

        /// <summary>
        /// Updates the name of the status with the given ID.
        /// </summary>
        /// <param name="statusId">The status ID.</param>
        /// <param name="statusName">Name of the status.</param>
        public static void UpdateStatus(int statusId, string statusName)
        {
            ListEntryInfo leadItem = new ListController().GetListEntryInfo(statusId);
            leadItem.Text = statusName;
            new ListController().UpdateListEntry(leadItem);
        }
    }
}
