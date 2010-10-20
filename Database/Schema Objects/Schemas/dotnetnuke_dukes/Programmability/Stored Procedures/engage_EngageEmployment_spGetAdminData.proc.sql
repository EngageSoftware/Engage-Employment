
CREATE PROCEDURE dotnetnuke_dukes.[dnn_EngageEmployment_spGetAdminData]
@JobGroupId int,
@PortalId int
AS
	SELECT DISTINCT j.JobId
	 INTO #jobs
	 FROM dnn_EngageEmployment_vwJobs j
	 LEFT JOIN dnn_EngageEmployment_JobJobGroup jjg on (j.JobId = jjg.JobId)
	 WHERE (@JobGroupId IS NULL OR JobGroupId = @JobGroupId)
	  AND PortalId = @PortalId
	 
	SELECT	JobId, PostedDate, SortOrder, LocationId, LocationName, 
			StateId, StateName, StateAbbreviation, CategoryId, 
			CategoryName, PositionId, JobTitle, JobDescription,
			(SELECT count(*) FROM dnn_EngageEmployment_UserJob uj WHERE uj.JobId = j.JobId) AS ApplicationCount
	 FROM dnn_EngageEmployment_vwJobs j
	 WHERE JobId IN (SELECT JobId FROM #jobs)
	 ORDER BY SortOrder, PostedDate