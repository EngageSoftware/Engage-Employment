ALTER TABLE [dbo].[engage_EngageEmployment_lkpLocation]
    ADD CONSTRAINT [engage_FK_EngageEmployment_lkpLocation_EngageEmployment_lkpState] FOREIGN KEY ([StateId]) REFERENCES [dbo].[engage_EngageEmployment_lkpState] ([StateId]) ON DELETE NO ACTION ON UPDATE NO ACTION;

