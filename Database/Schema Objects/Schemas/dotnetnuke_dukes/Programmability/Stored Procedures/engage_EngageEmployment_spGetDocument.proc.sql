
CREATE PROCEDURE dotnetnuke_dukes.[dnn_EngageEmployment_spGetDocument]
@documentId int
AS
	SELECT [DocumentId], [UserId], [FileName], [ContentType], [ContentLength], [ResumeData], [RevisionDate], [DocumentTypeId]
	FROM dnn_EngageEmployment_vwDocuments
	WHERE DocumentId = @documentId