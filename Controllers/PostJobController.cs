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
        [HttpGet]
        public IActionResult ViewApplicant(int? jobId)
        {
            var recruiterId = HttpContext.Session.GetInt32("RecruiterId");
            if (recruiterId == null)
                return RedirectToAction("Login", "Account");

            // Kunin lahat ng jobs ng recruiter
            var jobs = _db.JobPosts
                          .Where(j => j.RecruiterId == recruiterId)
                          .ToList();

            List<JobApplication> applicants = new List<JobApplication>();
            PostJob selectedJob = null;

            if (jobId != null)
            {
                // Kunin yung selected job
                selectedJob = _db.JobPosts
                                 .FirstOrDefault(j => j.Id == jobId && j.RecruiterId == recruiterId);

                if (selectedJob != null)
                {
                    // ✅ Include User para lumabas ang Name, Email, Contact
                    applicants = _db.JobApplications
                                    .Include(a => a.User)
                                    .Where(a => a.PostJobId == jobId)
                                    .ToList();
                }
            }

            ViewBag.JobTitle = selectedJob?.JobTitle;

            return View(new Tuple<IEnumerable<PostJob>, IEnumerable<JobApplication>>(jobs, applicants));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectApplication(int id)
        {
            var app = await _db.JobApplications.FindAsync(id);
            if (app == null) return NotFound();

            app.Status = "Rejected";
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "❌ Application rejected.";
            return RedirectToAction("ViewApplicant", new { jobId = app.PostJobId }); // ✅ goes back to same job applicants
        }

        [HttpPost]
        public async Task<IActionResult> AcceptApplication(int id)
        {
            var application = _db.JobApplications.FirstOrDefault(a => a.Id == id);
            if (application != null)
            {
                application.Status = "Accepted";
                _db.JobApplications.Update(application);
                await _db.SaveChangesAsync();
            }

            // Redirect pabalik sa ViewApplicant ng same job
            return RedirectToAction("ViewApplicant", new { jobId = application?.PostJobId });
        }

        // GET: Show Post Job form

        [HttpGet]
        public IActionResult PostJob()
        {
            return View();
        }
        public IActionResult ViewJob(int id)
        {
            var app = _db.JobApplications
      .Include(a => a.PostJob)
      .FirstOrDefault(a => a.Id == id);

            if (app == null)
                return NotFound();

            return View(app);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelApplication(int id)
        {
            var app = await _db.JobApplications.FindAsync(id);
            if (app == null)
            {
                TempData["ErrorMessage"] = "⚠️ Application not found.";
                return RedirectToAction("MyApplication", "PostJob");
            }

            _db.JobApplications.Remove(app);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "❌ Application cancelled successfully.";
            return RedirectToAction("MyApplication", "PostJob");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PostJob(PostJob model)
        {
            var recruiterId = HttpContext.Session.GetInt32("RecruiterId");

            if (recruiterId == null)
            {
                TempData["ErrorMessage"] = "⚠️ Please log in as recruiter before posting a job!";
                return RedirectToAction("Login", "Account");
            }

            var recruiter = _db.Recruiters.FirstOrDefault(r => r.Id == recruiterId.Value);
            if (recruiter == null)
            {
                TempData["ErrorMessage"] = "⚠️ Recruiter not found!";
                return RedirectToAction("JobList");
            }

            // ✅ Assign recruiter details
            model.RecruiterId = recruiter.Id;
            model.PostedBy = string.IsNullOrWhiteSpace(recruiter.RecruiterName)
                ? "Default Recruiter"
                : recruiter.RecruiterName;
            model.CompanyName = string.IsNullOrWhiteSpace(recruiter.CompanyName)
                ? "Default Company"
                : recruiter.CompanyName;

            // Default values
            model.JobTitle ??= "Untitled Job";
            model.JobDescription ??= "No description provided.";
            model.JobType ??= "Full-time";
            model.Location ??= "Not specified";
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


        // ✅ GET: List of recruiter's posted jobs with applicant count
        public IActionResult JobList()
        {
            var recruiterId = HttpContext.Session.GetInt32("RecruiterId");

            if (recruiterId == null)
                return RedirectToAction("Login", "Account");

            var jobs = _db.JobPosts
                .Include(j => j.Applications) // para makuha ang list ng applicants per job
                .Where(j => j.RecruiterId == recruiterId.Value)
                .OrderByDescending(j => j.DatePosted)
                .ToList(); 

            return View(jobs);
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
                .Include(j => j.Applications) // ✅ i-load ang applications para makuha ang applicant count
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
            return RedirectToAction("MyApplication");
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
