
CREATE VIEW dotnetnuke_dukes.[dnn_EngageEmployment_vwUserStatuses]
AS
select 
 UserStatusId, Status AS StatusName, PortalId
 from 
 dotnetnuke_dukes.dnn_EngageEmployment_lkpUserStatus
