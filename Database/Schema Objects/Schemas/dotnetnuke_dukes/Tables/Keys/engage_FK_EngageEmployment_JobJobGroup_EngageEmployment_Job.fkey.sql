ALTER TABLE [dotnetnuke_dukes].[dnn_EngageEmployment_JobJobGroup]
    ADD CONSTRAINT [dnn_FK_EngageEmployment_JobJobGroup_EngageEmployment_Job] FOREIGN KEY ([JobId]) REFERENCES [dotnetnuke_dukes].[dnn_EngageEmployment_Job] ([JobId]) ON DELETE NO ACTION ON UPDATE NO ACTION;

