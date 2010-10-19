ALTER TABLE [dbo].[engage_Users]
    ADD CONSTRAINT [DF_engage_Users_IsDeleted] DEFAULT ((0)) FOR [IsDeleted];

