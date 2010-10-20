
CREATE PROCEDURE dotnetnuke_dukes.[dnn_EngageEmployment_spInsertCategory]
@categoryName nvarchar(255),
@portalId int
AS
	INSERT dnn_EngageEmployment_lkpJobCategory
	([Description], PortalId)
	VALUES (@categoryName, @portalId)