// <copyright file="SqlDataProvider.cs" company="Engage Software">
// Engage: Employment
// Copyright (c) 2004-2013
// by Engage Software ( http://www.engagesoftware.com )
// </copyright>
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

namespace Engage.Dnn.Employment.Data
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Text;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Framework.Providers;

    using Microsoft.ApplicationBlocks.Data;

    using Utility = Engage.Utility;

    #endregion

    internal class SqlDataProvider : DataProvider
    {
        /// <summary>
        /// The length of the ApplicationProperty propertyValue column, past this length, use the text column, propertyText
        /// </summary>
        private const int ApplicationPropertyValueLength = 3750;

        private const string ModuleQualifier = "EngageEmployment_";

        private const string ProviderType = "data";

        private readonly string connectionString;

        // private string providerPath;
        private readonly string databaseOwner;

        private readonly string objectQualifier;

        private readonly ProviderConfiguration providerConfiguration = ProviderConfiguration.GetProviderConfiguration(ProviderType);

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

        public string ConnectionString
        {
            get { return this.connectionString; }
        }

        // public string ProviderPath
        // {
        // get
        // {
        // return this.providerPath;
        // }
        // }
        public string DatabaseOwner
        {
            get { return this.databaseOwner; }
        }

        public string NamePrefix
        {
            get { return this.databaseOwner + this.objectQualifier + ModuleQualifier; }
        }

        public string ObjectQualifier
        {
            get { return this.objectQualifier; }
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
                Utility.CreateIntegerParam("@applicationId", applicationId), 
                Utility.CreateIntegerParam("@resumeId", resumeId));
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", 
            Justification = "SQL does not contain un-paramterized input")]
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
                        string.Format(CultureInfo.InvariantCulture, "DELETE FROM {0}JobJobGroup WHERE JobId = @JobId", this.NamePrefix), 
                        Utility.CreateIntegerParam("@JobId", jobId));

                    // Add back the selected JobGroup assignments
                    if (jobGroups != null && jobGroups.Count > 0)
                    {
                        var sql = new StringBuilder(114);
                        sql.AppendFormat(CultureInfo.InvariantCulture, "INSERT {0}JobJobGroup (JobId, JobGroupId) ", this.NamePrefix);
                        sql.Append("VALUES (@jobId, @jobGroupId) ");

                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.Parameters.Add(Utility.CreateIntegerParam("@jobId", jobId));
                            cmd.Parameters.Add(Utility.CreateIntegerParam("@jobGroupId", jobGroups[0]));
                            cmd.CommandText = sql.ToString();
                            cmd.CommandType = CommandType.Text;
                            cmd.Transaction = tran;
                            cmd.Prepare();
                            cmd.ExecuteNonQuery();

                            for (var i = 1; i < jobGroups.Count; i++)
                            {
                                cmd.Parameters["@jobGroupId"].Value = jobGroups[i];
                                cmd.ExecuteNonQuery();
                            }

                            // cmd.Transaction.Commit();
                        }
                    }

                    tran.Commit();
                }
            }
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

            using (
                IDataReader dr = SqlHelper.ExecuteReader(
                    this.ConnectionString, CommandType.Text, sql.ToString(), Utility.CreateIntegerParam("@portalId", portalId)))
            {
                return dr.Read();
            }
        }

        public override void DeleteCategory(int categoryId)
        {
            this.ExecuteNonQuery("DeleteCategory", Utility.CreateIntegerParam("@categoryId", categoryId));
        }

        public override void DeleteJob(int jobId)
        {
            this.ExecuteNonQuery("DeleteJob", Utility.CreateIntegerParam("@JobId", jobId));
        }

        public override void DeleteJobGroup(int jobGroupId)
        {
            SqlHelper.ExecuteNonQuery(
                this.ConnectionString, 
                CommandType.Text, 
                string.Format(CultureInfo.InvariantCulture, "DELETE {0}JobGroup WHERE JobGroupId = @jobGroupId", this.NamePrefix), 
                Utility.CreateIntegerParam("@jobGroupId", jobGroupId));
        }

        public override void DeleteJobSearchQuery(int jobSearchQueryId)
        {
            SqlHelper.ExecuteNonQuery(
                this.ConnectionString, CommandType.Text, "delete " + this.NamePrefix + "UserJobSearch where UserSearchId = " + jobSearchQueryId);
        }

        public override void DeleteLocation(int locationId)
        {
            this.ExecuteNonQuery("DeleteLocation", Utility.CreateIntegerParam("@locationId", locationId));
        }

        public override void DeletePosition(int positionId)
        {
            this.ExecuteNonQuery("DeletePosition", Utility.CreateIntegerParam("@positionId", positionId));
        }

        public override void DeleteState(int stateId)
        {
            this.ExecuteNonQuery("DeleteState", Utility.CreateIntegerParam("@stateId", stateId));
        }

        public override void DeleteUserStatus(int statusId)
        {
            SqlHelper.ExecuteNonQuery(
                this.ConnectionString, 
                CommandType.Text, 
                "delete " + this.NamePrefix + "vwUserStatuses where UserStatusId = @statusId", 
                Utility.CreateIntegerParam("@statusId", statusId));
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
            sql.Append("j.RequiredQualifications, j.DesiredQualifications, j.CategoryName, j.CategoryId, j.ApplicationUrl, ");
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
                Utility.CreateIntegerParam("@jobGroupId", jobGroupId), 
                Utility.CreateIntegerParam("@portalId", portalId), 
                Utility.CreateDateTimeParam("@now", DateTime.Now));
        }

        public override DataSet GetAdminData(int? jobGroupId, int portalId)
        {
            DataSet adminData = this.ExecuteDataset("GetAdminData", Utility.CreateIntegerParam("@JobGroupId", jobGroupId), Utility.CreateIntegerParam("@PortalId", portalId));

            adminData.Tables[0].TableName = "Jobs";
            adminData.Tables[1].TableName = "ApplicationStatuses";
            adminData.Tables[2].TableName = "UserStatuses";

            return adminData;
        }

        public override IDataReader GetApplication(int applicationId)
        {
            var sql = new StringBuilder(147);
            sql.Append(" SELECT ApplicationId, UserId, DisplayName, JobId, AppliedDate, SalaryRequirement, Message, ApplicantName, ApplicantEmail, ApplicantPhone, StatusId ");
            sql.AppendFormat(" FROM {0}vwApplications ", this.NamePrefix);
            sql.Append(" WHERE ApplicationId = @applicationId ");

            return SqlHelper.ExecuteReader(
                this.ConnectionString, 
                CommandType.Text, 
                sql.ToString(), 
                Utility.CreateIntegerParam("@applicationId", applicationId));
        }

        public override DataTable GetApplicationDocuments(int applicationId)
        {
            var sql = new StringBuilder(512);

            sql.Append(" select ");
            sql.Append(" DocumentId, [DocumentTypeId], [FileName] ");
            sql.Append(" from ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " {0}vwDocuments ", this.NamePrefix);
            sql.Append(" where ");
            sql.Append(" ApplicationId = @applicationId");

            return SqlHelper.ExecuteDataset(
                this.ConnectionString, 
                CommandType.Text, 
                sql.ToString(), 
                Utility.CreateIntegerParam("@applicationId", applicationId)).Tables[0];
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
                Utility.CreateIntegerParam("@ApplicationId", applicationId)).Tables[0];
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
                Utility.CreateVarcharParam("@PropertyName", name), 
                Utility.CreateIntegerParam("@portalId", portalId));
        }

        public override DataTable GetApplications(int? jobGroupId, int portalId)
        {
            var sql = new StringBuilder(512);

            sql.Append(" SELECT ");
            sql.Append(" a.AppliedDate, a.DisplayName, a.JobId, a.JobTitle, a.LocationName, a.ApplicationId, a.UserId, ");
            sql.Append(" a.SalaryRequirement, a.Message, a.ApplicantName, a.ApplicantEmail, a.ApplicantPhone ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " FROM {0}vwApplications a ", this.NamePrefix);
            if (jobGroupId.HasValue)
            {
                sql.AppendFormat(CultureInfo.InvariantCulture, " JOIN {0}JobJobGroup jlg on (a.JobId = jlg.JobId) ", this.NamePrefix);
            }

            sql.Append(" WHERE a.PortalId = @portalId ");
            if (jobGroupId.HasValue)
            {
                sql.Append(" AND jlg.jobGroupId = @jobGroupId ");
            }

            sql.Append(" ORDER BY a.AppliedDate DESC ");

            return SqlHelper.ExecuteDataset(
                this.ConnectionString, 
                CommandType.Text, 
                sql.ToString(), 
                Utility.CreateIntegerParam("@jobGroupId", jobGroupId), 
                Utility.CreateIntegerParam("@portalId", portalId)).Tables[0];
        }

        /// <summary>
        /// Gets the applications for the given job.
        /// </summary>
        /// <param name="jobId">The ID of the job.</param>
        /// <param name="jobGroupId">The ID of the job group by which to filter results (or <c>null</c> to not filter).</param>
        /// <param name="applicationStatusId">The ID of the application status by which to filter applications.</param>
        /// <param name="userStatusId">The ID of user status by which to filter applications.</param>
        /// <param name="leadId">The ID of lead by which to filter applications.</param>
        /// <param name="dateFrom">The date from by which to filter applications.</param>
        /// <param name="dateTo">The date to by which to filter applications.</param>
        /// <param name="pageIndex">Zero-based index of the page of results to retrieve (ignored if <paramref name="pageSize"/> is null).</param>
        /// <param name="pageSize">The number of applications to retrieve per page (or <c>null</c> to retrieve all applications).</param>
        /// <param name="unpagedCount">The total number of applications which would be returned by this query if there was no paging applied.</param>
        /// <param name="fillDocumentsAndProperties">if set to <c>true</c> also retrieves the related documents (resumes and cover letters) and application properties (leads).</param>
        /// <returns>
        /// A <see cref="DataSet"/> with one table named <c>"Applications"</c> with the fields for a <see cref="JobApplication"/>.
        /// If <paramref name="fillDocumentsAndProperties"/>, the <see cref="DataSet"/> also contains tables named <c>"Documents"</c> and <c>"Properties"</c>
        /// </returns>
        public override DataSet GetApplicationsForJob(int jobId, int? jobGroupId, int? applicationStatusId, int? userStatusId, int? leadId, DateTime? dateFrom, DateTime? dateTo, int pageIndex, int? pageSize, out int unpagedCount, bool fillDocumentsAndProperties)
        {
            var applicationsDataSet = this.ExecuteDataset(
                "GetApplicationsForJob", 
                Utility.CreateIntegerParam("@jobId", jobId), 
                Utility.CreateIntegerParam("@jobGroupId", jobGroupId),
                Utility.CreateIntegerParam("@applicationStatusId", applicationStatusId),
                Utility.CreateIntegerParam("@userStatusId", userStatusId),
                Utility.CreateIntegerParam("@leadId", leadId),
                Utility.CreateDateTimeParam("@dateFrom", dateFrom),
                Utility.CreateDateTimeParam("@dateTo", dateTo),
                Utility.CreateIntegerParam("@index", pageIndex),
                Utility.CreateIntegerParam("@pageSize", pageSize),
                Utility.CreateBitParam("@fillDocumentsAndProperties", fillDocumentsAndProperties));

            unpagedCount = (int)applicationsDataSet.Tables[0].Rows[0][0];

            applicationsDataSet.Tables.RemoveAt(0);
            applicationsDataSet.Tables[0].TableName = "Applications";

            if (fillDocumentsAndProperties)
            {
                applicationsDataSet.Tables[1].TableName = "Documents";
                applicationsDataSet.Tables[2].TableName = "Properties";
            }

            return applicationsDataSet;
        }

        /// <summary>
        /// Gets a <see cref="DataSet"/> with all jobs in a <see cref="DataTable"/> named "Jobs," and all job groups in a <see cref="DataTable"/> named "JobGroup."
        /// They are collected by a <see cref="DataRelation"/> named "JobJobGroup."
        /// </summary>
        /// <param name="portalId">The id of the portal.</param>
        /// <returns>A <see cref="DataSet"/> with all jobs and their assigned groups.</returns>
        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", 
            Justification = "SQL does not contain un-parameterized input")]
        public override DataSet GetAssignedJobGroups(int portalId)
        {
            DataSet ds = null;
            try
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
                jobGroupSql.AppendFormat(
                    CultureInfo.InvariantCulture, "join {0}JobJobGroup jjg on (jjg.JobGroupId = jg.JobGroupId) ", this.NamePrefix);
                jobGroupSql.Append("where PortalId = @portalId ");

                using (var conn = new SqlConnection(this.ConnectionString))
                {
                    // ReSharper disable UseObjectOrCollectionInitializer
                    ds = new DataSet();
                    ds.Locale = CultureInfo.InvariantCulture;

                    // ReSharper restore UseObjectOrCollectionInitializer
                    using (var adapter = new SqlDataAdapter(jobsSql.ToString(), conn))
                    {
                        conn.Open();
                        adapter.SelectCommand.Parameters.Add(Utility.CreateIntegerParam("@portalId", portalId));
                        adapter.Fill(ds, "Jobs");
                    }

                    using (var adapter = new SqlDataAdapter(jobGroupSql.ToString(), conn))
                    {
                        adapter.SelectCommand.Parameters.Add(Utility.CreateIntegerParam("@portalId", portalId));
                        adapter.Fill(ds, "JobGroups");
                    }

                    ds.Relations.Add("JobJobGroup", ds.Tables["Jobs"].Columns["JobId"], ds.Tables["JobGroups"].Columns["JobId"]);
                    return ds;
                }
            }
            catch
            {
                if (ds != null)
                {
                    ds.Dispose();
                }

                throw;
            }
        }

        public override IDataReader GetCategories(int? jobGroupId, int portalId)
        {
            return this.ExecuteReader(
                "GetCategories", Utility.CreateIntegerParam("@jobGroupId", jobGroupId), Utility.CreateIntegerParam("@portalId", portalId));
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
                Utility.CreateVarcharParam("@name", categoryName, DataProvider.VarcharLength), 
                Utility.CreateIntegerParam("@portalId", portalId));

            return ConvertReturnValueToInt(categoryId);
        }

        public override IDataReader GetCommonWords()
        {
            return SqlHelper.ExecuteReader(this.connectionString, CommandType.Text, "select Word from " + this.NamePrefix + "lkpCommonWords");
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
            return this.ExecuteReader("GetDocument", Utility.CreateIntegerParam("@documentId", documentId));
        }

        public override int? GetDocumentId(int documentTypeId, int userId)
        {
            var sql = new StringBuilder(255);
            sql.AppendFormat(CultureInfo.InvariantCulture, "select max(ResumeId) from {0}Document ", this.NamePrefix);
            sql.Append(" where UserId = @userId ");
            sql.Append(" and DocumentTypeId = @documentTypeId ");

            object resumeId = SqlHelper.ExecuteScalar(
                this.ConnectionString, 
                CommandType.Text, 
                sql.ToString(), 
                Utility.CreateIntegerParam("@userId", userId), 
                Utility.CreateIntegerParam("@documentTypeId", documentTypeId));

            if (resumeId is int)
            {
                return (int)resumeId;
            }

            return null;
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
            return this.ExecuteReader("GetDocumentJobGroups", Utility.CreateIntegerParam("@documentId", documentId));
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
                Utility.CreateVarcharParam("@documentTypeName", documentTypeName), 
                Utility.CreateIntegerParam("@portalId", portalId));
        }

        public override IDataReader GetDocumentType(int documentTypeId)
        {
            var sql = new StringBuilder(128);
            sql.AppendFormat(
                CultureInfo.InvariantCulture, 
                "select documentTypeId, description from {0}DocumentType where documentTypeId = @documentTypeId", 
                this.NamePrefix);

            return SqlHelper.ExecuteReader(
                this.ConnectionString, CommandType.Text, sql.ToString(), Utility.CreateIntegerParam("@documentTypeId", documentTypeId));
        }

        public override IDataReader GetJob(int jobId)
        {
            var sql = new StringBuilder(512);

            sql.Append("select ");
            sql.Append(" JobId, JobTitle, PositionId, LocationName, LocationId, StateName, StateAbbreviation, StateId, ");
            sql.Append(" PostedDate, RequiredQualifications, DesiredQualifications, NotificationEmailAddress, ApplicationUrl, ");
            sql.Append(" CategoryName, CategoryId, IsHot, IsFilled, JobDescription, SortOrder, StartDate, ExpireDate ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " from {0}vwJobs ", this.NamePrefix);
            sql.Append(" where JobId = @jobId ");

            return SqlHelper.ExecuteReader(this.ConnectionString, CommandType.Text, sql.ToString(), Utility.CreateIntegerParam("@jobId", jobId));
        }

        public override DataTable GetJobGroup(int jobGroupId)
        {
            return
                SqlHelper.ExecuteDataset(
                    this.ConnectionString,
                    CommandType.Text,
                    string.Format(CultureInfo.InvariantCulture, "SELECT JobGroupId, [Name] FROM {0}JobGroup WHERE JobGroupId = @jobGroupId", this.NamePrefix),
                    Utility.CreateIntegerParam("@jobGroupId", jobGroupId)).Tables[0];
        }

        public override DataTable GetJobGroups(int portalId)
        {
            return
                SqlHelper.ExecuteDataset(
                    this.ConnectionString, 
                    CommandType.Text, 
                    string.Format(CultureInfo.InvariantCulture, "SELECT JobGroupId, [Name] FROM {0}JobGroup WHERE PortalId = @portalId ORDER BY [Name]", this.NamePrefix), 
                    Utility.CreateIntegerParam("@portalId", portalId)).Tables[0];
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
                    Utility.CreateIntegerParam("@locationId", locationId), 
                    Utility.CreateIntegerParam("@positionId", positionId)) as int?;
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
                Utility.CreateIntegerParam("@userId", userId), 
                Utility.CreateIntegerParam("@jobGroupId", jobGroupId), 
                Utility.CreateIntegerParam("@portalId", portalId));
        }

        public override IDataReader GetJobSearchQuery(int jobSearchQueryId)
        {
            var sql = new StringBuilder(512);

            sql.Append(" select ");
            sql.Append(" UserSearchId, SearchName, JobTitle, LocationName, StateName, Keywords, CreationDate, LocationId, StateId, CategoryId, CategoryName, PositionId, JobGroupId ");
            sql.Append(" from ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " {0}vwSavedSearches ", this.NamePrefix);
            sql.Append(" where UserSearchId = @searchId ");

            return SqlHelper.ExecuteReader(
                this.ConnectionString, CommandType.Text, sql.ToString(), Utility.CreateIntegerParam("@searchId", jobSearchQueryId));
        }

        public override DataTable GetJobSearchResults(int? positionId, int? categoryId, int? locationId, int? stateId, int? jobGroupId, int portalId)
        {
            var sql = new StringBuilder(512);

            sql.Append(" SELECT ");
            sql.Append(" j.JobId, j.JobTitle, j.LocationName, j.StateName, j.StateAbbreviation, j.CategoryName, ");
            sql.Append(" j.PostedDate, j.StartDate, j.RequiredQualifications, j.DesiredQualifications, j.JobDescription ");
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

            return
                SqlHelper.ExecuteDataset(
                    this.ConnectionString, 
                    CommandType.Text, 
                    sql.ToString(), 
                    Utility.CreateIntegerParam("@positionId", positionId), 
                    Utility.CreateIntegerParam("@locationId", locationId), 
                    Utility.CreateIntegerParam("@stateId", stateId), 
                    Utility.CreateIntegerParam("@jobGroupId", jobGroupId), 
                    Utility.CreateIntegerParam("@portalId", portalId), 
                    Utility.CreateIntegerParam("@categoryId", categoryId), 
                    Utility.CreateDateTimeParam("@now", DateTime.Now)).Tables[0];
        }

        public override IDataReader GetJobs(int? jobGroupId, int portalId)
        {
            int totalCount;
            return this.GetJobs(jobGroupId, portalId, null, null, 0, null, out totalCount);
        }

        public override IDataReader GetJobs(int? jobGroupId, int portalId, string jobTitle, int? locationId, int pageIndex, int? pageSize, out int totalCount)
        {
            var jobsReader = this.ExecuteReader(
                "GetJobs",
                Utility.CreateIntegerParam("@jobGroupId", jobGroupId),
                Utility.CreateIntegerParam("@portalId", portalId),
                Utility.CreateVarcharParam("@jobTitle", jobTitle),
                Utility.CreateIntegerParam("@locationId", locationId),
                Utility.CreateIntegerParam("@index", pageIndex),
                Utility.CreateIntegerParam("@pageSize", pageSize));

            jobsReader.Read();
            totalCount = jobsReader.GetInt32(0);
            
            jobsReader.NextResult();
            return jobsReader;
        }

        public override IDataReader GetJobs(int? userId, int? jobGroupId, int portalId)
        {
            var sql = new StringBuilder(1024);

            sql.Append(" SELECT ");
            sql.Append(" a.JobId, JobTitle, LocationName, StateName, ");
            sql.Append(" RequiredQualifications, DesiredQualifications, CategoryName, DisplayName, StatusId, ");
            sql.Append(" IsHot, PostedDate, AppliedDate, ApplicationId, SortOrder, UserId, ");
            sql.Append(" SalaryRequirement, Message, ApplicantName, ApplicantEmail, ApplicantPhone ");
            sql.Append(" FROM ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " {0}vwApplications a ", this.NamePrefix);
            if (jobGroupId.HasValue)
            {
                sql.AppendFormat(CultureInfo.InvariantCulture, " JOIN {0}JobJobGroup jlg on (a.JobId = jlg.JobId) ", this.NamePrefix);
            }

            sql.Append(" WHERE (UserId = @userId OR (userId IS NULL AND @userId IS NULL)) ");
            sql.Append(" AND PortalId = @portalId ");
            if (jobGroupId.HasValue)
            {
                sql.Append(" AND jlg.jobGroupId = @jobGroupId ");
            }

            sql.Append(" ORDER BY AppliedDate DESC ");

            return SqlHelper.ExecuteReader(
                this.ConnectionString, 
                CommandType.Text, 
                sql.ToString(), 
                Utility.CreateIntegerParam("@userId", userId), 
                Utility.CreateIntegerParam("@jobGroupId", jobGroupId), 
                Utility.CreateIntegerParam("@portalId", portalId));
        }

        public override DataTable GetKeywordSearchResults(string keyword, int? jobGroupId, int portalId)
        {
            var sql = new StringBuilder(256);
            sql.Append(" SELECT ");
            sql.Append(" j.JobId, j.JobTitle, j.LocationName, j.StateName, j.StateAbbreviation, j.CategoryName, ");
            sql.Append(" j.PostedDate, j.StartDate, j.RequiredQualifications, j.DesiredQualifications, j.JobDescription ");
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

            return
                SqlHelper.ExecuteDataset(
                    this.ConnectionString, 
                    CommandType.Text, 
                    sql.ToString(), 
                    Utility.CreateVarcharParam("@keyword", string.Format(CultureInfo.CurrentCulture, "%{0}%", keyword)), 
                    Utility.CreateIntegerParam("@jobGroupId", jobGroupId), 
                    Utility.CreateIntegerParam("@portalId", portalId), 
                    Utility.CreateDateTimeParam("@now", DateTime.Now)).Tables[0];
        }

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities", 
            Justification = "SQL does not contain un-parameterized input")]
        public override DataTable GetKeywordSearchResults(List<string> keywords, int? jobGroupId, int portalId)
        {
            DataTable searchResultsTable = null;

            try
            {
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
                            sql.Append(" j.PostedDate, j.StartDate, j.RequiredQualifications, j.DesiredQualifications, j.JobDescription ");
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
                                cmd.Parameters.Add(Utility.CreateIntegerParam("@jobGroupId", jobGroupId));
                            }

                            sql.Append(" ORDER BY ");
                            sql.Append(" j.PostedDate");

                            cmd.CommandText = sql.ToString();
                            cmd.Parameters.Add(Utility.CreateIntegerParam("@portalId", portalId));
                            cmd.Parameters.Add(Utility.CreateDateTimeParam("@now", DateTime.Now));
                            for (int i = 0; i < keywords.Count; i++)
                            {
                                cmd.Parameters.Add(
                                    Utility.CreateVarcharParam(
                                        "@keyword" + i.ToString(CultureInfo.InvariantCulture), 
                                        string.Format(CultureInfo.InvariantCulture, "%{0}%", keywords[i])));
                            }

                            // ReSharper disable UseObjectOrCollectionInitializer
                            searchResultsTable = new DataTable();
                            searchResultsTable.Locale = CultureInfo.InvariantCulture;

                            // ReSharper restore UseObjectOrCollectionInitializer
                            using (var dataAdapter = new SqlDataAdapter(cmd))
                            {
                                dataAdapter.Fill(searchResultsTable);
                            }
                        }
                    }
                }
            }
            catch
            {
                if (searchResultsTable != null)
                {
                    searchResultsTable.Dispose();
                }
            }

            return searchResultsTable;
        }

        public override IDataReader GetLocation(int locationId)
        {
            var sql = new StringBuilder(512);

            sql.Append(" select ");
            sql.Append(" LocationId, LocationName, StateId, StateName, StateAbbreviation ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " from {0}vwLocations ", this.NamePrefix);
            sql.Append(" where LocationId = @locationId");

            return SqlHelper.ExecuteReader(
                this.ConnectionString, CommandType.Text, sql.ToString(), Utility.CreateIntegerParam("@locationId", locationId));
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
                Utility.CreateVarcharParam("@locationName", locationName, DataProvider.VarcharLength), 
                Utility.CreateIntegerParam("@portalId", portalId), 
                Utility.CreateIntegerParam("@stateId", stateId));

            return ConvertReturnValueToInt(locationId);
        }

        public override IDataReader GetLocations(int? jobGroupId, int portalId)
        {
            return this.ExecuteReader(
                "GetLocations", Utility.CreateIntegerParam("@portalId", portalId), Utility.CreateIntegerParam("@jobGroupId", jobGroupId));
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
                this.ConnectionString, CommandType.Text, sql.ToString(), Utility.CreateIntegerParam("@positionId", positionId));
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
                Utility.CreateVarcharParam("@jobTitle", name, DataProvider.VarcharLength), 
                Utility.CreateIntegerParam("@portalId", portalId));

            return ConvertReturnValueToInt(positionId);
        }

        public override IDataReader GetPositions(int? jobGroupId, int portalId)
        {
            return this.ExecuteReader(
                "GetPositions", Utility.CreateIntegerParam("@jobGroupId", jobGroupId), Utility.CreateIntegerParam("@portalId", portalId));
        }

        public override int? GetResumeId(int userId)
        {
            return this.GetDocumentId(DocumentType.Resume.GetId(), userId);
        }

        public override int? GetDocumentIdForApplication(int applicationId, int documentTypeId)
        {
            var sql = new StringBuilder(255);
            sql.AppendFormat(CultureInfo.InvariantCulture, "SELECT d.ResumeId FROM {0}Document d ", this.NamePrefix);
            sql.AppendFormat(CultureInfo.InvariantCulture, " JOIN {0}ApplicationDocument ad ON (d.ResumeId = ad.ResumeId) ", this.NamePrefix);
            sql.Append(" WHERE ad.ApplicationId = @applicationId ");
            sql.Append(" AND d.DocumentTypeId = @documentTypeId ");

            object documentId = SqlHelper.ExecuteScalar(
                this.ConnectionString, 
                CommandType.Text, 
                sql.ToString(), 
                Utility.CreateIntegerParam("@applicationId", applicationId), 
                Utility.CreateIntegerParam("@documentTypeId", documentTypeId));

            if (documentId is int)
            {
                return (int)documentId;
            }

            return null;
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

            return SqlHelper.ExecuteReader(this.ConnectionString, CommandType.Text, sql.ToString(), Utility.CreateIntegerParam("@stateId", stateId));
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
                Utility.CreateVarcharParam("@stateName", name, DataProvider.VarcharLength), 
                Utility.CreateIntegerParam("@portalId", portalId));

            return ConvertReturnValueToInt(stateId);
        }

        public override IDataReader GetStates(int? jobGroupId, int portalId)
        {
            return this.ExecuteReader(
                "GetStates", Utility.CreateIntegerParam("@jobGroupId", jobGroupId), Utility.CreateIntegerParam("@portalId", portalId));
        }

        public override DataSet GetUnusedAdminData(int? jobGroupId, int portalId)
        {
            DataSet adminData = this.ExecuteDataset(
                "GetUnusedAdminData", Utility.CreateIntegerParam("@JobGroupId", jobGroupId), Utility.CreateIntegerParam("@PortalId", portalId));

            adminData.Tables[0].TableName = "States";
            adminData.Tables[1].TableName = "Locations";
            adminData.Tables[2].TableName = "Categories";
            adminData.Tables[3].TableName = "Positions";

            return adminData;
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

            return SqlHelper.ExecuteDataset(this.ConnectionString, CommandType.Text, sql.ToString(), Utility.CreateIntegerParam("@statusId", statusId)).Tables[0];
        }

        public override int? GetUserStatus(int portalId, int userId)
        {
            var sql = new StringBuilder(512);

            sql.Append(" select ");
            sql.Append(" UserStatusId ");
            sql.Append(" from ");
            sql.AppendFormat(CultureInfo.InvariantCulture, "{0}UserStatus", this.NamePrefix);
            sql.Append(" where ");
            sql.Append(" UserId = @userId ");
            sql.Append(" and PortalId = @portalId ");

            object statusId = SqlHelper.ExecuteScalar(
                this.ConnectionString,
                CommandType.Text,
                sql.ToString(),
                Utility.CreateIntegerParam("@userId", userId),
                Utility.CreateIntegerParam("@portalId", portalId));

            return ConvertReturnValueToInt(statusId);
        }

        public override DataTable GetUsersWithStatus(int portalId, int statusId)
        {
            var sql = new StringBuilder(512);

            sql.Append(" select ");
            sql.Append(" UserId, UserStatusId, PortalId ");
            sql.Append(" from ");
            sql.AppendFormat(CultureInfo.InvariantCulture, "{0}UserStatus", this.NamePrefix);
            sql.Append(" where ");
            sql.Append(" PortalId = @portalId ");
            sql.Append(" and UserStatusId = @statusId ");

            return
                SqlHelper.ExecuteDataset(
                    this.ConnectionString,
                    CommandType.Text,
                    sql.ToString(),
                    Utility.CreateIntegerParam("@portalId", portalId),
                    Utility.CreateIntegerParam("@statusId", statusId)).Tables[0];
        }

        public override void UpdateUserStatus(int portalId, int userId, int? statusId)
        {
            this.ExecuteNonQuery(
                "UpdateUserStatus",
                Utility.CreateIntegerParam("@portalId", portalId),
                Utility.CreateIntegerParam("@userId", userId),
                Utility.CreateIntegerParam("@userStatusId", statusId.HasValue ? statusId : null));
        }

        public override void DeleteUserStatus(int portalId, int userId)
        {
            var sql = new StringBuilder(512);

            sql.AppendFormat(CultureInfo.InvariantCulture, " delete {0}UserStatus", this.NamePrefix);
            sql.Append(" where UserId = @userId ");
            sql.Append(" and PortalId = @portalId ");

            SqlHelper.ExecuteNonQuery(
                this.ConnectionString,
                CommandType.Text,
                sql.ToString(),
                Utility.CreateIntegerParam("@userId", userId),
                Utility.CreateIntegerParam("@portalId", portalId));
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
                Utility.CreateVarcharParam("@status", status, DataProvider.VarcharLength), 
                Utility.CreateIntegerParam("@portalId", portalId));

            return ConvertReturnValueToInt(statusId);
        }

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

            return SqlHelper.ExecuteDataset(this.ConnectionString, CommandType.Text, sql.ToString(), Utility.CreateIntegerParam("@portalId", portalId)).Tables[0];
        }

        public override bool HasUserAppliedForJob(int jobId, int userId)
        {
            var sql = new StringBuilder(512);

            sql.Append(" SELECT TOP 1 NULL ");
            sql.AppendFormat(CultureInfo.InvariantCulture, " FROM {0}vwApplications ", this.NamePrefix);
            sql.Append(" WHERE JobId = @jobId ");
            sql.Append(" AND UserId = @userId");

            using (IDataReader dr = SqlHelper.ExecuteReader(
                    this.ConnectionString, 
                    CommandType.Text, 
                    sql.ToString(), 
                    Utility.CreateIntegerParam("@jobId", jobId), 
                    Utility.CreateIntegerParam("@userId", userId)))
            {
                return dr.Read();
            }
        }

        public override int InsertApplication(int jobId, int? userId, string salaryRequirement, string message, string name, string email, string phone)
        {
            var sql = new StringBuilder(512);
            sql.AppendFormat(CultureInfo.InvariantCulture, " INSERT {0}vwApplications ", this.NamePrefix);
            sql.Append(" (UserId, JobId, AppliedDate, SalaryRequirement, Message, ApplicantName, ApplicantEmail, ApplicantPhone) ");
            sql.Append(" VALUES (@userId, @jobId, getdate(), @salaryRequirement, @message, @applicantName, @applicantEmail, @applicantPhone) ");
            sql.Append(" SELECT SCOPE_IDENTITY() ");

            return (int)(decimal)SqlHelper.ExecuteScalar(
                this.ConnectionString, 
                CommandType.Text, 
                sql.ToString(), 
                Utility.CreateIntegerParam("@userId", userId), 
                Utility.CreateIntegerParam("@jobId", jobId),
                Utility.CreateVarcharParam("@salaryRequirement", salaryRequirement, DataProvider.VarcharLength),
                Utility.CreateVarcharParam("@message", message, DataProvider.VarcharLength),
                Utility.CreateVarcharParam("@applicantName", name, DataProvider.VarcharLength),
                Utility.CreateVarcharParam("@applicantEmail", email, DataProvider.VarcharLength),
                Utility.CreateVarcharParam("@applicantPhone", phone, DataProvider.VarcharLength));
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
                propertyText = Utility.CreateTextParam("@propertyText", value);
                propertyValue = new SqlParameter("@propertyValue", DBNull.Value);
            }
            else
            {
                propertyValue = Utility.CreateVarcharParam("@propertyValue", value, ApplicationPropertyValueLength);
                propertyText = new SqlParameter("@propertyText", DBNull.Value);
            }

            SqlHelper.ExecuteNonQuery(
                this.ConnectionString, 
                CommandType.Text, 
                sql.ToString(), 
                Utility.CreateIntegerParam("@applicationId", applicationId), 
                Utility.CreateIntegerParam("@applicationPropertyId", propertyId), 
                propertyValue, 
                propertyText, 
                Utility.CreateBitParam("@Visibility", true), 
                Utility.CreateDateTimeParam("@LastUpdatedDate", DateTime.Now));
        }

        public override void InsertCategory(string categoryName, int portalId)
        {
            this.ExecuteNonQuery(
                "InsertCategory",
                Utility.CreateVarcharParam("@categoryName", categoryName, DataProvider.VarcharLength), 
                Utility.CreateIntegerParam("@portalId", portalId));
        }

        public override int InsertDocument(int applicationId, int? userId, string fileName, string contentType, byte[] document, int documentTypeId)
        {
            return this.InsertDocument(applicationId, userId, fileName, contentType, document, documentTypeId, true);
        }

        public override int InsertDocument(
            int applicationId, int? userId, string fileName, string contentType, byte[] document, int documentTypeId, bool removeOldDocuments)
        {
            var sql = new StringBuilder(512);

            sql.AppendFormat(CultureInfo.InvariantCulture, " insert {0}Document ", this.NamePrefix);
            sql.Append(" (UserId, FileName, ContentType, ContentLength, ResumeData, RevisionDate, DocumentTypeId) values ");
            sql.Append(" (@userId, @fileName, @contentType, @contentLength, @resumeData, getdate(), @documentTypeId) ");
            sql.Append(" SELECT SCOPE_IDENTITY() ");

            var resumeId =
                (int)
                (decimal)
                SqlHelper.ExecuteScalar(
                    this.ConnectionString, 
                    CommandType.Text, 
                    sql.ToString(), 
                    Utility.CreateIntegerParam("@userId", userId),
                    Utility.CreateVarcharParam("@fileName", fileName, DataProvider.VarcharLength), 
                    Utility.CreateVarcharParam("@contentType", contentType), 
                    Utility.CreateIntegerParam("@contentLength", document.Length), 
                    Utility.CreateImageParam("@resumeData", document), 
                    Utility.CreateIntegerParam("@documentTypeId", documentTypeId));

            if (removeOldDocuments)
            {
                this.RemoveDocumentAssignments(applicationId, documentTypeId);
            }

            this.AssignDocumentToApplication(applicationId, resumeId);
            return resumeId;
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
            DateTime? expireDate, 
            string applicationUrl)
        {
            return
                (int)
                this.ExecuteScalar(
                    "InsertJob", 
                    Utility.CreateIntegerParam("@positionId", positionId), 
                    Utility.CreateIntegerParam("@locationId", locationId), 
                    Utility.CreateIntegerParam("@categoryId", categoryId), 
                    Utility.CreateBitParam("@isHot", isHot), 
                    Utility.CreateBitParam("@isFilled", isFilled), 
                    Utility.CreateTextParam("@requiredQualifications", requiredQualifications), 
                    Utility.CreateTextParam("@desiredQualifications", desiredQualifications), 
                    Utility.CreateIntegerParam("@revisingUser", userId), 
                    Utility.CreateIntegerParam("@sortOrder", sortOrder), 
                    Utility.CreateIntegerParam("@portalId", portalId), 
                    Utility.CreateTextParam("@notificationEmailAddress", notificationEmailAddress), 
                    Utility.CreateDateTimeParam("@startDate", startDate), 
                    Utility.CreateDateTimeParam("@expireDate", expireDate), 
                    Utility.CreateVarcharParam("@applicationUrl", applicationUrl, MaxUrlLength));
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
                Utility.CreateVarcharParam("@jobGroupName", jobGroupName, DataProvider.VarcharLength), 
                Utility.CreateIntegerParam("@portalId", portalId));
        }

        public override void InsertLocation(string locationName, int stateId, int portalId)
        {
            this.ExecuteNonQuery(
                "InsertLocation",
                Utility.CreateVarcharParam("@locationName", locationName, DataProvider.VarcharLength), 
                Utility.CreateIntegerParam("@stateId", stateId), 
                Utility.CreateIntegerParam("@portalId", portalId));
        }

        public override void InsertPosition(string jobTitle, string jobDescription, int portalId)
        {
            this.ExecuteNonQuery(
                "InsertPosition",
                Utility.CreateVarcharParam("@jobTitle", jobTitle, DataProvider.VarcharLength), 
                Utility.CreateTextParam("@jobDescription", jobDescription), 
                Utility.CreateIntegerParam("@portalId", portalId));
        }

        public override int InsertResume(int applicationId, int? userId, string fileName, string resumeType, byte[] resume)
        {
            return this.InsertDocument(applicationId, userId, fileName, resumeType, resume, DocumentType.Resume.GetId());
        }

        public override void InsertState(string name, string abbreviation, int portalId)
        {
            this.ExecuteNonQuery(
                "InsertState", 
                Utility.CreateIntegerParam("@portalId", portalId), 
                Utility.CreateVarcharParam("@stateName", name, DataProvider.VarcharLength), 
                Utility.CreateVarcharParam("@stateAbbreviation", abbreviation, DataProvider.AbbreviationLength));
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
                Utility.CreateVarcharParam("@status", statusName, DataProvider.VarcharLength), 
                Utility.CreateIntegerParam("@portalId", portalId));
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
            using (IDataReader dr = this.ExecuteReader("GetApplicationsByStatus", Utility.CreateIntegerParam("@statusId", statusId)))
            {
                return dr.Read();
            }
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

        public override bool IsJobGroupNameUsed(string jobGroupName, int portalId)
        {
            var sql = new StringBuilder(97);
            sql.AppendFormat(
                CultureInfo.InvariantCulture, 
                "select top 1 JobGroupId from {0}JobGroup where Name = @jobGroupName and PortalId = @portalId", 
                this.NamePrefix);

            using (
                IDataReader dr = SqlHelper.ExecuteReader(
                    this.ConnectionString, 
                    CommandType.Text, 
                    sql.ToString(),
                    Utility.CreateVarcharParam("@jobGroupName", jobGroupName, DataProvider.VarcharLength), 
                    Utility.CreateIntegerParam("@portalId", portalId)))
            {
                return dr.Read();
            }
        }

        public override bool IsJobGroupUsed(int jobGroupId)
        {
            var sql = new StringBuilder(97);
            sql.AppendFormat(
                CultureInfo.InvariantCulture, "select top 1 JobGroupId from {0}JobJobGroup where JobGroupId = @jobGroupId", this.NamePrefix);

            using (
                IDataReader dr = SqlHelper.ExecuteReader(
                    this.ConnectionString, CommandType.Text, sql.ToString(), Utility.CreateIntegerParam("@jobGroupId", jobGroupId)))
            {
                return dr.Read();
            }
        }

        public override bool IsJobInGroup(int jobId, int jobGroupId)
        {
            var sql = new StringBuilder(128);
            sql.AppendFormat(
                CultureInfo.InvariantCulture, 
                "select top 1 jobgroupid from {0}JobJobGroup where jobId = @jobId and jobgroupid = @jobGroupId", 
                this.NamePrefix);

            using (
                IDataReader dr = SqlHelper.ExecuteReader(
                    this.ConnectionString, 
                    CommandType.Text, 
                    sql.ToString(), 
                    Utility.CreateIntegerParam("@jobId", jobId), 
                    Utility.CreateIntegerParam("@jobGroupId", jobGroupId)))
            {
                return dr.Read();
            }
        }

        public override bool IsLocationUsed(int locationId)
        {
            var sql = new StringBuilder(90);
            sql.AppendFormat(CultureInfo.InvariantCulture, " select top 1 LocationId from {0}vwJobs where LocationId = @locationId", this.NamePrefix);

            using (
                IDataReader dr = SqlHelper.ExecuteReader(
                    this.ConnectionString, CommandType.Text, sql.ToString(), Utility.CreateIntegerParam("@locationId", locationId)))
            {
                return dr.Read();
            }
        }

        public override bool IsPositionUsed(int positionId)
        {
            var sql = new StringBuilder(97);
            sql.AppendFormat(CultureInfo.InvariantCulture, " SELECT TOP 1 NULL FROM {0}vwJobs WHERE PositionId = @positionId", this.NamePrefix);

            using (
                IDataReader dr = SqlHelper.ExecuteReader(
                    this.ConnectionString, CommandType.Text, sql.ToString(), Utility.CreateIntegerParam("@positionId", positionId)))
            {
                return dr.Read();
            }
        }

        public override bool IsPropertyValueUsed(int propertyId, string value)
        {
            var sql = new StringBuilder(228);
            sql.AppendFormat(CultureInfo.InvariantCulture, "select top 1 ApplicationPropertyId from {0}UserJobProperty ", this.NamePrefix);
            sql.Append(
                " where ApplicationPropertyId = @ApplicationPropertyId and (propertyValue = @propertyValue or (propertyValue is null and propertyText like @propertyText))");

            return
                SqlHelper.ExecuteReader(
                    this.ConnectionString, 
                    CommandType.Text, 
                    sql.ToString(), 
                    Utility.CreateIntegerParam("@ApplicationPropertyId", propertyId), 
                    Utility.CreateVarcharParam("@propertyValue", value, ApplicationPropertyValueLength), 
                    Utility.CreateTextParam("@propertyText", value)).Read();
        }

        public override bool IsStateUsed(int stateId)
        {
            var sql = new StringBuilder(87);
            sql.AppendFormat(CultureInfo.InvariantCulture, " select top 1 NULL from {0}vwLocations where StateId = @stateId", this.NamePrefix);

            using (
                IDataReader dr = SqlHelper.ExecuteReader(
                    this.ConnectionString, CommandType.Text, sql.ToString(), Utility.CreateIntegerParam("@stateId", stateId)))
            {
                return dr.Read();
            }
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
                Employment.Utility.UserStatusPropertyName, 
                Environment.NewLine);
            sql.AppendFormat(
                CultureInfo.InvariantCulture, 
                " select top 1 null as EmptyColumn from {0}{1}UserProfile where PropertyDefinitionID = @propertyId and PropertyValue = @statusId", 
                this.DatabaseOwner, 
                this.ObjectQualifier);
            using (
                IDataReader dr = SqlHelper.ExecuteReader(
                    this.ConnectionString, CommandType.Text, sql.ToString(), Utility.CreateIntegerParam("@statusId", statusId)))
            {
                return dr.Read();
            }
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
            if (!Utility.HasValue(keywords))
            {
                keywords = null;
            }

            this.ExecuteNonQuery(
                "SaveJobSearchQuery", 
                Utility.CreateIntegerParam("@userId", userId),
                Utility.CreateVarcharParam("@name", searchName, DataProvider.VarcharLength), 
                Utility.CreateIntegerParam("@positionId", positionId), 
                Utility.CreateIntegerParam("@stateId", stateId), 
                Utility.CreateIntegerParam("@locationId", locationId), 
                Utility.CreateVarcharParam("@keywords", keywords), 
                Utility.CreateIntegerParam("@jobGroupId", jobGroupId), 
                Utility.CreateIntegerParam("@portalId", portalId), 
                Utility.CreateIntegerParam("@categoryId", categoryId));
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
                Utility.CreateIntegerParam("@applicationId", applicationId), 
                Utility.CreateIntegerParam("@statusId", statusId), 
                Utility.CreateIntegerParam("@revisingUserId", revisingUserId));
        }

        public override void UpdateApplication(int applicationId, string salaryRequirement, string message, string name, string email, string phone)
        {
            var sql = new StringBuilder(512);
            sql.AppendFormat(CultureInfo.InvariantCulture, " UPDATE {0}vwApplications ", this.NamePrefix);
            sql.Append(" SET SalaryRequirement = @salaryRequirement, ");
            sql.Append(" Message = @message, ");
            sql.Append(" ApplicantName = @applicantName, ");
            sql.Append(" ApplicantEmail = @applicantEmail, ");
            sql.Append(" ApplicantPhone = @applicantPhone ");
            sql.Append(" WHERE ApplicationId = @applicationId ");

            SqlHelper.ExecuteNonQuery(
                this.ConnectionString, 
                CommandType.Text, 
                sql.ToString(), 
                Utility.CreateIntegerParam("@applicationId", applicationId),
                Utility.CreateVarcharParam("@salaryRequirement", salaryRequirement, DataProvider.VarcharLength),
                Utility.CreateVarcharParam("@message", message, DataProvider.VarcharLength),
                Utility.CreateVarcharParam("@applicantName", name, DataProvider.VarcharLength),
                Utility.CreateVarcharParam("@applicantEmail", email, DataProvider.VarcharLength),
                Utility.CreateVarcharParam("@applicantPhone", phone, DataProvider.VarcharLength));
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
                propertyText = Utility.CreateTextParam("@propertyText", value);
                propertyValue = new SqlParameter("@propertyValue", DBNull.Value);
            }
            else
            {
                propertyValue = Utility.CreateVarcharParam("@propertyValue", value, ApplicationPropertyValueLength);
                propertyText = new SqlParameter("@propertyText", DBNull.Value);
            }

            SqlHelper.ExecuteNonQuery(
                this.ConnectionString, 
                CommandType.Text, 
                sql.ToString(), 
                Utility.CreateIntegerParam("@applicationId", applicationId), 
                Utility.CreateIntegerParam("@applicationPropertyId", propertyId), 
                propertyValue, 
                propertyText, 
                Utility.CreateDateTimeParam("@LastUpdatedDate", DateTime.Now));
        }

        public override void UpdateCategory(int categoryId, string description)
        {
            this.ExecuteNonQuery(
                "UpdateCategory",
                Utility.CreateVarcharParam("@categoryName", description, DataProvider.VarcharLength), 
                Utility.CreateIntegerParam("@categoryId", categoryId));
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
            DateTime? expireDate, 
            string applicationUrl)
        {
            this.ExecuteNonQuery(
                "UpdateJob", 
                Utility.CreateIntegerParam("@jobId", jobId), 
                Utility.CreateIntegerParam("@positionId", positionId), 
                Utility.CreateIntegerParam("@locationId", locationId), 
                Utility.CreateIntegerParam("@categoryId", categoryId), 
                Utility.CreateBitParam("@isHot", isHot), 
                Utility.CreateBitParam("@isFilled", isFilled), 
                Utility.CreateTextParam("@desiredQualifications", desiredQualifications), 
                Utility.CreateTextParam("@requiredQualifications", requiredQualifications), 
                Utility.CreateIntegerParam("@revisingUser", userId), 
                Utility.CreateIntegerParam("@sortOrder", sortOrder), 
                Utility.CreateTextParam("@notificationEmailAddress", notificationEmailAddress), 
                Utility.CreateDateTimeParam("@startDate", startDate), 
                Utility.CreateDateTimeParam("@expireDate", expireDate),
                Utility.CreateVarcharParam("@applicationUrl", applicationUrl, DataProvider.MaxUrlLength));
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
                Utility.CreateIntegerParam("@jobGroupId", jobGroupId), 
                Utility.CreateVarcharParam("@jobGroupName", jobGroupName, 255));
        }

        public override void UpdateLocation(int locationId, string description, int stateId)
        {
            this.ExecuteNonQuery(
                "UpdateLocation",
                Utility.CreateVarcharParam("@locationName", description, DataProvider.VarcharLength), 
                Utility.CreateIntegerParam("@stateId", stateId), 
                Utility.CreateIntegerParam("@locationId", locationId));
        }

        public override void UpdatePosition(int positionId, string jobTitle, string jobDescription)
        {
            this.ExecuteNonQuery(
                "UpdatePosition",
                Utility.CreateVarcharParam("@jobTitle", jobTitle, DataProvider.VarcharLength), 
                Utility.CreateTextParam("@jobDescription", jobDescription), 
                Utility.CreateIntegerParam("@positionId", positionId));
        }

        public override void UpdateState(int id, string name, string abbreviation)
        {
            this.ExecuteNonQuery(
                "UpdateState", 
                Utility.CreateIntegerParam("@stateId", id),
                Utility.CreateVarcharParam("@stateName", name, DataProvider.VarcharLength),
                Utility.CreateVarcharParam("@stateAbbreviation", abbreviation, DataProvider.AbbreviationLength));
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
                Utility.CreateVarcharParam("@status", statusName, DataProvider.VarcharLength), 
                Utility.CreateIntegerParam("@statusId", statusId));
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
        /// Executes a SQL stored procedure without returning any value.
        /// </summary>
        /// <param name="storedProcedureName">Name of the stored procedure.  Does not include any prefix, for example <c>InsertJob</c> is translated to <c>dnn_EngageEmployment_spInsertJob</c>.</param>
        /// <param name="parameters">The parameters for this query.</param>
        private void ExecuteNonQuery(string storedProcedureName, params SqlParameter[] parameters)
        {
            SqlHelper.ExecuteNonQuery(this.ConnectionString, CommandType.StoredProcedure, this.NamePrefix + "sp" + storedProcedureName, parameters);
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
                Utility.CreateIntegerParam("@applicationId", applicationId), 
                Utility.CreateIntegerParam("@documentTypeId", documentTypeId));
        }
    }
}