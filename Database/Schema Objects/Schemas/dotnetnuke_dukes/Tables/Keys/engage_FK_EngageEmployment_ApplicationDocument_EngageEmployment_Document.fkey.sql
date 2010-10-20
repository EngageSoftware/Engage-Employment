ALTER TABLE [dotnetnuke_dukes].[dnn_EngageEmployment_ApplicationDocument]
    ADD CONSTRAINT [dnn_FK_EngageEmployment_ApplicationDocument_EngageEmployment_Document] FOREIGN KEY ([ResumeId]) REFERENCES [dotnetnuke_dukes].[dnn_EngageEmployment_Document] ([ResumeId]) ON DELETE NO ACTION ON UPDATE NO ACTION;

