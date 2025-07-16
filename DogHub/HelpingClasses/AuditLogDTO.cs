using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DogHub.HelpingClasses
{
    public class AuditLogDTO
    {
        public int PK_AuditLogId { get; set; }
        public string ActionType { get; set; }
        public string AffectedTable { get; set; }
        public string AuditDetails { get; set; }
        public int? SysCreatedID { get; set; }
        public DateTime? SysCreatedDate { get; set; }
    }
}