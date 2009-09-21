// <copyright file="SqlDataProvider.cs" company="Engage Software">
// Engage: Employment
// Copyright (c) 2004-2009
// by Engage Software ( http://www.engagesoftware.com )
// </copyright>
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

namespace Engage.Dnn.Employment.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Text;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework.Providers;
    using Microsoft.ApplicationBlocks.Data;

    internal class SqlDataProvider : DataProvider
    {
        #region Constants

        private const string ProviderType = "data";

        /// <summary>
        /// The length of <c>(n)varchar</c> fields in the database for general text fields.
        /// Does not include State <c>abbreviation</c> or CommonWords <c>locale</c> fields, the length of which is defined in <see cref="AbbreviationLength"/>.
        /// Also does not include Job <c>RequiredQualifications</c> or <c>DesiredQualifications</c>, UserJobSearch <c>Keywords</c> or <c>SearchSql</c>, or Position <c>description</c>, which are text fields.
        /// </summary>
        private const int VarcharLength = 255;

        // disable never used warning
#pragma warning disable 169 

        /// <summary>
        /// The length of <c>(n)varchar</c> fields in the database for small text fields.  Includes State <c>abbreviation</c> and CommonWords <c>locale</c>.
        /// </summary>
        private const int AbbreviationLength = 10;
#pragma warning restore 169

        /// <summary>
        /// The length of the ApplicationProperty propertyValue column, past this length, use the text column, propertyText
        /// </summary>
        private const int ApplicationPropertyValueLength = 3750;

        private const string ModuleQualifier = "EngageEmployment_";

        private const int MaxEmailLength = 320;

        #endregion

        #region Private Members

        private readonly ProviderConfiguration providerConfiguration = ProviderConfiguration.GetProviderConfiguration(ProviderType);

        private readonly string connectionString;

        // private string providerPath;
        private readonly string objectQualifier;

        private readonly string databaseOwner;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlDataProvider"/> class.
        /// </summary>
        public SqlDataProvider()
        {
            var provider = (Provider)this.providerConfiguration.Providers[this.providerConfiguration.DefaultProvider];

            this.connectionString = Config.GetConnectionString();

            if (string.IsNullOrEmpty(this.connectionString))
            {
                this.connectionString = provider.Attributes["connectionString"];
            }

            this.objectQualifier = provider.Attributes["objectQualifier"];
            if (!string.IsNullOrEmpty(this.objectQualifier) && !this.objectQualifier.EndsWith("_", StringComparison.OrdinalIgnoreCase))
            {
                this.objectQualifier += "_";
            }

            this.databaseOwner = provider.Attributes["databaseOwner"];
            if (!string.IsNullOrEmpty(this.databaseOwner) && !this.databaseOwner.EndsWith(".", StringComparison.OrdinalIgnoreCase))
            {
                this.databaseOwner += ".";
            }
        }

        #endregion

        #region Properties

        public string ConnectionString
        {
            get
            {
                return this.connectionString;
            }
        }

        // public string ProviderPath
        // {
        // get
        // {
        // return this.providerPath;
        // }
        // }
        public string ObjectQualifier
        {
            get
            {
                return this.objectQualifier;
            }
        }

        public string DatabaseOwner
        {
            get
            {
                return this.databaseOwner;
            }
        }

        public string NamePrefix
        {
            get
            {
                return this.databaseOwner + this.objectQualifier + ModuleQualifier;
            }
        }

        #endregion

        #region Job

        public override DataTable GetAdminData(int? jobGroupId, int portalId)
        {
            return
                    this.ExecuteDataset(
                            "GetAdminData", 
                            Engage.Utility.CreateIntegerParam("@JobGroupId", jobGroupId), 
                            Engage.Utility.CreateIntegerParam("@PortalId", portalId)).Tables[0];
        }

        public override DataSet GetUnusedAdminData(int? jobGroupId, int portalId)
        {
            DataSet adminData = this.ExecuteDataset(
                    "GetUnusedAdminData", 
                    Engage.Utility.CreateIntegerParam("@JobGroupId", jobGroupId), 
                    Engage.Utility.CreateIntegerParam("@PortalId", portalId));

            adminData.Tables[0].TableName = "States";
            adminData.Tables[1].TableName = "Locations";
            adminData.Tables[2].TableName = "Categories";
            adminData.Tables[3].TableName = "Positions";

            return adminData;
        }

        public override int InsertJob(
            int userId, 
            int positionId, 
            int locationId, 
            int categoryId, 
            bool isHot, 
            bool isFilled, 
            string requiredQualifications, 
            string desiredQualifications, 
            int sortOrder, 
            int portalId, 
            string notificationEmailAddress, 
            DateTime startDate, 
            DateTime? expireDate)
        {
            return (int)this.ExecuteScalar(
                "InsertJob",
                Engage.Utility.CreateIntegerParam("@positionId", positionId),
                Engage.Utility.CreateIntegerParam("@locationId", locationId),
                Engage.Utility.CreateIntegerParam("@categoryId", categoryId),
                Engage.Utility.CreateBitParam("@isHot", isHot),
                Engage.Utility.CreateBitParam("@isFilled", isFilled),
                Engage.Utility.CreateTextParam("@requiredQualifications", requiredQualifications),
                Engage.Utility.CreateTextParam("@desiredQualifications", desiredQualifications),
                Engage.Utility.CreateIntegerParam("@revisingUser", userId),
                Engage.Utility.CreateIntegerParam("@sortOrder", sortOrder),
                Engage.Utility.CreateIntegerParam("@portalId", portalId),
                Engage.Utility.CreateVarcharParam("@notificationEmailAddress", notificationEmailAddress, MaxEmailLength),
                Engage.Utility.CreateDateTimeParam("@startDate", startDate),
                Engage.Utility.CreateDateTimeParam("@expireDate", expireDate));
        }

        public override void UpdateJob(
            int userId, 
            int jobId, 
            int positionId, 
            int locationId, 
            int categoryId, 
            bool isHot, 
            bool isFilled, 
            string requiredQualifications, 
            string desiredQualifications, 
            int sortOrder, 
            string notificationEmailAddress, 
            DateTime startDate, 
            DateTime? expireDate)
        {
            this.ExecuteNonQuery(
                    "UpdateJob", 
                    Engage.Utility.CreateIntegerParam("@jobId", jobId), 
                    Engage.Utility.CreateIntegerParam("@positionId", positionId), 
                    Engage.Utility.CreateIntegerParam("@locationId", locationId), 
                    Engage.Utility.CreateIntegerParam("@categoryId", categoryId), 
                    Engage.Utility.CreateBitParam("@isHot", isHot), 
                    Engage.Utility.CreateBitParam("@isFilled", isFilled), 
                    Engage.Utility.CreateTextParam("@desiredQualifications", desiredQualifications), 
                    Engage.Utility.CreateTextParam("@requiredQualifications", requiredQualifications), 
                    Engage.Utility.CreateIntegerParam("@revisingUser", userId), 
                    Engage.Utility.CreateIntegerParam("@sortOrder", sortOrder), 
                    Engage.Utility.CreateVarcharParam("@notificationEmailAddress", notificationEmailAddress, MaxEmailLength),
                    Engage.Utility.CreateDateTimeParam("@startDate", startDate),
                    Engage.Utility.CreateDateTimeParam("@expireDate", expireDate));
        }

        public override void DeleteJob(int jobId)
        {
            this.ExecuteNonQuery("DeleteJob", Engage.Utility.CreateIntegerParam("@JobId", jobId));
        }

        public override IDataReader GetJob(int jobId)
        {
            var sql = new StringBuilder(512);

            sql.Append("select ");
            sql.Append(" JobId, JobTitle, PositionId, LocationName, LocationId, StateName, StateAbbreviation, StateId, ");
            sql.Append(" PostedDate, RequiredQualifications, DesiredQualifications, NotificationEmailAddress, ");
            sql.Append(" CategoryName, CategoryId, IsHot, IsFilled, JobDescription, SortOrder, StartDate, ExpireDate ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " from {0}vwJobs ", this.NamePrefix);
            sql.Append(" where JobId = @jobId ");

            return SqlHelper.ExecuteReader(
                    this.ConnectionString, CommandType.Text, sql.ToString(), Engage.Utility.CreateIntegerParam("@jobId", jobId));
        }

        public override IDataReader GetActiveJobs(bool onlyHotJobs, int? jobGroupId, int portalId)
        {
            return this.GetActiveJobs(onlyHotJobs, null, jobGroupId, portalId);
        }

        public override IDataReader GetActiveJobs(bool onlyHotJobs, int? maximumNumberOfJobs, int? jobGroupId, int portalId)
        {
            var sql = new StringBuilder(512);

            sql.Append(" SELECT ");
            if (maximumNumberOfJobs.HasValue)
            {
                sql.AppendFormat(CultureInfo.InvariantCulture, " TOP {0} ", maximumNumberOfJobs.Value);
            }

            sql.Append("j.JobId, j.JobTitle, j.PositionId, j.LocationName, j.LocationId, j.StateName, j.StateAbbreviation, j.StateId, ");
            sql.Append("j.RequiredQualifications, j.DesiredQualifications, j.CategoryName, j.CategoryId, ");
            sql.Append("j.IsHot, j.IsFilled, j.PostedDate, j.JobDescription, j.SortOrder, j.NotificationEmailAddress, j.StartDate, j.ExpireDate ");
            sql.Append("FROM ");
            sql.AppendFormat(CultureInfo.InvariantCulture, "{0}vwJobs j ", this.NamePrefix);
            if (jobGroupId.HasValue)
            {
                sql.AppendFormat(CultureInfo.InvariantCulture, " JOIN {0}JobJobGroup jlg ON (j.JobId = jlg.JobId) ", this.NamePrefix);
            }

            sql.Append(" WHERE ");
            sql.Append(" j.IsFilled = 0 ");
            sql.Append(" AND j.PortalId = @portalId ");
            sql.Append(" AND j.StartDate < @now ");
            sql.Append(" AND (j.ExpireDate IS NULL OR j.ExpireDate > @now) ");
            if (jobGroupId.HasValue)
            {
                sql.Append(" AND jlg.jobGroupId = @jobGroupId ");
            }

            if (onlyHotJobs)
            {
                sql.Append(" AND j.IsHot = 1 ");
            }

            sql.Append(" ORDER BY ");
            sql.Append(" j.SortOrder, j.CategoryName, j.JobTitle");

            return SqlHelper.ExecuteReader(
                    this.ConnectionString, 
                    CommandType.Text, 
                    sql.ToString(), 
                    Engage.Utility.CreateIntegerParam("@jobGroupId", jobGroupId), 
                    Engage.Utility.CreateIntegerParam("@portalId", portalId),
                    Engage.Utility.CreateDateTimeParam("@now", DateTime.Now));
        }

        public override IDataReader GetJobs(int? jobGroupId, int portalId)
        {
            var sql = new StringBuilder(512);

            sql.Append(" select ");
            sql.Append(" j.JobId, j.JobTitle, j.PositionId, j.LocationName, j.LocationId, j.StateName, j.StateAbbreviation, j.StateId, ");
            sql.Append(" j.RequiredQualifications, j.DesiredQualifications, j.CategoryName, j.CategoryId, j.NotificationEmailAddress, ");
            sql.Append(" j.IsHot, j.IsFilled, j.PostedDate, j.JobDescription, j.SortOrder, j.RevisingUser, j.RevisionDate, j.StartDate, j.ExpireDate ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " from {0}vwJobs j", this.NamePrefix);
            if (jobGroupId.HasValue)
            {
                sql.AppendFormat(CultureInfo.InvariantCulture, " join {0}JobJobGroup jlg on (j.JobId = jlg.JobId) ", this.NamePrefix);
            }

            sql.Append(" where j.PortalId = @portalId ");
            if (jobGroupId.HasValue)
            {
                sql.Append(" and jlg.jobGroupId = @jobGroupId ");
            }

            sql.Append(" order by ");
            sql.Append(" j.SortOrder, j.CategoryName, j.JobTitle ");

            return SqlHelper.ExecuteReader(
                    this.ConnectionString, 
                    CommandType.Text, 
                    sql.ToString(), 
                    Engage.Utility.CreateIntegerParam("@jobGroupId", jobGroupId), 
                    Engage.Utility.CreateIntegerParam("@portalId", portalId));
        }

        public override int? GetJobId(int locationId, int positionId)
        {
            var sql = new StringBuilder(512);

            sql.Append(" select top 1 JobId ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " from {0}vwJobs ", this.NamePrefix);
            sql.Append(" where ");
            sql.Append(" LocationId = @locationId ");
            sql.Append(" and PositionId = @positionId ");

            return
                    SqlHelper.ExecuteScalar(
                            this.ConnectionString, 
                            CommandType.Text, 
                            sql.ToString(), 
                            Engage.Utility.CreateIntegerParam("@locationId", locationId), 
                            Engage.Utility.CreateIntegerParam("@positionId", positionId)) as int?;
        }

        /// <summary>
        /// Determines whether this portal has been setup to create jobs;
        /// that is, whether there is at least one <see cref="Location"/>, <see cref="Position"/>, and <see cref="Category"/> defined.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <returns>
        /// <c>true</c> if this portal has been setup to create jobs; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanCreateJob(int portalId)
        {
            var sql = new StringBuilder(256);
            sql.AppendFormat("select top 1 null from {0}vwLocations l", this.NamePrefix);
            sql.AppendFormat(" join {0}vwPositions p on (p.PortalId = l.PortalId)", this.NamePrefix);
            sql.AppendFormat(" join {0}vwCategories c on (c.PortalId = p.PortalId)", this.NamePrefix);
            sql.Append("where l.portalId = @portalId");

            using (IDataReader dr = SqlHelper.ExecuteReader(this.ConnectionString, CommandType.Text, sql.ToString(), Engage.Utility.CreateIntegerParam("@portalId", portalId)))
            {
                return dr.Read();
            }
        }

        #endregion

        #region UserJob

        public override IDataReader GetJobs(int? userId, int? jobGroupId, int portalId)
        {
            var sql = new StringBuilder(1024);

            sql.Append(" select ");
            sql.Append(" a.JobId, JobTitle, LocationName, StateName, ");
            sql.Append(" RequiredQualifications, DesiredQualifications, CategoryName, DisplayName, StatusId, ");
            sql.Append(" IsHot, PostedDate, AppliedDate, ApplicationId, SortOrder, UserId, SalaryRequirement, Message ");
            sql.Append(" from ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " {0}vwApplications a ", this.NamePrefix);
            if (jobGroupId.HasValue)
            {
                sql.AppendFormat(CultureInfo.InvariantCulture, " join {0}JobJobGroup jlg on (a.JobId = jlg.JobId) ", this.NamePrefix);
            }

            sql.Append(" where (UserId = @userId OR (userId IS NULL AND @userId IS NULL)) ");
            sql.Append(" and PortalId = @portalId ");
            if (jobGroupId.HasValue)
            {
                sql.Append(" and jlg.jobGroupId = @jobGroupId ");
            }

            sql.Append(" order by ");
            sql.Append(" AppliedDate desc ");

            return SqlHelper.ExecuteReader(
                    this.ConnectionString, 
                    CommandType.Text, 
                    sql.ToString(), 
                    Engage.Utility.CreateIntegerParam("@userId", userId), 
                    Engage.Utility.CreateIntegerParam("@jobGroupId", jobGroupId), 
                    Engage.Utility.CreateIntegerParam("@portalId", portalId));
        }

        public override bool HasUserAppliedForJob(int jobId, int userId)
        {
            var sql = new StringBuilder(512);

            sql.Append(" select top 1 NULL ");
            sql.Append(" from ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " {0}vwApplications ", this.NamePrefix);
            sql.Append(" where ");
            sql.Append(" JobId = @jobId ");
            sql.Append(" and UserId = @userId");

            using (
                    IDataReader dr = SqlHelper.ExecuteReader(
                            this.ConnectionString, 
                            CommandType.Text, 
                            sql.ToString(), 
                            Engage.Utility.CreateIntegerParam("@jobId", jobId), 
                            Engage.Utility.CreateIntegerParam("@userId", userId)))
            {
                return dr.Read();
            }
        }

        #endregion

        #region JobApplication

        public override IDataReader GetApplication(int applicationId)
        {
            var sql = new StringBuilder(147);
            sql.Append(" select ApplicationId, UserId, DisplayName, JobId, AppliedDate, SalaryRequirement, Message, StatusId ");
            sql.AppendFormat(" from {0}vwApplications ", this.NamePrefix);
            sql.Append(" where ApplicationId = @applicationId ");

            return SqlHelper.ExecuteReader(
                    this.ConnectionString, CommandType.Text, sql.ToString(), Engage.Utility.CreateIntegerParam("@applicationId", applicationId));
        }

        public override int InsertApplication(int jobId, int? userId, string salaryRequirement, string message)
        {
            var sql = new StringBuilder(512);

            sql.AppendFormat(CultureInfo.InvariantCulture, " insert {0}vwApplications ", this.NamePrefix);
            sql.Append(" (UserId, JobId, AppliedDate, SalaryRequirement, Message) values ");
            sql.Append(" (@userId, @jobId, getdate(), @salaryRequirement, @message) ");
            sql.Append(" SELECT SCOPE_IDENTITY() ");

            return (int)(decimal)SqlHelper.ExecuteScalar(
                this.ConnectionString, 
                CommandType.Text, 
                sql.ToString(), 
                Engage.Utility.CreateIntegerParam("@userId", userId), 
                Engage.Utility.CreateIntegerParam("@jobId", jobId), 
                Engage.Utility.CreateVarcharParam("@salaryRequirement", salaryRequirement, VarcharLength), 
                Engage.Utility.CreateVarcharParam("@message", message, VarcharLength));
        }

        /// <summary>
        /// Updates the <see cref="ApplicationStatus"/> of the given <see cref="JobApplication"/>.
        /// </summary>
        /// <param name="applicationId">The application id.</param>
        /// <param name="statusId">The status id.</param>
        /// <param name="revisingUserId">The ID of the user updating this application.</param>
        public override void UpdateApplication(int applicationId, int? statusId, int revisingUserId)
        {
            this.ExecuteNonQuery(
                    "UpdateApplication", 
                    Engage.Utility.CreateIntegerParam("@applicationId", applicationId), 
                    Engage.Utility.CreateIntegerParam("@statusId", statusId), 
                    Engage.Utility.CreateIntegerParam("@revisingUserId", revisingUserId));
        }

        public override void UpdateApplication(int applicationId, string salaryRequirement, string message)
        {
            var sql = new StringBuilder(512);

            sql.AppendFormat(CultureInfo.InvariantCulture, " update {0}vwApplications ", this.NamePrefix);
            sql.Append(" set SalaryRequirement = @salaryRequirement, ");
            sql.Append(" Message = @message ");
            sql.Append(" where ApplicationId = @applicationId ");

            SqlHelper.ExecuteNonQuery(
                    this.ConnectionString, 
                    CommandType.Text, 
                    sql.ToString(), 
                    Engage.Utility.CreateIntegerParam("@applicationId", applicationId), 
                    Engage.Utility.CreateVarcharParam("@salaryRequirement", salaryRequirement, VarcharLength), 
                    Engage.Utility.CreateVarcharParam("@message", message, VarcharLength));
        }

        public override DataTable GetApplications(int? jobGroupId, int portalId)
        {
            var sql = new StringBuilder(512);

            sql.Append(" select ");
            sql.Append(
                    " a.AppliedDate, a.DisplayName, a.JobId, a.JobTitle, a.LocationName, a.ApplicationId, a.UserId, a.SalaryRequirement, a.Message ");
            sql.Append(" from ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " {0}vwApplications a ", this.NamePrefix);
            if (jobGroupId.HasValue)
            {
                sql.AppendFormat(CultureInfo.InvariantCulture, " join {0}JobJobGroup jlg on (a.JobId = jlg.JobId) ", this.NamePrefix);
            }

            sql.Append(" where a.PortalId = @portalId ");
            if (jobGroupId.HasValue)
            {
                sql.Append(" and jlg.jobGroupId = @jobGroupId ");
            }

            sql.Append(" order by ");
            sql.Append(" a.AppliedDate desc ");

            return
                    SqlHelper.ExecuteDataset(
                            this.ConnectionString, 
                            CommandType.Text, 
                            sql.ToString(), 
                            Engage.Utility.CreateIntegerParam("@jobGroupId", jobGroupId), 
                            Engage.Utility.CreateIntegerParam("@portalId", portalId)).Tables[0];
        }

        public override IDataReader GetApplicationsForJob(int jobId, int? jobGroupId)
        {
            var sql = new StringBuilder(512);

            sql.Append(" select ");
            sql.Append(" AppliedDate, DisplayName, a.JobId, JobTitle, LocationName, ApplicationId, UserId, SalaryRequirement, Message, StatusId ");
            sql.Append(" from ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " {0}vwApplications a ", this.NamePrefix);
            if (jobGroupId.HasValue)
            {
                sql.AppendFormat(CultureInfo.InvariantCulture, " join {0}JobJobGroup jlg on (a.JobId = jlg.JobId) ", this.NamePrefix);
            }

            sql.Append(" where a.jobId = @jobId ");
            if (jobGroupId.HasValue)
            {
                sql.Append(" and jobGroupId = @jobGroupId ");
            }

            sql.Append(" order by ");
            sql.Append(" AppliedDate desc ");

            return SqlHelper.ExecuteReader(
                    this.ConnectionString, 
                    CommandType.Text, 
                    sql.ToString(), 
                    Engage.Utility.CreateIntegerParam("@jobId", jobId), 
                    Engage.Utility.CreateIntegerParam("@jobGroupId", jobGroupId));
        }

        #endregion

        #region Document

        public override DataTable GetApplicationDocuments(int applicationId)
        {
            var sql = new StringBuilder(512);

            sql.Append(" select ");
            sql.Append(" DocumentId, [UserId], [FileName], [ContentType], [ContentLength], [ResumeData], [RevisionDate], [DocumentTypeId] ");
            sql.Append(" from ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " {0}vwDocuments ", this.NamePrefix);
            sql.Append(" where ");
            sql.Append(" ApplicationId = @applicationId");

            return
                    SqlHelper.ExecuteDataset(
                            this.ConnectionString, 
                            CommandType.Text, 
                            sql.ToString(), 
                            Engage.Utility.CreateIntegerParam("@applicationId", applicationId)).Tables[0];
        }

        public override IDataReader GetDocumentType(string documentTypeName, int? portalId)
        {
            var sql = new StringBuilder(128);
            sql.AppendFormat(CultureInfo.InvariantCulture, "select documentTypeId, description from {0}DocumentType ", this.NamePrefix);
            sql.Append(" where description = @documentTypeName ");
            sql.Append(" and (PortalId = @portalId OR (PortalId IS NULL and @portalId IS NULL)) ");

            return SqlHelper.ExecuteReader(
                    this.ConnectionString, 
                    CommandType.Text, 
                    sql.ToString(), 
                    Engage.Utility.CreateVarcharParam("@documentTypeName", documentTypeName), 
                    Engage.Utility.CreateIntegerParam("@portalId", portalId));
        }

        public override IDataReader GetDocumentType(int documentTypeId)
        {
            var sql = new StringBuilder(128);
            sql.AppendFormat(
                    CultureInfo.InvariantCulture, 
                    "select documentTypeId, description from {0}DocumentType where documentTypeId = @documentTypeId", 
                    this.NamePrefix);

            return SqlHelper.ExecuteReader(
                    this.ConnectionString, CommandType.Text, sql.ToString(), Engage.Utility.CreateIntegerParam("@documentTypeId", documentTypeId));
        }

        public override int InsertResume(int applicationId, int? userId, string fileName, string resumeType, byte[] resume)
        {
            return this.InsertDocument(applicationId, userId, fileName, resumeType, resume, DocumentType.Resume.GetId());
        }

        public override int InsertDocument(int applicationId, int? userId, string fileName, string contentType, byte[] document, int documentTypeId)
        {
            return this.InsertDocument(applicationId, userId, fileName, contentType, document, documentTypeId, true);
        }

        public override int InsertDocument(int applicationId, int? userId, string fileName, string contentType, byte[] document, int documentTypeId, bool removeOldDocuments)
        {
            var sql = new StringBuilder(512);

            sql.AppendFormat(CultureInfo.InvariantCulture, " insert {0}Document ", this.NamePrefix);
            sql.Append(" (UserId, FileName, ContentType, ContentLength, ResumeData, RevisionDate, DocumentTypeId) values ");
            sql.Append(" (@userId, @fileName, @contentType, @contentLength, @resumeData, getdate(), @documentTypeId) ");
            sql.Append(" SELECT SCOPE_IDENTITY() ");

            var resumeId = (int)(decimal)SqlHelper.ExecuteScalar(
                this.ConnectionString, 
                CommandType.Text, 
                sql.ToString(), 
                Engage.Utility.CreateIntegerParam("@userId", userId), 
                Engage.Utility.CreateVarcharParam("@fileName", fileName, VarcharLength), 
                Engage.Utility.CreateVarcharParam("@contentType", contentType), 
                Engage.Utility.CreateIntegerParam("@contentLength", document.Length), 
                Engage.Utility.CreateImageParam("@resumeData", document), 
                Engage.Utility.CreateIntegerParam("@documentTypeId", documentTypeId));

            if (removeOldDocuments)
            {
                this.RemoveDocumentAssignments(applicationId, documentTypeId);
            }

            this.AssignDocumentToApplication(applicationId, resumeId);
            return resumeId;
        }

        /// <summary>
        /// Gets information about the document with the given ID.  Columns include DocumentId/<see cref="int"/>, UserId/<see cref="int"/>, FileName/<see cref="string"/>, ContentType/<see cref="string"/>, ContentLength/<see cref="int"/>, ResumeData/<see cref="byte"/>[], RevisionDate/<see cref="DateTime"/>, DocumentTypeId/<see cref="int"/>
        /// </summary>
        /// <param name="documentId">The document ID.</param>
        /// <returns>
        /// Information about the document with the given ID
        /// </returns>
        public override IDataReader GetDocument(int documentId)
        {
            return this.ExecuteReader("GetDocument", Engage.Utility.CreateIntegerParam("@documentId", documentId));
        }

        public override int GetResumeId(int userId)
        {
            return this.GetDocumentId(DocumentType.Resume.GetId(), userId);
        }

        public override int GetResumeIdForApplication(int applicationId)
        {
            int id = Null.NullInteger;

            var sql = new StringBuilder(255);
            sql.AppendFormat(CultureInfo.InvariantCulture, "select d.ResumeId from {0}Document d ", this.NamePrefix);
            sql.AppendFormat(CultureInfo.InvariantCulture, " join {0}ApplicationDocument ad ON (d.ResumeId = d.ResumeId) ", this.NamePrefix);
            sql.Append(" where ad.ApplicationId = @applicationId ");
            sql.Append(" and d.DocumentTypeId = @documentTypeId ");

            object resumeId = SqlHelper.ExecuteScalar(
                    this.ConnectionString, 
                    CommandType.Text, 
                    sql.ToString(), 
                    Engage.Utility.CreateIntegerParam("@applicationId", applicationId), 
                    Engage.Utility.CreateIntegerParam("@documentTypeId", DocumentType.Resume.GetId()));

            if (resumeId is int)
            {
                id = (int)resumeId;
            }

            return id;
        }

        public override int GetDocumentId(int documentTypeId, int userId)
        {
            int id = Null.NullInteger;

            var sql = new StringBuilder(255);
            sql.AppendFormat(CultureInfo.InvariantCulture, "select max(ResumeId) from {0}Document ", this.NamePrefix);
            sql.Append(" where UserId = @userId ");
            sql.Append(" and DocumentTypeId = @documentTypeId ");

            object resumeId = SqlHelper.ExecuteScalar(
                    this.ConnectionString, 
                    CommandType.Text, 
                    sql.ToString(), 
                    Engage.Utility.CreateIntegerParam("@userId", userId), 
                    Engage.Utility.CreateIntegerParam("@documentTypeId", documentTypeId));

            if (resumeId is int)
            {
                id = (int)resumeId;
            }

            return id;
        }

        public override void AssignDocumentToApplication(int applicationId, int resumeId)
        {
            var sql = new StringBuilder(128);
            sql.AppendFormat(CultureInfo.InvariantCulture, "INSERT INTO {0}ApplicationDocument (ApplicationId, ResumeId)", this.NamePrefix);
            sql.Append("values (@applicationId, @resumeId)");

            SqlHelper.ExecuteNonQuery(
                    this.ConnectionString, 
                    CommandType.Text, 
                    sql.ToString(), 
                    Engage.Utility.CreateIntegerParam("@applicationId", applicationId), 
                    Engage.Utility.CreateIntegerParam("@resumeId", resumeId));
        }

        private void RemoveDocumentAssignments(int applicationId, int documentTypeId)
        {
            var sql = new StringBuilder(128);
            sql.AppendFormat(CultureInfo.InvariantCulture, "DELETE {0}ApplicationDocument ", this.NamePrefix);
            sql.AppendFormat(CultureInfo.InvariantCulture, " FROM {0}ApplicationDocument ad ", this.NamePrefix);
            sql.AppendFormat(CultureInfo.InvariantCulture, " JOIN {0}Document d ON (ad.ResumeId = d.ResumeId) ", this.NamePrefix);
            sql.Append(" WHERE ad.ApplicationId = @applicationId ");
            sql.Append(" AND d.DocumentTypeId = @documentTypeId ");

            SqlHelper.ExecuteNonQuery(
                    this.ConnectionString, 
                    CommandType.Text, 
                    sql.ToString(), 
                    Engage.Utility.CreateIntegerParam("@applicationId", applicationId), 
                    Engage.Utility.CreateIntegerParam("@documentTypeId", documentTypeId));
        }

        /// <summary>
        /// Gets the IDs of the job groups that can view the document with the given ID.
        /// </summary>
        /// <param name="documentId">The document ID.</param>
        /// <returns>
        /// A reader representing a list of the IDs of the job groups that can view the document with the given ID
        /// </returns>
        public override IDataReader GetDocumentJobGroups(int documentId)
        {
            return this.ExecuteReader("GetDocumentJobGroups", Engage.Utility.CreateIntegerParam("@documentId", documentId));
        }

        #endregion

        #region State

        public override IDataReader GetStates(int? jobGroupId, int portalId)
        {
            return this.ExecuteReader(
                    "GetStates", 
                    Engage.Utility.CreateIntegerParam("@jobGroupId", jobGroupId), 
                    Engage.Utility.CreateIntegerParam("@portalId", portalId));
        }

        public override IDataReader GetState(int stateId)
        {
            var sql = new StringBuilder(512);

            sql.Append(" select ");
            sql.Append(" StateId, StateName , StateAbbreviation ");
            sql.Append(" from ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " {0}vwStates ", this.NamePrefix);
            sql.Append(" where ");
            sql.Append(" StateId = @stateId");

            return SqlHelper.ExecuteReader(
                    this.ConnectionString, CommandType.Text, sql.ToString(), Engage.Utility.CreateIntegerParam("@stateId", stateId));
        }

        public override void UpdateState(int id, string name, string abbreviation)
        {
            this.ExecuteNonQuery(
                    "UpdateState", 
                    Engage.Utility.CreateIntegerParam("@stateId", id), 
                    Engage.Utility.CreateVarcharParam("@stateName", name, VarcharLength), 
                    Engage.Utility.CreateVarcharParam("@stateAbbreviation", abbreviation, AbbreviationLength));
        }

        public override void InsertState(string name, string abbreviation, int portalId)
        {
            this.ExecuteNonQuery(
                    "InsertState", 
                    Engage.Utility.CreateIntegerParam("@portalId", portalId), 
                    Engage.Utility.CreateVarcharParam("@stateName", name, VarcharLength), 
                    Engage.Utility.CreateVarcharParam("@stateAbbreviation", abbreviation, AbbreviationLength));
        }

        public override int? GetStateId(string name, int portalId)
        {
            var sql = new StringBuilder(512);

            sql.Append(" select top 1 StateId ");
            sql.Append(" from ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " {0}vwStates ", this.NamePrefix);
            sql.Append(" where ");
            sql.Append(" StateName = @stateName ");
            sql.Append(" and PortalId = @portalId ");

            object stateId = SqlHelper.ExecuteScalar(
                    this.ConnectionString, 
                    CommandType.Text, 
                    sql.ToString(), 
                    Engage.Utility.CreateVarcharParam("@stateName", name, VarcharLength), 
                    Engage.Utility.CreateIntegerParam("@portalId", portalId));

            return ConvertReturnValueToInt(stateId);
        }

        public override bool IsStateUsed(int stateId)
        {
            var sql = new StringBuilder(87);
            sql.AppendFormat(CultureInfo.InvariantCulture, " select top 1 NULL from {0}vwLocations where StateId = @stateId", this.NamePrefix);

            using (
                    IDataReader dr = SqlHelper.ExecuteReader(
                            this.ConnectionString, CommandType.Text, sql.ToString(), Engage.Utility.CreateIntegerParam("@stateId", stateId)))
            {
                return dr.Read();
            }
        }

        public override void DeleteState(int stateId)
        {
            this.ExecuteNonQuery("DeleteState", Engage.Utility.CreateIntegerParam("@stateId", stateId));
        }

        #endregion

        #region Location

        public override IDataReader GetLocations(int? jobGroupId, int portalId)
        {
            return this.ExecuteReader(
                    "GetLocations", 
                    Engage.Utility.CreateIntegerParam("@portalId", portalId), 
                    Engage.Utility.CreateIntegerParam("@jobGroupId", jobGroupId));
        }

        public override IDataReader GetLocation(int locationId)
        {
            var sql = new StringBuilder(512);

            sql.Append(" select ");
            sql.Append(" LocationId, LocationName, StateId, StateName, StateAbbreviation ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " from {0}vwLocations ", this.NamePrefix);
            sql.Append(" where LocationId = @locationId");

            return SqlHelper.ExecuteReader(
                    this.ConnectionString, CommandType.Text, sql.ToString(), Engage.Utility.CreateIntegerParam("@locationId", locationId));
        }

        public override void UpdateLocation(int locationId, string description, int stateId)
        {
            this.ExecuteNonQuery(
                    "UpdateLocation", 
                    Engage.Utility.CreateVarcharParam("@locationName", description, VarcharLength), 
                    Engage.Utility.CreateIntegerParam("@stateId", stateId), 
                    Engage.Utility.CreateIntegerParam("@locationId", locationId));
        }

        public override void InsertLocation(string locationName, int stateId, int portalId)
        {
            this.ExecuteNonQuery(
                    "InsertLocation", 
                    Engage.Utility.CreateVarcharParam("@locationName", locationName, VarcharLength), 
                    Engage.Utility.CreateIntegerParam("@stateId", stateId), 
                    Engage.Utility.CreateIntegerParam("@portalId", portalId));
        }

        public override int? GetLocationId(string locationName, int? stateId, int portalId)
        {
            var sql = new StringBuilder(512);

            sql.Append(" select top 1 LocationId ");
            sql.Append(" from ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " {0}vwLocations ", this.NamePrefix);
            sql.Append(" where ");
            sql.Append(" locationName = @locationName ");
            sql.Append(" and PortalId = @portalId ");
            if (stateId.HasValue)
            {
                sql.Append(" and StateId = @stateId ");
            }

            object locationId = SqlHelper.ExecuteScalar(
                    this.ConnectionString, 
                    CommandType.Text, 
                    sql.ToString(), 
                    Engage.Utility.CreateVarcharParam("@locationName", locationName, VarcharLength), 
                    Engage.Utility.CreateIntegerParam("@portalId", portalId), 
                    Engage.Utility.CreateIntegerParam("@stateId", stateId));

            return ConvertReturnValueToInt(locationId);
        }

        public override bool IsLocationUsed(int locationId)
        {
            var sql = new StringBuilder(90);
            sql.AppendFormat(CultureInfo.InvariantCulture, " select top 1 LocationId from {0}vwJobs where LocationId = @locationId", this.NamePrefix);

            using (
                    IDataReader dr = SqlHelper.ExecuteReader(
                            this.ConnectionString, CommandType.Text, sql.ToString(), Engage.Utility.CreateIntegerParam("@locationId", locationId)))
            {
                return dr.Read();
            }
        }

        public override void DeleteLocation(int locationId)
        {
            this.ExecuteNonQuery("DeleteLocation", Engage.Utility.CreateIntegerParam("@locationId", locationId));
        }

        #endregion

        #region Category

        public override IDataReader GetCategories(int? jobGroupId, int portalId)
        {
            return this.ExecuteReader(
                    "GetCategories", 
                    Engage.Utility.CreateIntegerParam("@jobGroupId", jobGroupId), 
                    Engage.Utility.CreateIntegerParam("@portalId", portalId));
        }

        public override IDataReader GetCategory(int categoryId)
        {
            var sql = new StringBuilder(512);

            sql.Append(" select ");
            sql.Append(" CategoryId, CategoryName ");
            sql.Append(" from ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " {0}vwCategories ", this.NamePrefix);
            sql.Append(" where ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " CategoryId = {0}", categoryId);

            return SqlHelper.ExecuteReader(this.ConnectionString, CommandType.Text, sql.ToString());
        }

        public override void UpdateCategory(int categoryId, string description)
        {
            this.ExecuteNonQuery(
                    "UpdateCategory", 
                    Engage.Utility.CreateVarcharParam("@categoryName", description, VarcharLength), 
                    Engage.Utility.CreateIntegerParam("@categoryId", categoryId));
        }

        public override void InsertCategory(string categoryName, int portalId)
        {
            this.ExecuteNonQuery(
                    "InsertCategory", 
                    Engage.Utility.CreateVarcharParam("@categoryName", categoryName, VarcharLength), 
                    Engage.Utility.CreateIntegerParam("@portalId", portalId));
        }

        public override int? GetCategoryId(string categoryName, int portalId)
        {
            var sql = new StringBuilder(128);
            sql.AppendFormat(
                    CultureInfo.InvariantCulture, 
                    "select top 1 CategoryId from {0}vwCategories where CategoryName = @name and PortalId = @portalId", 
                    this.NamePrefix);

            object categoryId = SqlHelper.ExecuteScalar(
                    this.ConnectionString, 
                    CommandType.Text, 
                    sql.ToString(), 
                    Engage.Utility.CreateVarcharParam("@name", categoryName, VarcharLength), 
                    Engage.Utility.CreateIntegerParam("@portalId", portalId));

            return ConvertReturnValueToInt(categoryId);
        }

        public override bool IsCategoryUsed(int categoryId)
        {
            var sql = new StringBuilder(512);

            sql.Append(" select top 1 null ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " from {0}vwJobs ", this.NamePrefix);
            sql.AppendFormat(CultureInfo.InvariantCulture, " where CategoryId = {0}", categoryId);

            using (IDataReader dr = SqlHelper.ExecuteReader(this.ConnectionString, CommandType.Text, sql.ToString()))
            {
                return dr.Read();
            }
        }

        public override void DeleteCategory(int categoryId)
        {
            this.ExecuteNonQuery("DeleteCategory", Engage.Utility.CreateIntegerParam("@categoryId", categoryId));
        }

        #endregion

        #region Position

        public override IDataReader GetPositions(int? jobGroupId, int portalId)
        {
            return this.ExecuteReader(
                    "GetPositions", 
                    Engage.Utility.CreateIntegerParam("@jobGroupId", jobGroupId), 
                    Engage.Utility.CreateIntegerParam("@portalId", portalId));
        }

        public override IDataReader GetPosition(int positionId)
        {
            var sql = new StringBuilder(512);

            sql.Append(" select ");
            sql.Append(" PositionId, JobTitle, JobDescription ");
            sql.Append(" from ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " {0}vwPositions ", this.NamePrefix);
            sql.Append(" where ");
            sql.Append(" PositionId = @positionId ");

            return SqlHelper.ExecuteReader(
                    this.ConnectionString, CommandType.Text, sql.ToString(), Engage.Utility.CreateIntegerParam("@positionId", positionId));
        }

        public override void UpdatePosition(int positionId, string jobTitle, string jobDescription)
        {
            this.ExecuteNonQuery(
                    "UpdatePosition", 
                    Engage.Utility.CreateVarcharParam("@jobTitle", jobTitle, VarcharLength), 
                    Engage.Utility.CreateTextParam("@jobDescription", jobDescription), 
                    Engage.Utility.CreateIntegerParam("@positionId", positionId));
        }

        public override void InsertPosition(string jobTitle, string jobDescription, int portalId)
        {
            this.ExecuteNonQuery(
                    "InsertPosition", 
                    Engage.Utility.CreateVarcharParam("@jobTitle", jobTitle, VarcharLength), 
                    Engage.Utility.CreateTextParam("@jobDescription", jobDescription), 
                    Engage.Utility.CreateIntegerParam("@portalId", portalId));
        }

        public override int? GetPositionId(string name, int portalId)
        {
            var sql = new StringBuilder(512);

            sql.Append(" select PositionId ");
            sql.Append(" from ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " {0}vwPositions ", this.NamePrefix);
            sql.Append(" where ");
            sql.Append(" JobTitle = @jobTitle ");
            sql.Append(" and PortalId = @portalId ");

            object positionId = SqlHelper.ExecuteScalar(
                    this.ConnectionString, 
                    CommandType.Text, 
                    sql.ToString(), 
                    Engage.Utility.CreateVarcharParam("@jobTitle", name, VarcharLength), 
                    Engage.Utility.CreateIntegerParam("@portalId", portalId));

            return ConvertReturnValueToInt(positionId);
        }

        public override bool IsPositionUsed(int positionId)
        {
            var sql = new StringBuilder(97);
            sql.AppendFormat(CultureInfo.InvariantCulture, " SELECT TOP 1 NULL FROM {0}vwJobs WHERE PositionId = @positionId", this.NamePrefix);

            using (
                    IDataReader dr = SqlHelper.ExecuteReader(
                            this.ConnectionString, CommandType.Text, sql.ToString(), Engage.Utility.CreateIntegerParam("@positionId", positionId)))
            {
                return dr.Read();
            }
        }

        public override void DeletePosition(int positionId)
        {
            this.ExecuteNonQuery("DeletePosition", Engage.Utility.CreateIntegerParam("@positionId", positionId));
        }

        #endregion

        #region UserStatus & ApplicationStatus

        public override DataTable GetUserStatuses(int portalId)
        {
            var sql = new StringBuilder(512);

            sql.Append(" select ");
            sql.Append(" UserStatusId, StatusName ");
            sql.Append(" from ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " {0}vwUserStatuses ", this.NamePrefix);
            sql.Append(" where PortalId = @portalId ");
            sql.Append(" order by ");
            sql.Append(" StatusName ");

            return SqlHelper.ExecuteDataset(this.ConnectionString, CommandType.Text, sql.ToString(), Engage.Utility.CreateIntegerParam("@portalId", portalId)).Tables[0];
        }

        public override DataTable GetUserStatus(int statusId)
        {
            var sql = new StringBuilder(512);

            sql.Append(" select ");
            sql.Append(" UserStatusId, StatusName ");
            sql.Append(" from ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " {0}vwUserStatuses ", this.NamePrefix);
            sql.Append(" where ");
            sql.Append(" UserStatusId = @statusId ");

            return SqlHelper.ExecuteDataset(this.ConnectionString, CommandType.Text, sql.ToString(), Engage.Utility.CreateIntegerParam("@statusId", statusId)).Tables[0];
        }

        public override void UpdateUserStatus(int statusId, string statusName)
        {
            var sql = new StringBuilder(512);

            sql.AppendFormat(CultureInfo.InvariantCulture, " update {0}vwUserStatuses ", this.NamePrefix);
            sql.Append(" set ");
            sql.Append(" StatusName = @status ");
            sql.Append(" where ");
            sql.Append(" UserStatusId = @statusId ");

            SqlHelper.ExecuteNonQuery(
                    this.ConnectionString, 
                    CommandType.Text, 
                    sql.ToString(), 
                    Engage.Utility.CreateVarcharParam("@status", statusName, VarcharLength), 
                    Engage.Utility.CreateIntegerParam("@statusId", statusId));
        }

        public override void InsertUserStatus(string statusName, int portalId)
        {
            var sql = new StringBuilder(512);

            sql.AppendFormat(CultureInfo.InvariantCulture, " insert {0}vwUserStatuses ", this.NamePrefix);
            sql.Append(" (StatusName, PortalId ) ");
            sql.Append(" values (@status, @portalId)");

            SqlHelper.ExecuteNonQuery(
                    this.ConnectionString, 
                    CommandType.Text, 
                    sql.ToString(), 
                    Engage.Utility.CreateVarcharParam("@status", statusName, VarcharLength), 
                    Engage.Utility.CreateIntegerParam("@portalId", portalId));
        }

        public override void DeleteUserStatus(int statusId)
        {
            SqlHelper.ExecuteNonQuery(
                    this.ConnectionString, 
                    CommandType.Text, 
                    "delete " + this.NamePrefix + "vwUserStatuses where UserStatusId = @statusId", 
                    Engage.Utility.CreateIntegerParam("@statusId", statusId));
        }

        public override int? GetUserStatusId(string status, int portalId)
        {
            var sql = new StringBuilder(128);
            sql.AppendFormat(
                    CultureInfo.InvariantCulture, 
                    "select top 1 UserStatusId from {0}vwUserStatuses where StatusName = @status and PortalId = @portalId", 
                    this.NamePrefix);

            object statusId = SqlHelper.ExecuteScalar(
                    this.ConnectionString, 
                    CommandType.Text, 
                    sql.ToString(), 
                    Engage.Utility.CreateVarcharParam("@status", status, VarcharLength), 
                    Engage.Utility.CreateIntegerParam("@portalId", portalId));

            return ConvertReturnValueToInt(statusId);
        }

        public override bool IsUserStatusUsed(int statusId)
        {
            var sql = new StringBuilder(299);
            sql.AppendFormat(CultureInfo.InvariantCulture, " declare @propertyId int{0}", Environment.NewLine);
            sql.AppendFormat(
                    CultureInfo.InvariantCulture, 
                    " select @propertyId = PropertyDefinitionId from {0}{1}ProfilePropertyDefinition where PropertyCategory = 'Engage: Employment' and PropertyName = '{2}'{3}", 
                    this.DatabaseOwner, 
                    this.ObjectQualifier, 
                    Utility.UserStatusPropertyName, 
                    Environment.NewLine);
            sql.AppendFormat(
                    CultureInfo.InvariantCulture, 
                    " select top 1 null as EmptyColumn from {0}{1}UserProfile where PropertyDefinitionID = @propertyId and PropertyValue = @statusId", 
                    this.DatabaseOwner, 
                    this.ObjectQualifier);
            using (
                    IDataReader dr = SqlHelper.ExecuteReader(
                            this.ConnectionString, CommandType.Text, sql.ToString(), Engage.Utility.CreateIntegerParam("@statusId", statusId)))
            {
                return dr.Read();
            }
        }

        /// <summary>
        /// Determines whether the specified status is used by any applications
        /// </summary>
        /// <param name="statusId">The status id.</param>
        /// <returns>
        /// <c>true</c> if the application status is used by any application; otherwise, <c>false</c>.
        /// </returns>
        public override bool IsApplicationStatusUsed(int statusId)
        {
            using (IDataReader dr = this.ExecuteReader("GetApplicationsByStatus", Engage.Utility.CreateIntegerParam("@statusId", statusId)))
            {
                return dr.Read();
            }
        }

        #endregion

        #region JobGroup

        public override DataTable GetJobGroups(int portalId)
        {
            return SqlHelper.ExecuteDataset(
                this.ConnectionString, 
                CommandType.Text, 
                string.Format(CultureInfo.InvariantCulture, "SELECT JobGroupId, [Name] FROM {0}JobGroup WHERE PortalId = @portalId ORDER BY [Name]", this.NamePrefix), 
                Engage.Utility.CreateIntegerParam("@portalId", portalId)).Tables[0];
        }

        public override DataTable GetJobGroup(int jobGroupId)
        {
            return SqlHelper.ExecuteDataset(
                this.ConnectionString, 
                CommandType.Text, 
                string.Format(CultureInfo.InvariantCulture, "SELECT JobGroupId, [Name] FROM {0}JobGroup WHERE JobGroupId = @jobGroupId", this.NamePrefix), 
                Engage.Utility.CreateIntegerParam("@jobGroupId", jobGroupId)).Tables[0];
        }

        public override void UpdateJobGroup(int jobGroupId, string jobGroupName)
        {
            var sql = new StringBuilder(128);
            sql.AppendFormat(CultureInfo.InvariantCulture, "UPDATE {0}JobGroup ", this.NamePrefix);
            sql.Append(" SET [Name] = @jobGroupName ");
            sql.Append(" WHERE JobGroupId = @jobGroupId ");

            SqlHelper.ExecuteNonQuery(
                    this.ConnectionString, 
                    CommandType.Text, 
                    sql.ToString(), 
                    Engage.Utility.CreateIntegerParam("@jobGroupId", jobGroupId), 
                    Engage.Utility.CreateVarcharParam("@jobGroupName", jobGroupName, 255));
        }

        public override void InsertJobGroup(string jobGroupName, int portalId)
        {
            var sql = new StringBuilder(86);
            sql.AppendFormat(CultureInfo.InvariantCulture, "INSERT INTO {0}JobGroup ([NAME], [PortalId])", this.NamePrefix);
            sql.Append("VALUES (@jobGroupName, @portalId)");

            SqlHelper.ExecuteNonQuery(
                    this.ConnectionString, 
                    CommandType.Text, 
                    sql.ToString(), 
                    Engage.Utility.CreateVarcharParam("@jobGroupName", jobGroupName, VarcharLength), 
                    Engage.Utility.CreateIntegerParam("@portalId", portalId));
        }

        public override bool IsJobGroupUsed(int jobGroupId)
        {
            var sql = new StringBuilder(97);
            sql.AppendFormat(CultureInfo.InvariantCulture, "select top 1 JobGroupId from {0}JobJobGroup where JobGroupId = @jobGroupId", this.NamePrefix);

            using (IDataReader dr = SqlHelper.ExecuteReader(this.ConnectionString, CommandType.Text, sql.ToString(), Engage.Utility.CreateIntegerParam("@jobGroupId", jobGroupId)))
            {
                return dr.Read();
            }
        }

        public override bool IsJobGroupNameUsed(string jobGroupName, int portalId)
        {
            var sql = new StringBuilder(97);
            sql.AppendFormat(CultureInfo.InvariantCulture, "select top 1 JobGroupId from {0}JobGroup where Name = @jobGroupName and PortalId = @portalId", this.NamePrefix);

            using (IDataReader dr = SqlHelper.ExecuteReader(
                            this.ConnectionString, 
                            CommandType.Text, 
                            sql.ToString(), 
                            Engage.Utility.CreateVarcharParam("@jobGroupName", jobGroupName, VarcharLength), 
                            Engage.Utility.CreateIntegerParam("@portalId", portalId)))
            {
                return dr.Read();
            }
        }

        public override void DeleteJobGroup(int jobGroupId)
        {
            SqlHelper.ExecuteNonQuery(
                    this.ConnectionString, 
                    CommandType.Text, 
                    string.Format(CultureInfo.InvariantCulture, "DELETE {0}JobGroup WHERE JobGroupId = @jobGroupId", this.NamePrefix), 
                    Engage.Utility.CreateIntegerParam("@jobGroupId", jobGroupId));
        }

        #endregion

        #region JobJobGroup

        public override void AssignJobToJobGroups(int jobId, List<int> jobGroups)
        {
            using (var conn = new SqlConnection(this.ConnectionString))
            {
                conn.Open();
                using (SqlTransaction tran = conn.BeginTransaction())
                {
                    // Remove all JobGroup assignments for this job
                    SqlHelper.ExecuteNonQuery(
                            tran, 
                            CommandType.Text, 
                            String.Format(CultureInfo.InvariantCulture, "DELETE FROM {0}JobJobGroup WHERE JobId = @JobId", this.NamePrefix), 
                            Engage.Utility.CreateIntegerParam("@JobId", jobId));

                    // Add back the selected JobGroup assignments
                    if (jobGroups != null && jobGroups.Count > 0)
                    {
                        var sql = new StringBuilder(114);
                        sql.AppendFormat(CultureInfo.InvariantCulture, "INSERT {0}JobJobGroup (JobId, JobGroupId) ", this.NamePrefix);
                        sql.Append("VALUES (@jobId, @jobGroupId) ");

                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.Parameters.Add(Engage.Utility.CreateIntegerParam("@jobId", jobId));
                            cmd.Parameters.Add(Engage.Utility.CreateIntegerParam("@jobGroupId", jobGroups[0]));
                            cmd.CommandText = sql.ToString();
                            cmd.CommandType = CommandType.Text;
                            cmd.Transaction = tran;
                            cmd.Prepare();
                            cmd.ExecuteNonQuery();

                            for (int i = 1; i < jobGroups.Count; i++)
                            {
                                cmd.Parameters["@jobGroupId"].Value = jobGroups[i];
                                cmd.ExecuteNonQuery();
                            }

                            // cmd.Transaction.Commit();
                        }
                    }

                    tran.Commit();
                    conn.Close();
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="DataSet"/> with all jobs in a <see cref="DataTable"/> named "Jobs," and all job groups in a <see cref="DataTable"/> named "JobGroup."
        /// They are collected by a <see cref="DataRelation"/> named "JobJobGroup."
        /// </summary>
        /// <returns>A <see cref="DataSet"/> with all jobs and their assigned groups.</returns>
        public override DataSet GetAssignedJobGroups(int portalId)
        {
            var jobsSql = new StringBuilder(1024);
            jobsSql.Append("select ");
            jobsSql.Append("JobId, JobTitle, LocationName, StateName, ");
            jobsSql.Append("RequiredQualifications, DesiredQualifications, CategoryName, ");
            jobsSql.Append("IsHot, PostedDate, IsFilled ");
            jobsSql.AppendFormat(CultureInfo.InvariantCulture, "from {0}vwJobs ", this.NamePrefix);
            jobsSql.Append("where PortalId = @portalId ");

            var jobGroupSql = new StringBuilder(128);
            jobGroupSql.Append("select ");
            jobGroupSql.Append("jjg.JobId, jg.JobGroupId, jg.Name ");
            jobGroupSql.AppendFormat(CultureInfo.InvariantCulture, "from {0}JobGroup jg ", this.NamePrefix);
            jobGroupSql.AppendFormat(CultureInfo.InvariantCulture, "join {0}JobJobGroup jjg on (jjg.JobGroupId = jg.JobGroupId) ", this.NamePrefix);
            jobGroupSql.Append("where PortalId = @portalId ");

            using (var conn = new SqlConnection(this.ConnectionString))
            {
                var ds = new DataSet { Locale = CultureInfo.InvariantCulture };
                using (var adapter = new SqlDataAdapter(jobsSql.ToString(), conn))
                {
                    conn.Open();
                    adapter.SelectCommand.Parameters.Add(Engage.Utility.CreateIntegerParam("@portalId", portalId));
                    adapter.Fill(ds, "Jobs");
                }

                using (var adapter = new SqlDataAdapter(jobGroupSql.ToString(), conn))
                {
                    adapter.SelectCommand.Parameters.Add(Engage.Utility.CreateIntegerParam("@portalId", portalId));
                    adapter.Fill(ds, "JobGroups");
                    conn.Close();
                }

                ds.Relations.Add("JobJobGroup", ds.Tables["Jobs"].Columns["JobId"], ds.Tables["JobGroups"].Columns["JobId"]);
                return ds;
            }
        }

        public override bool IsJobInGroup(int jobId, int jobGroupId)
        {
            var sql = new StringBuilder(128);
            sql.AppendFormat(CultureInfo.InvariantCulture, "select top 1 jobgroupid from {0}JobJobGroup where jobId = @jobId and jobgroupid = @jobGroupId", this.NamePrefix);

            using (IDataReader dr = SqlHelper.ExecuteReader(
                this.ConnectionString, 
                CommandType.Text, 
                sql.ToString(), 
                Engage.Utility.CreateIntegerParam("@jobId", jobId), 
                Engage.Utility.CreateIntegerParam("@jobGroupId", jobGroupId)))
            {
                return dr.Read();
            }
        }

        #endregion

        #region JobSearchQuery

        public override IDataReader GetJobSearchQuery(int jobSearchQueryId)
        {
            var sql = new StringBuilder(512);

            sql.Append(" select ");
            sql.Append(" UserSearchId, SearchName, JobTitle, LocationName, StateName, Keywords, CreationDate, LocationId, StateId, CategoryId, CategoryName, PositionId, JobGroupId ");
            sql.Append(" from ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " {0}vwSavedSearches ", this.NamePrefix);
            sql.Append(" where UserSearchId = @searchId ");

            return SqlHelper.ExecuteReader(
                    this.ConnectionString, CommandType.Text, sql.ToString(), Engage.Utility.CreateIntegerParam("@searchId", jobSearchQueryId));
        }

        public override IDataReader GetJobSearchQueries(int userId, int? jobGroupId, int portalId)
        {
            var sql = new StringBuilder(512);

            sql.Append(" select ");
            sql.Append(" UserSearchId, SearchName, JobTitle, LocationName, StateName, ");
            sql.Append(" Keywords, CreationDate, JobGroupId, PositionId, LocationId, StateId, CategoryId, CategoryName ");
            sql.Append(" from ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " {0}vwSavedSearches ", this.NamePrefix);
            sql.Append(" where UserId = @userId ");
            sql.Append(" and PortalId = @portalId ");
            if (jobGroupId.HasValue)
            {
                sql.Append(" and JobGroupId = @jobGroupId ");
            }

            sql.Append(" order by ");
            sql.Append(" CreationDate desc ");

            return SqlHelper.ExecuteReader(
                    this.ConnectionString, 
                    CommandType.Text, 
                    sql.ToString(), 
                    Engage.Utility.CreateIntegerParam("@userId", userId), 
                    Engage.Utility.CreateIntegerParam("@jobGroupId", jobGroupId), 
                    Engage.Utility.CreateIntegerParam("@portalId", portalId));
        }

        public override DataTable GetJobSearchResults(int? positionId, int? categoryId, int? locationId, int? stateId, int? jobGroupId, int portalId)
        {
            var sql = new StringBuilder(512);

            sql.Append(" SELECT ");
            sql.Append(" j.JobId, j.JobTitle, j.LocationName, j.StateName, j.StateAbbreviation, j.CategoryName, ");
            sql.Append(" j.PostedDate, j.RequiredQualifications, j.DesiredQualifications, j.JobDescription ");
            sql.Append(" FROM ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " {0}vwJobs j ", this.NamePrefix);
            if (jobGroupId.HasValue)
            {
                sql.AppendFormat(CultureInfo.InvariantCulture, " JOIN {0}JobJobGroup jjg ON (j.JobId = jjg.JobId) ", this.NamePrefix);
            }

            sql.Append(" WHERE j.IsFilled = 0 ");
            sql.Append(" AND j.PortalId = @portalId ");
            sql.Append(" AND j.StartDate < @now ");
            sql.Append(" AND (j.ExpireDate IS NULL OR j.ExpireDate > @now) ");

            if (positionId.HasValue)
            {
                sql.Append(" AND j.PositionId = @positionId ");
            }

            if (categoryId.HasValue)
            {
                sql.Append(" AND j.CategoryId = @categoryId ");
            }

            if (locationId.HasValue)
            {
                sql.Append(" AND j.LocationId = @locationId ");
            }

            if (stateId.HasValue)
            {
                sql.Append(" AND j.StateId = @stateId ");
            }

            if (jobGroupId.HasValue)
            {
                sql.Append(" AND jjg.jobGroupId = @jobGroupId ");
            }

            sql.Append(" ORDER BY ");
            sql.Append(" j.PostedDate DESC");

            return SqlHelper.ExecuteDataset(
                this.ConnectionString, 
                CommandType.Text, 
                sql.ToString(), 
                Engage.Utility.CreateIntegerParam("@positionId", positionId), 
                Engage.Utility.CreateIntegerParam("@locationId", locationId), 
                Engage.Utility.CreateIntegerParam("@stateId", stateId), 
                Engage.Utility.CreateIntegerParam("@jobGroupId", jobGroupId), 
                Engage.Utility.CreateIntegerParam("@portalId", portalId), 
                Engage.Utility.CreateIntegerParam("@categoryId", categoryId),
                Engage.Utility.CreateDateTimeParam("@now", DateTime.Now)).Tables[0];
        }

        public override DataTable GetKeywordSearchResults(string keyword, int? jobGroupId, int portalId)
        {
            var sql = new StringBuilder(256);
            sql.Append(" SELECT ");
            sql.Append(" j.JobId, j.JobTitle, j.LocationName, j.StateName, j.StateAbbreviation, j.CategoryName, ");
            sql.Append(" j.PostedDate, j.RequiredQualifications, j.DesiredQualifications, j.JobDescription ");
            sql.Append(" FROM ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " {0}vwJobs j ", this.NamePrefix);
            if (jobGroupId.HasValue)
            {
                sql.AppendFormat(CultureInfo.InvariantCulture, " JOIN {0}JobJobGroup jjg ON (j.JobId = jjg.JobId) ", this.NamePrefix);
            }

            sql.Append(" WHERE j.IsFilled = 0 ");
            sql.Append(" AND j.PortalId = @portalId ");
            sql.Append(" AND j.StartDate < @now ");
            sql.Append(" AND (j.ExpireDate IS NULL OR j.ExpireDate > @now) ");
            sql.Append(" AND (j.RequiredQualifications LIKE @keyword ");
            sql.Append(" OR j.DesiredQualifications LIKE @keyword ");
            sql.Append(" OR j.JobDescription LIKE @keyword ");
            sql.Append(" OR j.JobTitle LIKE @keyword) ");
            if (jobGroupId.HasValue)
            {
                sql.Append(" AND jjg.jobGroupId = @jobGroupId ");
            }

            sql.Append(" ORDER BY ");
            sql.Append(" j.PostedDate DESC");

            return SqlHelper.ExecuteDataset(
                this.ConnectionString, 
                CommandType.Text, 
                sql.ToString(), 
                Engage.Utility.CreateVarcharParam("@keyword", String.Format(CultureInfo.CurrentCulture, "%{0}%", keyword)), 
                Engage.Utility.CreateIntegerParam("@jobGroupId", jobGroupId),
                Engage.Utility.CreateIntegerParam("@portalId", portalId),
                Engage.Utility.CreateDateTimeParam("@now", DateTime.Now)).Tables[0];
        }

        public override DataTable GetKeywordSearchResults(List<string> keywords, int? jobGroupId, int portalId)
        {
            DataTable searchResultsTable = null;
            var sql = new StringBuilder(1024);
            if (keywords != null)
            {
                using (var conn = new SqlConnection(this.ConnectionString))
                {
                    conn.Open();
                    using (SqlCommand cmd = conn.CreateCommand())
                    {
                        sql.Append(" SELECT ");
                        sql.Append(" j.JobId, j.JobTitle, j.LocationName, j.StateName, j.StateAbbreviation, j.CategoryName, ");
                        sql.Append(" j.PostedDate, j.RequiredQualifications, j.DesiredQualifications, j.JobDescription ");
                        sql.Append(" FROM ");
                        sql.AppendFormat(CultureInfo.InvariantCulture, " {0}vwJobs j ", this.NamePrefix);
                        if (jobGroupId.HasValue)
                        {
                            sql.AppendFormat(CultureInfo.InvariantCulture, " JOIN {0}JobJobGroup jjg ON (j.JobId = jjg.JobId) ", this.NamePrefix);
                        }

                        sql.Append(" WHERE j.IsFilled = 0 ");
                        sql.Append(" AND j.PortalId = @portalId ");
                        sql.Append(" AND j.StartDate < @now ");
                        sql.Append(" AND (j.ExpireDate IS NULL OR j.ExpireDate > @now) ");

                        for (int i = 0; i < keywords.Count; i++)
                        {
                            sql.AppendFormat(CultureInfo.InvariantCulture, " AND (j.RequiredQualifications LIKE @keyword{0} ", i);
                            sql.AppendFormat(CultureInfo.InvariantCulture, " OR j.DesiredQualifications LIKE @keyword{0} ", i);
                            sql.AppendFormat(CultureInfo.InvariantCulture, " OR j.JobDescription LIKE @keyword{0} ", i);
                            sql.AppendFormat(CultureInfo.InvariantCulture, " OR j.JobTitle LIKE @keyword{0}) ", i);
                        }

                        if (jobGroupId.HasValue)
                        {
                            sql.Append(" AND jjg.jobGroupId = @jobGroupId ");
                            cmd.Parameters.Add(Engage.Utility.CreateIntegerParam("@jobGroupId", jobGroupId));
                        }

                        sql.Append(" ORDER BY ");
                        sql.Append(" j.PostedDate");

                        cmd.CommandText = sql.ToString();
                        cmd.Parameters.Add(Engage.Utility.CreateIntegerParam("@portalId", portalId));
                        cmd.Parameters.Add(Engage.Utility.CreateDateTimeParam("@now", DateTime.Now));
                        for (int i = 0; i < keywords.Count; i++)
                        {
                            cmd.Parameters.Add(Engage.Utility.CreateVarcharParam(
                                "@keyword" + i.ToString(CultureInfo.InvariantCulture),
                                String.Format(CultureInfo.InvariantCulture, "%{0}%", keywords[i])));
                        }

                        searchResultsTable = new DataTable { Locale = CultureInfo.InvariantCulture };
                        var dataAdapter = new SqlDataAdapter(cmd);
                        dataAdapter.Fill(searchResultsTable);
                    }

                    conn.Close();
                }
            }

            return searchResultsTable;
        }

        public override void SaveJobSearchQuery(
                int userId, 
                string searchName, 
                int? positionId, 
                int? categoryId, 
                int? stateId, 
                int? locationId, 
                string keywords, 
                int? jobGroupId, 
                int portalId)
        {
            if (!Engage.Utility.HasValue(keywords))
            {
                keywords = null;
            }

            this.ExecuteNonQuery(
                    "SaveJobSearchQuery", 
                    Engage.Utility.CreateIntegerParam("@userId", userId), 
                    Engage.Utility.CreateVarcharParam("@name", searchName, VarcharLength), 
                    Engage.Utility.CreateIntegerParam("@positionId", positionId), 
                    Engage.Utility.CreateIntegerParam("@stateId", stateId), 
                    Engage.Utility.CreateIntegerParam("@locationId", locationId), 
                    Engage.Utility.CreateVarcharParam("@keywords", keywords), 
                    Engage.Utility.CreateIntegerParam("@jobGroupId", jobGroupId), 
                    Engage.Utility.CreateIntegerParam("@portalId", portalId), 
                    Engage.Utility.CreateIntegerParam("@categoryId", categoryId));
        }

        public override void DeleteJobSearchQuery(int jobSearchQueryId)
        {
            SqlHelper.ExecuteNonQuery(
                    this.ConnectionString, CommandType.Text, "delete " + this.NamePrefix + "UserJobSearch where UserSearchId = " + jobSearchQueryId);
        }

        #endregion

        #region Lookup/Lead

        public override bool IsPropertyValueUsed(int propertyId, string value)
        {
            var sql = new StringBuilder(228);
            sql.AppendFormat(CultureInfo.InvariantCulture, "select top 1 ApplicationPropertyId from {0}UserJobProperty ", this.NamePrefix);
            sql.Append(" where ApplicationPropertyId = @ApplicationPropertyId and (propertyValue = @propertyValue or (propertyValue is null and propertyText like @propertyText))");

            return SqlHelper.ExecuteReader(
                this.ConnectionString, 
                CommandType.Text, 
                sql.ToString(), 
                Engage.Utility.CreateIntegerParam("@ApplicationPropertyId", propertyId), 
                Engage.Utility.CreateVarcharParam("@propertyValue", value, ApplicationPropertyValueLength), 
                Engage.Utility.CreateTextParam("@propertyText", value)).Read();
        }

        public override IDataReader GetApplicationProperty(string name, int? portalId)
        {
            var sql = new StringBuilder(128);
            sql.Append("select [ApplicationPropertyId],[DataType],[DefaultValue],[PropertyName],[Required],[ViewOrder],[Visible] ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " from {0}ApplicationProperty ", this.NamePrefix);
            sql.Append(" where PropertyName = @PropertyName ");
            sql.Append(" and (PortalId = @portalId OR (PortalId IS NULL and @portalId IS NULL)) ");

            return SqlHelper.ExecuteReader(
                    this.ConnectionString, 
                    CommandType.Text, 
                    sql.ToString(), 
                    Engage.Utility.CreateVarcharParam("@PropertyName", name), 
                    Engage.Utility.CreateIntegerParam("@portalId", portalId));
        }

        public override DataTable GetApplicationProperties(int applicationId)
        {
            var sql = new StringBuilder(128);
            sql.Append("select ujp.[ApplicationId],ujp.[ApplicationPropertyId],ujp.[Visibility],ap.[PropertyName], ");
            sql.Append(" CASE WHEN ujp.[PropertyValue] IS NULL THEN ujp.[PropertyText] ELSE ujp.[PropertyValue] END AS PropertyValue ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " from {0}UserJobProperty ujp ", this.NamePrefix);
            sql.AppendFormat(CultureInfo.InvariantCulture, " join {0}ApplicationProperty ap ON (ujp.ApplicationPropertyId = ap.ApplicationPropertyId) ", this.NamePrefix);
            sql.Append(" where ApplicationId = @ApplicationId ");

            return SqlHelper.ExecuteDataset(
                this.ConnectionString, 
                CommandType.Text, 
                sql.ToString(), 
                Engage.Utility.CreateIntegerParam("@ApplicationId", applicationId)).Tables[0];
        }

        public override void InsertApplicationProperty(int applicationId, int propertyId, string value)
        {
            var sql = new StringBuilder(512);
            sql.AppendFormat(CultureInfo.InvariantCulture, "INSERT INTO {0}UserJobProperty ", this.NamePrefix);
            sql.Append(" ([ApplicationId], [ApplicationPropertyId], [PropertyValue], [PropertyText], [Visibility], [LastUpdatedDate]) ");
            sql.Append(" VALUES(@ApplicationId, @ApplicationPropertyId, @PropertyValue, @PropertyText, @Visibility, @LastUpdatedDate) ");

            SqlParameter propertyValue, propertyText;
            if (value != null && value.Length > ApplicationPropertyValueLength)
            {
                propertyText = Engage.Utility.CreateTextParam("@propertyText", value);
                propertyValue = new SqlParameter("@propertyValue", DBNull.Value);
            }
            else
            {
                propertyValue = Engage.Utility.CreateVarcharParam("@propertyValue", value, ApplicationPropertyValueLength);
                propertyText = new SqlParameter("@propertyText", DBNull.Value);
            }

            SqlHelper.ExecuteNonQuery(
                    this.ConnectionString, 
                    CommandType.Text, 
                    sql.ToString(), 
                    Engage.Utility.CreateIntegerParam("@applicationId", applicationId), 
                    Engage.Utility.CreateIntegerParam("@applicationPropertyId", propertyId), 
                    propertyValue, 
                    propertyText, 
                    Engage.Utility.CreateBitParam("@Visibility", true), 
                    Engage.Utility.CreateDateTimeParam("@LastUpdatedDate", DateTime.Now));
        }

        public override void UpdateApplicationProperty(int applicationId, int propertyId, string value)
        {
            var sql = new StringBuilder(512);
            sql.AppendFormat(CultureInfo.InvariantCulture, "UPDATE {0}UserJobProperty ", this.NamePrefix);
            sql.Append(" SET [PropertyValue] = @PropertyValue, ");
            sql.Append(" [PropertyText] = @PropertyText, ");
            sql.Append(" [LastUpdatedDate] = @LastUpdatedDate ");
            sql.Append(" WHERE ApplicationId = @applicationId ");
            sql.Append("  AND ApplicationPropertyId = @applicationPropertyId ");

            SqlParameter propertyValue, propertyText;
            if (value != null && value.Length > ApplicationPropertyValueLength)
            {
                propertyText = Engage.Utility.CreateTextParam("@propertyText", value);
                propertyValue = new SqlParameter("@propertyValue", DBNull.Value);
            }
            else
            {
                propertyValue = Engage.Utility.CreateVarcharParam("@propertyValue", value, ApplicationPropertyValueLength);
                propertyText = new SqlParameter("@propertyText", DBNull.Value);
            }

            SqlHelper.ExecuteNonQuery(
                    this.ConnectionString, 
                    CommandType.Text, 
                    sql.ToString(), 
                    Engage.Utility.CreateIntegerParam("@applicationId", applicationId), 
                    Engage.Utility.CreateIntegerParam("@applicationPropertyId", propertyId), 
                    propertyValue, 
                    propertyText, 
                    Engage.Utility.CreateDateTimeParam("@LastUpdatedDate", DateTime.Now));
        }

        #endregion

        #region Utility

        /// <summary>
        /// Executes a SQL stored procedure without returning any value.
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.  Does not include any prefix, for example <c>InsertJob</c> is translated to <c>dnn_EngageEmployment_spInsertJob</c>.</param>
        /// <param name="parameters">The parameters for this query.</param>
        private void ExecuteNonQuery(string storedProcedureName, params SqlParameter[] parameters)
        {
            SqlHelper.ExecuteNonQuery(this.ConnectionString, CommandType.StoredProcedure, this.NamePrefix + "sp" + storedProcedureName, parameters);
        }

        /// <summary>
        /// Executes a SQL stored procedure, returning the results as a <see cref="DataSet"/>.
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.  Does not include any prefix, for example <c>GetJobs</c> is translated to <c>dnn_EngageEmployment_spGetJobs</c>.</param>
        /// <param name="parameters">The parameters for this query.</param>
        /// <returns>The results of the stored procedure as a <see cref="DataSet"/></returns>
        private DataSet ExecuteDataset(string storedProcedureName, params SqlParameter[] parameters)
        {
            return SqlHelper.ExecuteDataset(
                    this.ConnectionString, CommandType.StoredProcedure, this.NamePrefix + "sp" + storedProcedureName, parameters);
        }

        /// <summary>
        /// Executes a SQL stored procedure, returning the results as a <see cref="SqlDataReader"/>.
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.  Does not include any prefix, for example <c>GetJobs</c> is translated to <c>dnn_EngageEmployment_spGetJobs</c>.</param>
        /// <param name="parameters">The parameters for this query.</param>
        /// <returns>The results of the stored procedure as a <see cref="SqlDataReader"/></returns>
        private SqlDataReader ExecuteReader(string storedProcedureName, params SqlParameter[] parameters)
        {
            return SqlHelper.ExecuteReader(
                    this.ConnectionString, CommandType.StoredProcedure, this.NamePrefix + "sp" + storedProcedureName, parameters);
        }

        /// <summary>
        /// Executes a SQL stored procedure, returning a single value.
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.  Does not include any prefix, for example <c>InsertJob</c> is translated to <c>dnn_EngageEmployment_spInsertJob</c>.</param>
        /// <param name="parameters">The parameters for this query.</param>
        /// <returns>The (single) result of the stored procedure</returns>
        private object ExecuteScalar(string storedProcedureName, params SqlParameter[] parameters)
        {
            return SqlHelper.ExecuteScalar(
                    this.ConnectionString, CommandType.StoredProcedure, this.NamePrefix + "sp" + storedProcedureName, parameters);
        }

        public override IDataReader GetCommonWords()
        {
            return SqlHelper.ExecuteReader(this.connectionString, CommandType.Text, "select Word from " + this.NamePrefix + "lkpCommonWords");
        }

        private static int? ConvertReturnValueToInt(object value)
        {
            int? returnValue = null;
            if (value is int)
            {
                returnValue = (int)value;
            }
            else if (value != null && !(value is DBNull))
            {
                returnValue = Convert.ToInt32(value, CultureInfo.InvariantCulture);
            }

            return returnValue;
        }

        #endregion
    }
}