
CREATE PROCEDURE dbo.[engage_EngageEmployment_spUpdateApplication]
@applicationId int,
@statusId int,
@revisingUserId int
AS
	UPDATE engage_EngageEmployment_vwApplications
	SET StatusId = @statusId,
		RevisingUserId = @revisingUserId
	WHERE ApplicationId = @applicationId