
CREATE VIEW dbo.[engage_EngageEmployment_vwDocuments]
AS
select a.ApplicationId, d.ResumeId as DocumentId, d.FileName, d.ContentType, d.ContentLength, d.ResumeData, dt.DocumentTypeId, dt.Description as DocumentType, d.UserId, d.RevisionDate
from engage_EngageEmployment_vwApplications a
join engage_EngageEmployment_ApplicationDocument ad ON (ad.ApplicationId = a.ApplicationId)
join engage_EngageEmployment_Document d ON (ad.ResumeId = d.ResumeId)
join engage_EngageEmployment_DocumentType dt ON (d.DocumentTypeId = dt.DocumentTypeId)