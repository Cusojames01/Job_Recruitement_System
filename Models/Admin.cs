using System.ComponentModel.DataAnnotations;

namespace Job_Recruitment_System.Models
{
    public class Admin
    {
        [Key]
        public int Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } = "Admin";
    }
}
