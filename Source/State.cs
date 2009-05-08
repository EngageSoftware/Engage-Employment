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
    internal class State
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int? _stateId;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called in Eval statements from markup")]
        public int? StateId
        {
            [DebuggerStepThrough]
            get { return _stateId; }
            //[DebuggerStepThrough]
            //set { _stateId = value; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _stateName;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification="Called in Eval statements from markup")]
        public string StateName
        {
            [DebuggerStepThrough]
            get { return _stateName; }
            [DebuggerStepThrough]
            set { _stateName = value; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _abbreviation;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called in Eval statements from markup")]
        public string Abbreviation
        {
            [DebuggerStepThrough]
            get { return _abbreviation; }
            [DebuggerStepThrough]
            set { _abbreviation = value; }
        }

        public State(string stateName, string abbreviation)
            : this(null, stateName, abbreviation) {}

        private State(int? stateId, string stateName, string abbreviation)
        {
            this._stateId = stateId;
            this._stateName = stateName;
            this._abbreviation = abbreviation;
        }

        public void Save(int portalId)
        {
            if (_stateId.HasValue)
            {
                UpdateState(_stateId.Value, _stateName, _abbreviation);
            }
            else
            {
                InsertState(this._stateName, this._abbreviation, portalId);
            }
        }

        public bool IsUsed()
        {
            ValidateStateId();
            return IsStateUsed(_stateId.Value);
        }

        public void Delete()
        {
            ValidateStateId();
            DeleteState(_stateId.Value);
        }

        private static State FillState(IDataRecord dr)
        {
            return new State((int)dr["StateId"], dr["StateName"].ToString(), dr["StateAbbreviation"].ToString());
        }

        private void ValidateStateId()
        {
            if (!_stateId.HasValue)
            {
                throw new InvalidOperationException("This method is only valid for States that have been retrieved from the database");
            }
        }

        public static List<State> LoadStates(int? jobGroupId, int portalId)
        {
            List<State> states = new List<State>();
            using (IDataReader dr = DataProvider.Instance().GetStates(jobGroupId, portalId))
            {
                while (dr.Read())
                {
                    states.Add(FillState(dr));
                }
            }
            return states;
        }

        public static State LoadState(int stateId)
        {
            using (IDataReader dr = DataProvider.Instance().GetState(stateId))
            {
                if (dr.Read())
                {
                    return FillState(dr);
                }
            }
            return null;
        }

        public static void UpdateState(int id, string name, string abbreviation)
        {
            DataProvider.Instance().UpdateState(id, name, abbreviation);
        }

        public static void InsertState(string name, string abbreviation, int portalId)
        {
            DataProvider.Instance().InsertState(name, abbreviation, portalId);
        }

        public static int? GetStateId(string name, int portalId)
        {
            return DataProvider.Instance().GetStateId(name, portalId);
        }

        internal static bool IsStateUsed(int stateId)
        {
            return DataProvider.Instance().IsStateUsed(stateId);
        }

        internal static void DeleteState(int stateId)
        {
            DataProvider.Instance().DeleteState(stateId);
        }
    }
}
