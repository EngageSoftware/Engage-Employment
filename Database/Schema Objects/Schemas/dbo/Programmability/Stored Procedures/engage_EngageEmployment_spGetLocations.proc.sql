
CREATE PROCEDURE dbo.[engage_EngageEmployment_spGetLocations]
@portalId int,
@jobGroupId int
AS
	SELECT DISTINCT LocationId, LocationName, StateId, StateName, StateAbbreviation
	FROM engage_EngageEmployment_vwLocations
	WHERE PortalId = @portalId
	 AND (@jobGroupId IS NULL OR JobGroupId = @jobGroupId)
	ORDER BY StateName, LocationName