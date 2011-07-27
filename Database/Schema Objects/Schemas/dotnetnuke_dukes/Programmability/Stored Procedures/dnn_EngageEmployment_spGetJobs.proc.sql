

CREATE PROCEDURE dotnetnuke_dukes.dnn_EngageEmployment_spGetJobs (
    @jobGroupId int,
    @portalId int,
	@jobTitle nvarchar(255),
	@locationId int,
    @index int, 
    @pageSize int
)
AS
BEGIN
 
 
CREATE TABLE #results (
[Id] int NOT NULL IDENTITY(1,1),
[JobId] int
)
 
INSERT INTO #results ([JobId]) 
SELECT A.JobId FROM
    (SELECT DISTINCT j.JobId, j.SortOrder, j.CategoryName, j.JobTitle
    FROM dotnetnuke_dukes.dnn_EngageEmployment_vwJobs j
    LEFT JOIN dotnetnuke_dukes.dnn_EngageEmployment_JobJobGroup jjg on (j.JobId = jjg.JobId) 
    WHERE j.PortalId = @portalId
    AND (@jobGroupId IS NULL OR jjg.jobGroupId = @jobGroupId)
	AND (@jobTitle IS NULL OR j.JobTitle LIKE '%' + @jobTitle + '%')
	AND (@locationId is NULL OR j.LocationId = @locationId)
	) AS A
ORDER BY A.SortOrder, A.CategoryName, A.JobTitle
 
SELECT @@RowCount AS TotalRecords
 
IF (@pageSize = 0 OR @pageSize IS NULL)
    BEGIN
        SELECT j.JobId, j.JobTitle, j.PositionId, j.LocationName, j.LocationId, j.StateName, j.StateAbbreviation, j.StateId, 
               j.RequiredQualifications, j.DesiredQualifications, j.CategoryName, j.CategoryId, j.NotificationEmailAddress, j.ApplicationUrl, 
               j.IsHot, j.IsFilled, j.PostedDate, j.JobDescription, j.SortOrder, j.RevisingUser, j.RevisionDate, j.StartDate, j.ExpireDate 
        FROM #results r
        JOIN dotnetnuke_dukes.dnn_EngageEmployment_vwJobs j ON (r.JobId = j.JobId)
        ORDER BY r.Id
    END
ELSE
    BEGIN
        SELECT j.JobId, j.JobTitle, j.PositionId, j.LocationName, j.LocationId, j.StateName, j.StateAbbreviation, j.StateId, 
               j.RequiredQualifications, j.DesiredQualifications, j.CategoryName, j.CategoryId, j.NotificationEmailAddress, j.ApplicationUrl, 
               j.IsHot, j.IsFilled, j.PostedDate, j.JobDescription, j.SortOrder, j.RevisingUser, j.RevisionDate, j.StartDate, j.ExpireDate 
        FROM #results r
        JOIN dotnetnuke_dukes.dnn_EngageEmployment_vwJobs j ON (r.JobId = j.JobId)
        WHERE (id >= @index * @pageSize + 1) AND id < (@index * @pageSize) + @pageSize + 1
        ORDER BY r.Id
    END
END