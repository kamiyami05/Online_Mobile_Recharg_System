using System.ComponentModel.DataAnnotations;

namespace sem3.Models.ModelViews
{
    public class Login
    {
        [Required(ErrorMessage = "Please enter your phone number.")]
        [Display(Name = "Phone Number")]
        [RegularExpression(@"^(03|05|07|08|09)\d{8}$",
            ErrorMessage = "Invalid phone number format. (10 digits, starting with 03, 05, 07, 08, or 09)")]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "Please enter your password.")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}