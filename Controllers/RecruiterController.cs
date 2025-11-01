using Job_Recruitment_System.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Job_Recruitment_System.Controllers
{
    public class RecruiterController : Controller
    {
        private readonly ILogger<RecruiterController> _logger;
        private readonly JobDBContext _db;

        public RecruiterController(ILogger<RecruiterController> logger, JobDBContext db)
        {
            _logger = logger;
            _db = db;
        }

        public IActionResult Login() => View();
        public IActionResult RegisterRecruiter() => View();
        public IActionResult AddRecruiter() => View();
        public IActionResult RecruiterDashBoard() => View();

        // ✅ Display Profile of Logged-in Recruiter
        public IActionResult RecruiterProfile()
        {
            int? recruiterId = HttpContext.Session.GetInt32("RecruiterId");

            if (recruiterId == null)
                return RedirectToAction("Login", "Account");

            var recruiter = _db.Recruiters.FirstOrDefault(r => r.Id == recruiterId);
            if (recruiter == null) return NotFound("Recruiter not found.");

            return View(recruiter);
        }

        // ============================
        // REGISTER RECRUITER with Image
        // ============================
        [HttpPost]
        public async Task<IActionResult> RegisterRecruiter(Recruiter recruiter, IFormFile ProfileImage)
        {
            if (ModelState.IsValid)
            {
                // Handle image upload
                if (ProfileImage != null && ProfileImage.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(ProfileImage.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/recruiters", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ProfileImage.CopyToAsync(stream);
                    }

                    recruiter.ProfileImage = fileName;
                }

                _db.Recruiters.Add(recruiter);
                await _db.SaveChangesAsync();

                return RedirectToAction("Login", "Account");
            }
            return View(recruiter);
        }

        // ============================
        // ADD RECRUITER (Admin) with Image
        // ============================
        [HttpPost]
        public async Task<IActionResult> AddRecruiter(Recruiter recruiter, IFormFile ProfileImage)
        {
            if (ModelState.IsValid)
            {
                if (ProfileImage != null && ProfileImage.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(ProfileImage.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/recruiters", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ProfileImage.CopyToAsync(stream);
                    }

                    recruiter.ProfileImage = fileName;
                }

                _db.Recruiters.Add(recruiter);
                await _db.SaveChangesAsync();

                return RedirectToAction("RecruiterView");
            }
            return View(recruiter);
        }

        // ============================
        // EDIT RECRUITER with Image
        // ============================
        public IActionResult EditRecruiter(int id)
        {
            var recruiter = _db.Recruiters.FirstOrDefault(r => r.Id == id);
            if (recruiter == null) return NotFound();
            return View(recruiter);
        }

        [HttpPost]
        public async Task<IActionResult> EditRecruiter(Recruiter recruiter, IFormFile ProfileImage)
        {
            if (ModelState.IsValid)
            {
                var existing = _db.Recruiters.FirstOrDefault(r => r.Id == recruiter.Id);
                if (existing != null)
                {
                    existing.CompanyName = recruiter.CompanyName;
                    existing.RecruiterName = recruiter.RecruiterName;
                    existing.Sex = recruiter.Sex;
                    existing.Email = recruiter.Email;
                    existing.Contact = recruiter.Contact;

                    // Update profile image if uploaded
                    if (ProfileImage != null && ProfileImage.Length > 0)
                    {
                        var fileName = Guid.NewGuid() + Path.GetExtension(ProfileImage.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/recruiters", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ProfileImage.CopyToAsync(stream);
                        }

                        existing.ProfileImage = fileName;
                    }

                    _db.Recruiters.Update(existing);
                    await _db.SaveChangesAsync();
                    return RedirectToAction("RecruiterView");
                }
            }
            return View(recruiter);
        }

        // ============================
        // DELETE RECRUITER
        // ============================
        public IActionResult DeleteRecruiter(int id)
        {
            var recruiter = _db.Recruiters.FirstOrDefault(u => u.Id == id);
            if (recruiter == null) return NotFound();

            _db.Recruiters.Remove(recruiter);
            _db.SaveChanges();
            return RedirectToAction("RecruiterView");
        }

        // ============================
        // VIEW ALL RECRUITERS
        // ============================
        public IActionResult RecruiterView()
        {
            var recruiterList = _db.Recruiters.ToList();
            return View(recruiterList);
        }

        // ============================
        // UPDATE PROFILE (Logged-in)
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(Recruiter updatedRecruiter, IFormFile ProfileImage)
        {
            var recruiter = _db.Recruiters.FirstOrDefault(r => r.Id == updatedRecruiter.Id);
            if (recruiter != null)
            {
                recruiter.CompanyName = updatedRecruiter.CompanyName;
                recruiter.RecruiterName = updatedRecruiter.RecruiterName;
                recruiter.Sex = updatedRecruiter.Sex;
                recruiter.Email = updatedRecruiter.Email;
                recruiter.Contact = updatedRecruiter.Contact;

                // Update profile image
                if (ProfileImage != null && ProfileImage.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/recruiters");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid() + Path.GetExtension(ProfileImage.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ProfileImage.CopyToAsync(stream);
                    }

                    recruiter.ProfileImage = fileName;  // <-- dito naseset ang file name sa DB
                }

                _db.Recruiters.Update(recruiter);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("RecruiterProfile");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
