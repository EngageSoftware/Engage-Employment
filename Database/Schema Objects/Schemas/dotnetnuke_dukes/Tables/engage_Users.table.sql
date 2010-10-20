CREATE TABLE [dotnetnuke_dukes].[dnn_Users] (
    [UserID]               INT            IDENTITY (1, 1) NOT NULL,
    [Username]             NVARCHAR (100) NOT NULL,
    [FirstName]            NVARCHAR (50)  NOT NULL,
    [LastName]             NVARCHAR (50)  NOT NULL,
    [IsSuperUser]          BIT            NOT NULL,
    [AffiliateId]          INT            NULL,
    [Email]                NVARCHAR (256) NULL,
    [DisplayName]          NVARCHAR (128) NOT NULL,
    [UpdatePassword]       BIT            NOT NULL,
    [LastIPAddress]        NVARCHAR (50)  NULL,
    [IsDeleted]            BIT            NOT NULL,
    [CreatedByUserID]      INT            NULL,
    [CreatedOnDate]        DATETIME       NULL,
    [LastModifiedByUserID] INT            NULL,
    [LastModifiedOnDate]   DATETIME       NULL
);

