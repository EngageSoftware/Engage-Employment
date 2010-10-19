
CREATE PROCEDURE dbo.[engage_EngageEmployment_spUpdatePosition]
@PositionId int,
@JobTitle nvarchar(255),
@JobDescription ntext
AS

	UPDATE engage_EngageEmployment_lkpJobPosition
	SET JobTitle = @JobTitle,
	[Description] = @JobDescription
	WHERE JobPositionId = @PositionId