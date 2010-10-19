
CREATE VIEW dbo.[engage_EngageEmployment_vwStates]
AS
SELECT DISTINCT
 s.StateId, [Name] AS StateName, Abbreviation AS StateAbbreviation, s.PortalId, JobGroupId
 FROM 
 dbo.engage_EngageEmployment_lkpState s
 LEFT JOIN dbo.engage_EngageEmployment_lkpLocation l ON (s.StateId = l.StateId)
 LEFT JOIN dbo.engage_EngageEmployment_Job j ON (l.LocationId = j.LocationId)
 LEFT JOIN dbo.engage_EngageEmployment_JobJobGroup jjg ON (j.JobId = jjg.JobId)