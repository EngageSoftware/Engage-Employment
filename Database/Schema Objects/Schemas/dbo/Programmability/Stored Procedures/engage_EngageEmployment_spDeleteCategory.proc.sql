
CREATE PROCEDURE dbo.[engage_EngageEmployment_spDeleteCategory]  
@categoryId int  
AS  
 DELETE engage_EngageEmployment_lkpJobCategory
    WHERE JobCategoryId = @categoryId