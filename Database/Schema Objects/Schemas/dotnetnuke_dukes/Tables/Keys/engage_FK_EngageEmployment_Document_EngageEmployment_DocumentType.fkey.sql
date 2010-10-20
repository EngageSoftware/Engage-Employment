ALTER TABLE [dotnetnuke_dukes].[dnn_EngageEmployment_Document]
    ADD CONSTRAINT [dnn_FK_EngageEmployment_Document_EngageEmployment_DocumentType] FOREIGN KEY ([DocumentTypeId]) REFERENCES [dotnetnuke_dukes].[dnn_EngageEmployment_DocumentType] ([DocumentTypeId]) ON DELETE NO ACTION ON UPDATE NO ACTION;

