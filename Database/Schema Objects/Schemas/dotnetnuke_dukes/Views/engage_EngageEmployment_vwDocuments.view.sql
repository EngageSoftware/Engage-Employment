
CREATE VIEW dotnetnuke_dukes.[dnn_EngageEmployment_vwDocuments]
AS
select a.ApplicationId, d.ResumeId as DocumentId, d.FileName, d.ContentType, d.ContentLength, d.ResumeData, dt.DocumentTypeId, dt.Description as DocumentType, d.UserId, d.RevisionDate
from dnn_EngageEmployment_vwApplications a
join dnn_EngageEmployment_ApplicationDocument ad ON (ad.ApplicationId = a.ApplicationId)
join dnn_EngageEmployment_Document d ON (ad.ResumeId = d.ResumeId)
join dnn_EngageEmployment_DocumentType dt ON (d.DocumentTypeId = dt.DocumentTypeId)
