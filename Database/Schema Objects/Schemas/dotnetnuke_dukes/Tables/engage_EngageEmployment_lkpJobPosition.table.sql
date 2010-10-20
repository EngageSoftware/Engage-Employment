CREATE TABLE [dotnetnuke_dukes].[dnn_EngageEmployment_lkpJobPosition] (
    [JobPositionId] INT            IDENTITY (1, 1) NOT NULL,
    [JobTitle]      NVARCHAR (255) NOT NULL,
    [Description]   NTEXT          NULL,
    [PortalId]      INT            NOT NULL
);

