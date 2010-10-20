

CREATE PROCEDURE dotnetnuke_dukes.dnn_EngageEmployment_spInsertJob
@positionId int,   
@locationId int,   
@categoryId int,   
@isHot bit,   
@isFilled bit,   
@requiredQualifications ntext,   
@desiredQualifications ntext,    
@revisingUser int,   
@sortOrder int,   
@portalId int,   
@notificationEmailAddress nvarchar(320),
@startDate datetime,
@expireDate datetime,
@applicationUrl nvarchar(2000)
AS  
 INSERT dnn_EngageEmployment_Job  
 (JobPositionId, LocationId, JobCategoryId, PostedDate, IsHot, IsFilled, RequiredQualifications, 
  DesiredQualifications, RevisingUser, RevisionDate, SortOrder, PortalId, NotificationEmailAddress,
  StartDate, ExpireDate, ApplicationUrl)
 VALUES (@positionId, @locationId, @categoryId, GETDATE(), @isHot, @isFilled, @requiredQualifications,
         @desiredQualifications,  @revisingUser, GETDATE(), @sortOrder, @portalId, @NotificationEmailAddress,
         @startDate, @expireDate, @applicationUrl)  
 SELECT CONVERT(int, SCOPE_IDENTITY())