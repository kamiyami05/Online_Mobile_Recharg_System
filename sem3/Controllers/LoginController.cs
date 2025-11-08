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
        private readonly OnlineRechargeDBEntities _db = new OnlineRechargeDBEntities();

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
                if (user != null && VerifyPassword(model.Password, user.PasswordHash))
                {
                    Session["CurrentUser"] = new User
                    {
                        UserID = user.UserID,
                        FullName = user.FullName,
                        MobileNumber = user.MobileNumber,
                        Email = user.Email,
                        PasswordHash = user.PasswordHash,
                        RoleID = user.UserID,
                        Address = user.Address,
                        RegistrationDate = user.RegistrationDate
                    };

                    Session["CurrentUserId"] = user.UserID;

                    // Lấy tên role từ bảng Roles
                    var role = _db.Roles.FirstOrDefault(r => r.RoleID == user.RoleID);
                    if (role != null && role.RoleName.ToLower() == "admin")
                        return RedirectToAction("Index", "User", new { area = "Admin" });

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Incorrect mobile number or password.");
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
                    MobileNumber = model.Phone,
                    Email = model.Email,
                    PasswordHash = HashPassword(model.Password),
                    RoleID = 2, // Mặc định là User
                    Address = model.Address,
                    RegistrationDate = DateTime.Now
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