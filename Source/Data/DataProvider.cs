// <copyright file="DataProvider.cs" company="Engage Software">
// Engage: Employment
// Copyright (c) 2004-2010
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
    using System.Diagnostics;
    using DotNetNuke.Common.Utilities;

    internal abstract class DataProvider
    {
        /// <summary>
        /// The length of <c>(n)varchar</c> fields in the database for general text fields.
        /// Does not include State <c>abbreviation</c> or CommonWords <c>locale</c> fields, the length of which is defined in <see cref="AbbreviationLength"/>.
        /// Also does not include Job <c>RequiredQualifications</c> or <c>DesiredQualifications</c>, UserJobSearch <c>Keywords</c> or <c>SearchSql</c>, or Position <c>description</c>, which are text fields.
        /// </summary>
        public const int VarcharLength = 255;

        /// <summary>
        /// The length of <c>(n)varchar</c> fields in the database for small text fields.  Includes State <c>abbreviation</c> and CommonWords <c>locale</c>.
        /// </summary>
        public const int AbbreviationLength = 10;

        /// <summary>
        /// The length of columns containing URLs (<c>Job.ApplicationUrl</c>)
        /// </summary>
        public const int MaxUrlLength = 2000;

        #region Shared/Static Methods
        // singleton reference to the instantiated object 
        private static DataProvider provider;

        // return the provider
        [DebuggerStepThrough]
        public static DataProvider Instance()
        {
            if (provider == null)
            {
                string assembly = "Engage.Dnn.Employment.Data.SqlDataProvider,EngageEmployment";
                var objectType = Type.GetType(assembly, true, true);

                provider = (DataProvider)Activator.CreateInstance(objectType);
                DataCache.SetCache(objectType.FullName, provider);
            }

            return provider;
        }

        #endregion

        #region Job

        public abstract DataTable GetAdminData(int? jobGroupId, int portalId);

        public abstract DataSet GetUnusedAdminData(int? jobGroupId, int portalId);

        public abstract int InsertJob(
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
            string applicationUrl);

        public abstract void UpdateJob(
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
            string applicationUrl);

        public abstract void DeleteJob(int jobId);

        public abstract IDataReader GetJob(int jobId);

        public abstract IDataReader GetJobs(int? jobGroupId, int portalId);

        public abstract IDataReader GetActiveJobs(bool onlyHotJobs, int? jobGroupId, int portalId);

        public abstract IDataReader GetActiveJobs(bool onlyHotJobs, int? maximumNumberOfJobs, int? jobGroupId, int portalId);

        public abstract int? GetJobId(int locationId, int positionId);

        public abstract bool CanCreateJob(int portalId);

        #endregion

        #region UserJob

        public abstract IDataReader GetJobs(int? userId, int? jobGroupId, int portalId);

        public abstract bool HasUserAppliedForJob(int jobId, int userId);

        ////public abstract bool HasJobBeenAppliedFor(int jobId);

        #endregion

        #region JobApplication

        public abstract IDataReader GetApplication(int applicationId);

        public abstract int InsertApplication(int jobId, int? userId, string salaryRequirement, string message);

        /// <summary>
        /// Updates the <see cref="ApplicationStatus"/> of the given <see cref="JobApplication"/>.
        /// </summary>
        /// <param name="applicationId">The application id.</param>
        /// <param name="statusId">The status id.</param>
        /// <param name="revisingUserId">The ID of the user updating this application.</param>
        public abstract void UpdateApplication(int applicationId, int? statusId, int revisingUserId);

        public abstract void UpdateApplication(int jobId, string salaryRequirement, string message);

        public abstract DataTable GetApplications(int? jobGroupId, int portalId);

        public abstract IDataReader GetApplicationsForJob(int jobId, int? jobGroupId);

        #endregion

        #region Document

        public abstract DataTable GetApplicationDocuments(int applicationId);

        public abstract IDataReader GetDocumentType(string documentTypeName, int? portalId);

        public abstract IDataReader GetDocumentType(int documentTypeId);

        /// <summary>
        /// Gets information about the document with the given ID.  Columns include DocumentId/<see cref="int"/>, UserId/<see cref="int"/>, FileName/<see cref="string"/>, ContentType/<see cref="string"/>, ContentLength/<see cref="int"/>, ResumeData/<see cref="byte"/>[], RevisionDate/<see cref="DateTime"/>, DocumentTypeId/<see cref="int"/>
        /// </summary>
        /// <param name="documentId">The document ID.</param>
        /// <returns>Information about the document with the given ID</returns>
        public abstract IDataReader GetDocument(int documentId);

        public abstract int GetResumeId(int userId);

        public abstract int GetResumeIdForApplication(int applicationId);

        public abstract int GetDocumentId(int documentTypeId, int userId);

        public abstract int InsertResume(int applicationId, int? userId, string fileName, string resumeType, byte[] resume);

        public abstract int InsertDocument(int applicationId, int? userId, string fileName, string contentType, byte[] document, int documentTypeId);

        public abstract int InsertDocument(
            int applicationId, int? userId, string fileName, string contentType, byte[] document, int documentTypeId, bool removeOldDocuments);

        public abstract void AssignDocumentToApplication(int applicationId, int id);

        /// <summary>
        /// Gets the IDs of the job groups that can view the document with the given ID.
        /// </summary>
        /// <param name="documentId">The document ID.</param>
        /// <returns>A reader representing a list of the IDs of the job groups that can view the document with the given ID</returns>
        public abstract IDataReader GetDocumentJobGroups(int documentId);

        #endregion

        #region State

        public abstract IDataReader GetStates(int? jobGroupId, int portalId);

        public abstract IDataReader GetState(int stateId);

        public abstract void UpdateState(int id, string name, string abbreviation); ////, string abbreviation);

        public abstract void InsertState(string name, string abbreviation, int portalId); ////, string abbreviation);

        public abstract int? GetStateId(string name, int portalId);

        public abstract bool IsStateUsed(int stateId);

        public abstract void DeleteState(int stateId);

        #endregion

        #region Location

        public abstract IDataReader GetLocations(int? jobGroupId, int portalId);

        public abstract IDataReader GetLocation(int locationId);

        public abstract void UpdateLocation(int locationId, string description, int stateId);

        public abstract void InsertLocation(string locationName, int stateId, int portalId);

        public abstract int? GetLocationId(string locationName, int? stateId, int portalId);

        public abstract bool IsLocationUsed(int locationId);

        public abstract void DeleteLocation(int locationId);

        #endregion

        #region Category

        public abstract IDataReader GetCategories(int? jobGroupId, int portalId);

        public abstract IDataReader GetCategory(int categoryId);

        public abstract void UpdateCategory(int categoryId, string description);

        public abstract void InsertCategory(string categoryName, int portalId);

        public abstract int? GetCategoryId(string categoryName, int portalId);

        public abstract bool IsCategoryUsed(int categoryId);

        public abstract void DeleteCategory(int categoryId);

        #endregion

        #region Position

        public abstract IDataReader GetPositions(int? jobGroupId, int portalId);

        public abstract IDataReader GetPosition(int positionId);

        public abstract void UpdatePosition(int positionId, string jobTitle, string jobDescription);

        public abstract void InsertPosition(string jobTitle, string jobDescription, int portalId);

        public abstract int? GetPositionId(string name, int portalId);

        public abstract bool IsPositionUsed(int positionId);

        public abstract void DeletePosition(int positionId);

        #endregion

        #region UserStatus & ApplicationStatus

        public abstract DataTable GetUserStatuses(int portalId);

        public abstract DataTable GetUserStatus(int statusId);

        public abstract void UpdateUserStatus(int statusId, string statusName);

        public abstract void InsertUserStatus(string statusName, int portalId);

        public abstract void DeleteUserStatus(int statusId);

        public abstract int? GetUserStatusId(string status, int portalId);

        public abstract bool IsUserStatusUsed(int statusId);

        public abstract bool IsApplicationStatusUsed(int statusId);

        #endregion

        #region JobGroup

        public abstract DataTable GetJobGroups(int portalId);

        public abstract DataTable GetJobGroup(int jobGroupId);

        public abstract void UpdateJobGroup(int jobGroupId, string jobGroupName);

        public abstract void InsertJobGroup(string jobGroupName, int portalId);

        public abstract bool IsJobGroupUsed(int jobGroupId);

        public abstract bool IsJobGroupNameUsed(string jobGroupName, int portalId);

        public abstract void DeleteJobGroup(int jobGroupId);

        #endregion

        #region JobJobGroup

        public abstract void AssignJobToJobGroups(int jobId, List<int> jobGroups);

        /// <summary>
        /// Gets a <see cref="DataSet"/> with all jobs in a <see cref="DataTable"/> named "Jobs," and all job groups in a <see cref="DataTable"/> named "JobGroup."
        /// They are collected by a <see cref="DataRelation"/> named "JobJobGroup."
        /// </summary>
        /// <param name="portalId">The id of the portal.</param>
        /// <returns>A <see cref="DataSet"/> with all jobs and their assigned groups.</returns>
        public abstract DataSet GetAssignedJobGroups(int portalId);

        public abstract bool IsJobInGroup(int id, int jobGroupId);

        #endregion

        #region JobSearchQuery

        public abstract IDataReader GetJobSearchQuery(int jobSearchQueryId);

        public abstract IDataReader GetJobSearchQueries(int userId, int? jobGroupId, int portalId);

        public abstract DataTable GetJobSearchResults(int? positionId, int? categoryId, int? locationId, int? stateId, int? jobGroupId, int portalId);

        public abstract DataTable GetKeywordSearchResults(string keyword, int? jobGroupId, int portalId);

        public abstract DataTable GetKeywordSearchResults(List<string> keywords, int? jobGroupId, int portalId);

        public abstract void SaveJobSearchQuery(int userId, string searchName, int? positionId, int? categoryId, int? stateId, int? locationId, string keywords, int? jobGroupId, int portalId);
        
        public abstract void DeleteJobSearchQuery(int jobSearchQueryId);

        #endregion

        #region ApplicationPropertyDefinition

        public abstract bool IsPropertyValueUsed(int propertyId, string value);

        public abstract IDataReader GetApplicationProperty(string name, int? portalId);

        public abstract DataTable GetApplicationProperties(int applicationId);

        public abstract void InsertApplicationProperty(int applicationId, int propertyId, string value);

        public abstract void UpdateApplicationProperty(int applicationId, int propertyId, string value);

        #endregion

        #region Utility

        public abstract IDataReader GetCommonWords();

        #endregion
    }
}
