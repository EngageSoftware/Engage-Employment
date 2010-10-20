
CREATE PROCEDURE dotnetnuke_dukes.[dnn_EngageEmployment_spInsertLocation]
@locationName nvarchar(255),
@stateId int,
@portalId int
AS
	INSERT dnn_EngageEmployment_lkpLocation
	([Description], Stateid, PortalId)
	VALUES (@locationName, @stateId, @portalId)