using Job_Recruitment_System.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Job_Recruitment_System.Controllers
{
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly JobDBContext _db;

        public UserController(ILogger<UserController> logger, JobDBContext db)
        {
            _logger = logger;
            _db = db;
        }

        // GET: Register User
        public IActionResult RegisterUser()
        {
            return View();
        }

        public IActionResult AddUser()
        {
            return View();
        }

        public IActionResult UserDashBoard()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }
        public IActionResult UserProfile()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = _db.Users.FirstOrDefault(r => r.Id == userId);
            if (user== null) return NotFound("User not found.");

            return View(user);
        }
   

        // POST: Register User with Profile Image
        [HttpPost]
        public async Task<IActionResult> RegisterUser(User user, IFormFile ProfileImage)
        {
            if (ModelState.IsValid)
            {
                if (ProfileImage != null && ProfileImage.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(ProfileImage.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/users", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ProfileImage.CopyToAsync(stream);
                    }

                    user.ProfileImage = fileName;
                }

                _db.Users.Add(user);
                await _db.SaveChangesAsync();
                return RedirectToAction("Login", "Account");
            }
            return View(user);
        }
        [HttpPost]
        public async Task<IActionResult> AddUser(User user, IFormFile ProfileImage)
        {
            if (ModelState.IsValid)
            {
                if (ProfileImage != null && ProfileImage.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(ProfileImage.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/users", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ProfileImage.CopyToAsync(stream);
                    }

                    user.ProfileImage = fileName;
                }

                _db.Users.Add(user);
                await _db.SaveChangesAsync();
                return RedirectToAction("UserView");
            }

            return View(user);
        }

        // GET: Edit User
        public IActionResult EditUser(int id)
        {
            var user = _db.Users.FirstOrDefault(u => u.Id == id);
            if (user == null) return NotFound();
            return View(user);
        }

        // POST: Edit User with Profile Image
        [HttpPost]
        public async Task<IActionResult> EditUser(User user, IFormFile ProfileImage)
        {
            if (ModelState.IsValid)
            {
                var existing = _db.Users.FirstOrDefault(u => u.Id == user.Id);
                if (existing != null)
                {
                    existing.FullName = user.FullName;
                    existing.Sex = user.Sex;
                    existing.Email = user.Email;
                    existing.Password = user.Password;
                    existing.Contact = user.Contact;

                    if (ProfileImage != null && ProfileImage.Length > 0)
                    {
                        var fileName = Guid.NewGuid() + Path.GetExtension(ProfileImage.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/users", fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await ProfileImage.CopyToAsync(stream);
                        }

                        existing.ProfileImage = fileName;
                    }

                    _db.Users.Update(existing);
                    await _db.SaveChangesAsync();
                    return RedirectToAction("UserView");
                }
            }
            return View(user);
        }

        // GET: List of Users
        public IActionResult UserView()
        {
            var userList = _db.Users.ToList();
            return View(userList);
        }

        // GET: Delete User
        public IActionResult DeleteUser(int id)
        {
            var user = _db.Users.FirstOrDefault(u => u.Id == id);
            if (user == null) return NotFound();

            _db.Users.Remove(user);
            _db.SaveChanges();
            return RedirectToAction("UserView");
        }
        [HttpPost]
        public async Task<IActionResult> UpdateProfile(User updatedUser, IFormFile ProfileImage)
        {
            var user = _db.Users.FirstOrDefault(r => r.Id == updatedUser.Id);
            if (user != null)
            {

                user.FullName = updatedUser.FullName;
                user.Sex = updatedUser.Sex;
                user.Email = updatedUser.Email;
                user.Contact = updatedUser.Contact;

                
                // Update profile image
                if (ProfileImage != null && ProfileImage.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/users");
                    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                    var fileName = Guid.NewGuid() + Path.GetExtension(ProfileImage.FileName);
                    var filePath = Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ProfileImage.CopyToAsync(stream);
                    }

                    user.ProfileImage = fileName;  // <-- dito naseset ang file name sa DB
                }

                _db.Users.Update(user);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction("UserProfile");
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
