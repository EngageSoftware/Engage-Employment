
CREATE PROCEDURE dotnetnuke_dukes.[dnn_EngageEmployment_spGetStates]
@portalId int,
@jobGroupId int
AS
	SELECT DISTINCT StateId, StateName, StateAbbreviation
	FROM dnn_EngageEmployment_vwStates
	WHERE PortalId = @portalId
	 AND (@jobGroupId IS NULL OR JobGroupId = @jobGroupId)
	ORDER BY StateName