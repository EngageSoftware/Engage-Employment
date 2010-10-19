ALTER TABLE [dbo].[engage_Users]
    ADD CONSTRAINT [DF_engage_Users_IsSuperUser] DEFAULT ((0)) FOR [IsSuperUser];

