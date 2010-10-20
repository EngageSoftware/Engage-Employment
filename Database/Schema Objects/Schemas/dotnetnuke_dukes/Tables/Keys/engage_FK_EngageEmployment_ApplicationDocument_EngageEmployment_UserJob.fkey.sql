ALTER TABLE [dotnetnuke_dukes].[dnn_EngageEmployment_ApplicationDocument]
    ADD CONSTRAINT [dnn_FK_EngageEmployment_ApplicationDocument_EngageEmployment_UserJob] FOREIGN KEY ([ApplicationId]) REFERENCES [dotnetnuke_dukes].[dnn_EngageEmployment_UserJob] ([ApplicationId]) ON DELETE NO ACTION ON UPDATE NO ACTION;

