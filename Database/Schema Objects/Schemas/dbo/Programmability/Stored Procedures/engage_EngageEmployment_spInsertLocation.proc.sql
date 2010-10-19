
CREATE PROCEDURE dbo.[engage_EngageEmployment_spInsertLocation]
@locationName nvarchar(255),
@stateId int,
@portalId int
AS
	INSERT engage_EngageEmployment_lkpLocation
	([Description], Stateid, PortalId)
	VALUES (@locationName, @stateId, @portalId)