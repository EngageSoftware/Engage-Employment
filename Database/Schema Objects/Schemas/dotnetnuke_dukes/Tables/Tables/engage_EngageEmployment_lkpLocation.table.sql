CREATE TABLE [dbo].[engage_EngageEmployment_lkpLocation] (
    [LocationId]  INT            IDENTITY (1, 1) NOT NULL,
    [Description] NVARCHAR (255) NOT NULL,
    [StateId]     INT            NOT NULL,
    [PortalId]    INT            NOT NULL
);

