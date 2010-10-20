
CREATE PROCEDURE dotnetnuke_dukes.[dnn_EngageEmployment_spInsertPosition]
@JobTitle nvarchar(255),
@JobDescription ntext,
@portalId int
AS
	INSERT dnn_EngageEmployment_lkpJobPosition
	(JobTitle, [Description], PortalId)
	VALUES (@JobTitle, @JobDescription, @portalId)