
CREATE PROCEDURE dbo.[engage_EngageEmployment_spDeletePosition]  
@positionId int  
AS  
 DELETE engage_EngageEmployment_lkpJobPosition
    WHERE JobPositionId = @positionId