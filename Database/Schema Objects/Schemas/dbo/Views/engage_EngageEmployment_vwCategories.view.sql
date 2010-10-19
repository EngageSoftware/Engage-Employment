
CREATE VIEW dbo.[engage_EngageEmployment_vwCategories]
AS 
	SELECT DISTINCT c.JobCategoryId AS CategoryId, c.Description AS CategoryName, c.PortalId, jjg.JobGroupId
	FROM [engage_EngageEmployment_lkpJobCategory] c
	 LEFT JOIN [engage_EngageEmployment_Job] j ON (j.JobCategoryId = c.JobCategoryId)
	 LEFT JOIN [engage_EngageEmployment_JobJobGroup] jjg ON (j.JobId = jjg.JobId)