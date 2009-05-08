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
using System.Web;
using Engage.Dnn.Employment.Data;

namespace Engage.Dnn.Employment
{
    public class Job
    {
        private Job()
        {
        }

        #region Properties
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int _jobId = -1;
        public int JobId
        {
            [DebuggerStepThrough]
            get { return this._jobId; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int _categoryId;        
        public int CategoryId
        {
            [DebuggerStepThrough]
            get { return this._categoryId; }
            [DebuggerStepThrough]
            set { this._categoryId = value; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _categoryName;
        public string CategoryName
        {
            [DebuggerStepThrough]
            set { this._categoryName = value; }
            [DebuggerStepThrough]
            get { return (this._categoryName ?? string.Empty); }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _requiredQualifications;
        public string RequiredQualifications
        {
            [DebuggerStepThrough]
            set { this._requiredQualifications = value; }
            [DebuggerStepThrough]
            get { return (this._requiredQualifications ?? string.Empty); }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _desiredQualifications;
        public string DesiredQualifications
        {
            [DebuggerStepThrough]
            set { this._desiredQualifications = value; }
            [DebuggerStepThrough]
            get { return (this._desiredQualifications ?? string.Empty); }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _title;
        public string Title
        {
            [DebuggerStepThrough]
            set { this._title = value; }
            [DebuggerStepThrough]
            get { return (this._title ?? string.Empty); }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int _positionId;        
        public int PositionId
        {
            [DebuggerStepThrough]
            get { return this._positionId; }
            [DebuggerStepThrough]
            set { this._positionId = value; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _description;
        public string Description
        {
            [DebuggerStepThrough]
            set { this._description = value; }
            [DebuggerStepThrough]
            get { return (this._description ?? string.Empty); }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int _locationId;        
        public int LocationId
        {
            [DebuggerStepThrough]
            get { return this._locationId; }
            [DebuggerStepThrough]
            set { this._locationId = value; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _locationName;
        public string LocationName
        {
            [DebuggerStepThrough]
            private set { this._locationName = value; }
            [DebuggerStepThrough]
            get { return (this._locationName ?? string.Empty); }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _postedDate;
        public string PostedDate
        {
            [DebuggerStepThrough]
            get { return (this._postedDate ?? string.Empty); }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int _stateId;        
        public int StateId
        {
            [DebuggerStepThrough]
            get { return this._stateId; }
            [DebuggerStepThrough]
            set { this._stateId = value; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _stateName;
        public string StateName
        {
            [DebuggerStepThrough]
            private set { this._stateName = value; }
            [DebuggerStepThrough]
            get { return (this._stateName ?? string.Empty); }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _stateAbbreviation;
        public string StateAbbreviation
        {
            [DebuggerStepThrough]
            private set { this._stateAbbreviation = value; }
            [DebuggerStepThrough]
            get { return (this._stateAbbreviation ?? string.Empty); }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool _isHot;
        public bool IsHot
        {
            [DebuggerStepThrough]
            set { this._isHot = value; }
            [DebuggerStepThrough]
            get { return this._isHot; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool _isFilled;
        public bool IsFilled
        {
            [DebuggerStepThrough]
            set { this._isFilled = value; }
            [DebuggerStepThrough]
            get { return this._isFilled; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int _sortOrder;
        public int SortOrder
        {
            [DebuggerStepThrough]
            get { return this._sortOrder; }
            [DebuggerStepThrough]
            set { this._sortOrder = value; }
        }

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string _notificationEmailAddress;
        public string NotificationEmailAddress
        {
            [DebuggerStepThrough]
            get { return this._notificationEmailAddress ?? string.Empty; }
            [DebuggerStepThrough]
            set { this._notificationEmailAddress = value; }
        }

        public static int CurrentJobId
        {
            set
            {
                if (HttpContext.Current != null)
                {
                    HttpContext.Current.Session["JobId"] = value;
                }
            }

            get
            {
                object jobId = null;
                if (HttpContext.Current != null)
                {
                    jobId = HttpContext.Current.Session["JobId"];
                }
                return (jobId == null ? -1 : Convert.ToInt32(jobId, CultureInfo.InvariantCulture));
            }
        }
        #endregion

        public static Job CreateJob()
        {
            return new Job();
        }

        public void Save(int userId, int? jobGroupId, int portalId)
        {
            if (this._jobId == -1)
            {
                InsertJob(userId, jobGroupId, portalId);
            }
            else
            {
                UpdateJob(userId);
            }
        }

        public void Delete()
        {
            Delete(this._jobId);
        }

        public static void Delete(int jobId)
        {
            DataProvider.Instance().DeleteJob(jobId);
        }

        public static Job Load(int id)
        {
            using (IDataReader dr = DataProvider.Instance().GetJob(id))
            {
                if (dr.Read())
                {
                    return FillJob(dr);
                }
            }
            return null;
        }

        public static ReadOnlyCollection<Job> Load(int? maximumNumberOfJobs, bool limitRandomly, bool onlyHotJobs, int? jobGroupId, int portalId)
        {
            List<Job> jobs = new List<Job>();
            using (IDataReader dr = (limitRandomly ? DataProvider.Instance().GetJobs(onlyHotJobs, jobGroupId, portalId) : DataProvider.Instance().GetJobs(onlyHotJobs, maximumNumberOfJobs, jobGroupId, portalId)))
            {
                while (dr.Read())
                {
                    jobs.Add(FillJob(dr));
                }
            }

            if (maximumNumberOfJobs.HasValue && jobs.Count > maximumNumberOfJobs.Value)
            {
                if (limitRandomly)
                {
                    Utility.GetRandomSelection(jobs, maximumNumberOfJobs.Value);
                }
                else
                {
                    jobs.RemoveRange(maximumNumberOfJobs.Value, jobs.Count - maximumNumberOfJobs.Value);
                }
            }

            return jobs.AsReadOnly();
        }

        public static ReadOnlyCollection<Job> LoadAll(int? jobGroupId, int portalId)
        {
            List<Job> jobs = new List<Job>();
            using (IDataReader dr = DataProvider.Instance().GetJobs(jobGroupId, portalId))
            {
                while (dr.Read())
                {
                    jobs.Add(FillJob(dr));
                }
            }

            return jobs.AsReadOnly();
        }

        public static int? GetJobId(int locationId, int positionId)
        {
            return DataProvider.Instance().GetJobId(locationId, positionId);
        }

        private static Job FillJob(IDataRecord reader)
        {
            Job j = new Job();
            j._jobId = (int)reader["JobId"];
            j._title = reader["JobTitle"].ToString();
            j._positionId = (int)reader["PositionId"];
            j._description = reader["JobDescription"].ToString();
            j._locationName = reader["LocationName"].ToString();
            j._locationId = (int)reader["LocationId"];
            j._stateName = reader["StateName"].ToString();
            j._stateAbbreviation = reader["StateAbbreviation"].ToString();
            j._stateId = (int)reader["StateId"];
            j._postedDate = reader["PostedDate"].ToString();
            j._isHot = (bool)reader["IsHot"];
            j._isFilled = (bool)reader["IsFilled"];
            j._categoryName = reader["CategoryName"].ToString();
            j._categoryId = (int)reader["CategoryId"];
            j._requiredQualifications = reader["RequiredQualifications"].ToString();
            j._desiredQualifications = reader["DesiredQualifications"].ToString();
            j._sortOrder = (int)reader["SortOrder"];
            j._notificationEmailAddress = reader["NotificationEmailAddress"].ToString();
            return j;
        }

        private void InsertJob(int userId, int? jobGroupId, int portalId)
        {
            int jobId = DataProvider.Instance().InsertJob(userId, this._positionId, this._locationId, this._categoryId, this._isHot, this._isFilled, this._requiredQualifications, this._desiredQualifications, this._sortOrder, portalId, this._notificationEmailAddress);
            if (jobGroupId.HasValue)
            {
                List<int> jobGroupList = new List<int>(1);
                jobGroupList.Add(jobGroupId.Value);
                DataProvider.Instance().AssignJobToJobGroups(jobId, jobGroupList);
            }
        }

        private void UpdateJob(int userId)
        {
            DataProvider.Instance().UpdateJob(userId, this._jobId, this._positionId, this._locationId, this._categoryId, this._isHot, this._isFilled, this._requiredQualifications, this._desiredQualifications, this._sortOrder, this._notificationEmailAddress);
        }

        public static DataTable GetAdminData(int? jobGroupId, int portalId)
        {
            return DataProvider.Instance().GetAdminData(jobGroupId, portalId);
        }

        public static DataSet GetUnusedAdminData(int? jobGroupId, int portalId)
        {
            return DataProvider.Instance().GetUnusedAdminData(jobGroupId, portalId);
        }

        public static bool CanCreateJob(int portalId)
        {
            return DataProvider.Instance().CanCreateJob(portalId);
        }
    } 
}
