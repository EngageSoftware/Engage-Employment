// <copyright file="Location.cs" company="Engage Software">
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
    using Data;

    internal class Location
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int? _locationId;
        public int? LocationId
        {
            [DebuggerStepThrough]
            get { return _locationId; }
            //[DebuggerStepThrough]
            //set { _locationId = value; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _locationName;
        public string LocationName
        {
            [DebuggerStepThrough]
            get { return _locationName; }
            [DebuggerStepThrough]
            set { _locationName = value; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int _stateId;
        public int StateId
        {
            [DebuggerStepThrough]
            get { return _stateId; }
            [DebuggerStepThrough]
            set { _stateId = value; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _stateName;
        public string StateName
        {
            [DebuggerStepThrough]
            get { return _stateName; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _stateAbbreviation;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called in Eval statements from markup")]
        public string StateAbbreviation
        {
            [DebuggerStepThrough]
            get { return _stateAbbreviation; }
        }

        private Location(int locationId, string locationName, int stateId)
            : this(locationId, locationName, stateId, string.Empty, string.Empty)
        { }

        public Location(string locationName, int stateId)
            : this(null, locationName, stateId, string.Empty, string.Empty)
        { }

        private Location(int? locationId, string locationName, int stateId, string stateName, string stateAbbreviation)
        {
            _locationId = locationId;
            _locationName = locationName;
            _stateId = stateId;
            _stateName = stateName;
            _stateAbbreviation = stateAbbreviation;
        }

        public void Save(int portalId)
        {
            if (_locationId.HasValue)
            {
                UpdateLocation(_locationId.Value, _locationName, _stateId);
            }
            else
            {
                InsertLocation(_locationName, _stateId, portalId);
            }
        }

        /// <summary>
        /// Determines whether this instance is used by any jobs.
        /// </summary>
        /// <returns>
        /// 	<c>true</c> if this instance is used by any jobs; otherwise, <c>false</c>.
        /// </returns>
        public bool IsUsed()
        {
            ValidateLocationId();
            return IsLocationUsed(_locationId.Value);
        }

        public void Delete()
        {
            ValidateLocationId();
            DeleteLocation(_locationId.Value);
        }

        private static Location FillLocation(IDataRecord dr)
        {
            return new Location((int)dr["LocationId"], dr["LocationName"].ToString(), (int)dr["StateId"], dr["StateName"].ToString(), dr["StateAbbreviation"].ToString());
        }

        private void ValidateLocationId()
        {
            if (!_locationId.HasValue)
            {
                throw new InvalidOperationException("This method is only valid for Locations that have been retrieved from the database");
            }
        }

        public static List<Location> LoadLocations(int? jobGroupId, int portalId)
        {
            List<Location> locations = new List<Location>();
            using (IDataReader dr = DataProvider.Instance().GetLocations(jobGroupId, portalId))
            {
                while (dr.Read())
                {
                    locations.Add(FillLocation(dr));
                }
            }
            return locations;
        }

        public static Location LoadLocation(int locationId)
        {
            using (IDataReader dr = DataProvider.Instance().GetLocation(locationId))
            {
                if (dr.Read())
                {
                    return FillLocation(dr);
                }
            }
            return null;
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

        internal static bool IsLocationUsed(int locationId)
        {
            return DataProvider.Instance().IsLocationUsed(locationId);
        }

        internal static void DeleteLocation(int locationId)
        {
            DataProvider.Instance().DeleteLocation(locationId);
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
    }
}
