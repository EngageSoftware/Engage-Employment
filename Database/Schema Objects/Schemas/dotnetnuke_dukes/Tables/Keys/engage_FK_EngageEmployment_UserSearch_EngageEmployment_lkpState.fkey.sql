ALTER TABLE [dotnetnuke_dukes].[dnn_EngageEmployment_UserJobSearch]
    ADD CONSTRAINT [dnn_FK_EngageEmployment_UserSearch_EngageEmployment_lkpState] FOREIGN KEY ([StateId]) REFERENCES [dotnetnuke_dukes].[dnn_EngageEmployment_lkpState] ([StateId]) ON DELETE NO ACTION ON UPDATE NO ACTION;

