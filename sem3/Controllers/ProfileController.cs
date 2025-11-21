using Microsoft.AspNet.Identity;
using sem3.Models.Entities;
using sem3.Models.ModelViews;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
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
                PasswordHash = ""
            };

            ViewBag.DoNotDisturbStatus = GetServiceStatus(userFromDb.UserID, "Do Not Disturb");
            ViewBag.CallerTunesStatus = GetServiceStatus(userFromDb.UserID, "Caller Tunes");
            ViewBag.SelectedTune = GetSelectedTune(userFromDb.UserID);

            return View(viewModel);
        }

        // POST: Profile/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ChangePassword(ChangePassword model)
        {
            if (Session["CurrentUser"] == null)
            {
                return Json(new { success = false, message = "Session expired. Please login again." });
            }

            var sessionUser = Session["CurrentUser"] as UserM;
            var userInDb = _db.Users.Find(sessionUser.UserID);
            if (userInDb == null)
                return Json(new { success = false, message = "User not found." });

            // Verify current password
            if (!VerifyPassword(model.OldPassword, userInDb.PasswordHash))
            {
                return Json(new { success = false, message = "Current password is incorrect." });
            }

            // Validate new password
            if (string.IsNullOrEmpty(model.NewPassword) || model.NewPassword.Length < 6)
            {
                return Json(new { success = false, message = "New password must be at least 6 characters long." });
            }

            if (model.NewPassword != model.ConfirmPassword)
            {
                return Json(new { success = false, message = "New password and confirmation do not match." });
            }

            // Update password
            try
            {
                userInDb.PasswordHash = HashPassword(model.NewPassword);
                _db.SaveChanges();

                // Update session if needed
                sessionUser.PasswordHash = userInDb.PasswordHash;
                Session["CurrentUser"] = sessionUser;

                return Json(new { success = true, message = "Password changed successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        // Helper methods for password hashing and verification
        private bool VerifyPassword(string providedPassword, string hashedPassword)
        {
            var passwordHasher = new PasswordHasher();
            return passwordHasher.VerifyHashedPassword(hashedPassword, providedPassword) == PasswordVerificationResult.Success;
        }

        // Hash a password
        private string HashPassword(string password)
        {
            var passwordHasher = new PasswordHasher();
            return passwordHasher.HashPassword(password);
        }

        // POST: Profile/UpdateInfo
        [HttpPost]
        public JsonResult UpdateInfo(int UserID, string FullName, string MobileNumber, string Email, string Address)
        {
            try
            {
                var user = _db.Users.FirstOrDefault(u => u.UserID == UserID);
                if (user == null)
                    return Json(new { success = false, message = "User not found." });

                // Validate email format
                if (!new EmailAddressAttribute().IsValid(Email))
                    return Json(new { success = false, message = "Invalid email format." });

                // Check duplicate email
                if (_db.Users.Any(u => u.Email == Email && u.UserID != UserID))
                    return Json(new { success = false, message = "Email already exists." });

                // Validate Vietnamese phone number (10 digits)
                if (!Regex.IsMatch(MobileNumber, @"^\d{10}$"))
                    return Json(new { success = false, message = "Phone number must be 10 digits." });

                // Check duplicate phone number
                if (_db.Users.Any(u => u.MobileNumber == MobileNumber && u.UserID != UserID))
                    return Json(new { success = false, message = "Phone number already exists." });

                // Save changes
                user.FullName = FullName;
                user.MobileNumber = MobileNumber;
                user.Email = Email;
                user.Address = Address;

                _db.SaveChanges();

                return Json(new { success = true, message = "Profile updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Profile/VerifyPassword
        [HttpPost]
        public JsonResult VerifyPassword(string password)
        {
            if (Session["CurrentUser"] == null)
                return Json(new { success = false, message = "Session expired" });

            var sessionUser = Session["CurrentUser"] as UserM;
            var userInDb = _db.Users.Find(sessionUser.UserID);
            if (userInDb == null)
                return Json(new { success = false, message = "User not found" });

            bool valid = VerifyPassword(password, userInDb.PasswordHash);
            return Json(new { success = valid, message = valid ? "Password verified" : "Incorrect password" });
        }

        // Helper methods to get service status and selected tune
        private bool GetServiceStatus(int userId, string serviceName)
        {
            var service = _db.Services.FirstOrDefault(s => s.ServiceName == serviceName);
            if (service == null) return false;

            var setting = _db.UserServiceSettings
                .FirstOrDefault(us => us.UserID == userId && us.ServiceID == service.ServiceID);

            return setting?.IsEnabled ?? false;
        }

        // Get selected tune for Caller Tunes service
        private string GetSelectedTune(int userId)
        {
            var service = _db.Services.FirstOrDefault(s => s.ServiceName == "Caller Tunes");
            if (service == null) return "Default";

            var setting = _db.UserServiceSettings
                .FirstOrDefault(us => us.UserID == userId && us.ServiceID == service.ServiceID);

            return setting?.SelectedTune ?? "Default";
        }

        // POST: Profile/ToggleDoNotDisturb
        [HttpPost]
        public JsonResult ToggleDoNotDisturb()
        {
            try
            {
                if (Session["CurrentUser"] == null)
                    return Json(new { success = false, message = "Session expired" });

                var sessionUser = Session["CurrentUser"] as UserM;
                var doNotDisturbService = _db.Services.FirstOrDefault(s => s.ServiceName == "Do Not Disturb");

                if (doNotDisturbService == null)
                    return Json(new { success = false, message = "Service not found" });

                var existingSetting = _db.UserServiceSettings
                    .FirstOrDefault(us => us.UserID == sessionUser.UserID && us.ServiceID == doNotDisturbService.ServiceID);

                if (existingSetting == null)
                {
                    // Tạo mới setting
                    var newSetting = new Models.Entities.UserServiceSetting
                    {
                        UserID = sessionUser.UserID,
                        ServiceID = doNotDisturbService.ServiceID,
                        IsEnabled = true,
                        UpdatedDate = DateTime.Now
                    };
                    _db.UserServiceSettings.Add(newSetting);
                }
                else
                {
                    // Toggle trạng thái
                    existingSetting.IsEnabled = !existingSetting.IsEnabled;
                    existingSetting.UpdatedDate = DateTime.Now;
                }

                _db.SaveChanges();

                var newStatus = existingSetting?.IsEnabled ?? true;
                return Json(new
                {
                    success = true,
                    message = $"Do Not Disturb {(newStatus ? "enabled" : "disabled")}",
                    isEnabled = newStatus
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Profile/UpdateCallerTune
        [HttpPost]
        public JsonResult UpdateCallerTune(string selectedTune)
        {
            try
            {
                if (Session["CurrentUser"] == null)
                    return Json(new { success = false, message = "Session expired" });

                var sessionUser = Session["CurrentUser"] as UserM;
                var callerTunesService = _db.Services.FirstOrDefault(s => s.ServiceName == "Caller Tunes");

                if (callerTunesService == null)
                    return Json(new { success = false, message = "Service not found" });

                // Validate selected tune - mở rộng cho phép cả file upload
                var allowedTunes = new[] { "Default", "Waiting.mp3" };
                bool isUploadedFile = !allowedTunes.Contains(selectedTune) && selectedTune.StartsWith($"user_{sessionUser.UserID}_");

                if (!isUploadedFile && !allowedTunes.Contains(selectedTune))
                    return Json(new { success = false, message = "Invalid tune selection" });

                var existingSetting = _db.UserServiceSettings
                    .FirstOrDefault(us => us.UserID == sessionUser.UserID && us.ServiceID == callerTunesService.ServiceID);

                if (existingSetting == null)
                {
                    var newSetting = new Models.Entities.UserServiceSetting
                    {
                        UserID = sessionUser.UserID,
                        ServiceID = callerTunesService.ServiceID,
                        IsEnabled = selectedTune != "Default",
                        SelectedTune = selectedTune,
                        UpdatedDate = DateTime.Now
                    };
                    _db.UserServiceSettings.Add(newSetting);
                }
                else
                {
                    // Nếu đang chuyển từ file upload sang default/waiting, xóa file cũ
                    if (!string.IsNullOrEmpty(existingSetting.SelectedTune) &&
                        existingSetting.SelectedTune != "Default" &&
                        existingSetting.SelectedTune != "Waiting.mp3" &&
                        selectedTune != existingSetting.SelectedTune)
                    {
                        var uploadsDir = Server.MapPath("~/Content/audio/uploads/");
                        var oldFilePath = Path.Combine(uploadsDir, existingSetting.SelectedTune);
                        if (System.IO.File.Exists(oldFilePath))
                            System.IO.File.Delete(oldFilePath);
                    }

                    existingSetting.SelectedTune = selectedTune;
                    existingSetting.IsEnabled = selectedTune != "Default";
                    existingSetting.UpdatedDate = DateTime.Now;
                }

                _db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = $"Caller tune updated to {(selectedTune == "Default" ? "Default" : "your custom tune")}",
                    selectedTune = selectedTune,
                    isEnabled = selectedTune != "Default"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Profile/UploadCallerTune
        [HttpPost]
        public JsonResult UploadCallerTune()
        {
            try
            {
                if (Session["CurrentUser"] == null)
                    return Json(new { success = false, message = "Session expired" });

                var sessionUser = Session["CurrentUser"] as UserM;
                var callerTunesService = _db.Services.FirstOrDefault(s => s.ServiceName == "Caller Tunes");

                if (callerTunesService == null)
                    return Json(new { success = false, message = "Service not found" });

                if (Request.Files.Count == 0)
                    return Json(new { success = false, message = "No file selected" });

                var file = Request.Files[0];

                // Validate file
                if (file == null || file.ContentLength == 0)
                    return Json(new { success = false, message = "Please select a valid file" });

                // Check file type
                if (Path.GetExtension(file.FileName).ToLower() != ".mp3")
                    return Json(new { success = false, message = "Only MP3 files are allowed" });

                // Check file size (max 10MB)
                if (file.ContentLength > 10 * 1024 * 1024)
                    return Json(new { success = false, message = "File size must be less than 10MB" });

                // Create uploads directory if not exists
                var uploadsDir = Server.MapPath("~/Content/audio/uploads/");
                if (!Directory.Exists(uploadsDir))
                    Directory.CreateDirectory(uploadsDir);

                // Generate unique filename
                var fileName = $"user_{sessionUser.UserID}_{DateTime.Now:yyyyMMddHHmmss}_{Path.GetFileName(file.FileName)}";
                var filePath = Path.Combine(uploadsDir, fileName);

                // Save file
                file.SaveAs(filePath);

                // Update user's caller tune setting
                var existingSetting = _db.UserServiceSettings
                    .FirstOrDefault(us => us.UserID == sessionUser.UserID && us.ServiceID == callerTunesService.ServiceID);

                if (existingSetting == null)
                {
                    var newSetting = new Models.Entities.UserServiceSetting
                    {
                        UserID = sessionUser.UserID,
                        ServiceID = callerTunesService.ServiceID,
                        IsEnabled = true,
                        SelectedTune = fileName,
                        UpdatedDate = DateTime.Now
                    };
                    _db.UserServiceSettings.Add(newSetting);
                }
                else
                {
                    // Delete old file if exists
                    if (!string.IsNullOrEmpty(existingSetting.SelectedTune) && existingSetting.SelectedTune != "Default" && existingSetting.SelectedTune != "Waiting.mp3")
                    {
                        var oldFilePath = Path.Combine(uploadsDir, existingSetting.SelectedTune);
                        if (System.IO.File.Exists(oldFilePath))
                            System.IO.File.Delete(oldFilePath);
                    }

                    existingSetting.SelectedTune = fileName;
                    existingSetting.IsEnabled = true;
                    existingSetting.UpdatedDate = DateTime.Now;
                }

                _db.SaveChanges();

                return Json(new
                {
                    success = true,
                    message = "Caller tune uploaded successfully!",
                    selectedTune = fileName,
                    isEnabled = true
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Upload failed: " + ex.Message });
            }
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