CREATE TABLE [dbo].[engage_EngageEmployment_lkpJobCategory] (
    [JobCategoryId] INT            IDENTITY (1, 1) NOT NULL,
    [Description]   NVARCHAR (255) NOT NULL,
    [PortalId]      INT            NOT NULL
);

