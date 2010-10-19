

CREATE PROCEDURE dbo.[engage_EngageEmployment_spDeleteLocation]  
@locationId int  
AS  
 DELETE engage_EngageEmployment_lkpLocation  
    WHERE LocationId = @locationId