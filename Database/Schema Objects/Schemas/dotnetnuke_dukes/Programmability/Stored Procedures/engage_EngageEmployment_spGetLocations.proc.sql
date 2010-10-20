
CREATE PROCEDURE dotnetnuke_dukes.[dnn_EngageEmployment_spGetLocations]
@portalId int,
@jobGroupId int
AS
	SELECT DISTINCT LocationId, LocationName, StateId, StateName, StateAbbreviation
	FROM dnn_EngageEmployment_vwLocations
	WHERE PortalId = @portalId
	 AND (@jobGroupId IS NULL OR JobGroupId = @jobGroupId)
	ORDER BY StateName, LocationName