using System;

namespace sem3.Models.ModelViews
{
    public class UserM
    {
        public int Id { get; set; }
        public string FullName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Role { get; set; } = "User";
        public string Address { get; set; } = "";
        public DateTime? CreatedAt { get; set; } = DateTime.Now;
        public bool? IsActive { get; set; } = true;
    }
}
