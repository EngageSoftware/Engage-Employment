
CREATE PROCEDURE dotnetnuke_dukes.[dnn_EngageEmployment_spDeletePosition]  
@positionId int  
AS  
 DELETE dnn_EngageEmployment_lkpJobPosition
    WHERE JobPositionId = @positionId