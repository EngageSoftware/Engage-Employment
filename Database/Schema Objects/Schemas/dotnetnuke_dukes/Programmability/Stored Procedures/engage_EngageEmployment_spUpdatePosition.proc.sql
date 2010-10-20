
CREATE PROCEDURE dotnetnuke_dukes.[dnn_EngageEmployment_spUpdatePosition]
@PositionId int,
@JobTitle nvarchar(255),
@JobDescription ntext
AS

	UPDATE dnn_EngageEmployment_lkpJobPosition
	SET JobTitle = @JobTitle,
	[Description] = @JobDescription
	WHERE JobPositionId = @PositionId