
CREATE VIEW dotnetnuke_dukes.[dnn_EngageEmployment_vwLocations]
AS 
	SELECT DISTINCT l.LocationId, l.Description as LocationName, s.StateId, s.Name as StateName, s.Abbreviation as StateAbbreviation, jjg.JobGroupId, l.PortalId
	FROM [dnn_EngageEmployment_lkpLocation] l
	 JOIN [dnn_EngageEmployment_lkpState] s ON (l.StateId = s.StateId)
	 LEFT JOIN [dnn_EngageEmployment_Job] j ON (j.LocationId = l.LocationId)
	 LEFT JOIN [dnn_EngageEmployment_JobJobGroup] jjg ON (j.JobId = jjg.JobId)
