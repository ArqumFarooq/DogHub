using DogHub.Models;
using DogHub.DAL;
using System;

namespace DogHub.BL
{
    public class AuditLogBL
    {
        public void AddAuditLog(string actionType, string affectedTable, string auditDetails, int? createdById, DogHubEntities de)
        {
            AuditLog log = new AuditLog
            {
                ActionType = actionType,
                AffectedTable = affectedTable,
                AuditDetails = auditDetails,
                SysCreatedID = createdById,
                SysCreatedDate = DateTime.Now
            };

            new AuditLogDAL().AddAuditLog(log, de);
        }
    }
}
