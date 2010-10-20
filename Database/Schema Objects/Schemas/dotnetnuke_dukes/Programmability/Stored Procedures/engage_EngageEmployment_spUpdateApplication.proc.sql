
CREATE PROCEDURE dotnetnuke_dukes.[dnn_EngageEmployment_spUpdateApplication]
@applicationId int,
@statusId int,
@revisingUserId int
AS
	UPDATE dnn_EngageEmployment_vwApplications
	SET StatusId = @statusId,
		RevisingUserId = @revisingUserId
	WHERE ApplicationId = @applicationId