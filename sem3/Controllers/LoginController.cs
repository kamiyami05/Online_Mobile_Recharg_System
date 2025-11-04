using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using sem3.Models.Entities;
using sem3.Models.ModelViews;
using User = sem3.Models.ModelViews.UserM;

namespace sem3.Controllers
{
    public class LoginController : Controller
    {
        private readonly Recharge_SystemEntities _db = new Recharge_SystemEntities();

        public ActionResult Login()
        {
            return View(new Login());
        }

        [HttpPost]
        public ActionResult Login(Login model)
        {
            if (ModelState.IsValid)
            {
                var user = _db.Users.FirstOrDefault(u => u.Email == model.Email);
                if (user != null && VerifyPassword(model.Password, user.Password))
                {
                    Session["CurrentUser"] = new User
                    {
                        Id = user.Id,
                        FullName = user.FullName,
                        Phone = user.Phone,
                        Email = user.Email,
                        Password = user.Password,
                        Role = user.Role,
                        Address = user.Address,
                        CreatedAt = (DateTime)user.CreatedAt,
                        IsActive = (bool)user.IsActive
                    };

                    Session["CurrentUserId"] = user.Id;

                    if (user.Role.ToLower() == "admin")
                        return RedirectToAction("Index", "User", new { area = "Admin" });

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Incorrect email or password.");
            }
            return View(model);
        }

        public ActionResult Logout()
        {
            Session.Clear();
            if (Request.Cookies["ASP.NET_SessionId"] != null)
                Response.Cookies["ASP.NET_SessionId"].Expires = DateTime.Now.AddDays(-1);

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Register()
        {
            return View(new Register());
        }

        [HttpPost]
        public ActionResult Register(Register model)
        {
            if (ModelState.IsValid)
            {
                if (_db.Users.Any(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("", "Email already exists.");
                    return View(model);
                }

                var newUser = new sem3.Models.Entities.User
                {
                    FullName = model.FullName,
                    Phone = model.Phone,
                    Email = model.Email,
                    Password = HashPassword(model.Password),
                    Role = "User",
                    Address = model.Address,
                    CreatedAt = DateTime.Now,
                    IsActive = true
                };

                _db.Users.Add(newUser);
                try
                {
                    _db.SaveChanges();
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"An error occurred: {ex.Message}");
                    return View(model);
                }

                return RedirectToAction("Login");
            }
            return View(model);
        }

        private bool VerifyPassword(string providedPassword, string hashedPassword)
        {
            var passwordHasher = new PasswordHasher();
            return passwordHasher.VerifyHashedPassword(hashedPassword, providedPassword) == PasswordVerificationResult.Success;
        }

        private string HashPassword(string password)
        {
            var passwordHasher = new PasswordHasher();
            return passwordHasher.HashPassword(password);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _db.Dispose();
            base.Dispose(disposing);
        }
    }
}
