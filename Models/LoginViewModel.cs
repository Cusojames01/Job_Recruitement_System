using System.ComponentModel.DataAnnotations;

namespace Job_Recruitment_System.Models
{
    public class LoginViewModel
    {
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string Role { get; set; }
    }
}
