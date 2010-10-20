ALTER TABLE [dotnetnuke_dukes].[dnn_EngageEmployment_UserJobSearch]
    ADD CONSTRAINT [dnn_FK_EngageEmployment_UserJobSearch_EngageEmployment_JobGroup] FOREIGN KEY ([JobGroupId]) REFERENCES [dotnetnuke_dukes].[dnn_EngageEmployment_JobGroup] ([JobGroupId]) ON DELETE NO ACTION ON UPDATE NO ACTION;

