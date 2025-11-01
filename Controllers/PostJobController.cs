using Job_Recruitment_System.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Job_Recruitment_System.Controllers
{
    public class PostJobController : Controller
    {
        private readonly JobDBContext _db;
        private readonly IWebHostEnvironment _environment;

        public PostJobController (JobDBContext db, IWebHostEnvironment environment)
        {
            _db = db;
            _environment = environment;
        }

        // ======================================================
        // ================= RECRUITER SIDE =====================
        // ======================================================

        // GET: Show Post Job form
        [HttpGet]
        public IActionResult PostJob()
        {
            return View();
        }

        // POST: Save Job
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostJob(PostJob model)
        {
            var recruiterId = HttpContext.Session.GetInt32("RecruiterId");

            if (recruiterId != null)
            {
                var recruiter = _db.Recruiters.FirstOrDefault(r => r.Id == recruiterId.Value);
                if (recruiter != null)
                {
                    model.RecruiterId = recruiter.Id;
                    model.PostedBy = string.IsNullOrWhiteSpace(recruiter.RecruiterName)
                        ? "Default Recruiter"
                        : recruiter.RecruiterName;

                    model.CompanyName = string.IsNullOrWhiteSpace(recruiter.CompanyName)
                        ? "Default Company"
                        : recruiter.CompanyName;
                }
                else
                {
                    model.RecruiterId = 0;
                    model.PostedBy = "Default Recruiter";
                    model.CompanyName = "Default Company";
                }
            }
            else
            {
                model.RecruiterId = 0;
                model.PostedBy = "Default Recruiter";
                model.CompanyName = "Default Company";
            }

            // Set default values for required fields if empty
            model.JobTitle ??= "Untitled Job";
            model.JobDescription ??= "No description provided.";
            model.JobType ??= "Full-time";
            model.Location ??= "Not specified";

            // Optional fields
            model.SalaryRange ??= null;
            model.ExperienceLevel ??= null;
            model.Qualifications ??= null;
            model.Deadline ??= null;
            model.DatePosted = DateTime.Now;
            model.Status = "Open";

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "⚠️ Fill all required fields!";
                return View(model);
            }

            try
            {
                _db.JobPosts.Add(model);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "✅ Job posted successfully!";
                return RedirectToAction("JobList");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "❌ Failed to save job: " + ex.Message;
                return View(model);
            }
        }



        // GET: List of recruiter's posted jobs
        public IActionResult JobList()
        {
            var recruiterId = HttpContext.Session.GetInt32("RecruiterId");
            if (recruiterId != null)
            {
                var jobs = _db.JobPosts
                    .Where(j => j.RecruiterId == recruiterId.Value)
                    .OrderByDescending(j => j.DatePosted)
                    .ToList();
                return View(jobs);
            }
            return View(Enumerable.Empty<PostJob>());
        }

        // GET: Job Details
        public IActionResult JobDetails(int id)
        {
            var job = _db.JobPosts.FirstOrDefault(j => j.Id == id);
            if (job == null) return NotFound();

            var recruiterId = HttpContext.Session.GetInt32("RecruiterId");
            ViewBag.CanEdit = (recruiterId != null && recruiterId.Value == job.RecruiterId);

            return View(job);
        }

        // GET: Edit Job
        public IActionResult EditJobPost(int id)
        {
            var job = _db.JobPosts.FirstOrDefault(j => j.Id == id);
            if (job == null) return NotFound();

            var recruiterId = HttpContext.Session.GetInt32("RecruiterId");
            if (recruiterId == null || recruiterId.Value != job.RecruiterId)
                return Unauthorized();

            return View(job);
        }

        // POST: Edit Job
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditJobPost(int id, PostJob model)
        {
            if (id != model.Id) return BadRequest();

            var job = _db.JobPosts.FirstOrDefault(j => j.Id == id);
            if (job == null) return NotFound();

            var recruiterId = HttpContext.Session.GetInt32("RecruiterId");
            if (recruiterId == null || recruiterId.Value != job.RecruiterId)
                return Unauthorized();

            if (!ModelState.IsValid) return View(model);

            try
            {
                job.JobTitle = model.JobTitle;
                job.JobDescription = model.JobDescription;
                job.JobType = model.JobType;
                job.SalaryRange = model.SalaryRange;
                job.Location = model.Location;
                job.ExperienceLevel = model.ExperienceLevel;
                job.Qualifications = model.Qualifications;
                job.Deadline = model.Deadline;
                job.Status = model.Status;

                _db.Update(job);
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "✅ Job updated successfully!";
                return RedirectToAction("JobDetails", new { id = job.Id });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "❌ Failed to update job: " + ex.Message;
                return View(model);
            }
        }

        // POST: Delete Job
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var job = _db.JobPosts.FirstOrDefault(j => j.Id == id);
            if (job == null) return NotFound();

            var recruiterId = HttpContext.Session.GetInt32("RecruiterId");
            if (recruiterId == null || recruiterId.Value != job.RecruiterId)
                return Unauthorized();

            try
            {
                _db.JobPosts.Remove(job);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "✅ Job deleted successfully!";
                return RedirectToAction("JobList");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "❌ Failed to delete job: " + ex.Message;
                return RedirectToAction("JobDetails", new { id });
            }
        }

        // ======================================================
        // ================= USER SIDE ==========================
        // ======================================================

        // VIEW: All Job Posts (Available Jobs)
        public IActionResult UserViewPost()
        {
            var jobs = _db.JobPosts
                .Where(j => j.Status == "Open")
                .OrderByDescending(j => j.DatePosted)
                .ToList();

            return View(jobs);
        }

        // VIEW: Job Details + Apply Form
        [HttpGet]
        public IActionResult UserJobDetails(int id)
        {
            var job = _db.JobPosts.FirstOrDefault(j => j.Id == id);
            if (job == null)
                return NotFound();

            return View(job);
        }

        // POST: Apply to Job (upload PDF resume)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Apply(int id, IFormFile resume)
        {
            var job = _db.JobPosts.FirstOrDefault(j => j.Id == id);
            if (job == null)
                return NotFound();

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "⚠️ Please log in first before applying.";
                return RedirectToAction("Login", "Account");
            }

            if (resume == null || resume.Length == 0)
            {
                TempData["ErrorMessage"] = "❌ Please upload your resume in PDF format.";
                return RedirectToAction("UserJobDetails", new { id });
            }

            if (!resume.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] = "❌ Only PDF files are allowed.";
                return RedirectToAction("UserJobDetails", new { id });
            }

            var uploadFolder = Path.Combine(_environment.WebRootPath, "Resumes");
            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(resume.FileName)}";
            var filePath = Path.Combine(uploadFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await resume.CopyToAsync(stream);
            }

            var application = new JobApplication
            {
                PostJobId = job.Id,
                UserId = userId.Value,
                ResumePath = "/Resumes/" + fileName,
                AppliedDate = DateTime.Now
            };

            _db.JobApplications.Add(application);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "✅ Application submitted successfully!";
            return RedirectToAction("UserViewPost");
        }

        // VIEW: My Applications
        public IActionResult MyApplication()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "⚠️ Please log in first.";
                return RedirectToAction("Login", "Account");
            }

            var applications = _db.JobApplications
                .Where(a => a.UserId == userId)
                .Include(a => a.PostJob)
                .OrderByDescending(a => a.AppliedDate)
                .ToList();

            return View(applications);
        }
    }
}
