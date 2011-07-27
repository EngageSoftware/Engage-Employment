CREATE TABLE [dotnetnuke_dukes].[dnn_EngageEmployment_Job] (
    [JobId]                    INT             IDENTITY (1, 1) NOT NULL,
    [JobPositionId]            INT             NOT NULL,
    [LocationId]               INT             NOT NULL,
    [JobCategoryId]            INT             NOT NULL,
    [PostedDate]               DATETIME        NOT NULL,
    [IsHot]                    BIT             NOT NULL,
    [IsFilled]                 BIT             NOT NULL,
    [RequiredQualifications]   NTEXT           NULL,
    [DesiredQualifications]    NTEXT           NULL,
    [RevisingUser]             INT             NOT NULL,
    [RevisionDate]             DATETIME        NOT NULL,
    [SortOrder]                INT             NOT NULL,
    [PortalId]                 INT             NOT NULL,
    [NotificationEmailAddress] NTEXT           NULL,
    [StartDate]                DATETIME        NOT NULL,
    [ExpireDate]               DATETIME        NULL,
    [ApplicationUrl]           NVARCHAR (2000) NULL
);



