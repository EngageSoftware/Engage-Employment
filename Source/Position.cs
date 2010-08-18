// <copyright file="Position.cs" company="Engage Software">
// Engage: Employment
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
    #region

    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.CodeAnalysis;

    using Engage.Dnn.Employment.Data;

    #endregion

    internal class Position
    {
        private Position(int? positionId, string jobTitle, string jobDescription)
        {
            this.PositionId = positionId;
            this.JobTitle = jobTitle;
            this.JobDescription = jobDescription;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Only used via reflection (i.e. databinding)")]
        public string JobDescription { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Only used via reflection (i.e. databinding)")]
        public string JobTitle { get; set; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Only used via reflection (i.e. databinding)")]
        public int? PositionId { get; private set; }

        public static int? GetPositionId(string name, int portalId)
        {
            return DataProvider.Instance().GetPositionId(name, portalId);
        }

        public static void InsertPosition(string jobTitle, string description, int portalId)
        {
            DataProvider.Instance().InsertPosition(jobTitle, description, portalId);
        }

        public static List<Position> LoadPositions(int? jobGroupId, int portalId)
        {
            var positions = new List<Position>();
            using (var dr = DataProvider.Instance().GetPositions(jobGroupId, portalId))
            {
                while (dr.Read())
                {
                    positions.Add(FillPosition(dr));
                }
            }

            return positions;
        }

        public static void UpdatePosition(int id, string jobTitle, string description)
        {
            DataProvider.Instance().UpdatePosition(id, jobTitle, description);
        }

        internal static void DeletePosition(int positionId)
        {
            DataProvider.Instance().DeletePosition(positionId);
        }

        internal static bool IsPositionUsed(int positionId)
        {
            return DataProvider.Instance().IsPositionUsed(positionId);
        }

        private static Position FillPosition(IDataRecord dr)
        {
            return new Position((int)dr["PositionId"], dr["JobTitle"].ToString(), dr["JobDescription"].ToString());
        }
    }
}