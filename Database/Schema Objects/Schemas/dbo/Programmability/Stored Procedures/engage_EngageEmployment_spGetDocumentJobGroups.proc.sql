
CREATE PROCEDURE dbo.[engage_EngageEmployment_spGetDocumentJobGroups]
@documentId int
AS
	SELECT DISTINCT jobGroupId FROM engage_EngageEmployment_vwDocuments d
	 JOIN engage_EngageEmployment_vwApplications a ON (d.ApplicationId = a.ApplicationId)
	 JOIN engage_EngageEmployment_JobJobGroup jjg ON (a.JobId = jjg.JobId)
	WHERE DocumentId = @documentId