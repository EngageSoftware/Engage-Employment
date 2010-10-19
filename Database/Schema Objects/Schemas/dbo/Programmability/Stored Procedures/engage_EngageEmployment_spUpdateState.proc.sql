
CREATE PROCEDURE dbo.[engage_EngageEmployment_spUpdateState]
@stateId int,
@stateName nvarchar(255),
@stateAbbreviation nvarchar(10)
AS
	UPDATE engage_EngageEmployment_lkpState
	SET [Name] = @stateName,
	 Abbreviation = @stateAbbreviation
	WHERE StateId = @stateId