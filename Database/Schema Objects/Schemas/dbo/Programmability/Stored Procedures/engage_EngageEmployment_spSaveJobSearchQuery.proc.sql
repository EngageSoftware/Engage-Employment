
CREATE PROCEDURE dbo.[engage_EngageEmployment_spSaveJobSearchQuery]
@userId int, 
@name nvarchar(255), 
@positionId int, 
@categoryId int, 
@stateId int, 
@locationId int, 
@keywords ntext, 
@jobGroupId int, 
@portalId int
AS
	INSERT engage_EngageEmployment_UserJobSearch
	(UserId, Description, JobPositionId, CategoryId, StateId, LocationId, Keywords, CreationDate, JobGroupId, PortalId)
	VALUES (@userId, @name, @positionId, @categoryId, @stateId, @locationId, @keywords, getdate(), @jobGroupId, @portalId)