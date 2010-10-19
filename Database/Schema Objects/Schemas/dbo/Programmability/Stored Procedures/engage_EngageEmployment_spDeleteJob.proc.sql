
CREATE PROCEDURE dbo.[engage_EngageEmployment_spDeleteJob]
@JobId int
AS

	DELETE [engage_EngageEmployment_JobJobGroup]
	WHERE JobId = @JobId
	
	DELETE [engage_EngageEmployment_UserJobProperty]
	WHERE ApplicationId IN (SELECT ApplicationId FROM [engage_EngageEmployment_UserJob] WHERE JobId = @JobId)
	
	DELETE [engage_EngageEmployment_ApplicationDocument]
	WHERE ApplicationId IN (SELECT ApplicationId FROM [engage_EngageEmployment_UserJob] WHERE JobId = @JobId)
	
	DELETE [engage_EngageEmployment_UserJob]
	WHERE JobId = @JobId
	
	DELETE [engage_EngageEmployment_Job]
	WHERE JobId = @JobId