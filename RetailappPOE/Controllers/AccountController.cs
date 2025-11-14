using Microsoft.AspNetCore.Mvc;
using RetailappPOE.Data;
using RetailappPOE.Models;
using RetailappPOE.Utils;

namespace RetailappPOE.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _ctx;
        public AccountController(ApplicationDbContext ctx) => _ctx = ctx;

        [HttpGet] public IActionResult Login() => View();
        [HttpGet] public IActionResult Register() => View();

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _ctx.Users.FirstOrDefault(u => u.Username == username);
            if (user == null || !PasswordHasher.Verify(password, user.PasswordHash))
            {
                ViewBag.Error = "Invalid credentials.";
                return View();
            }

            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("Role", user.Role);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult Register(User model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (_ctx.Users.Any(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Username already taken.");
                return View(model);
            }

            if (_ctx.Users.Any(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email already registered.");
                return View(model);
            }

            // Hash password from [NotMapped] field
            model.PasswordHash = PasswordHasher.Hash(model.Password);
            model.Role = "Customer"; // Default role

            _ctx.Users.Add(model);
            _ctx.SaveChanges();

            // Auto-login
            HttpContext.Session.SetString("Username", model.Username);
            HttpContext.Session.SetInt32("UserId", model.Id);
            HttpContext.Session.SetString("Role", model.Role);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}