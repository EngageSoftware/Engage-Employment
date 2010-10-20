
CREATE PROCEDURE dotnetnuke_dukes.[dnn_EngageEmployment_spDeleteJob]
@JobId int
AS

	DELETE [dnn_EngageEmployment_JobJobGroup]
	WHERE JobId = @JobId
	
	DELETE [dnn_EngageEmployment_UserJobProperty]
	WHERE ApplicationId IN (SELECT ApplicationId FROM [dnn_EngageEmployment_UserJob] WHERE JobId = @JobId)
	
	DELETE [dnn_EngageEmployment_ApplicationDocument]
	WHERE ApplicationId IN (SELECT ApplicationId FROM [dnn_EngageEmployment_UserJob] WHERE JobId = @JobId)
	
	DELETE [dnn_EngageEmployment_UserJob]
	WHERE JobId = @JobId
	
	DELETE [dnn_EngageEmployment_Job]
	WHERE JobId = @JobId