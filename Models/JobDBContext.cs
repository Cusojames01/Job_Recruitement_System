using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace Job_Recruitment_System.Models
{
    public class JobDBContext : DbContext
    {
        public JobDBContext(DbContextOptions<JobDBContext> options)
            : base(options)
        {
        }

        // 🔹 Tables
        public DbSet<Recruiter> Recruiters { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<PostJob> JobPosts { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PostJob>()
                .HasOne(p => p.Recruiter)
                .WithMany() // Pwede rin may ICollection<PostJob> sa Recruiter para 1 recruiter → maraming jobs
                .HasForeignKey(p => p.RecruiterId)
                .OnDelete(DeleteBehavior.SetNull); // Kung idelete ang recruiter, null lang ang recruiterId

            modelBuilder.Entity<JobApplication>()
                .HasOne(a => a.PostJob)
                .WithMany(p => p.Applications)
                .HasForeignKey(a => a.PostJobId)
                .OnDelete(DeleteBehavior.Cascade); // Kapag nadelete ang job, automatic madelete rin apps
        }


    }
}
