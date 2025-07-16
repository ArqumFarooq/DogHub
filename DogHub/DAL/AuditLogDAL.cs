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
        public void AddAuditLog(AuditLog log, DogHubEntities de)
        {
            de.AuditLogs.Add(log);
            de.SaveChanges();
        }
    }
}
