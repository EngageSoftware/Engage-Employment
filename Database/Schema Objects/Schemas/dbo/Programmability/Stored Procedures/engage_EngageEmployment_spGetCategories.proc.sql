
CREATE PROCEDURE dbo.[engage_EngageEmployment_spGetCategories]
@portalId int,
@jobGroupId int
AS
	SELECT DISTINCT CategoryId, CategoryName
	FROM engage_EngageEmployment_vwCategories
	WHERE PortalId = @portalId
	 AND (@jobGroupId IS NULL OR JobGroupId = @jobGroupId)
	ORDER BY CategoryName