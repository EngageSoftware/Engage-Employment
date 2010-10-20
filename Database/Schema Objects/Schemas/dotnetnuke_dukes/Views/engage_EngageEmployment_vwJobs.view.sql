

CREATE VIEW dotnetnuke_dukes.dnn_EngageEmployment_vwJobs
AS  
SELECT  
j.JobId, p.JobTitle, p.JobPositionId AS PositionId, l.[Description] AS LocationName,   
l.LocationId, s.[Name] AS StateName, s.Abbreviation AS StateAbbreviation, s.StateId,  
j.RequiredQualifications, j.DesiredQualifications, jc.[Description] AS CategoryName,   
jc.JobCategoryId AS CategoryId, j.IsHot, j.PostedDate, p.[Description] AS JobDescription,   
j.IsFilled, j.SortOrder, j.RevisingUser, j.PortalId, j.NotificationEmailAddress,  
j.RevisionDate, j.StartDate, j.ExpireDate, j.ApplicationUrl
FROM  
dnn_EngageEmployment_job j  
JOIN dnn_EngageEmployment_lkpJobPosition p ON (j.JobPositionId = p.JobPositionId)  
JOIN dnn_EngageEmployment_lkpLocation l ON (j.LocationId = l.LocationId)  
JOIN dnn_EngageEmployment_lkpState s ON (l.StateId = s.StateId)  
JOIN dnn_EngageEmployment_lkpJobCategory jc ON (j.JobCategoryId = jc.JobCategoryId)
