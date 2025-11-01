using Job_Recruitment_System.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Job_Recruitment_System.Controllers
{
    public class AccountController : Controller
    {


        private readonly ILogger<AccountController> _logger;
        private readonly JobDBContext _db;

        public AccountController(ILogger<AccountController> logger, JobDBContext db)
        {
            _logger = logger;
            _db = db;
        }
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult ChooseRole()
        {
            return View();
        }
        public IActionResult UserDashBoard()
        {
            return View();
        }

        public IActionResult RecruiterDashBoard()
        {
            return View();
        }

        public IActionResult RecruiterProfile()
        {
            return View();
        }
        public IActionResult Logout()
        {
            // Kung gumagamit ng cookie authentication
            HttpContext.SignOutAsync(); // Linisin ang authentication cookie

            // Optional: i-clear ang session kung may session data
            HttpContext.Session.Clear();

            return RedirectToAction("Login", "Account"); // Ibabalik sa login page
        }
        public IActionResult AdminDashboard()
        {
            // Statistics
            var totalUsers = _db.Users.Count();
            var totalRecruiters = _db.Recruiters.Count();
            var totalAll = totalUsers + totalRecruiters;

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalRecruiters = totalRecruiters;
            ViewBag.TotalAll = totalAll;

            // Optional: Lists for tables
            var users = _db.Users.ToList();
            var recruiters = _db.Recruiters.ToList();

            ViewBag.Users = users;
            ViewBag.Recruiters = recruiters;

            return View();
        }
        [HttpPost]
        public IActionResult Login(LoginViewModel model, string email, string password)
        {
            // 🔹 Recruiter Login
            var recruiter = _db.Recruiters.FirstOrDefault(r => r.Email == email && r.Password == password);
            if (recruiter != null)
            {
                // Save Recruiter Info sa Session
                HttpContext.Session.SetInt32("RecruiterId", recruiter.Id);
                HttpContext.Session.SetString("RecruiterName", recruiter.RecruiterName);

                // Redirect sa Profile page
                return RedirectToAction("RecruiterDashBoard", "Recruiter");
            }

            // 🔹 Admin Login
            var admin = _db.Admins.FirstOrDefault(a => a.Email == model.Email && a.Password == model.Password);
            if (admin != null)
            {
                HttpContext.Session.SetString("Role", "Admin");
                return RedirectToAction("AdminDashboard", "Account");
            }

            // 🔹 Applicant Login
            var user = _db.Users.FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);
            if (user != null && user.Role == "Applicant")
            {
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("UserName", user.FullName);
                HttpContext.Session.SetString("Role", "Applicant");
                return RedirectToAction("UserDashBoard", "User");
            }

            ViewBag.ErrorMessage = "Invalid email or password.";
            return View(model);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
    }
