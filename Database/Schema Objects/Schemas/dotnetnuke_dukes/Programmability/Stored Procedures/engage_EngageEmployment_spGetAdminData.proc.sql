
CREATE PROCEDURE dotnetnuke_dukes.dnn_EngageEmployment_spGetAdminData
@JobGroupId int,
@PortalId int
AS
	SELECT DISTINCT j.JobId
	 INTO #jobs
	 FROM dotnetnuke_dukes.dnn_EngageEmployment_vwJobs j
	 LEFT JOIN dotnetnuke_dukes.dnn_EngageEmployment_JobJobGroup jjg on (j.JobId = jjg.JobId)
	 WHERE (@JobGroupId IS NULL OR JobGroupId = @JobGroupId)
	  AND PortalId = @PortalId
	 
	SELECT	JobId, PostedDate, StartDate, SortOrder, LocationId, LocationName, 
			StateId, StateName, StateAbbreviation, CategoryId, 
			CategoryName, PositionId, JobTitle, JobDescription,
			(SELECT count(*) FROM dotnetnuke_dukes.dnn_EngageEmployment_UserJob uj WHERE uj.JobId = j.JobId) AS ApplicationCount
	 FROM dotnetnuke_dukes.dnn_EngageEmployment_vwJobs j
	 WHERE JobId IN (SELECT JobId FROM #jobs)
	 ORDER BY SortOrder, PostedDate

    SELECT COUNT(*) AS Count, a.JobId, a.StatusId
    FROM  dotnetnuke_dukes.dnn_EngageEmployment_vwApplications a
    WHERE a.JobId IN (SELECT JobId FROM #jobs)
      AND a.StatusId IS NOT NULL
    GROUP BY a.JobId, a.StatusId

    SELECT a.JobId, COUNT(us.UserStatusId) AS Count, us.UserStatusId, l.Status
    FROM dotnetnuke_dukes.dnn_EngageEmployment_vwApplications a
    JOIN dotnetnuke_dukes.dnn_EngageEmployment_UserStatus us ON us.UserId = a.UserId
    JOIN dotnetnuke_dukes.dnn_EngageEmployment_lkpUserStatus l ON l.UserStatusId = us.UserStatusId
    WHERE a.JobId IN (SELECT JobId FROM #jobs)
      AND a.UserId IS NOT NULL
    GROUP BY a.JobId, us.UserStatusId, l.Status
    ORDER BY a.JobId