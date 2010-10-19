ALTER TABLE [dbo].[engage_EngageEmployment_Document]
    ADD CONSTRAINT [engage_FK_EngageEmployment_Document_EngageEmployment_DocumentType] FOREIGN KEY ([DocumentTypeId]) REFERENCES [dbo].[engage_EngageEmployment_DocumentType] ([DocumentTypeId]) ON DELETE NO ACTION ON UPDATE NO ACTION;

