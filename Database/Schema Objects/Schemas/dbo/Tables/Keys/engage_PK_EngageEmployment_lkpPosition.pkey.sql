ALTER TABLE [dbo].[engage_EngageEmployment_lkpJobPosition]
    ADD CONSTRAINT [engage_PK_EngageEmployment_lkpPosition] PRIMARY KEY CLUSTERED ([JobPositionId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);

