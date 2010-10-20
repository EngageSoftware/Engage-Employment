
CREATE PROCEDURE dotnetnuke_dukes.[dnn_EngageEmployment_spUpdateCategory]
@categoryId int,
@categoryName nvarchar(255)
AS
	UPDATE dnn_EngageEmployment_lkpJobCategory
	SET [Description] = @categoryName
	WHERE JobCategoryId = @categoryId