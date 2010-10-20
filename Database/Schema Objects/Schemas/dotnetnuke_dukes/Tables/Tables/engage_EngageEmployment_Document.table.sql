CREATE TABLE [dbo].[engage_EngageEmployment_Document] (
    [ResumeId]       INT            IDENTITY (1, 1) NOT NULL,
    [UserId]         INT            NULL,
    [FileName]       NVARCHAR (255) NOT NULL,
    [ContentType]    NVARCHAR (255) NOT NULL,
    [ContentLength]  INT            NOT NULL,
    [ResumeData]     IMAGE          NOT NULL,
    [RevisionDate]   DATETIME       NOT NULL,
    [DocumentTypeId] INT            NOT NULL
);

