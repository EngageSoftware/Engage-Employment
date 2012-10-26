// <copyright file="JobApplication.cs" company="Engage Software">
// Engage: Employment - http://www.engagesoftware.com
// Copyright (c) 2004-2011
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
    using System.Linq;

    using Engage.Dnn.Employment.Data;

    /// <summary>
    /// An application for a job opening
    /// </summary>
    internal class JobApplication
    {
        /// <summary>
        /// Backing field for <see cref="Job"/>
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Job job;

        /// <summary>
        /// Backing field for <see cref="GetDocuments"/>
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<Document> documents;

        /// <summary>
        /// Backing field for <see cref="GetApplicationProperties"/>
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private Dictionary<string, string> applicationProperties;

        /// <summary>
        /// Prevents a default instance of the JobApplication class from being created
        /// </summary>
        private JobApplication()
        {
            this.JobId = -1;
        }

        public int ApplicationId { get; private set; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called only via Eval()")]
        public DateTime AppliedForDate { get; private set; }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Called only via Eval()")]
        public Job Job
        {
            get
            {
                if (this.job == null)
                {
                    this.job = Job.Load(this.JobId);
                }

                return this.job;
            }
        }

        public int JobId { get; private set; }

        public string Message { get; private set; }

        public string SalaryRequirement { get; private set; }

        public int? StatusId { get; set; }

        public int? UserId { get; private set; }

        //// public static bool HasJobBeenAppliedFor(int id)
        //// {
        ////     return DataProvider.Instance().HasJobBeenAppliedFor(id);
        //// }

        public static int Apply(
            int jobId, 
            int? userId, 
            string resumeFileName, 
            string resumeContentType, 
            byte[] resumeData, 
            string coverLetterFileName, 
            string coverLetterContentType, 
            byte[] coverLetterData, 
            string salaryRequirement, 
            string message, 
            int? leadId)
        {
            // we need to insert the applicaiton before the documents in order to satisfy the foriegn key.  BD
            int applicationId = DataProvider.Instance().InsertApplication(jobId, userId, salaryRequirement, message);

            // don't check for an empty resumé here, we'll check inside InsertResume, since resumés can carry over from previous applications. BD
            int resumeId = InsertResume(applicationId, userId, resumeFileName, resumeContentType, resumeData);
            if (Engage.Utility.HasValue(coverLetterFileName) && coverLetterData != null && coverLetterData.Length > 0)
            {
                DataProvider.Instance().InsertDocument(
                    applicationId, userId, coverLetterFileName, coverLetterContentType, coverLetterData, DocumentType.CoverLetter.GetId());
            }

            DataProvider.Instance().InsertApplicationProperty(
                applicationId, 
                ApplicationPropertyDefinition.Lead.GetId(), 
                leadId.HasValue ? leadId.Value.ToString(CultureInfo.InvariantCulture) : null);
            return resumeId;
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

        public static IDataReader GetDocument(int documentId)
        {
            return DataProvider.Instance().GetDocument(documentId);
        }

        public static int GetResumeId(int userId)
        {
            return DataProvider.Instance().GetResumeId(userId);
        }

        public static bool HasAppliedForJob(int jobId, int userId)
        {
            return DataProvider.Instance().HasUserAppliedForJob(jobId, userId);
        }

        public static JobApplication Load(int applicationId)
        {
            using (var dr = DataProvider.Instance().GetApplication(applicationId))
            {
                if (dr.Read())
                {
                    return FillApplication(dr);
                }
            }

            return null;
        }

        public static ReadOnlyCollection<JobApplication> LoadApplicationsForJob(
            int jobId, 
            int? jobGroupId, 
            int? applicationStatusId, 
            int? userStatusId, 
            int? leadId, 
            DateTime? dateFrom, 
            DateTime? dateTo,
            int pageIndex, 
            int? pageSize, 
            out int unpagedCount, 
            bool fillDocumentsAndProperties)
        {
            using (var applicationsDataSet = DataProvider.Instance().GetApplicationsForJob(jobId, jobGroupId, applicationStatusId, userStatusId, leadId, dateFrom, dateTo, pageIndex, pageSize, out unpagedCount, fillDocumentsAndProperties))
            {
                var documentsRelation = applicationsDataSet.Relations.Add(
                    applicationsDataSet.Tables["Applications"].Columns["ApplicationId"],
                    applicationsDataSet.Tables["Documents"].Columns["ApplicationId"]);
                var propertiesRelation = applicationsDataSet.Relations.Add(
                    applicationsDataSet.Tables["Applications"].Columns["ApplicationId"],
                    applicationsDataSet.Tables["Properties"].Columns["ApplicationId"]);

                return (from DataRow applicationRow in applicationsDataSet.Tables["Applications"].Rows
                        select FillApplication(
                            applicationRow,
                            fillDocumentsAndProperties ? applicationRow.GetChildRows(documentsRelation) : null,
                            fillDocumentsAndProperties ? applicationRow.GetChildRows(propertiesRelation) : null))
                        .ToList()
                        .AsReadOnly();
            }
        }

        public static int UpdateApplication(
            int applicationId, 
            int? userId, 
            string resumeFileName, 
            string resumeContentType, 
            byte[] resumeData, 
            string coverLetterFileName, 
            string coverLetterContentType, 
            byte[] coverLetterData, 
            string salaryRequirement, 
            string message, 
            int? leadId)
        {
            DataProvider.Instance().UpdateApplication(applicationId, salaryRequirement, message);
            int? resumeId = null;
            if (Engage.Utility.HasValue(resumeFileName) && resumeData != null && resumeData.Length > 0)
            {
                resumeId = DataProvider.Instance().InsertDocument(
                    applicationId, userId, resumeFileName, resumeContentType, resumeData, DocumentType.Resume.GetId());
            }

            if (Engage.Utility.HasValue(coverLetterFileName) && coverLetterData != null && coverLetterData.Length > 0)
            {
                DataProvider.Instance().InsertDocument(
                    applicationId, userId, coverLetterFileName, coverLetterContentType, coverLetterData, DocumentType.CoverLetter.GetId());
            }

            DataProvider.Instance().UpdateApplicationProperty(
                applicationId, 
                ApplicationPropertyDefinition.Lead.GetId(), 
                leadId.HasValue ? leadId.Value.ToString(CultureInfo.InvariantCulture) : null);

            return resumeId ?? DataProvider.Instance().GetResumeIdForApplication(applicationId);
        }

        /// <summary>
        /// Gets a list of the properties for this application.  Presently, this should only be Lead.
        /// </summary>
        /// <returns>A list of the properties for this application</returns>
        public Dictionary<string, string> GetApplicationProperties()
        {
            if (this.applicationProperties == null)
            {
                var applicationPropertiesTable = DataProvider.Instance().GetApplicationProperties(this.ApplicationId);
                this.SetApplicationProperties(applicationPropertiesTable.Rows.Cast<DataRow>());
            }

            return this.applicationProperties;
        }

        /// <summary>
        /// Gets a list of the <see cref="Document"/>s for this application.
        /// </summary>
        /// <returns>A list of the <see cref="Document"/>s for this application</returns>
        public List<Document> GetDocuments()
        {
            if (this.documents == null)
            {
                this.documents = Document.GetDocuments(this.ApplicationId);
            }

            return this.documents;
        }

        /// <summary>
        /// Saves this instance.  Presently, this only updates <see cref="StatusId"/>, since that is the only settable property.
        /// </summary>
        /// <param name="revisingUserId">The id of the revising user.</param>
        public void Save(int revisingUserId)
        {
            DataProvider.Instance().UpdateApplication(this.ApplicationId, this.StatusId, revisingUserId);
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

        private static JobApplication FillApplication(DataRow applicationRow, DataRow[] documentRows, DataRow[] propertyRows)
        {
            var jobApplication = new JobApplication
                {
                    ApplicationId = (int)applicationRow["ApplicationId"],
                    JobId = (int)applicationRow["JobId"],
                    UserId = applicationRow["UserId"] as int?,
                    AppliedForDate = (DateTime)applicationRow["AppliedDate"],
                    SalaryRequirement = applicationRow["SalaryRequirement"] as string,
                    Message = applicationRow["Message"] as string,
                    StatusId = applicationRow["StatusId"] as int?
                };

            if (documentRows != null && propertyRows != null)
            {
                jobApplication.SetDocuments(documentRows);
                jobApplication.SetApplicationProperties(propertyRows);
            }

            return jobApplication;
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

        /// <summary>
        /// Sets the list of the <see cref="applicationProperties"/> for this application.  Presently, this should only be Lead.
        /// </summary>
        /// <param name="propertyRows">The rows for each application property for this application.</param>
        private void SetApplicationProperties(IEnumerable<DataRow> propertyRows)
        {
            this.applicationProperties = propertyRows.ToDictionary(
                row => (string)row["PropertyName"], 
                row => row["PropertyValue"] is DBNull ? string.Empty : (string)row["PropertyValue"]);
        }

        /// <summary>
        /// Sets the list of <see cref="documents"/> for this application (i.e. the application's resume and [optional] cover letter).
        /// </summary>
        /// <param name="documentRows">The rows for each document related to this application.</param>
        private void SetDocuments(IEnumerable<DataRow> documentRows)
        {
            this.documents = Document.FillDocuments(documentRows);
        }
    }
}