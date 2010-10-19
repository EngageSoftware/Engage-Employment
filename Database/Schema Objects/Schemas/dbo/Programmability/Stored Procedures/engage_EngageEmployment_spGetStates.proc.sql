
CREATE PROCEDURE dbo.[engage_EngageEmployment_spGetStates]
@portalId int,
@jobGroupId int
AS
	SELECT DISTINCT StateId, StateName, StateAbbreviation
	FROM engage_EngageEmployment_vwStates
	WHERE PortalId = @portalId
	 AND (@jobGroupId IS NULL OR JobGroupId = @jobGroupId)
	ORDER BY StateName