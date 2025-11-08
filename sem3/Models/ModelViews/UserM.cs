using System;

namespace sem3.Models.ModelViews
{
    public class UserM
    {
        public int UserID { get; set; }
        public string FullName { get; set; } = "";
        public string MobileNumber { get; set; } = "";
        public string Email { get; set; } = "";
        public string PasswordHash { get; set; } = "";
        public int? RoleID { get; set; } = 2;
        public string Address { get; set; } = "";
        public DateTime? RegistrationDate { get; set; } = DateTime.Now;
    }
}