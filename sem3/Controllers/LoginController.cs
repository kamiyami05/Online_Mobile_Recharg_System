using Microsoft.AspNet.Identity;
using sem3.Models.Entities;
using sem3.Models.ModelViews;
using System;
using System.Collections;
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
                System.Diagnostics.Debug.WriteLine($"=== LOGIN ATTEMPT ===");
                System.Diagnostics.Debug.WriteLine($"Phone: {model.PhoneNumber}, Password: {model.Password}");

                // 1. Kiểm tra trong bảng AdminUsers trước
                var adminUser = _db.AdminUsers.FirstOrDefault(a => a.MobileNumber == model.PhoneNumber);

                if (adminUser != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Admin user found: {adminUser.Username}");
                    System.Diagnostics.Debug.WriteLine($"Admin PasswordHash: {adminUser.PasswordHash}");
                    System.Diagnostics.Debug.WriteLine($"Admin PasswordHash length: {adminUser.PasswordHash?.Length}");
                    System.Diagnostics.Debug.WriteLine($"Admin PasswordHash type: {adminUser.PasswordHash?.GetType()}");

                    // Hiển thị dưới dạng hex và bytes để debug
                    byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(adminUser.PasswordHash);
                    string passwordHex = BitConverter.ToString(passwordBytes).Replace("-", "");
                    System.Diagnostics.Debug.WriteLine($"Admin PasswordHash as hex: {passwordHex}");
                    System.Diagnostics.Debug.WriteLine($"Admin PasswordHash as bytes: {string.Join(", ", passwordBytes)}");

                    bool isAdminPasswordValid = VerifyPasswordForAdmin(model.Password, adminUser.PasswordHash);
                    System.Diagnostics.Debug.WriteLine($"Admin password verification result: {isAdminPasswordValid}");

                    if (isAdminPasswordValid)
                    {
                        System.Diagnostics.Debug.WriteLine($"ADMIN LOGIN SUCCESSFUL");
                        // Đăng nhập thành công với tài khoản admin
                        Session["CurrentUser"] = new User
                        {
                            UserID = adminUser.AdminID,
                            FullName = "Administrator",
                            MobileNumber = adminUser.MobileNumber,
                            Email = adminUser.Email,
                            Address = "System Admin"
                        };

                        Session["CurrentUserId"] = adminUser.AdminID;
                        Session["IsAdmin"] = true;

                        return RedirectToAction("Index", "Usermgmt", new { area = "Admin" });
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"ADMIN PASSWORD INVALID");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"No admin user found with phone: {model.PhoneNumber}");
                }

                // 2. Nếu không phải admin, kiểm tra trong bảng Users
                var user = _db.Users.FirstOrDefault(u => u.MobileNumber == model.PhoneNumber);
                if (user != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Regular user found: {user.FullName}");
                    bool isUserPasswordValid = VerifyPasswordForUser(model.Password, user.PasswordHash);
                    System.Diagnostics.Debug.WriteLine($"User password verification result: {isUserPasswordValid}");

                    if (isUserPasswordValid)
                    {
                        System.Diagnostics.Debug.WriteLine($"USER LOGIN SUCCESSFUL");
                        Session["CurrentUser"] = new User
                        {
                            UserID = user.UserID,
                            FullName = user.FullName,
                            MobileNumber = user.MobileNumber,
                            Email = user.Email,
                            PasswordHash = user.PasswordHash,
                            Address = user.Address,
                            RegistrationDate = user.RegistrationDate,
                        };

                        Session["CurrentUserId"] = user.UserID;
                        Session["IsAdmin"] = false;

                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"No regular user found with phone: {model.PhoneNumber}");
                }

                System.Diagnostics.Debug.WriteLine($"LOGIN FAILED - No valid user found");
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
                // Kiểm tra số điện thoại đã tồn tại trong cả 2 bảng
                bool phoneExistsInAdmin = _db.AdminUsers.Any(a => a.MobileNumber == model.Phone);
                bool phoneExistsInUsers = _db.Users.Any(u => u.MobileNumber == model.Phone);

                if (phoneExistsInAdmin || phoneExistsInUsers)
                {
                    ModelState.AddModelError("", "Phone number already exists.");
                    return View(model);
                }

                // Tiếp tục tạo user mới
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
                    return RedirectToAction("Login");
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
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

        // Verify User's password
        private bool VerifyPasswordForUser(string providedPassword, string hashedPassword)
        {
            var passwordHasher = new PasswordHasher();
            return passwordHasher.VerifyHashedPassword(hashedPassword, providedPassword) == PasswordVerificationResult.Success;
        }

        // Verify Admin's password
        private bool VerifyPasswordForAdmin(string providedPassword, string hashedPassword)
        {
            // So sánh trực tiếp vì password đang là plain text
            return providedPassword == hashedPassword;
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