CREATE TABLE [dbo].[engage_EngageEmployment_UserJobProperty] (
    [UserJobPropertyId]     INT             IDENTITY (1, 1) NOT NULL,
    [ApplicationId]         INT             NOT NULL,
    [ApplicationPropertyId] INT             NOT NULL,
    [PropertyValue]         NVARCHAR (3750) NULL,
    [PropertyText]          NTEXT           NULL,
    [Visibility]            INT             NOT NULL,
    [LastUpdatedDate]       DATETIME        NOT NULL
);

