
CREATE PROCEDURE dbo.[engage_EngageEmployment_spGetDocument]
@documentId int
AS
	SELECT [DocumentId], [UserId], [FileName], [ContentType], [ContentLength], [ResumeData], [RevisionDate], [DocumentTypeId]
	FROM engage_EngageEmployment_vwDocuments
	WHERE DocumentId = @documentId