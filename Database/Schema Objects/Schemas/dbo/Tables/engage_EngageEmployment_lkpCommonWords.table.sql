CREATE TABLE [dbo].[engage_EngageEmployment_lkpCommonWords] (
    [CommonWordID] INT            IDENTITY (1, 1) NOT NULL,
    [Word]         NVARCHAR (255) NOT NULL,
    [Locale]       NVARCHAR (10)  NULL
);

