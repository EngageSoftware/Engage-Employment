
CREATE VIEW dbo.[engage_EngageEmployment_vwUserStatuses]
AS
select 
 UserStatusId, Status AS StatusName, PortalId
 from 
 dbo.engage_EngageEmployment_lkpUserStatus