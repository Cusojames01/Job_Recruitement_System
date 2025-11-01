using System;
using System.ComponentModel.DataAnnotations;

namespace Job_Recruitment_System.Models
{
    public class PostJob
    {
        [Key]
        public int Id { get; set; }

        // Recruiter info (nullable para puwede walang login)
        public int? RecruiterId { get; set; }

        [StringLength(100)]
        public string PostedBy { get; set; } = "Anonymous";

        [StringLength(150)]
        public string CompanyName { get; set; } = "Anonymous Company";

        // Job details
        [Required]
        [StringLength(100)]
        public string JobTitle { get; set; }

        [Required]
        public string JobDescription { get; set; }

        [Required]
        [StringLength(50)]
        public string JobType { get; set; }

        [Required]
        [StringLength(150)]
        public string Location { get; set; }

        public string? SalaryRange { get; set; }
        public string? ExperienceLevel { get; set; }
        public string? Qualifications { get; set; }
        public DateTime? Deadline { get; set; }

        public DateTime DatePosted { get; set; } = DateTime.Now;

        [StringLength(20)]
        public string Status { get; set; } = "Open";

        public Recruiter? Recruiter { get; set; }

        // Optional: One job can have many applications
        public ICollection<JobApplication>? Applications { get; set; }
    }
}
