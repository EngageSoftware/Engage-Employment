
CREATE PROCEDURE dotnetnuke_dukes.[dnn_EngageEmployment_spDeleteCategory]  
@categoryId int  
AS  
 DELETE dnn_EngageEmployment_lkpJobCategory
    WHERE JobCategoryId = @categoryId