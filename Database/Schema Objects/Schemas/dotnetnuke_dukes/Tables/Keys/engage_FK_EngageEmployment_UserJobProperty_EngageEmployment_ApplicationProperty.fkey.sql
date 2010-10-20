ALTER TABLE [dotnetnuke_dukes].[dnn_EngageEmployment_UserJobProperty]
    ADD CONSTRAINT [dnn_FK_EngageEmployment_UserJobProperty_EngageEmployment_ApplicationProperty] FOREIGN KEY ([ApplicationPropertyId]) REFERENCES [dotnetnuke_dukes].[dnn_EngageEmployment_ApplicationProperty] ([ApplicationPropertyId]) ON DELETE CASCADE ON UPDATE NO ACTION;

