
CREATE PROCEDURE dbo.[engage_EngageEmployment_spInsertState]
@stateName nvarchar(255),
@stateAbbreviation nvarchar(10),
@portalId int
AS
	INSERT engage_EngageEmployment_lkpState
	(Name, PortalId, Abbreviation)
	VALUES (@stateName, @portalId, @stateAbbreviation)