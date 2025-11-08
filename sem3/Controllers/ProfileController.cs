using Microsoft.AspNet.Identity;
using sem3.Models.Entities;
using sem3.Models.ModelViews;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace sem3.Controllers
{
    public class ProfileController : Controller
    {
        private readonly OnlineRechargeDBEntities _db = new OnlineRechargeDBEntities();

        // GET: Profile/Index
        public ActionResult Index()
        {
            if (Session["CurrentUser"] == null)
            {
                return RedirectToAction("Login", "Login");
            }

            var sessionUser = Session["CurrentUser"] as UserM;
            var userFromDb = _db.Users.Find(sessionUser.UserID);
            if (userFromDb == null)
            {
                Session.Clear();
                return RedirectToAction("Login", "Login");
            }

            var viewModel = new UserM
            {
                UserID = userFromDb.UserID,
                FullName = userFromDb.FullName,
                MobileNumber = userFromDb.MobileNumber,
                Email = userFromDb.Email,
                Address = userFromDb.Address,
                RegistrationDate = userFromDb.RegistrationDate,
                PasswordHash = "",
                RoleID = userFromDb.RoleID,
            };

            // Truyền các model rỗng cho các form popup
            ViewBag.EmailForm = new ChangeEmail();
            ViewBag.PasswordForm = new ChangePassword();

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePassword model)
        {
            if (Session["CurrentUser"] == null)
            {
                return RedirectToAction("Login", "Login");
            }
            var sessionUser = Session["CurrentUser"] as UserM;
            var userInDb = _db.Users.Find(sessionUser.UserID);
            if (userInDb == null) return HttpNotFound();

            if (!VerifyPassword(model.OldPassword, userInDb.PasswordHash))
            {
                ModelState.AddModelError("OldPassword", "Incorrect current password.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    userInDb.PasswordHash = HashPassword(model.NewPassword);
                    _db.SaveChanges();
                    TempData["SuccessMessage"] = "Password updated successfully!";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Password update failed: " + string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
            }
            return RedirectToAction("Index");
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

        [HttpPost]
        public JsonResult UpdateInfo(int UserID, string FullName, string MobileNumber, string Email, string Address)
        {
            try
            {
                var user = _db.Users.FirstOrDefault(u => u.UserID == UserID);
                if (user == null)
                    return Json(new { success = false, message = "User not found." });

                // Validate định dạng email
                if (!new EmailAddressAttribute().IsValid(Email))
                    return Json(new { success = false, message = "Invalid email format." });

                // Check trùng email
                if (_db.Users.Any(u => u.Email == Email && u.UserID != UserID))
                    return Json(new { success = false, message = "Email already exists." });

                // Validate số điện thoại VN 10 số
                if (!Regex.IsMatch(MobileNumber, @"^\d{10}$"))
                    return Json(new { success = false, message = "Phone number must be 10 digits." });

                // Check trùng số điện thoại
                if (_db.Users.Any(u => u.MobileNumber == MobileNumber && u.UserID != UserID))
                    return Json(new { success = false, message = "Phone number already exists." });

                // Lưu thay đổi
                user.FullName = FullName;
                user.MobileNumber = MobileNumber;
                user.Email = Email;
                user.Address = Address;

                _db.SaveChanges();

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpPost]
        public JsonResult VerifyPassword(string password)
        {
            if (Session["CurrentUser"] == null)
                return Json(new { success = false });

            var sessionUser = Session["CurrentUser"] as UserM;
            var userInDb = _db.Users.Find(sessionUser.UserID);
            if (userInDb == null)
                return Json(new { success = false });

            bool valid = VerifyPassword(password, userInDb.PasswordHash);
            return Json(new { success = valid });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}