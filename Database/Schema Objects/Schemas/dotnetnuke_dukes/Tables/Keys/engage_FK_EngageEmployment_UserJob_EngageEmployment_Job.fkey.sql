ALTER TABLE [dotnetnuke_dukes].[dnn_EngageEmployment_UserJob]
    ADD CONSTRAINT [dnn_FK_EngageEmployment_UserJob_EngageEmployment_Job] FOREIGN KEY ([JobId]) REFERENCES [dotnetnuke_dukes].[dnn_EngageEmployment_Job] ([JobId]) ON DELETE NO ACTION ON UPDATE NO ACTION;

