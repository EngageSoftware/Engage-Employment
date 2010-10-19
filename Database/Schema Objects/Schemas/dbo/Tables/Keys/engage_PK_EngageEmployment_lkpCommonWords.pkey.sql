ALTER TABLE [dbo].[engage_EngageEmployment_lkpCommonWords]
    ADD CONSTRAINT [engage_PK_EngageEmployment_lkpCommonWords] PRIMARY KEY CLUSTERED ([CommonWordID] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);

