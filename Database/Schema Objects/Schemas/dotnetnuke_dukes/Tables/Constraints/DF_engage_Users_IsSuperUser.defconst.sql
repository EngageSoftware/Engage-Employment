ALTER TABLE [dotnetnuke_dukes].[dnn_Users]
    ADD CONSTRAINT [DF_dnn_Users_IsSuperUser] DEFAULT ((0)) FOR [IsSuperUser];

