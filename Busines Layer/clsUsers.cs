using System;
using System.Collections.Generic;
using DataAccess_Layer;

namespace Business_Layer
{
    public class clsUser
    {
        public enum enMode { AddNew = 0, Update = 1 }
        public enMode Mode = enMode.AddNew;

        public int Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
        public string PasswordHash { get; set; }

        // خاصية تجميع الاسم الكامل لسهولة العرض في الواجهات
        public string FullName => $"{FirstName} {LastName}".Trim();

        public string RefreshTokenHash { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }
        public DateTime? RefreshTokenRevokedAt { get; set; }

        // كائن DTO الخاص بالمستخدم لتمريره لطبقة البيانات
        public UsersDTO U_DTO
        {
            get => new UsersDTO(this.Id, this.Email, this.FirstName, 
                this.LastName, this.Role, this.IsActive, this.PasswordHash,this.RefreshTokenHash,this.RefreshTokenExpiresAt,this.RefreshTokenRevokedAt);
        }

        // منشئ الكائن في وضع الإضافة الافتراضي
        public clsUser()
        {
            this.Id = -1;
            this.Email = string.Empty;
            this.FirstName = string.Empty;
            this.LastName = string.Empty;
            this.Role = "User";
            this.IsActive = true;
            this.PasswordHash = string.Empty;

            this.Mode = enMode.AddNew;
        }

        // منشئ الكائن الداخلي عند القراءة من طبقة البيانات
        public clsUser(UsersDTO userDTO)
        {
            this.Id = userDTO.Id;
            this.Email = userDTO.Email;
            this.FirstName = userDTO.FirstName;
            this.LastName = userDTO.LastName;
            this.Role = userDTO.Role;
            this.IsActive = userDTO.IsActive;
            this.PasswordHash = userDTO.PasswordHash;

            this.RefreshTokenHash = userDTO.RefreshTokenHash;
            this.RefreshTokenExpiresAt = userDTO.RefreshTokenExpiresAt;
            this.RefreshTokenRevokedAt = userDTO.RefreshTokenRevokedAt;

            this.Mode = enMode.Update;
        }

        // 1. البحث عن مستخدم بالمعرّف (ID)
        public static clsUser Find(int userId)
        {
            UsersDTO dto = clsUsersData.GetByUserID(userId);

            if (dto != null)
            {
                return new clsUser(dto);
            }
            return null;
        }

        // 2. جلب قائمة بجميع المستخدمين
        public static List<UsersDTO> GetAllUsers()
        {
            return clsUsersData.GetAllUsers();
        }

        // إضافة مستخدم جديد عبر استدعاء طبقة البيانات
        private bool _AddNewUser(string ipAddress)
        {
            this.Id = clsUsersData.AddNewUser(this.U_DTO, ipAddress);
            return (this.Id != -1);
        }

        // 3. دالة الحفظ
        public bool Save(string ipAddress)
        {
            switch (this.Mode)
            {
                case enMode.AddNew:
                    if (_AddNewUser(ipAddress))
                    {
                        this.Mode = enMode.Update;
                        return true;
                    }
                    return false;

                case enMode.Update:
                    // ملاحظة: يتم استدعاء دالة التعديل هنا عند توفرها في طبقة البيانات
                    return false;
            }

            return false;
        }

        // 4. تفعيل أو تعطيل حساب المستخدم
        public static bool ToggleStatus(int userId, bool isActive, int modifiedByUserId, string ipAddress)
        {
            return clsUsersData.InActiveUser(userId, isActive, modifiedByUserId, ipAddress);
        }

        // دالة مساعدة لتغيير حالة الكائن الحالي مباشرة
        public bool ChangeStatus(bool newStatus, int modifiedByUserId, string ipAddress)
        {
            if (this.Id == -1) return false;

            if (clsUsersData.InActiveUser(this.Id, newStatus, modifiedByUserId, ipAddress))
            {
                this.IsActive = newStatus;
                return true;
            }
            return false;
        }

        public static bool UpdateRefreshToken(int userId, string tokenHash, DateTime expiresAt)
        {
            return clsUsersData.UpdateRefreshToken(userId, tokenHash, expiresAt);
        }

        public static bool UpdateRefreshTokenRevoked(int userId)
        {
            return clsUsersData.UpdateRevokeUserRefreshToken(userId);
        }
    }
}