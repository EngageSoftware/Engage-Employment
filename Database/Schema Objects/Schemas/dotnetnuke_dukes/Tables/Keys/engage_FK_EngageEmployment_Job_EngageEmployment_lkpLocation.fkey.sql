ALTER TABLE [dotnetnuke_dukes].[dnn_EngageEmployment_Job]
    ADD CONSTRAINT [dnn_FK_EngageEmployment_Job_EngageEmployment_lkpLocation] FOREIGN KEY ([LocationId]) REFERENCES [dotnetnuke_dukes].[dnn_EngageEmployment_lkpLocation] ([LocationId]) ON DELETE NO ACTION ON UPDATE NO ACTION;

