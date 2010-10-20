ALTER TABLE [dotnetnuke_dukes].[dnn_EngageEmployment_UserJobSearch]
    ADD CONSTRAINT [dnn_FK_EngageEmployment_UserSearch_EngageEmployment_JobPosition] FOREIGN KEY ([JobPositionId]) REFERENCES [dotnetnuke_dukes].[dnn_EngageEmployment_lkpJobPosition] ([JobPositionId]) ON DELETE NO ACTION ON UPDATE NO ACTION;

