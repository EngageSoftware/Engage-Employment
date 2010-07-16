// <copyright file="State.cs" company="Engage Software">
// Engage: Employment
// Copyright (c) 2004-2010
// by Engage Software ( http://www.engagesoftware.com )
// </copyright>

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using Engage.Dnn.Employment.Data;

namespace Engage.Dnn.Employment
{
    
    internal class State
    {
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called in Eval statements from markup")]
        public int? StateId { get; private set; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification="Called in Eval statements from markup")]
        public string StateName { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called in Eval statements from markup")]
        public string Abbreviation { get; set; }

        private State(int? stateId, string stateName, string abbreviation)
        {
            this.StateId = stateId;
            this.StateName = stateName;
            this.Abbreviation = abbreviation;
        }

        private static State FillState(IDataRecord dr)
        {
            return new State((int)dr["StateId"], dr["StateName"].ToString(), dr["StateAbbreviation"].ToString());
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
