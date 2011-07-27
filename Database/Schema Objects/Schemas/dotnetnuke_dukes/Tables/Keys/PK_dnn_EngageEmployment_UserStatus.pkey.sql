ALTER TABLE [dotnetnuke_dukes].[dnn_EngageEmployment_UserStatus]
    ADD CONSTRAINT [PK_dnn_EngageEmployment_UserStatus] PRIMARY KEY CLUSTERED ([UserStatusId] ASC, [UserId] ASC, [PortalId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);

