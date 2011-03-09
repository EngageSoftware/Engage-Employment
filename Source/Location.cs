// <copyright file="Location.cs" company="Engage Software">
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

    using Data;

    internal class Location
    {
        private Location(int? locationId, string locationName, int stateId, string stateName, string stateAbbreviation)
        {
            this.LocationId = locationId;
            this.LocationName = locationName;
            this.StateId = stateId;
            this.StateName = stateName;
            this.StateAbbreviation = stateAbbreviation;
        }

        public int? LocationId { get; private set; }

        public string LocationName { get; set; }

        public int StateId { get; set; }

        public string StateName { get; private set; }

        public string StateAbbreviation { get; private set; }

        public static List<Location> LoadLocations(int? jobGroupId, int portalId)
        {
            var locations = new List<Location>();
            using (IDataReader dr = DataProvider.Instance().GetLocations(jobGroupId, portalId))
            {
                while (dr.Read())
                {
                    locations.Add(FillLocation(dr));
                }
            }

            return locations;
        }

        public static void UpdateLocation(int id, string description, int stateId)
        {
            DataProvider.Instance().UpdateLocation(id, description, stateId);
        }

        public static void InsertLocation(string description, int stateId, int portalId)
        {
            DataProvider.Instance().InsertLocation(description, stateId, portalId);
        }

        public static int? GetLocationId(string name, int? stateId, int portalId)
        {
            return DataProvider.Instance().GetLocationId(name, stateId, portalId);
        }

        /// <summary>
        /// Determines whether a location can be created in the portal with the specified ID.
        /// </summary>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>
        /// <c>true</c> if a location can be created in the portal with the specified ID; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanCreateLocation(int portalId)
        {
            return State.LoadStates(null, portalId).Count > 0;
        }

        /// <summary>
        /// Determines whether this instance is used by any jobs.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this instance is used by any jobs; otherwise, <c>false</c>.
        /// </returns>
        public bool IsUsed()
        {
            this.ValidateLocationId();
            return IsLocationUsed(this.LocationId.Value);
        }

        internal static bool IsLocationUsed(int locationId)
        {
            return DataProvider.Instance().IsLocationUsed(locationId);
        }

        internal static void DeleteLocation(int locationId)
        {
            DataProvider.Instance().DeleteLocation(locationId);
        }

        private static Location FillLocation(IDataRecord dr)
        {
            return new Location((int)dr["LocationId"], dr["LocationName"].ToString(), (int)dr["StateId"], dr["StateName"].ToString(), dr["StateAbbreviation"].ToString());
        }

        private void ValidateLocationId()
        {
            if (!this.LocationId.HasValue)
            {
                throw new InvalidOperationException("This method is only valid for Locations that have been retrieved from the database");
            }
        }
    }
}
