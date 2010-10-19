ALTER TABLE [dbo].[engage_EngageEmployment_JobJobGroup]
    ADD CONSTRAINT [engage_FK_EngageEmployment_JobJobGroup_EngageEmployment_JobGroup] FOREIGN KEY ([JobGroupId]) REFERENCES [dbo].[engage_EngageEmployment_JobGroup] ([JobGroupId]) ON DELETE NO ACTION ON UPDATE NO ACTION;

