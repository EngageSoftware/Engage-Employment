ALTER TABLE [dbo].[engage_EngageEmployment_Job]
    ADD CONSTRAINT [engage_FK_EngageEmployment_Job_EngageEmployment_lkpJobCategory] FOREIGN KEY ([JobCategoryId]) REFERENCES [dbo].[engage_EngageEmployment_lkpJobCategory] ([JobCategoryId]) ON DELETE NO ACTION ON UPDATE NO ACTION;

