
CREATE VIEW dbo.[engage_EngageEmployment_vwSavedSearches]
AS
select 
 us.UserSearchId, us.UserId, us.[Description] AS SearchName, us.Keywords, us.CreationDate, us.JobGroupId,
 jp.JobPositionId AS PositionId, jp.JobTitle, 
 l.LocationId, l.[Description] as LocationName, s.StateId, s.[Name] as StateName, s.Abbreviation as StateAbbreviation,
 us.CategoryId, c.Description as CategoryName, us.PortalId
 from 
 dbo.engage_EngageEmployment_UserJobSearch us
 left join dbo.engage_EngageEmployment_lkpJobPosition jp on (us.JobPositionId = jp.JobPositionId) 
 left join dbo.engage_EngageEmployment_lkpLocation l on (us.LocationId = l.LocationId) 
 left join dbo.engage_EngageEmployment_lkpState s on (us.StateId = s.StateId) 
 left join dbo.engage_EngageEmployment_lkpJobCategory c on (us.CategoryId = c.JobCategoryId)