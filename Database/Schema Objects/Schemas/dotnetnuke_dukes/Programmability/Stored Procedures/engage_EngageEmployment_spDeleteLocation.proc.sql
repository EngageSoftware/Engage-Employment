

CREATE PROCEDURE dotnetnuke_dukes.[dnn_EngageEmployment_spDeleteLocation]  
@locationId int  
AS  
 DELETE dnn_EngageEmployment_lkpLocation  
    WHERE LocationId = @locationId