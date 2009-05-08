//Engage: Employment - http://www.engagesoftware.com
//Copyright (c) 2004-2009
//by Engage Software ( http://www.engagesoftware.com )

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
//TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
//THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
//CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
//DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Engage.Dnn.Employment.Data;

namespace Engage.Dnn.Employment
{
    internal class Position
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int? _positionId;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called in Eval statements from markup")]
        public int? PositionId
        {
            [DebuggerStepThrough]
            get { return _positionId; }
            //[DebuggerStepThrough]
            //set { _positionId = value; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _jobTitle;
        public string JobTitle
        {
            [DebuggerStepThrough]
            get { return _jobTitle; }
            [DebuggerStepThrough]
            set { _jobTitle = value; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _jobDescription;
        public string JobDescription
        {
            [DebuggerStepThrough]
            get { return _jobDescription; }
            [DebuggerStepThrough]
            set { _jobDescription = value; }
        }

        public Position(string jobTitle, string jobDescription) : this(null, jobTitle, jobDescription) {}

        private Position(int? positionId, string jobTitle, string jobDescription)
        {
            _positionId = positionId;
            _jobTitle = jobTitle;
            _jobDescription = jobDescription;
        }

        public void Save(int portalId)
        {
            if (_positionId.HasValue)
            {
                UpdatePosition(_positionId.Value, _jobTitle, _jobDescription);
            }
            else
            {
                InsertPosition(_jobTitle, _jobDescription, portalId);
            }
        }

        public bool IsUsed()
        {
            ValidatePositionId();
            return IsPositionUsed(_positionId.Value);
        }

        public void Delete()
        {
            ValidatePositionId();
            DeletePosition(_positionId.Value);
        }

        private static Position FillPosition(IDataRecord dr)
        {
            return new Position((int)dr["PositionId"], dr["JobTitle"].ToString(), dr["JobDescription"].ToString());
        }

        private void ValidatePositionId()
        {
            if (!_positionId.HasValue)
            {
                throw new InvalidOperationException("This method is only valid for Positions that have been retrieved from the database");
            }
        }

        public static List<Position> LoadPositions(int? jobGroupId, int portalId)
        {
            List<Position> positions = new List<Position>();
            using (IDataReader dr = DataProvider.Instance().GetPositions(jobGroupId, portalId))
            {
                while (dr.Read())
                {
                    positions.Add(FillPosition(dr));
                }
            }
            return positions;
        }

        public static Position LoadPosition(int id)
        {
            using (IDataReader dr = DataProvider.Instance().GetPosition(id))
            {
                if (dr.Read())
                {
                    return FillPosition(dr);
                }
            }
            return null;
        }

        public static void UpdatePosition(int id, string jobTitle, string description)
        {
            DataProvider.Instance().UpdatePosition(id, jobTitle, description);
        }

        public static void InsertPosition(string jobTitle, string description, int portalId)
        {
            DataProvider.Instance().InsertPosition(jobTitle, description, portalId);
        }

        public static int? GetPositionId(string name, int portalId)
        {
            return DataProvider.Instance().GetPositionId(name, portalId);
        }

        internal static bool IsPositionUsed(int positionId)
        {
            return DataProvider.Instance().IsPositionUsed(positionId);
        }

        internal static void DeletePosition(int positionId)
        {
            DataProvider.Instance().DeletePosition(positionId);
        }
    }
}
