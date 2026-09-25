using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.Net;

namespace DataAccess_Layer
{
    public class StudentDTO
    {
        // 1. أضف هذا المُنشئ الفارغ (هذا هو الحل الجذري للمشكلة)
        public StudentDTO()
        {
        }
        public StudentDTO(int id, int userid, DateTime birthdate, string stunumber, string phone, string address, DateTime createdAt)
        {
            this.Id = id;
            this.UserId = userid;
            this.DateOfBirth = birthdate;
            this.StudentNumber = stunumber;
            this.Phone = phone;
            this.Address = address;
            this.CreatedAt = createdAt;
        }


        public int Id { get; set; }
        public int UserId { get; set; }
        public string StudentNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime CreatedAt { get; set; }


    }

    public class clsStudentData

    {

        static string _connectionString = "workstation id=StudentDB.mssql.somee.com;packet size=4096;user id=ahmed123_SQLLogin_1;pwd=7sopal3ghh;data source=StudentDB.mssql.somee.com;persist security info=False;initial catalog=StudentDB;TrustServerCertificate=True";

        public static List<StudentDTO> GetAllStudents()
        {
            List<StudentDTO> students = new List<StudentDTO>();
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {


                SqlCommand command = new SqlCommand("SP_GetAllStudents", connection);
                {
                    command.CommandType = CommandType.StoredProcedure;
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            students.Add(new StudentDTO
                            (
                                reader.GetInt32(reader.GetOrdinal("Id")),
                                reader.GetInt32(reader.GetOrdinal("UserId")),
                                reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
                                reader.GetString(reader.GetOrdinal("StudentNumber")),
                                reader.GetString(reader.GetOrdinal("Phone")),
                                reader.GetString(reader.GetOrdinal("Address")),
                                reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                            ));
                        }

                    }


                }
                return students;
            }
        }

        public static StudentDTO GetByStudetID(int studentId)
        {
           
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {


                SqlCommand command = new SqlCommand("SP_GetStudentById", connection);
                {
                    command.CommandType = CommandType.StoredProcedure;
                  
                    command.Parameters.AddWithValue("@StudentId", studentId);
                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new StudentDTO
                            (
                                reader.GetInt32(reader.GetOrdinal("Id")),
                                reader.GetInt32(reader.GetOrdinal("UserId")),
                                reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
                                reader.GetString(reader.GetOrdinal("StudentNumber")),
                                reader.GetString(reader.GetOrdinal("Phone")),
                                reader.GetString(reader.GetOrdinal("Address")),
                                reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
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

        public static StudentDTO GetStudentByUserID(int userId)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM Students WHERE UserId = @UserId";

                // إضافة using لتنظيف الذاكرة بشكل صحيح
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@UserId", userId);

                    connection.Open();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new StudentDTO
                            (
                                reader.GetInt32(reader.GetOrdinal("Id")),
                                reader.GetInt32(reader.GetOrdinal("UserId")),
                                reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
                                reader.GetString(reader.GetOrdinal("StudentNumber")),

                                // التحقق من أن القيمة ليست NULL قبل قراءتها كنص
                                reader.IsDBNull(reader.GetOrdinal("Phone")) ? string.Empty : reader.GetString(reader.GetOrdinal("Phone")),
                                reader.IsDBNull(reader.GetOrdinal("Address")) ? string.Empty : reader.GetString(reader.GetOrdinal("Address")),

                                reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
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
        public static int AddNewStudent(StudentDTO student,UsersDTO user,int createdByUserId,
   string ipAddress)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("sp_CreateStudent", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                connection.Open();

                command.Parameters.AddWithValue("@Email", user.Email);
                command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
                command.Parameters.AddWithValue("@FirstName", user.FirstName);
                command.Parameters.AddWithValue("@LastName", user.LastName);
                command.Parameters.AddWithValue("@StudentNumber", student.StudentNumber);
                command.Parameters.AddWithValue("@DateOfBirth", student.DateOfBirth);
                command.Parameters.AddWithValue("@Phone", student.Phone);
                command.Parameters.AddWithValue("@Address", student.Address);
                command.Parameters.AddWithValue("@CreatedByUserId", createdByUserId);
                command.Parameters.AddWithValue("@IpAddress", (object?)ipAddress ?? DBNull.Value);

                var outputParameter = new SqlParameter(
            "@NewStudentId",
            SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };

                command.Parameters.Add(outputParameter);

                command.ExecuteNonQuery();

                return (int)outputParameter.Value;

            }

        }

        public static bool UpdateStudent(StudentDTO student,int modifiedByUserId,string ipAddress)
        {
            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("sp_UpdateStudentContact", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                connection.Open();

                command.Parameters.AddWithValue("@StudentId", student.Id);
            
           
                command.Parameters.AddWithValue("@Phone", student.Phone);
                command.Parameters.AddWithValue("@Address", student.Address);
                command.Parameters.AddWithValue("@ModifiedByUserId", modifiedByUserId);
                command.Parameters.AddWithValue("@IpAddress", (object?)ipAddress ?? DBNull.Value);

              

                command.ExecuteNonQuery();

                return true;

            }

        }

        public static bool DeleteStudent(int studentId, int deletedByUserId, string ipAddress)
        {

            using (var connection = new SqlConnection(_connectionString))
            using (var command = new SqlCommand("SP_DeleteStudent", connection))
            {
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.AddWithValue("@StudentId", studentId);
                command.Parameters.AddWithValue("@DeletedByUserId", deletedByUserId);
                command.Parameters.AddWithValue("@IpAddress", ipAddress);

                connection.Open();
                 command.ExecuteNonQuery();
                return true;


            }
        }

    }

    }

