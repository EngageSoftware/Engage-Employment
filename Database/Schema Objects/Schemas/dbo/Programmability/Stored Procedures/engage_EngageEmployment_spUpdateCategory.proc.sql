
CREATE PROCEDURE dbo.[engage_EngageEmployment_spUpdateCategory]
@categoryId int,
@categoryName nvarchar(255)
AS
	UPDATE engage_EngageEmployment_lkpJobCategory
	SET [Description] = @categoryName
	WHERE JobCategoryId = @categoryId