// <copyright file="JobSearchQuery.cs" company="Engage Software">
// Engage: Employment
// Copyright (c) 2004-2014
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
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text;
    using System.Text.RegularExpressions;
    using Data;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;

    [Serializable]
    internal class JobSearchQuery
    {
        private const string Ellipsis = "...";

        private const int QualificationsFieldLength = 150;

        private const int WordsPerSnippet = 10;

        /// <summary>
        /// Backing field for <see cref="CleanKeywordList"/>
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] 
        private List<string> cleanKeywordList;

        /// <summary>
        /// Backing field for <see cref="KeywordList"/>
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] 
        private List<string> keywordList;

        /// <summary>
        /// Backing field for <see cref="Keywords"/>
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)] 
        private string keywords;

        /// <summary>
        /// Initializes a new instance of the <see cref="JobSearchQuery"/> class.
        /// </summary>
        public JobSearchQuery()
        {
            this.Category = string.Empty;
            this.JobPosition = string.Empty;
            this.LocationName = string.Empty;
            this.StateName = string.Empty;
            this.Id = -1;
        }

        /// <summary>
        /// Gets the category name.
        /// </summary>
        /// <value>The category name.</value>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called only via Eval()")]
        public string Category { get; private set; }

        /// <summary>
        /// Gets the creation date of this query.
        /// </summary>
        /// <value>The creation date.</value>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called only via Eval()")]
        public DateTime CreationDate { get; private set; }

        /// <summary>
        /// Gets the title of the position.
        /// </summary>
        /// <value>The job position's title.</value>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called only via Eval()")]
        public string JobPosition { get; private set; }

        /// <summary>
        /// Gets the name of the location.
        /// </summary>
        /// <value>The name of the location.</value>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called only via Eval()")]
        public string LocationName { get; private set; }

        /// <summary>
        /// Gets the name of the region.
        /// </summary>
        /// <value>The name of the region.</value>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called only via Eval()")]
        public string StateName { get; private set; }

        public int? CategoryId
        {
            get;
            set;
        }

        public List<string> CleanKeywordList
        {
            get { return this.cleanKeywordList ?? (this.cleanKeywordList = Utility.RemoveCommonWords(this.KeywordList)); }
        }

        public string Description
        {
            get;
            set;
        }

        public int Id
        {
            get;
            private set;
        }

        public int? JobGroupId
        {
            private get;
            set;
        }

        public int? JobPositionId
        {
            get;
            set;
        }

        public List<string> KeywordList
        {
            get 
            {
                return this.keywordList ??
                       (this.keywordList = Engage.Utility.HasValue(this.keywords) ? Utility.SplitQuoted(this.keywords) : new List<string>(0));
            }
        }

        public string Keywords
        {
            [DebuggerStepThrough]
            get
            {
                return this.keywords;
            }

            [DebuggerStepThrough]
            set
            {
                this.keywords = value;
                this.keywordList = null;
                this.cleanKeywordList = null;
            }
        }

        public int? LocationId
        {
            get;
            set;
        }

        public int? StateId
        {
            get;
            set;
        }

        private static int PortalId
        {
            get
            {
                return PortalController.GetCurrentPortalSettings().PortalId;
            }
        }

        public static void Delete(int userSearchId)
        {
            DataProvider.Instance().DeleteJobSearchQuery(userSearchId);
        }

        public static JobSearchQuery Load(int userSearchId)
        {
            using (IDataReader dr = DataProvider.Instance().GetJobSearchQuery(userSearchId))
            {
                if (dr.Read())
                {
                    return FillJobSearchQuery(dr);
                }
            }

            return null;
        }

        public static ReadOnlyCollection<JobSearchQuery> LoadSearches(int userId, int? jobGroupId)
        {
            var queries = new List<JobSearchQuery>();
            using (IDataReader dr = DataProvider.Instance().GetJobSearchQueries(userId, jobGroupId, PortalId))
            {
                while (dr.Read())
                {
                    queries.Add(FillJobSearchQuery(dr));
                }
            }

            return queries.AsReadOnly();
        }

        public DataTable Execute()
        {
            DataTable searchResultsTable = DataProvider.Instance().GetJobSearchResults(this.JobPositionId, this.CategoryId, this.LocationId, this.StateId, this.JobGroupId, PortalId);
            DataTable keywordResults = this.GetKeywordSearchResults();
            if (keywordResults != null)
            {
                if (keywordResults.Rows.Count == 0)
                {
                    // valid keywords supplied no results
                    searchResultsTable.Rows.Clear();
                }
                else
                {
                    // intersect the results
                    searchResultsTable = IntersectTables(searchResultsTable, keywordResults);
                }
            }

            if (searchResultsTable != null)
            {
                FormatKeywordResults(this.KeywordList, searchResultsTable);
            }

            return searchResultsTable;
        }

        public void Save(int queryUserId)
        {
            DataProvider.Instance().SaveJobSearchQuery(
                    queryUserId, 
                    this.Description, 
                    this.JobPositionId, 
                    this.CategoryId, 
                    this.StateId, 
                    this.LocationId, 
                    this.keywords, 
                    this.JobGroupId, 
                    PortalId);
        }

        private static int CalculateBeginPosition(string rowText, int beginPosition, int maxWordCount)
        {
            int wordCount = 0;
            while (beginPosition > 0 && wordCount < maxWordCount)
            {
                beginPosition--;
                if (char.IsWhiteSpace(rowText[beginPosition]))
                {
                    wordCount++;
                }
            }

            while (char.IsWhiteSpace(rowText[beginPosition]))
            {
                beginPosition++; // don't start with whitespace
            }

            return beginPosition;
        }

        private static int CalculateEndPosition(string rowText, int beginPosition, int maxWordCount)
        {
            int wordCount = 0;
            int endPosition = beginPosition;
            while (endPosition < rowText.Length - 1 && wordCount < maxWordCount)
            {
                endPosition++;
                if (char.IsWhiteSpace(rowText[endPosition]))
                {
                    wordCount++;
                }
            }

            while (char.IsWhiteSpace(rowText[endPosition]))
            {
                endPosition--; // don't end with whitespace
            }

            return endPosition;
        }

        private static string CreateSearchResultsSnippets(string rowText, Queue<int> keywordPositions)
        {
            bool needsEndingEllipsis = false;
            var qualificationsTextBuilder = new StringBuilder(QualificationsFieldLength);

            while (qualificationsTextBuilder.Length < QualificationsFieldLength && keywordPositions.Count > 0)
            {
                int beginPosition = CalculateBeginPosition(rowText, keywordPositions.Dequeue(), WordsPerSnippet / 2);

                if (beginPosition <= 0)
                {
                    beginPosition = 0;
                }
                else
                {
                    qualificationsTextBuilder.Append(Ellipsis);
                }

                int endPosition;
                if (keywordPositions.Count > 0)
                {
                    endPosition = CalculateEndPosition(rowText, beginPosition, WordsPerSnippet);
                    qualificationsTextBuilder.Append(rowText, beginPosition, endPosition - beginPosition + 1);
                    while (keywordPositions.Count > 0 && endPosition >= keywordPositions.Peek())
                    {
                        keywordPositions.Dequeue();
                    }
                }
                else
                {
                    endPosition = beginPosition + (QualificationsFieldLength - qualificationsTextBuilder.Length);
                    if (endPosition >= rowText.Length)
                    {
                        endPosition = rowText.Length - 1;
                    }

                    qualificationsTextBuilder.Append(rowText, beginPosition, endPosition - beginPosition + 1);
                }

                needsEndingEllipsis = endPosition != rowText.Length - 1;
            }

            if (needsEndingEllipsis)
            {
                qualificationsTextBuilder.Append(Ellipsis);
            }

            return qualificationsTextBuilder.ToString();
        }

        private static JobSearchQuery FillJobSearchQuery(IDataRecord dr)
        {
            return new JobSearchQuery
                       {
                               Id = (int)dr["UserSearchId"],
                               Description = dr["SearchName"].ToString(),
                               JobPosition = dr["JobTitle"].ToString(),
                               JobPositionId = dr["PositionId"] as int?,
                               CategoryId = dr["CategoryId"] as int?,
                               Category = dr["CategoryName"].ToString(),
                               LocationName = dr["LocationName"].ToString(),
                               LocationId = dr["LocationId"] as int?,
                               StateName = dr["StateName"].ToString(),
                               StateId = dr["StateId"] as int?,
                               keywords = dr["Keywords"].ToString(),
                               CreationDate = (DateTime)dr["CreationDate"],
                               JobGroupId = dr["JobGroupId"] as int?
                       };
        }

        private static void FillKeywordPositions(string rowText, string keyword, Queue<int> keywordPositions)
        {
            int keywordPosition = 0;
            do
            {
                keywordPosition = rowText.IndexOf(keyword, keywordPosition, StringComparison.CurrentCultureIgnoreCase);
                if (keywordPosition > -1)
                {
                    keywordPositions.Enqueue(keywordPosition);
                    keywordPosition += keyword.Length;
                }
            }
            while (keywordPosition > -1);
        }

        private static void FormatKeywordResults(IEnumerable<string> keywords, DataTable dt)
        {
            DataColumn requiredQualificationsColumn = dt.Columns["RequiredQualifications"];
            DataColumn desiredQualificationsColumn = dt.Columns["DesiredQualifications"];
            DataColumn jobDescriptionColumn = dt.Columns["JobDescription"];
            DataColumn searchResultsColumn = dt.Columns.Add("SearchResults", typeof(string));

            foreach (DataRow row in dt.Rows)
            {
                row[searchResultsColumn] = 
                    HtmlUtils.StripTags(
                        (string)row[jobDescriptionColumn] + Ellipsis + " " + (string)row[requiredQualificationsColumn] + Ellipsis + " " + (string)row[desiredQualificationsColumn], 
                        false)
                    .Replace(Environment.NewLine, " ")
                    .Trim();
                row[requiredQualificationsColumn] = string.Empty;
                row[desiredQualificationsColumn] = string.Empty;
                row[jobDescriptionColumn] = string.Empty;

                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    var cellContents = row[i];
                    var rowText = cellContents as string;
                    if (Engage.Utility.HasValue(rowText))
                    {
                        bool isQualificationsColumn = false;
                        Queue<int> keywordPositions = null;
                        if (dt.Columns[i].Equals(searchResultsColumn))
                        {
                            isQualificationsColumn = true;
                            keywordPositions = new Queue<int>();
                        }

                        foreach (string keyword in keywords)
                        {
                            // add <b> tags around keywords
                            ////rowText = Regex.Replace(rowText, "(" + Regex.Escape(keyword) + ")", "<b>$1</b>", RegexOptions.IgnoreCase | RegexOptions.Singleline);

                            // make whitespace a consistent single space (makes counting words much easier)
                            rowText = Regex.Replace(rowText, @"\s{2,}", " ", RegexOptions.Compiled);

                            if (isQualificationsColumn)
                            {
                                FillKeywordPositions(rowText, keyword, keywordPositions);
                            }
                        }

                        // if we're dealing with the qualifications field and it needs to be trimmed
                        if (keywordPositions != null && rowText.Length > QualificationsFieldLength)
                        {
                            if (keywordPositions.Count > 0)
                            {
                                rowText = CreateSearchResultsSnippets(rowText, keywordPositions);
                            }
                            else
                            {
                                rowText = rowText.Substring(0, QualificationsFieldLength) + Ellipsis;
                            }
                        }

                        ////row[i] = HtmlConverter.ValidateAndCloseTags(rowText); // make sure we didn't cut out any closing <b> tags somehow
                        row[i] = rowText;
                    }
                }
            }

            dt.Columns.Remove(requiredQualificationsColumn);
            dt.Columns.Remove(desiredQualificationsColumn);
        }

        private static DataTable IntersectTables(DataTable dt1, DataTable dt2)
        {
            DataTable dt = dt2.Clone();
            dt.Rows.Clear();

            foreach (DataRow r in dt2.Rows)
            {
                DataRow[] rows = dt1.Select("JobId = " + ((int)r["JobId"]).ToString(CultureInfo.InvariantCulture));
                if (rows.Length > 0)
                {
                    dt.ImportRow(r);
                }
            }

            return dt;
        }

        private DataTable GetKeywordSearchResults()
        {
            DataTable keywordSearchResultsTable = null;

            // if there are keywords, add those in
            if (this.CleanKeywordList.Count == 1)
            {
                keywordSearchResultsTable = DataProvider.Instance().GetKeywordSearchResults(this.CleanKeywordList[0], this.JobGroupId, PortalId);
            }
            else if (this.CleanKeywordList.Count > 0)
            {
                keywordSearchResultsTable = DataProvider.Instance().GetKeywordSearchResults(this.CleanKeywordList, this.JobGroupId, PortalId);
            }

            return keywordSearchResultsTable;
        }
    }
}