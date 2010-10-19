ALTER TABLE [dbo].[engage_EngageEmployment_lkpJobCategory]
    ADD CONSTRAINT [engage_PK_EngageEmployment_lkpPositionType] PRIMARY KEY CLUSTERED ([JobCategoryId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);

