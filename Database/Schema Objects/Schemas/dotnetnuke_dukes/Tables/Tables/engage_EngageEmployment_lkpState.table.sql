CREATE TABLE [dbo].[engage_EngageEmployment_lkpState] (
    [StateId]      INT            IDENTITY (1, 1) NOT NULL,
    [Name]         NVARCHAR (255) NOT NULL,
    [Abbreviation] NVARCHAR (10)  NULL,
    [PortalId]     INT            NOT NULL
);

