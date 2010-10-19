
CREATE PROCEDURE dbo.[engage_EngageEmployment_spInsertCategory]
@categoryName nvarchar(255),
@portalId int
AS
	INSERT engage_EngageEmployment_lkpJobCategory
	([Description], PortalId)
	VALUES (@categoryName, @portalId)