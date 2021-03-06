﻿
CREATE PROCEDURE dotnetnuke_dukes.[dnn_EngageEmployment_spGetUnusedAdminData]
@JobGroupId int,
@PortalId int
AS
	SELECT StateId, Name AS StateName, Abbreviation AS StateAbbreviation
	 FROM dnn_EngageEmployment_lkpState s
	 WHERE PortalId = @PortalId
	  AND NOT EXISTS (SELECT NULL FROM dnn_EngageEmployment_lkpLocation l WHERE l.StateId = s.StateId)
	 ORDER BY Name

	SELECT LocationId, LocationName, StateId, StateName, StateAbbreviation
	 FROM dnn_EngageEmployment_vwLocations l
	 WHERE PortalId = @PortalId
	  AND (NOT EXISTS (SELECT NULL FROM dnn_EngageEmployment_vwJobs j WHERE j.LocationId = l.LocationId)
	   OR NOT EXISTS (SELECT NULL FROM dnn_EngageEmployment_vwLocations WHERE LocationId = l.LocationId AND (@JobGroupId IS NULL OR JobGroupId = @JobGroupId)))
	 ORDER BY LocationName

	SELECT CategoryId, CategoryName
	 FROM dnn_EngageEmployment_vwCategories c
	 WHERE PortalId = @PortalId
	  AND (NOT EXISTS (SELECT NULL FROM dnn_EngageEmployment_vwJobs j WHERE j.CategoryId = c.CategoryId)
	   OR NOT EXISTS (SELECT NULL FROM dnn_EngageEmployment_vwCategories WHERE CategoryId = c.CategoryId AND (@JobGroupId IS NULL OR JobGroupId = @JobGroupId)))
	 ORDER BY CategoryName

	SELECT PositionId, JobTitle
	 FROM dnn_EngageEmployment_vwPositions p
	 WHERE PortalId = @PortalId
	  AND (NOT EXISTS (SELECT NULL FROM dnn_EngageEmployment_vwJobs j WHERE j.PositionId = p.PositionId)
	   OR NOT EXISTS (SELECT NULL FROM dnn_EngageEmployment_vwPositions WHERE PositionId = p.PositionId AND (@JobGroupId IS NULL OR JobGroupId = @JobGroupId)))
	 ORDER BY JobTitle