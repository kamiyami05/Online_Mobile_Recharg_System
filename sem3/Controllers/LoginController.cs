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
                var user = _db.Users.FirstOrDefault(u => u.MobileNumber == model.PhoneNumber);

                if (user != null && VerifyPassword(model.Password, user.PasswordHash))
                {
                    var role = _db.Roles.FirstOrDefault(r => r.RoleID == user.RoleID);

                    Session["CurrentUser"] = new User
                    {
                        UserID = user.UserID,
                        FullName = user.FullName,
                        MobileNumber = user.MobileNumber,
                        Email = user.Email,
                        PasswordHash = user.PasswordHash,
                        RoleID = user.UserID,
                        RoleName = role?.RoleName ?? "User",
                        Address = user.Address,
                        RegistrationDate = user.RegistrationDate
                    };

                    Session["CurrentUserId"] = user.UserID;

                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError("", "Incorrect phone number or password.");
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
        public ActionResult Register(Register model) // model bây giờ chỉ chứa 3 trường
        {
            if (ModelState.IsValid)
            {
                if (_db.Users.Any(u => u.MobileNumber == model.Phone))
                {
                    ModelState.AddModelError("", "Phone number already exists.");
                    return View(model);
                }

                var newUser = new sem3.Models.Entities.User
                {
                    FullName = model.FullName,
                    MobileNumber = model.Phone,
                    PasswordHash = HashPassword(model.Password),
                    RoleID = 2,
                    RegistrationDate = DateTime.Now,

                    Email = null,
                    Address = null
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