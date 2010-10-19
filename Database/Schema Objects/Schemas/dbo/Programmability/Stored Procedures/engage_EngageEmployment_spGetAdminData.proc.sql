
CREATE PROCEDURE dbo.[engage_EngageEmployment_spGetAdminData]
@JobGroupId int,
@PortalId int
AS
	SELECT DISTINCT j.JobId
	 INTO #jobs
	 FROM engage_EngageEmployment_vwJobs j
	 LEFT JOIN engage_EngageEmployment_JobJobGroup jjg on (j.JobId = jjg.JobId)
	 WHERE (@JobGroupId IS NULL OR JobGroupId = @JobGroupId)
	  AND PortalId = @PortalId
	 
	SELECT	JobId, PostedDate, SortOrder, LocationId, LocationName, 
			StateId, StateName, StateAbbreviation, CategoryId, 
			CategoryName, PositionId, JobTitle, JobDescription,
			(SELECT count(*) FROM engage_EngageEmployment_UserJob uj WHERE uj.JobId = j.JobId) AS ApplicationCount
	 FROM engage_EngageEmployment_vwJobs j
	 WHERE JobId IN (SELECT JobId FROM #jobs)
	 ORDER BY SortOrder, PostedDate