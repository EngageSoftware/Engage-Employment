
CREATE PROCEDURE dotnetnuke_dukes.[dnn_EngageEmployment_spUpdateLocation]
@locationName nvarchar(255),
@stateId int,
@locationId int
AS
	UPDATE dnn_EngageEmployment_lkpLocation
	SET [Description] = @locationName,
	 StateId = @stateId
	WHERE LocationId = @locationId