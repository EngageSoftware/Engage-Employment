
CREATE PROCEDURE dotnetnuke_dukes.[dnn_EngageEmployment_spGetDocumentJobGroups]
@documentId int
AS
	SELECT DISTINCT jobGroupId FROM dnn_EngageEmployment_vwDocuments d
	 JOIN dnn_EngageEmployment_vwApplications a ON (d.ApplicationId = a.ApplicationId)
	 JOIN dnn_EngageEmployment_JobJobGroup jjg ON (a.JobId = jjg.JobId)
	WHERE DocumentId = @documentId