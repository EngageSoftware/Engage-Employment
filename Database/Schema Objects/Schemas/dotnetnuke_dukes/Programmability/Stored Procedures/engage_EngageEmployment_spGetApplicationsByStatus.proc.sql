
CREATE PROCEDURE dotnetnuke_dukes.[dnn_EngageEmployment_spGetApplicationsByStatus]
@statusId int
AS
	SELECT ApplicationId, UserId, JobId, AppliedDate, SalaryRequirement, Message, PortalId, PositionId, LocationId, CategoryId, StatusId
	FROM dnn_EngageEmployment_vwApplications WHERE StatusId = @statusId
