using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess_Layer
{
    public class AuditLogDTO
    {
        public AuditLogDTO(
            int id,
            int userId,
            string action,
            string entityName,
            string entityId,
            string ipAddress,
            string details,
            DateTime createdAt)
        {
            this.Id = id;
            this.UserId = userId;
            this.Action = action;
            this.EntityName = entityName;
            this.EntityId = entityId;
            this.IpAddress = ipAddress;
            this.Details = details;
            this.CreatedAt = createdAt;
        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; }
        public string EntityName { get; set; }
        public string EntityId { get; set; }
        public string IpAddress { get; set; }
        public string Details { get; set; }
        public DateTime CreatedAt { get; set; }
    }
    public class clsAuditLogsData
    {
        static string _connectionString = "workstation id=StudentDB.mssql.somee.com;packet size=4096;user id=ahmed123_SQLLogin_1;pwd=7sopal3ghh;data source=StudentDB.mssql.somee.com;persist security info=False;initial catalog=StudentDB;TrustServerCertificate=True";
        public static List<AuditLogDTO> GetAllAuditLogs()
        {
            List<AuditLogDTO> AuditLog = new List<AuditLogDTO>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {


                SqlCommand command = new SqlCommand("SP_GetRecordsAuditLogs", connection);
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            AuditLog.Add(new AuditLogDTO
                            (
                               reader.GetInt32(reader.GetOrdinal("Id")),
        reader.IsDBNull(reader.GetOrdinal("UserId")) ? 0 : reader.GetInt32(reader.GetOrdinal("UserId")),
        reader.IsDBNull(reader.GetOrdinal("Action")) ? string.Empty : reader.GetString(reader.GetOrdinal("Action")),
        reader.IsDBNull(reader.GetOrdinal("EntityName")) ? string.Empty : reader.GetString(reader.GetOrdinal("EntityName")),
        reader.IsDBNull(reader.GetOrdinal("EntityId")) ? string.Empty : reader.GetString(reader.GetOrdinal("EntityId")),
        reader.IsDBNull(reader.GetOrdinal("IpAddress")) ? string.Empty : reader.GetString(reader.GetOrdinal("IpAddress")),
        reader.IsDBNull(reader.GetOrdinal("Details")) ? string.Empty : reader.GetString(reader.GetOrdinal("Details")),
        reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                            )); 
                        }

                    }


                }
                return AuditLog;
            }


        }

        public static void AddLog(int? userId, string action, string entityName, string entityId, string ipAddress, string details)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("SP_AddAuditLog", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                // استخدمنا DBNull.Value في حال كانت القيم فارغة (مثل حالة تسجيل الدخول الفاشل)
                command.Parameters.AddWithValue("@UserId", (object?)userId ?? DBNull.Value);
                command.Parameters.AddWithValue("@Action", action);
                command.Parameters.AddWithValue("@EntityName", (object?)entityName ?? DBNull.Value);
                command.Parameters.AddWithValue("@EntityId", (object?)entityId ?? DBNull.Value);
                command.Parameters.AddWithValue("@IpAddress", (object?)ipAddress ?? DBNull.Value);
                command.Parameters.AddWithValue("@Details", (object?)details ?? DBNull.Value);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
