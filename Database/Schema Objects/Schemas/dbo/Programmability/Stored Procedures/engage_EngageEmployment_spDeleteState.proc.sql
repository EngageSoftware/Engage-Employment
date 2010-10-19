
CREATE PROCEDURE dbo.[engage_EngageEmployment_spDeleteState]  
@stateId int  
AS  
 DELETE engage_EngageEmployment_lkpState
    WHERE StateId = @stateId