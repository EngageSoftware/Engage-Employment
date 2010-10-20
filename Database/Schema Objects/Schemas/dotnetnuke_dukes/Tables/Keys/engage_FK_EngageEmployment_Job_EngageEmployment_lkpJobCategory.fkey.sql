ALTER TABLE [dotnetnuke_dukes].[dnn_EngageEmployment_Job]
    ADD CONSTRAINT [dnn_FK_EngageEmployment_Job_EngageEmployment_lkpJobCategory] FOREIGN KEY ([JobCategoryId]) REFERENCES [dotnetnuke_dukes].[dnn_EngageEmployment_lkpJobCategory] ([JobCategoryId]) ON DELETE NO ACTION ON UPDATE NO ACTION;

