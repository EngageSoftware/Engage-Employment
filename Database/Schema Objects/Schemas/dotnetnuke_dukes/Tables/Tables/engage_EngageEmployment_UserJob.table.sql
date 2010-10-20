CREATE TABLE [dbo].[engage_EngageEmployment_UserJob] (
    [ApplicationId]     INT            IDENTITY (1, 1) NOT NULL,
    [UserId]            INT            NULL,
    [JobId]             INT            NOT NULL,
    [AppliedDate]       DATETIME       NOT NULL,
    [SalaryRequirement] NVARCHAR (255) NULL,
    [Message]           NVARCHAR (255) NULL,
    [StatusId]          INT            NULL,
    [RevisingUserId]    INT            NULL
);

