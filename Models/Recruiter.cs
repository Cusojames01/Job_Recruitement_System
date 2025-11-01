using System.ComponentModel.DataAnnotations;

namespace Job_Recruitment_System.Models
{
    public class Recruiter
    {
        [Key]
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public string RecruiterName { get; set; }
        public string Sex { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }

        [Phone]
        public string Contact { get; set; }

        public string Role { get; set; } = "Recruiter"; // for User

        public string? ProfileImage { get; set; }

    }
}
