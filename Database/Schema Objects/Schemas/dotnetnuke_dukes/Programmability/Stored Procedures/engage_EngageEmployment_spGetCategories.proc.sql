
CREATE PROCEDURE dotnetnuke_dukes.[dnn_EngageEmployment_spGetCategories]
@portalId int,
@jobGroupId int
AS
	SELECT DISTINCT CategoryId, CategoryName
	FROM dnn_EngageEmployment_vwCategories
	WHERE PortalId = @portalId
	 AND (@jobGroupId IS NULL OR JobGroupId = @jobGroupId)
	ORDER BY CategoryName