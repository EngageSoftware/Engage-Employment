CREATE TABLE [dotnetnuke_dukes].[dnn_EngageEmployment_lkpUserStatus] (
    [UserStatusId] INT            IDENTITY (1, 1) NOT NULL,
    [Status]       NVARCHAR (255) NOT NULL,
    [PortalId]     INT            NOT NULL
);

