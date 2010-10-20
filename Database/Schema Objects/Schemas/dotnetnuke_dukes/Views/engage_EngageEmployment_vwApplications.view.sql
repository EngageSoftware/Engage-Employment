
CREATE VIEW dotnetnuke_dukes.[dnn_EngageEmployment_vwApplications]
AS
select uj.ApplicationId, uj.UserId, uj.JobId, uj.AppliedDate, uj.SalaryRequirement, uj.Message,
 j.PortalId, j.JobTitle, j.PositionId, j.LocationName, j.LocationId, j.StateName, j.StateId, 
 j.RequiredQualifications, j.DesiredQualifications, j.CategoryId, j.CategoryName, j.IsHot, j.PostedDate, 
 j.JobDescription, j.IsFilled, j.SortOrder, uj.StatusId, uj.RevisingUserId, u.DisplayName
from dnn_EngageEmployment_UserJob uj
join dnn_EngageEmployment_vwJobs j ON (uj.JobId = j.JobId)
left join dnn_Users u ON (u.UserId = uj.UserId)
