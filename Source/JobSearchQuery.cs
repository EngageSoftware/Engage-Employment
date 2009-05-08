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
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using Engage.Dnn.Employment.Data;

namespace Engage.Dnn.Employment
{
    [Serializable]
    internal class JobSearchQuery
    {
        private const int WordsPerSnippet = 10;
        private const int QualificationsFieldLength = 150;
        private const string Ellipsis = "...";

        #region Properties

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int _id = -1;
        public int Id
        {
            [DebuggerStepThrough]
            get { return _id; }
        }

        //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
        //private int _userId = -1;
        //public int UserId
        //{
        //    [DebuggerStepThrough]
        //    set { _userId = value; }
        //    [DebuggerStepThrough]
        //    get { return _userId; }
        //}

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _description;
        public string Description
        {
            [DebuggerStepThrough]
            set { _description = value; }
            [DebuggerStepThrough]
            get { return _description; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int? _jobPositionId;
        public int? JobPositionId
        {
            [DebuggerStepThrough]
            set { _jobPositionId = value; }
            [DebuggerStepThrough]
            get { return _jobPositionId; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _jobPosition;
        public string JobPosition
        {
            get { return _jobPosition ?? string.Empty; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int? _categoryId;
        public int? CategoryId
        {
            [DebuggerStepThrough]
            get { return _categoryId; }
            [DebuggerStepThrough]
            set { _categoryId = value; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _category;
        public string Category
        {
            get { return _category ?? string.Empty; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int? _stateId;
        public int? StateId
        {
            [DebuggerStepThrough]
            set { _stateId = value; }
            [DebuggerStepThrough]
            get { return _stateId; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _stateName;
        public string StateName
        {
            get { return _stateName ?? string.Empty; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int? _locationId;
        public int? LocationId
        {
            [DebuggerStepThrough]
            set { _locationId = value; }
            [DebuggerStepThrough]
            get { return _locationId; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _locationName;
        public string LocationName
        {
            get { return _locationName ?? string.Empty; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int? _jobGroupId;
        public int? JobGroupId
        {
            [DebuggerStepThrough]
            set { _jobGroupId = value; }
            //[DebuggerStepThrough]
            //get { return _jobGroupId; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _keywords;
        public string Keywords
        {
            [DebuggerStepThrough]
            set
            {
                _keywords = value;
                _keywordList = null;
                _cleanKeywordList = null;
            }
            [DebuggerStepThrough]
            get { return _keywords; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<string> _keywordList;
        public List<string> KeywordList
        {
            get
            {
                if (_keywordList == null)
                {
                    if (Engage.Utility.HasValue(_keywords))
                    {
                        _keywordList = Utility.SplitQuoted(_keywords);
                    }
                    else
                    {
                        _keywordList = new List<string>(0);
                    }
                }
                return _keywordList;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<string> _cleanKeywordList;
        public List<string> CleanKeywordList
        {
            get
            {
                if (_cleanKeywordList == null)
                {
                    _cleanKeywordList = Utility.RemoveCommonWords(KeywordList);
                }
                return _cleanKeywordList;
            }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private DateTime _creationDate;
        public DateTime CreationDate
        {
            [DebuggerStepThrough]
            get { return _creationDate; }
        }

        private static int PortalId
        {
            get { return PortalController.GetCurrentPortalSettings().PortalId; }
        }
        #endregion

        public DataTable Execute()
        {
            DataTable dt = DataProvider.Instance().GetJobSearchResults(_jobPositionId, _categoryId, _locationId, _stateId, _jobGroupId, PortalId);
            DataTable keywordResults = GetKeywordSearchResults();
            if (keywordResults != null)
            {
                if (keywordResults.Rows.Count == 0)
                {
                    //valid keywords supplied no results
                    dt.Rows.Clear();
                }
                else
                {
                    //intersect the results
                    dt = IntersectTables(dt, keywordResults);
                }
            }
            if (dt != null)
            {
                FormatKeywordResults(KeywordList, dt);
            }

            return dt;
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
            DataTable dt = null;

            //if there are keywords, add those in
            if (CleanKeywordList.Count == 1)
            {
                dt = DataProvider.Instance().GetKeywordSearchResults(CleanKeywordList[0], _jobGroupId, PortalId);
            }
            else if (CleanKeywordList.Count > 0)
            {
                dt = DataProvider.Instance().GetKeywordSearchResults(CleanKeywordList, _jobGroupId, PortalId);
            }

            return dt;
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
                                (string)row[jobDescriptionColumn] + Ellipsis + " "
                                + (string)row[requiredQualificationsColumn] + Ellipsis + " "
                                + (string)row[desiredQualificationsColumn], false).Replace(Environment.NewLine, " ").Trim();
                row[requiredQualificationsColumn] = string.Empty;
                row[desiredQualificationsColumn] = string.Empty;
                row[jobDescriptionColumn] = string.Empty;

                for (int i = 0; i < dt.Columns.Count; i++)
                {
                    object cellContents = row[i];
                    string rowText = cellContents as string;
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
                            //add <b> tags around keywords
                            rowText = Regex.Replace(rowText, "(" + Regex.Escape(keyword) + ")", "<b>$1</b>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
                            //make whitespace a consistent single space (makes counting words much easier)
                            rowText = Regex.Replace(rowText, @"\s{2,}", " ", RegexOptions.Compiled);

                            if (isQualificationsColumn)
                            {
                                FillKeywordPositions(rowText, keyword, keywordPositions);
                            }
                        }

                        //if we're dealing with the qualifications field and it needs to be trimmed
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

                        row[i] = HtmlConverter.ValidateAndCloseTags(rowText); //make sure we didn't cut out any closing <b> tags somehow
                    }
                }
            }

            dt.Columns.Remove(requiredQualificationsColumn);
            dt.Columns.Remove(desiredQualificationsColumn);
        }

        private static void FillKeywordPositions(string rowText, string keyword, Queue<int> keywordPositions) {
            int keywordPosition = 0;
            do
            {
                keywordPosition = rowText.IndexOf(keyword, keywordPosition, StringComparison.CurrentCultureIgnoreCase);
                if (keywordPosition > -1)
                {
                    keywordPositions.Enqueue(keywordPosition);
                    keywordPosition += keyword.Length;
                }
            } while (keywordPosition > -1);
        }

        private static string CreateSearchResultsSnippets(string rowText, Queue<int> keywordPositions)
        {
            bool needsEndingEllipsis = false;
            StringBuilder qualificationsText = new StringBuilder(QualificationsFieldLength);

            while (qualificationsText.Length < QualificationsFieldLength && keywordPositions.Count > 0)
            {
                int beginPosition = CalculateBeginPosition(rowText, keywordPositions.Dequeue(), WordsPerSnippet / 2);

                if (beginPosition <= 0)
                {
                    beginPosition = 0;
                }
                else
                {
                    qualificationsText.Append(Ellipsis);
                }

                int endPosition;
                if (keywordPositions.Count > 0)
                {
                    endPosition = CalculateEndPosition(rowText, beginPosition, WordsPerSnippet);
                    qualificationsText.Append(rowText, beginPosition, endPosition - beginPosition + 1);
                    while (keywordPositions.Count > 0 && endPosition >= keywordPositions.Peek())
                    {
                        keywordPositions.Dequeue();
                    }
                }
                else
                {
                    endPosition = beginPosition + (QualificationsFieldLength - qualificationsText.Length);
                    if (endPosition >= rowText.Length)
                    {
                        endPosition = rowText.Length - 1;
                    }
                    qualificationsText.Append(rowText, beginPosition, endPosition - beginPosition + 1);
                }
                needsEndingEllipsis = (endPosition != rowText.Length - 1);
            }
            if (needsEndingEllipsis)
            {
                qualificationsText.Append(Ellipsis);
            }
            return qualificationsText.ToString();
        }

        private static int CalculateBeginPosition(string rowText, int beginPosition, int maxWordCount) {
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
                beginPosition++; //don't start with whitespace
            }
            return beginPosition;
        }

        private static int CalculateEndPosition(string rowText, int beginPosition, int maxWordCount) {
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
                endPosition--; //don't end with whitespace
            }
            return endPosition;
        }

        public void Save(int queryUserId)
        {
            DataProvider.Instance().SaveJobSearchQuery(queryUserId, _description, _jobPositionId, _categoryId, _stateId, _locationId, _keywords, _jobGroupId, PortalId);
        }

        public static ReadOnlyCollection<JobSearchQuery> LoadSearches(int userId, int? jobGroupId)
        {
            List<JobSearchQuery> queries = new List<JobSearchQuery>();
            using (IDataReader dr = DataProvider.Instance().GetJobSearchQueries(userId, jobGroupId, PortalId))
            {
                while (dr.Read())
                {
                    queries.Add(FillJobSearchQuery(dr));
                }
            }

            return queries.AsReadOnly();
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

        private static JobSearchQuery FillJobSearchQuery(IDataRecord dr) {
            JobSearchQuery q = new JobSearchQuery();

            q._id = (int)dr["UserSearchId"];
            q._description = dr["SearchName"].ToString();
            q._jobPosition = dr["JobTitle"].ToString();
            q._jobPositionId = dr["PositionId"] as int?;
            q._categoryId = dr["CategoryId"] as int?;
            q._category = dr["CategoryName"].ToString();
            q._locationName = dr["LocationName"].ToString();
            q._locationId = dr["LocationId"] as int?;
            q._stateName = dr["StateName"].ToString();
            q._stateId = dr["StateId"] as int?;
            q._keywords = dr["Keywords"].ToString();
            q._creationDate = (DateTime)dr["CreationDate"];
            q._jobGroupId = dr["JobGroupId"] as int?;

            return q;
        }

        //public void Delete()
        //{
        //    Delete(_id);
        //}

        public static void Delete(int userSearchId)
        {
            DataProvider.Instance().DeleteJobSearchQuery(userSearchId);
        }
    } 
}
