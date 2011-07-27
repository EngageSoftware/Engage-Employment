ALTER TABLE [dotnetnuke_dukes].[dnn_EngageEmployment_UserStatus]
    ADD CONSTRAINT [dnn_FK_EngageEmployment_UserStatus_EngageEmployment_lkpUserStatus] FOREIGN KEY ([UserStatusId]) REFERENCES [dotnetnuke_dukes].[dnn_EngageEmployment_lkpUserStatus] ([UserStatusId]) ON DELETE NO ACTION ON UPDATE NO ACTION;

