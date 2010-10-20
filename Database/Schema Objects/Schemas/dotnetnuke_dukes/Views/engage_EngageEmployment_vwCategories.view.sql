
CREATE VIEW dotnetnuke_dukes.[dnn_EngageEmployment_vwCategories]
AS 
	SELECT DISTINCT c.JobCategoryId AS CategoryId, c.Description AS CategoryName, c.PortalId, jjg.JobGroupId
	FROM [dnn_EngageEmployment_lkpJobCategory] c
	 LEFT JOIN [dnn_EngageEmployment_Job] j ON (j.JobCategoryId = c.JobCategoryId)
	 LEFT JOIN [dnn_EngageEmployment_JobJobGroup] jjg ON (j.JobId = jjg.JobId)
