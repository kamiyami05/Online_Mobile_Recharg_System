using Microsoft.AspNet.Identity;
using sem3.Models.Entities;
using sem3.Models.ModelViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
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
                    Session["CurrentUser"] = new User
                    {
                        UserID = user.UserID,
                        FullName = user.FullName,
                        MobileNumber = user.MobileNumber,
                        Email = user.Email,
                        PasswordHash = user.PasswordHash,
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
        public ActionResult Register(Register model)
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
                    RegistrationDate = DateTime.Now,
                    Email = null,
                    Address = null
                };

                _db.Users.Add(newUser);
                try
                {
                    _db.SaveChanges();

                    // Nếu thành công
                    return RedirectToAction("Login");
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    // Lỗi validation
                    var errorMessages = new List<string>();
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            errorMessages.Add($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                        }
                    }
                    ModelState.AddModelError("", "Validation errors: " + string.Join("; ", errorMessages));
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateException ex)
                {
                    // QUAN TRỌNG: Lấy inner exception sâu nhất
                    Exception inner = ex;
                    while (inner.InnerException != null)
                    {
                        inner = inner.InnerException;
                    }
                    ModelState.AddModelError("", $"Database error: {inner.Message}");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Unexpected error: {ex.Message}");
                }
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