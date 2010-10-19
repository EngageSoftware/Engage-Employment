
CREATE PROCEDURE dbo.[engage_EngageEmployment_spGetPositions]
@portalId int,
@jobGroupId int
AS
    CREATE TABLE #positions (PositionId INT PRIMARY KEY);

	INSERT INTO #positions
	SELECT DISTINCT PositionId
	FROM engage_EngageEmployment_vwPositions
	WHERE PortalId = @portalId
	 AND (@jobGroupId IS NULL OR JobGroupId = @jobGroupId)
	
	SELECT JobPositionId AS PositionId, JobTitle, [Description] AS JobDescription
	FROM engage_EngageEmployment_lkpJobPosition
	WHERE JobPositionId in (SELECT PositionId FROM #positions)
	ORDER BY JobTitle