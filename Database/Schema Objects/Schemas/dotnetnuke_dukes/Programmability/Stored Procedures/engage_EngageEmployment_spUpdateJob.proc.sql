

CREATE PROCEDURE dotnetnuke_dukes.dnn_EngageEmployment_spUpdateJob
@jobId int,  
@positionId int,  
@categoryId int,  
@locationId int,  
@isHot bit,  
@isFilled bit,  
@requiredQualifications ntext,  
@desiredQualifications ntext,  
@revisingUser int,  
@sortOrder int,  
@notificationEmailAddress nvarchar(320),
@startDate datetime,
@expireDate datetime,
@applicationUrl nvarchar(2000)
AS  
 UPDATE dnn_EngageEmployment_Job  
 SET JobPositionId = @positionId,   
        LocationId = @locationId,   
        JobCategoryId = @categoryId,   
        IsHot = @isHot,   
        IsFilled = @isFilled,   
        RequiredQualifications = @requiredQualifications,   
        DesiredQualifications = @desiredQualifications,   
        RevisingUser = @revisingUser,   
        RevisionDate = GETDATE(),   
        SortOrder = @sortOrder,   
        NotificationEmailAddress = @notificationEmailAddress,
        StartDate = @startDate,
        ExpireDate = @expireDate,
        ApplicationUrl = @applicationUrl
    WHERE JobId = @jobId