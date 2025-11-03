using System;

namespace Job_Recruitment_System.Models
{
    public class JobApplication
    {
        public int Id { get; set; }
        public int PostJobId { get; set; }
        public int UserId { get; set; }
        public string ResumePath { get; set; }
        public DateTime AppliedDate { get; set; }

        // ✅ Added fields
        public string? Status { get; set; } = "Pending"; // Pending, Accepted, Rejected


        // Relationships
        public PostJob PostJob { get; set; }
        public User User { get; set; }
    }
}
