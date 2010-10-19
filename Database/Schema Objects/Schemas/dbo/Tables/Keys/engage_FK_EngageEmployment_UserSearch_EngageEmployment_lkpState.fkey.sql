ALTER TABLE [dbo].[engage_EngageEmployment_UserJobSearch]
    ADD CONSTRAINT [engage_FK_EngageEmployment_UserSearch_EngageEmployment_lkpState] FOREIGN KEY ([StateId]) REFERENCES [dbo].[engage_EngageEmployment_lkpState] ([StateId]) ON DELETE NO ACTION ON UPDATE NO ACTION;

