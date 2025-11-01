using Job_Recruitment_System.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Job_Recruitment_System
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 🔹 Add services
            builder.Services.AddControllersWithViews();

            // 🔹 Add DbContext
            builder.Services.AddDbContext<JobDBContext>(options =>
                options.UseMySql(
                    builder.Configuration.GetConnectionString("DefaultConnection"),
                    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
                ));

            // 🔹 Add Session support
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // only HTTPS
                options.Cookie.SameSite = SameSiteMode.Lax; // prevents CSRF issues
            });

            // 🔹 Add HttpContextAccessor
            builder.Services.AddHttpContextAccessor();

            // 🔹 Add authentication using cookies
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                });

            var app = builder.Build();

            // 🔹 Seed default admin
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<JobDBContext>();
                if (!db.Admins.Any(a => a.Email == "admin@gmail.com"))
                {
                    db.Admins.Add(new Admin
                    {
                        Email = "admin@gmail.com",
                        Password = "admin123",
                        Role = "Admin"
                    });
                    db.SaveChanges();
                }
            }

            // 🔹 Middleware pipeline
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession(); // ✅ Session middleware

            app.UseAuthentication();
            app.UseAuthorization();

            // 🔹 Default route
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Account}/{action=Login}/{id?}");

            app.Run();
        }
    }
}
