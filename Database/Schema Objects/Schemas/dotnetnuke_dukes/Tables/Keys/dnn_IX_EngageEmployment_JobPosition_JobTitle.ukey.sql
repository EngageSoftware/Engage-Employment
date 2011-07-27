ALTER TABLE [dotnetnuke_dukes].[dnn_EngageEmployment_lkpJobPosition]
    ADD CONSTRAINT [dnn_IX_EngageEmployment_JobPosition_JobTitle] UNIQUE NONCLUSTERED ([JobTitle] ASC, [PortalId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF) ON [PRIMARY];

