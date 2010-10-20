ALTER TABLE [dotnetnuke_dukes].[dnn_EngageEmployment_lkpLocation]
    ADD CONSTRAINT [dnn_FK_EngageEmployment_lkpLocation_EngageEmployment_lkpState] FOREIGN KEY ([StateId]) REFERENCES [dotnetnuke_dukes].[dnn_EngageEmployment_lkpState] ([StateId]) ON DELETE NO ACTION ON UPDATE NO ACTION;

