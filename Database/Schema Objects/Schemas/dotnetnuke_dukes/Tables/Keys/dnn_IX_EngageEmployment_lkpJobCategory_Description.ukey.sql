ALTER TABLE [dotnetnuke_dukes].[dnn_EngageEmployment_lkpJobCategory]
    ADD CONSTRAINT [dnn_IX_EngageEmployment_lkpJobCategory_Description] UNIQUE NONCLUSTERED ([Description] ASC, [PortalId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF) ON [PRIMARY];

