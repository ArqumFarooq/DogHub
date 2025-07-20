using DogHub.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace DogHub.DAL
{
    public class AuditLogDAL
    {
        public List<AuditLog> GetAllAuditLogs(DogHubEntities db)
        {
            return db.AuditLogs.ToList();
        }
        public AuditLog GetAuditLogById(int id, DogHubEntities db)
        {
            return db.AuditLogs.FirstOrDefault(x => x.PK_AuditLogId == id);
        }

        public void AddAuditLog(AuditLog log, DogHubEntities de)
        {
            de.AuditLogs.Add(log);
            de.SaveChanges();
        }
    }
}
