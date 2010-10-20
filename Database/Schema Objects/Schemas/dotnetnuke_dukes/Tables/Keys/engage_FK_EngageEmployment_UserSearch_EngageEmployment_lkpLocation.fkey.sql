ALTER TABLE [dotnetnuke_dukes].[dnn_EngageEmployment_UserJobSearch]
    ADD CONSTRAINT [dnn_FK_EngageEmployment_UserSearch_EngageEmployment_lkpLocation] FOREIGN KEY ([LocationId]) REFERENCES [dotnetnuke_dukes].[dnn_EngageEmployment_lkpLocation] ([LocationId]) ON DELETE NO ACTION ON UPDATE NO ACTION;

