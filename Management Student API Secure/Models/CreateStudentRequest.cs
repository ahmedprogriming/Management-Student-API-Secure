namespace Management_Student_API_Secure.Models
{
    public class CreateStudentRequest
    {// 1. بيانات المستخدم الأساسية (User Data)
        public string Email { get; set; }
        public string Password { get; set; } // كلمة المرور غير المشفرة القادمة من المستخدم
        public string FirstName { get; set; }
        public string LastName { get; set; }

        // 2. بيانات الطالب الإضافية (Student Data)
        public string StudentNumber { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }
}
