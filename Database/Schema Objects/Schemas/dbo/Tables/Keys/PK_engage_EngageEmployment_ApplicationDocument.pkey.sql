ALTER TABLE [dbo].[engage_EngageEmployment_ApplicationDocument]
    ADD CONSTRAINT [PK_engage_EngageEmployment_ApplicationDocument] PRIMARY KEY CLUSTERED ([ApplicationId] ASC, [ResumeId] ASC) WITH (ALLOW_PAGE_LOCKS = ON, ALLOW_ROW_LOCKS = ON, PAD_INDEX = OFF, IGNORE_DUP_KEY = OFF, STATISTICS_NORECOMPUTE = OFF);

