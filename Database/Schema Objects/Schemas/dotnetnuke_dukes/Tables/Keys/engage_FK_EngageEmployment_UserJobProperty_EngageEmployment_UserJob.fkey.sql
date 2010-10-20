ALTER TABLE [dotnetnuke_dukes].[dnn_EngageEmployment_UserJobProperty]
    ADD CONSTRAINT [dnn_FK_EngageEmployment_UserJobProperty_EngageEmployment_UserJob] FOREIGN KEY ([ApplicationId]) REFERENCES [dotnetnuke_dukes].[dnn_EngageEmployment_UserJob] ([ApplicationId]) ON DELETE CASCADE ON UPDATE NO ACTION;

