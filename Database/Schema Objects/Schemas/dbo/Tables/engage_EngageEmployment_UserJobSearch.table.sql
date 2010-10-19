CREATE TABLE [dbo].[engage_EngageEmployment_UserJobSearch] (
    [UserSearchId]  INT            IDENTITY (1, 1) NOT NULL,
    [UserId]        INT            NOT NULL,
    [JobGroupId]    INT            NULL,
    [Description]   NVARCHAR (255) NOT NULL,
    [JobPositionId] INT            NULL,
    [StateId]       INT            NULL,
    [Keywords]      NTEXT          NULL,
    [CreationDate]  DATETIME       NOT NULL,
    [LocationId]    INT            NULL,
    [SearchSql]     NTEXT          NULL,
    [PortalId]      INT            NOT NULL,
    [CategoryId]    INT            NULL
);

