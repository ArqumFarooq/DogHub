using DogHub.Models;
using DogHub.DAL;
using System;
using System.Collections.Generic;

namespace DogHub.BL
{
    public class AuditLogBL
    {
        public List<AuditLog> GetAllAuditLogs(DogHubEntities db)
        {
            return new AuditLogDAL().GetAllAuditLogs(db);
        }

        public AuditLog GetAuditLogById(int id, DogHubEntities db)
        {
            return new AuditLogDAL().GetAuditLogById(id, db);
        }

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
