ALTER TABLE [dotnetnuke_dukes].[dnn_EngageEmployment_lkpLocation]
    ADD CONSTRAINT [dnn_IX_EngageEmployment_lkpLocation_Description_StateId] UNIQUE NONCLUSTERED ([Description] ASC, [StateId] ASC, [PortalId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF) ON [PRIMARY];

