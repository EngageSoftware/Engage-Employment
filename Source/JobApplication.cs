// <copyright file="JobApplication.cs" company="Engage Software">
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
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Data;

    /// <summary>
    /// An application for a job opening
    /// </summary>
    internal class JobApplication
    {
        public int ApplicationId { get; private set; }

        /// <summary>
        /// Backing field for <see cref="Job"/>
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Job job;

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called only via Eval()")]
        public Job Job
        {
            get { return this.job ?? (this.job = Job.Load(this.JobId)); }
        }

        public int JobId { get; private set; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called only via Eval()")]
        public DateTime AppliedForDate { get; private set; }

        public string SalaryRequirement { get; private set; }

        public string Message { get; private set; }

        public int? UserId { get; private set; }

        public int? StatusId { get; set; }

        /// <summary>
        /// Prevents a default instance of the JobApplication class from being created
        /// </summary>
        private JobApplication()
        {
            this.JobId = -1;
        }

        /// <summary>
        /// Gets a list of the <see cref="Document"/>s for this application.
        /// </summary>
        /// <returns>A list of the <see cref="Document"/>s for this application</returns>
        public List<Document> GetDocuments()
        {
            return Document.GetDocuments(this.ApplicationId);
        }

        /// <summary>
        /// Gets a list of the properties for this application.  Presently, this should only be Lead.
        /// </summary>
        /// <returns>A list of the properties for this application</returns>
        public Dictionary<string, string> GetApplicationProperties()
        {
            var properties = new Dictionary<string, string>();
            DataTable dt = DataProvider.Instance().GetApplicationProperties(this.ApplicationId);

            if (dt != null)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    DataRow row = dt.Rows[i];
                    string propertyValue = row["PropertyValue"] is DBNull ? string.Empty : (string)row["PropertyValue"];
                    properties.Add((string)row["PropertyName"], propertyValue);
                }
            }

            return properties;
        }

        /// <summary>
        /// Saves this instance.  Presently, this only updates <see cref="StatusId"/>, since that is the only settable property.
        /// </summary>
        public void Save(int revisingUserId)
        {
            DataProvider.Instance().UpdateApplication(this.ApplicationId, this.StatusId, revisingUserId);
        }

        #region Static Methods

        public static JobApplication Load(int applicationId)
        {
            using (IDataReader dr = DataProvider.Instance().GetApplication(applicationId))
            {
                if (dr.Read())
                {
                    return FillApplication(dr);
                }
            }
            return null;
        }

        public static ReadOnlyCollection<JobApplication> GetAppliedFor(int userId, int? jobGroupId, int portalId)
        {
            var applications = new List<JobApplication>();
            using (IDataReader dr = DataProvider.Instance().GetJobs(userId, jobGroupId, portalId))
            {
                while (dr.Read())
                {
                    applications.Add(FillApplication(dr));
                }
            }

            return applications.AsReadOnly();
        }

        public static bool HasAppliedForJob(int jobId, int userId)
        {
            return DataProvider.Instance().HasUserAppliedForJob(jobId, userId);
        }

        //public static bool HasJobBeenAppliedFor(int id)
        //{
        //    return DataProvider.Instance().HasJobBeenAppliedFor(id);
        //}

        public static int Apply(int jobId, int? userId, string resumeFileName, string resumeContentType, byte[] resumeData, string coverLetterFileName, string coverLetterContentType, byte[] coverLetterData, string salaryRequirement, string message, int? leadId)
        {
            //we need to insert the applicaiton before the documents in order to satisfy the foriegn key.  BD
            int applicationId = DataProvider.Instance().InsertApplication(jobId, userId, salaryRequirement, message);

            //don't check for an empty resumé here, we'll check inside InsertResume, since resumés can carry over from previous applications. BD
            int resumeId = InsertResume(applicationId, userId, resumeFileName, resumeContentType, resumeData);
            if (Engage.Utility.HasValue(coverLetterFileName) && coverLetterData != null && coverLetterData.Length > 0)
            {
                DataProvider.Instance().InsertDocument(applicationId, userId, coverLetterFileName, coverLetterContentType, coverLetterData, DocumentType.CoverLetter.GetId());
            }
            DataProvider.Instance().InsertApplicationProperty(applicationId, ApplicationPropertyDefinition.Lead.GetId(), leadId.HasValue ? leadId.Value.ToString(CultureInfo.InvariantCulture) : null);

            return resumeId;
        }

        public static int UpdateApplication(int applicationId, int? userId, string resumeFileName, string resumeContentType, byte[] resumeData, string coverLetterFileName, string coverLetterContentType, byte[] coverLetterData, string salaryRequirement, string message, int? leadId)
        {
            DataProvider.Instance().UpdateApplication(applicationId, salaryRequirement, message);
            int? resumeId = null;
            if (Engage.Utility.HasValue(resumeFileName) && resumeData != null && resumeData.Length > 0)
            {
                resumeId = DataProvider.Instance().InsertDocument(applicationId, userId, resumeFileName, resumeContentType, resumeData, DocumentType.Resume.GetId());
            }
            if (Engage.Utility.HasValue(coverLetterFileName) && coverLetterData != null && coverLetterData.Length > 0)
            {
                DataProvider.Instance().InsertDocument(applicationId, userId, coverLetterFileName, coverLetterContentType, coverLetterData, DocumentType.CoverLetter.GetId());
            }
            DataProvider.Instance().UpdateApplicationProperty(applicationId, ApplicationPropertyDefinition.Lead.GetId(), leadId.HasValue ? leadId.Value.ToString(CultureInfo.InvariantCulture) : null);

            return resumeId ?? DataProvider.Instance().GetResumeIdForApplication(applicationId);
        }

        //public static DataTable LoadApplications(int? jobGroupId)
        //{
        //    return DataProvider.Instance().GetApplications(jobGroupId, PortalId);
        //}

        public static ReadOnlyCollection<JobApplication> LoadApplicationsForJob(int jobId, int? jobGroupId)
        {
            var applications =new List<JobApplication>();
            using (IDataReader dr = DataProvider.Instance().GetApplicationsForJob(jobId, jobGroupId))
            {
                while (dr.Read())
                {
                    applications.Add(FillApplication(dr));
                }
            }
            return applications.AsReadOnly();
        }

        public static IDataReader GetDocument(int documentId)
        {
            return DataProvider.Instance().GetDocument(documentId);
        }

        public static int GetResumeId(int userId)
        {
            return DataProvider.Instance().GetResumeId(userId);
        }

        private static int InsertResume(int applicationId, int? userId, string fileName, string resumeType, byte[] resume)
        {
            int resumeId;

            if (userId.HasValue && resume.Length == 0)
            {
                resumeId = GetResumeId(userId.Value);
                DataProvider.Instance().AssignDocumentToApplication(applicationId, resumeId);
            }
            else
            {
                resumeId = DataProvider.Instance().InsertResume(applicationId, userId, fileName, resumeType, resume);
            }

            return resumeId;
        }

        private static JobApplication FillApplication(IDataRecord reader)
        {
            var jobApplication = new JobApplication 
            {
                ApplicationId = (int)reader["ApplicationId"],
                JobId = (int)reader["JobId"],
                UserId = reader["UserId"] as int?,
                AppliedForDate = (DateTime)reader["AppliedDate"],
                SalaryRequirement = reader["SalaryRequirement"] as string,
                Message = reader["Message"] as string,
                StatusId = reader["StatusId"] as int?
            };

            return jobApplication;
        }

        #endregion
    } 
}
