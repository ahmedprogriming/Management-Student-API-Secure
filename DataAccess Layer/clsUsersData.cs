using Microsoft.Data.SqlClient;
using System;
using System.Data;

namespace DataAccess_Layer
{

    public class UsersDTO
    {

        public UsersDTO(int id, string email, string firstName, string lastName,
            string role, bool isActive, string passwordHash,string refreshTokenHash,DateTime? refreshTokenExpiresAtatat, DateTime? refreshTokenRevokedAt)
        {
            this.Id = id;
            this.Email = email;
            this.FirstName = firstName;
            this.LastName = lastName;
            this.Role = role;
            this.IsActive = isActive;
         
            this.PasswordHash = passwordHash;
            this.RefreshTokenHash = refreshTokenHash;
            this.RefreshTokenExpiresAt = refreshTokenExpiresAtatat;
            this.RefreshTokenRevokedAt = refreshTokenRevokedAt;
         
        }
        public UsersDTO() 
        {
            this.Id = -1;
            this.Email = "";
            this.FirstName = "";
            this.LastName = "";
            this.Role = "";
            this.IsActive = false;

            this.PasswordHash = "";
        }

        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public string PasswordHash { get; set; }
        public string RefreshTokenHash { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }
        public DateTime? RefreshTokenRevokedAt { get; set; }


    }
    public class clsUsersData
    {
        static string _connectionString = "workstation id=StudentDB.mssql.somee.com;packet size=4096;user id=ahmed123_SQLLogin_1;pwd=7sopal3ghh;data source=StudentDB.mssql.somee.com;persist security info=False;initial catalog=StudentDB;TrustServerCertificate=True";
        public static List<UsersDTO> GetAllUsers()
        {
            List<UsersDTO> Users = new List<UsersDTO>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {


                SqlCommand command = new SqlCommand("SP_GetAllUsers", connection);
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Users.Add(new UsersDTO
                            (
                                reader.GetInt32(reader.GetOrdinal("Id")),
                                reader.GetString(reader.GetOrdinal("Email")),
                                reader.GetString(reader.GetOrdinal("FirstName")),
                                reader.GetString(reader.GetOrdinal("LastName")),
                                reader.GetString(reader.GetOrdinal("Role")),
                                reader.GetBoolean(reader.GetOrdinal("IsActive")),
                                reader.GetString(reader.GetOrdinal("PasswordHash")),
                                reader.IsDBNull(reader.GetOrdinal("RefreshTokenHash")) ? null : reader.GetString(reader.GetOrdinal("RefreshTokenHash")),
                                reader.IsDBNull(reader.GetOrdinal("RefreshTokenExpiresAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("RefreshTokenExpiresAt")),
                                reader.IsDBNull(reader.GetOrdinal("RefreshTokenRevokedAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("RefreshTokenRevokedAt"))
                            ));
                        }

                    }


                }
                return Users;
            }
        }

        public static UsersDTO GetByUserID(int userId)
        {

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {


                SqlCommand command = new SqlCommand("SP_GetUserById", connection);
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@UserId", userId);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UsersDTO
                            (
                                 reader.GetInt32(reader.GetOrdinal("Id")),
                                reader.GetString(reader.GetOrdinal("Email")),
                                reader.GetString(reader.GetOrdinal("FirstName")),
                                reader.GetString(reader.GetOrdinal("LastName")),
                                reader.GetString(reader.GetOrdinal("Role")),
                                reader.GetBoolean(reader.GetOrdinal("IsActive")),
                                reader.GetString(reader.GetOrdinal("PasswordHash")),
                                reader.IsDBNull(reader.GetOrdinal("RefreshTokenHash")) ? null : reader.GetString(reader.GetOrdinal("RefreshTokenHash")),
                                reader.IsDBNull(reader.GetOrdinal("RefreshTokenExpiresAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("RefreshTokenExpiresAt")),
                                reader.IsDBNull(reader.GetOrdinal("RefreshTokenRevokedAt")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("RefreshTokenRevokedAt"))
                            );
                        }
                        else
                        {
                            return null;
                        }

                    }


                }

            }
        }

        public static int AddNewUser( UsersDTO user,
  string ipAddress)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("SP_AddNewUser", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                connection.Open();

                command.Parameters.AddWithValue("@Email", user.Email);
                command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                command.Parameters.AddWithValue("@FirstName", user.FirstName);
                command.Parameters.AddWithValue("@LastName", user.LastName);
                command.Parameters.AddWithValue("@Role", user.Role);
                command.Parameters.AddWithValue("@IpAddress", (object?)ipAddress ?? DBNull.Value);

                var outputParameter = new SqlParameter(
            "@NewUserId",
            SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };

                command.Parameters.Add(outputParameter);

                command.ExecuteNonQuery();

                return (int)outputParameter.Value;

            }

        }

        public static bool InActiveUser(int  userId, bool isAtive, int createdByUserId,
  string ipAddress)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("sp_ToggleUserStatus", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                connection.Open();

                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@IsActive", isAtive);
                command.Parameters.AddWithValue("@ModifiedByUserId", createdByUserId);
                command.Parameters.AddWithValue("@IpAddress", (object?)ipAddress ?? DBNull.Value);

             

                command.ExecuteNonQuery();

                return true;

            }

        }

        public static bool UpdateRefreshToken(int userId, string tokenHash, DateTime expiresAt)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("SP_UpdateUserRefreshToken", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@RefreshTokenHash", tokenHash);
                command.Parameters.AddWithValue("@RefreshTokenExpiresAt", expiresAt);

                connection.Open();
                int rowsAffected = (int)command.ExecuteScalar();

                return rowsAffected > 0;
            }
        }

        public static bool UpdateRevokeUserRefreshToken(int userId)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("SP_RevokeUserRefreshToken", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@UserId", userId);

                connection.Open();
                command.ExecuteNonQuery();

                return true;
            }

           
        }
    }
}
