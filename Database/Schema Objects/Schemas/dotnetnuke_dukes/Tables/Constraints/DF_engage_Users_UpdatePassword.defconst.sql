ALTER TABLE [dotnetnuke_dukes].[dnn_Users]
    ADD CONSTRAINT [DF_dnn_Users_UpdatePassword] DEFAULT ((0)) FOR [UpdatePassword];

