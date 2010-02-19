// <copyright file="Job.cs" company="Engage Software">
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
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Diagnostics;
    using System.Globalization;
    using System.Web;
    using Data;

    public class Job
    {
        /// <summary>
        /// Backing field for <see cref="ApplicationUrl"/>
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string applicationUrl;

        /// <summary>
        /// Backing field for <see cref="CategoryName"/>
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string categoryName;

        /// <summary>
        /// Backing field for <see cref="Description"/>
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string description;

        /// <summary>
        /// Backing field for <see cref="DesiredQualifications"/>
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string desiredQualifications;

        /// <summary>
        /// Backing field for <see cref="LocationName"/>
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string locationName;

        /// <summary>
        /// Backing field for <see cref="NotificationEmailAddress"/>
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string notificationEmailAddress;

        /// <summary>
        /// Backing field for <see cref="PostedDate"/>
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string postedDate;

        /// <summary>
        /// Backing field for <see cref="RequiredQualifications"/>
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string requiredQualifications;

        /// <summary>
        /// Backing field for <see cref="StateAbbreviation"/>
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string stateAbbreviation;

        /// <summary>
        /// Backing field for <see cref="StateName"/>
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string stateName;

        /// <summary>
        /// Backing field for <see cref="Title"/>
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private string title;

        /// <summary>
        /// Prevents a default instance of the <see cref="Job"/> class from being created.
        /// </summary>
        private Job()
        {
            this.JobId = -1;
        }

        public static int CurrentJobId
        {
            get
            {
                object jobId = null;
                if (HttpContext.Current != null)
                {
                    jobId = HttpContext.Current.Session["JobId"];
                }

                return jobId == null ? -1 : Convert.ToInt32(jobId, CultureInfo.InvariantCulture);
            }

            set
            {
                if (HttpContext.Current != null)
                {
                    HttpContext.Current.Session["JobId"] = value;
                }
            }
        }

        public string ApplicationUrl
        {
            [DebuggerStepThrough]
            get
            {
                return this.applicationUrl ?? string.Empty;
            }

            [DebuggerStepThrough]
            set
            {
                this.applicationUrl = value;
            }
        }

        public int CategoryId
        {
            get;
            set;
        }

        public string CategoryName
        {
            [DebuggerStepThrough]
            get
            {
                return this.categoryName ?? string.Empty;
            }

            [DebuggerStepThrough]
            set
            {
                this.categoryName = value;
            }
        }

        public string Description
        {
            [DebuggerStepThrough]
            get
            {
                return this.description ?? string.Empty;
            }

            [DebuggerStepThrough]
            set
            {
                this.description = value;
            }
        }

        public string DesiredQualifications
        {
            [DebuggerStepThrough]
            get
            {
                return this.desiredQualifications ?? string.Empty;
            }

            [DebuggerStepThrough]
            set
            {
                this.desiredQualifications = value;
            }
        }

        /// <summary>
        /// Gets or sets the date and time on which this job posting should no longer be displayed to the public.
        /// </summary>
        /// <value>The expiration date of this job posting.</value>
        public DateTime? ExpireDate
        {
            get;
            set;
        }

        public bool IsFilled
        {
            get;
            set;
        }

        public bool IsHot
        {
            get;
            set;
        }

        public int JobId
        {
            get;
            private set;
        }

        public int LocationId
        {
            get;
            set;
        }

        public string LocationName
        {
            [DebuggerStepThrough]
            get
            {
                return this.locationName ?? string.Empty;
            }
        }

        public string NotificationEmailAddress
        {
            [DebuggerStepThrough]
            get
            {
                return this.notificationEmailAddress ?? string.Empty;
            }

            [DebuggerStepThrough]
            set
            {
                this.notificationEmailAddress = value;
            }
        }

        public int PositionId
        {
            get;
            set;
        }

        public string PostedDate
        {
            [DebuggerStepThrough]
            get
            {
                return this.postedDate ?? string.Empty;
            }
        }

        public string RequiredQualifications
        {
            [DebuggerStepThrough]
            get
            {
                return this.requiredQualifications ?? string.Empty;
            }

            [DebuggerStepThrough]
            set
            {
                this.requiredQualifications = value;
            }
        }

        public int SortOrder
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the date and time at which this job posting should start being displayed to the public.
        /// </summary>
        /// <value>The start date of this job posting.</value>
        public DateTime StartDate
        {
            get;
            set;
        }

        public string StateAbbreviation
        {
            [DebuggerStepThrough]
            get
            {
                return this.stateAbbreviation ?? string.Empty;
            }
        }

        public int StateId
        {
            get;
            set;
        }

        public string StateName
        {
            [DebuggerStepThrough]
            get
            {
                return this.stateName ?? string.Empty;
            }
        }

        public string Title
        {
            [DebuggerStepThrough]
            get
            {
                return this.title ?? string.Empty;
            }

            [DebuggerStepThrough]
            set
            {
                this.title = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is active, i.e. whether it is in the time frame between its <see cref="StartDate"/> and <see cref="ExpireDate"/>.
        /// </summary>
        /// <value><c>true</c> if this instance is active; otherwise, <c>false</c>.</value>
        public bool IsActive
        {
            get
            {
                return (!this.ExpireDate.HasValue || this.ExpireDate.Value.ToUniversalTime() > DateTime.UtcNow)
                       && this.StartDate.ToUniversalTime() < DateTime.UtcNow;
            }
        }

        public static bool CanCreateJob(int portalId)
        {
            return DataProvider.Instance().CanCreateJob(portalId);
        }

        public static Job CreateJob()
        {
            return new Job();
        }

        public static void Delete(int jobId)
        {
            DataProvider.Instance().DeleteJob(jobId);
        }

        public static DataTable GetAdminData(int? jobGroupId, int portalId)
        {
            return DataProvider.Instance().GetAdminData(jobGroupId, portalId);
        }

        public static int? GetJobId(int locationId, int positionId)
        {
            return DataProvider.Instance().GetJobId(locationId, positionId);
        }

        public static DataSet GetUnusedAdminData(int? jobGroupId, int portalId)
        {
            return DataProvider.Instance().GetUnusedAdminData(jobGroupId, portalId);
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

        /// <summary>
        /// Loads a listing of jobs for the given criteria.  Only retrieves jobs that have started and not ended.
        /// </summary>
        /// <param name="maximumNumberOfJobs">The maximum number of jobs to retrieve, or <c>null</c> for no limit.</param>
        /// <param name="limitRandomly">
        /// if set to <c>true</c>, retrieves a random segment of the jobs.  This only applies when 
        /// <paramref name="maximumNumberOfJobs"/> is less than the number of jobs retrieved.
        /// </param>
        /// <param name="onlyHotJobs">if set to <c>true</c> only retrieves jobs marked as hot.</param>
        /// <param name="jobGroupId">The job group ID, or <c>null</c> not to restrict the results to a particular job group.</param>
        /// <param name="portalId">The portal ID.</param>
        /// <returns>A collection of the active jobs meeting the given criteria</returns>
        public static ReadOnlyCollection<Job> LoadActiveJobs(int? maximumNumberOfJobs, bool limitRandomly, bool onlyHotJobs, int? jobGroupId, int portalId)
        {
            var jobs = new List<Job>();
            using (IDataReader dr = limitRandomly
                                             ? DataProvider.Instance().GetActiveJobs(onlyHotJobs, jobGroupId, portalId)
                                             : DataProvider.Instance().GetActiveJobs(onlyHotJobs, maximumNumberOfJobs, jobGroupId, portalId))
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
            var jobs = new List<Job>();
            using (IDataReader dr = DataProvider.Instance().GetJobs(jobGroupId, portalId))
            {
                while (dr.Read())
                {
                    jobs.Add(FillJob(dr));
                }
            }

            return jobs.AsReadOnly();
        }

        public void Delete()
        {
            Delete(this.JobId);
        }

        public void Save(int userId, int? jobGroupId, int portalId)
        {
            if (this.JobId == -1)
            {
                this.InsertJob(userId, jobGroupId, portalId);
            }
            else
            {
                this.UpdateJob(userId);
            }
        }

        private static Job FillJob(IDataRecord reader)
        {
            return new Job
                       {
                               JobId = (int)reader["JobId"],
                               Title = reader["JobTitle"].ToString(),
                               PositionId = (int)reader["PositionId"],
                               Description = reader["JobDescription"].ToString(),
                               locationName = reader["LocationName"].ToString(),
                               LocationId = (int)reader["LocationId"],
                               stateName = reader["StateName"].ToString(),
                               stateAbbreviation = reader["StateAbbreviation"].ToString(),
                               StateId = (int)reader["StateId"],
                               postedDate = reader["PostedDate"].ToString(),
                               IsHot = (bool)reader["IsHot"],
                               IsFilled = (bool)reader["IsFilled"],
                               CategoryName = reader["CategoryName"].ToString(),
                               CategoryId = (int)reader["CategoryId"],
                               RequiredQualifications = reader["RequiredQualifications"].ToString(),
                               DesiredQualifications = reader["DesiredQualifications"].ToString(),
                               SortOrder = (int)reader["SortOrder"],
                               NotificationEmailAddress = reader["NotificationEmailAddress"].ToString(),
                               StartDate = (DateTime)reader["StartDate"],
                               ExpireDate = reader["ExpireDate"] as DateTime?,
                               ApplicationUrl = reader["ApplicationUrl"].ToString()
                       };
        }

        private void InsertJob(int userId, int? jobGroupId, int portalId)
        {
            int jobId = DataProvider.Instance().InsertJob(
                    userId, 
                    this.PositionId, 
                    this.LocationId, 
                    this.CategoryId, 
                    this.IsHot, 
                    this.IsFilled, 
                    this.requiredQualifications, 
                    this.desiredQualifications, 
                    this.SortOrder, 
                    portalId, 
                    this.notificationEmailAddress,
                    this.StartDate,
                    this.ExpireDate,
                    this.ApplicationUrl);

            if (jobGroupId.HasValue)
            {
                DataProvider.Instance().AssignJobToJobGroups(jobId, new List<int>(1) { jobGroupId.Value });
            }
        }

        private void UpdateJob(int userId)
        {
            DataProvider.Instance().UpdateJob(
                    userId, 
                    this.JobId, 
                    this.PositionId, 
                    this.LocationId, 
                    this.CategoryId, 
                    this.IsHot, 
                    this.IsFilled, 
                    this.requiredQualifications, 
                    this.desiredQualifications, 
                    this.SortOrder, 
                    this.notificationEmailAddress, 
                    this.StartDate, 
                    this.ExpireDate,
                    this.ApplicationUrl);
        }
    }
}