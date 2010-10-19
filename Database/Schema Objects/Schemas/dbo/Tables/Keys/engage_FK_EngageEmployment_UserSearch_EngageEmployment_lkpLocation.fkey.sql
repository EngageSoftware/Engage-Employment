ALTER TABLE [dbo].[engage_EngageEmployment_UserJobSearch]
    ADD CONSTRAINT [engage_FK_EngageEmployment_UserSearch_EngageEmployment_lkpLocation] FOREIGN KEY ([LocationId]) REFERENCES [dbo].[engage_EngageEmployment_lkpLocation] ([LocationId]) ON DELETE NO ACTION ON UPDATE NO ACTION;

