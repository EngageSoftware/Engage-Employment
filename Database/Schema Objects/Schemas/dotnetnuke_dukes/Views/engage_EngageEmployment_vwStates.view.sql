
CREATE VIEW dotnetnuke_dukes.[dnn_EngageEmployment_vwStates]
AS
SELECT DISTINCT
 s.StateId, [Name] AS StateName, Abbreviation AS StateAbbreviation, s.PortalId, JobGroupId
 FROM 
 dotnetnuke_dukes.dnn_EngageEmployment_lkpState s
 LEFT JOIN dotnetnuke_dukes.dnn_EngageEmployment_lkpLocation l ON (s.StateId = l.StateId)
 LEFT JOIN dotnetnuke_dukes.dnn_EngageEmployment_Job j ON (l.LocationId = j.LocationId)
 LEFT JOIN dotnetnuke_dukes.dnn_EngageEmployment_JobJobGroup jjg ON (j.JobId = jjg.JobId)
