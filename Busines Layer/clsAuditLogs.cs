using System;
using System.Collections.Generic;
using DataAccess_Layer;

namespace Business_Layer
{
    public class clsAuditLog
    {
        public int Id { get; }
        public int UserId { get; }
        public string Action { get; }
        public string EntityName { get; }
        public string EntityId { get; }
        public string IpAddress { get; }
        public string Details { get; }
        public DateTime CreatedAt { get; }

        // كائن DTO المقابل
        public AuditLogDTO LogDTO
        {
            get => new AuditLogDTO(this.Id, this.UserId, this.Action, this.EntityName, this.EntityId, this.IpAddress, this.Details, this.CreatedAt);
        }

        // منشئ الكائن الداخلي للقراءة فقط (لأن سجلات الـ Audit لا تُعدّل برمجياً)
        public clsAuditLog(AuditLogDTO logDTO)
        {
            this.Id = logDTO.Id;
            this.UserId = logDTO.UserId;
            this.Action = logDTO.Action;
            this.EntityName = logDTO.EntityName;
            this.EntityId = logDTO.EntityId;
            this.IpAddress = logDTO.IpAddress;
            this.Details = logDTO.Details;
            this.CreatedAt = logDTO.CreatedAt;
        }

        // 1. جلب كافة سجلات التدقيق كقائمة DTO
        public static List<AuditLogDTO> GetAllAuditLogs()
        {
            return clsAuditLogsData.GetAllAuditLogs();
        }

        // 2. جلب كافة السجلات ككائنات Business Objects
        public static List<clsAuditLog> GetAllLogsList()
        {
            List<AuditLogDTO> listDTO = clsAuditLogsData.GetAllAuditLogs();
            List<clsAuditLog> logsList = new List<clsAuditLog>();

            foreach (var dto in listDTO)
            {
                logsList.Add(new clsAuditLog(dto));
            }

            return logsList;
        }

        public static void LogEvent(int? userId, string action, string entityName, string entityId, string ipAddress, string details)
        {
            clsAuditLogsData.AddLog(userId, action, entityName, entityId, ipAddress, details);
        }
    }
}