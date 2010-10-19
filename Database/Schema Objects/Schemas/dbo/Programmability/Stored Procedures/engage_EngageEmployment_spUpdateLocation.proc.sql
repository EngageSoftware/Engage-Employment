
CREATE PROCEDURE dbo.[engage_EngageEmployment_spUpdateLocation]
@locationName nvarchar(255),
@stateId int,
@locationId int
AS
	UPDATE engage_EngageEmployment_lkpLocation
	SET [Description] = @locationName,
	 StateId = @stateId
	WHERE LocationId = @locationId