
CREATE PROCEDURE dotnetnuke_dukes.dnn_EngageEmployment_spUpdateUserStatus
@portalId int,
@userId int,
@userStatusId int
AS
BEGIN TRAN
	IF @userStatusId IS NULL
	BEGIN
		DELETE dotnetnuke_dukes.dnn_EngageEmployment_UserStatus
		WHERE PortalId = @portalId AND UserId = @userId	
	END
	ELSE
	BEGIN 
		IF NOT EXISTS (SELECT NULL FROM dotnetnuke_dukes.dnn_EngageEmployment_UserStatus (HOLDLOCK) WHERE PortalId = @portalId AND UserId = @userId)
		BEGIN
			INSERT INTO dotnetnuke_dukes.dnn_EngageEmployment_UserStatus(PortalId, UserId, UserStatusId)
			VALUES(@portalId, @userId, @userStatusId)
		END
		ELSE
		BEGIN
			UPDATE dotnetnuke_dukes.dnn_EngageEmployment_UserStatus
			SET [UserStatusId] = @userStatusId
			WHERE PortalId = @portalId
			AND UserId = @userId
		END
	END
COMMIT