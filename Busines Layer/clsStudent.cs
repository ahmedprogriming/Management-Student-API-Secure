using System;
using System.Collections.Generic;
using DataAccess_Layer;

namespace Business_Layer
{
    public class clsStudent
    {
        public enum enMode { AddNew = 0, Update = 1 }
        public enMode Mode = enMode.AddNew;

        public int Id { get; set; }
        public int UserId { get; set; }
        public string StudentNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime CreatedAt { get; set; }

        // لربط بيانات المستخدم الأساسية عند إضافة طالب جديد
        public UsersDTO UserData { get; set; }

        // كائن الـ DTO الخاص بالطالب لتمريره لطبقة البيانات
        public StudentDTO S_DTO
        {
            get => new StudentDTO(this.Id, this.UserId, this.DateOfBirth, this.StudentNumber, this.Phone, this.Address, this.CreatedAt);
        }

        // منشئ الكائن في وضع الإضافة
        public clsStudent()
        {
            this.Id = -1;
            this.UserId = -1;
            this.StudentNumber = string.Empty;
            this.DateOfBirth = DateTime.MinValue;
            this.Phone = string.Empty;
            this.Address = string.Empty;
            this.CreatedAt = DateTime.UtcNow;

            this.UserData = new UsersDTO();
            this.Mode = enMode.AddNew;
        }

        // منشئ الكائن عند القراءة من قاعدة البيانات
        public clsStudent(StudentDTO studentDTO)
        {
            this.Id = studentDTO.Id;
            this.UserId = studentDTO.UserId;
            this.StudentNumber = studentDTO.StudentNumber;
            this.DateOfBirth = studentDTO.DateOfBirth;
            this.Phone = studentDTO.Phone;
            this.Address = studentDTO.Address;
            this.CreatedAt = studentDTO.CreatedAt;

            this.Mode = enMode.Update;
        }

        // 1. البحث عن طالب بالمعرف
        public static clsStudent Find(int studentId)
        {
            StudentDTO dto = clsStudentData.GetByStudetID(studentId);

            if (dto != null)
            {
                return new clsStudent(dto);
            }
            return null;
        }

        public static clsStudent FindByUserId(int userId)
        {
            StudentDTO dto = clsStudentData.GetStudentByUserID(userId);

            if (dto != null)
            {
                return new clsStudent(dto);
            }
            return null;
        }

        // 2. جلب جميع الطلاب
        public static List<StudentDTO> GetAllStudents()
        {
            return clsStudentData.GetAllStudents();
        }

        // إضافة طالب جديد داخلياً
        private bool _AddNewStudent(int createdByUserId, string ipAddress)
        {
            this.Id = clsStudentData.AddNewStudent(this.S_DTO, this.UserData, createdByUserId, ipAddress);
            return (this.Id != -1);
        }

        // تعديل بيانات طالب داخلياً
        private bool _UpdateStudent(int modifiedByUserId, string ipAddress)
        {
            return clsStudentData.UpdateStudent(this.S_DTO, modifiedByUserId, ipAddress);
        }

        // 3. دالة الحفظ الذكية (تقرر تلقائياً بين Add أو Update)
        public bool Save(int performedByUserId, string ipAddress)
        {
            switch (this.Mode)
            {
                case enMode.AddNew:
                    if (_AddNewStudent(performedByUserId, ipAddress))
                    {
                        this.Mode = enMode.Update;
                        return true;
                    }
                    return false;

                case enMode.Update:
                    return _UpdateStudent(performedByUserId, ipAddress);
            }

            return false;
        }

        // 4. حذف الطالب
        public static bool DeleteStudent(int studentId, int deletedByUserId, string ipAddress)
        {
            return clsStudentData.DeleteStudent(studentId, deletedByUserId, ipAddress);
        }
    }
}