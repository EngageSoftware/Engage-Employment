ALTER TABLE [dbo].[engage_EngageEmployment_UserJobSearch]
    ADD CONSTRAINT [engage_FK_EngageEmployment_UserSearch_EngageEmployment_JobPosition] FOREIGN KEY ([JobPositionId]) REFERENCES [dbo].[engage_EngageEmployment_lkpJobPosition] ([JobPositionId]) ON DELETE NO ACTION ON UPDATE NO ACTION;

