ALTER TABLE [dbo].[engage_Users]
    ADD CONSTRAINT [DF_engage_Users_UpdatePassword] DEFAULT ((0)) FOR [UpdatePassword];

