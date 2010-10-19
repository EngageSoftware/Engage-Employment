ALTER TABLE [dbo].[engage_EngageEmployment_UserJobProperty]
    ADD CONSTRAINT [engage_FK_EngageEmployment_UserJobProperty_EngageEmployment_ApplicationProperty] FOREIGN KEY ([ApplicationPropertyId]) REFERENCES [dbo].[engage_EngageEmployment_ApplicationProperty] ([ApplicationPropertyId]) ON DELETE CASCADE ON UPDATE NO ACTION;

