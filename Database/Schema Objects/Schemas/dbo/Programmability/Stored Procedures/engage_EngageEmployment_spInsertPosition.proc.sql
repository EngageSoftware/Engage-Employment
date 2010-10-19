
CREATE PROCEDURE dbo.[engage_EngageEmployment_spInsertPosition]
@JobTitle nvarchar(255),
@JobDescription ntext,
@portalId int
AS
	INSERT engage_EngageEmployment_lkpJobPosition
	(JobTitle, [Description], PortalId)
	VALUES (@JobTitle, @JobDescription, @portalId)