using System.ComponentModel.DataAnnotations;

namespace Job_Recruitment_System.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string FullName { get; set; }
     
        public string Sex { get; set; }
        public string Email { get; set; }
        public string Password{ get; set; }
        [Phone]
        public string Contact { get; set; }
        public string Role { get; set; } = "Applicant"; // for User

        public string? ProfileImage { get; set; }
    }
}
