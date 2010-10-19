
CREATE PROCEDURE dbo.[engage_EngageEmployment_spGetApplicationsByStatus]
@statusId int
AS
	SELECT ApplicationId, UserId, JobId, AppliedDate, SalaryRequirement, Message, PortalId, PositionId, LocationId, CategoryId, StatusId
	FROM engage_EngageEmployment_vwApplications WHERE StatusId = @statusId