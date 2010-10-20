
CREATE PROCEDURE dotnetnuke_dukes.[dnn_EngageEmployment_spDeleteState]  
@stateId int  
AS  
 DELETE dnn_EngageEmployment_lkpState
    WHERE StateId = @stateId