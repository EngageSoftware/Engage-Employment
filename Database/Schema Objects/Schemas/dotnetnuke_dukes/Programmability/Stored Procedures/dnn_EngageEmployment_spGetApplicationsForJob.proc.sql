

CREATE PROCEDURE dotnetnuke_dukes.dnn_EngageEmployment_spGetApplicationsForJob (
    @jobId int,
    @jobGroupId int,
    @applicationStatusId int,
    @userStatusId int,
	@leadId int,
	@dateFrom datetime,
	@dateTo datetime,
    @index int, 
    @pageSize int,
    @fillDocumentsAndProperties bit
)
AS
BEGIN

IF (@pageSize = 0)
    SET @pageSize = NULL
 
CREATE TABLE #results (
[Id] int NOT NULL IDENTITY(1,1),
[ApplicationId] int
)
 
INSERT INTO #results ([ApplicationId]) 
SELECT B.ApplicationId FROM
    (SELECT DISTINCT a.ApplicationId, a.AppliedDate
    FROM dotnetnuke_dukes.dnn_EngageEmployment_vwApplications a
    LEFT JOIN dotnetnuke_dukes.dnn_EngageEmployment_JobJobGroup jjg on (a.JobId = jjg.JobId) 
	LEFT JOIN dotnetnuke_dukes.dnn_EngageEmployment_UserStatus us on (a.UserId = us.UserId) 
	LEFT JOIN dotnetnuke_dukes.dnn_EngageEmployment_UserJobProperty ujp on (a.ApplicationId = ujp.ApplicationId)
	LEFT JOIN dotnetnuke_dukes.dnn_EngageEmployment_ApplicationProperty ap on (ujp.ApplicationPropertyId = ap.ApplicationPropertyId)
    WHERE a.JobId = @jobId
    AND (@jobGroupId IS NULL OR jjg.jobGroupId = @jobGroupId)
    AND (@applicationStatusId IS NULL OR (@applicationStatusId = 0 AND a.StatusId IS NULL) OR a.StatusId = @applicationStatusId)
    AND (@userStatusId IS NULL OR (@userStatusId = 0 AND us.UserStatusId IS NULL) OR us.UserStatusId = @userStatusId)
    AND (ap.PropertyName = 'Lead')
    AND (@leadId IS NULL OR ujp.PropertyValue = @leadId)
	AND (@dateFrom IS NULL OR a.AppliedDate >= @dateFrom)
	AND (@dateTo IS NULL OR a.AppliedDate <= @dateTo)
	) AS B
ORDER BY B.AppliedDate DESC
 
SELECT @@RowCount AS TotalRecords
 
IF (@pageSize IS NULL)
    BEGIN
        SELECT a.AppliedDate, a.DisplayName, a.JobId, a.JobTitle, a.LocationName, a.ApplicationId, a.UserId, a.SalaryRequirement, a.Message, a.StatusId
        FROM #results r
        JOIN dotnetnuke_dukes.dnn_EngageEmployment_vwApplications a ON (r.ApplicationId = a.ApplicationId)
        ORDER BY r.Id
    END
ELSE
    BEGIN
        SELECT a.AppliedDate, a.DisplayName, a.JobId, a.JobTitle, a.LocationName, a.ApplicationId, a.UserId, a.SalaryRequirement, a.Message, a.StatusId
        FROM #results r
        JOIN dotnetnuke_dukes.dnn_EngageEmployment_vwApplications a ON (r.ApplicationId = a.ApplicationId)
        WHERE (r.Id >= @index * @pageSize + 1) AND r.Id < (@index * @pageSize) + @pageSize + 1
        ORDER BY r.Id
    END
END

IF (@fillDocumentsAndProperties = 1)
BEGIN
    SELECT DocumentId, [DocumentTypeId], [FileName], ApplicationId
    FROM dotnetnuke_dukes.dnn_EngageEmployment_vwDocuments
    WHERE ApplicationId IN (SELECT ApplicationId 
                            FROM #results
                            WHERE @pageSize IS NULL
                               OR ((Id >= @index * @pageSize + 1) 
                                AND Id < (@index * @pageSize) + @pageSize + 1))

    SELECT ujp.[ApplicationId], ujp.[ApplicationPropertyId], ujp.[Visibility], ap.[PropertyName],
        CASE WHEN ujp.[PropertyValue] IS NULL THEN ujp.[PropertyText] ELSE ujp.[PropertyValue] END AS PropertyValue
    FROM dotnetnuke_dukes.dnn_EngageEmployment_UserJobProperty ujp 
    JOIN dotnetnuke_dukes.dnn_EngageEmployment_ApplicationProperty ap ON (ujp.ApplicationPropertyId = ap.ApplicationPropertyId)
    WHERE ApplicationId IN (SELECT ApplicationId 
                            FROM #results
                            WHERE @pageSize IS NULL
                               OR ((Id >= @index * @pageSize + 1) 
                                AND Id < (@index * @pageSize) + @pageSize + 1))
END