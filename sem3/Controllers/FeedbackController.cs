using sem3.Models.Entities;
using sem3.Models.ModelViews;
using System;
using System.Linq;
using System.Web.Mvc;

namespace sem3.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly OnlineRechargeDBEntities _db = new OnlineRechargeDBEntities();

        public ActionResult Index()
        {
            var model = new FeedbackM();

            if (Session["CurrentUser"] != null)
            {
                var user = Session["CurrentUser"] as sem3.Models.ModelViews.UserM;
                model.Name = user.FullName;
                model.Email = user.Email;
            }

            return View(model);
        }

        // POST: Feedback/Index
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(FeedbackM model)
        {
            if (ModelState.IsValid)
            {
                int? userId = Session["CurrentUserId"] as int?;

                if (!CanSubmitFeedback(model.Email, userId))
                {
                    ModelState.AddModelError("", "You can only submit one feedback per hour. Please wait before submitting another feedback.");
                    return View(model);
                }
                else
                {
                    // Kiểm tra cho guest user (dựa trên email)
                    var lastGuestFeedback = _db.Feedbacks
                        .Where(f => f.Email == model.Email && f.UserID == null)
                        .OrderByDescending(f => f.SubmitDate)
                        .FirstOrDefault();

                    if (lastGuestFeedback != null)
                    {
                        TimeSpan timeSinceLastFeedback = DateTime.Now - lastGuestFeedback.SubmitDate.Value;
                        if (timeSinceLastFeedback.TotalHours < 1)
                        {
                            int minutesLeft = 60 - (int)timeSinceLastFeedback.TotalMinutes;
                            ModelState.AddModelError("", $"Please wait {minutesLeft} minutes before submitting another feedback. You can only submit one feedback per hour.");
                            return View(model);
                        }
                    }
                }

                var newFeedback = new Feedback
                {
                    Name = model.Name,
                    Email = model.Email,
                    FeedbackText = model.FeedbackText,
                    SubmitDate = DateTime.Now,
                    Rating = model.Rating
                };

                if (Session["CurrentUserId"] != null)
                {
                    newFeedback.UserID = (int)Session["CurrentUserId"];
                }

                try
                {
                    _db.Feedbacks.Add(newFeedback);
                    _db.SaveChanges();
                    ViewBag.SuccessMessage = "Thank you! Your feedback has been submitted successfully.";
                    return View(new FeedbackM());
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occurred: " + ex.Message);
                }
            }
            return View(model);
        }

        // Kiểm tra xem người dùng có thể gửi phản hồi hay không
        private bool CanSubmitFeedback(string email, int? userId)
        {
            DateTime oneHourAgo = DateTime.Now.AddHours(-1);

            if (userId.HasValue)
            {
                // Kiểm tra cho registered user
                return !_db.Feedbacks.Any(f => f.UserID == userId && f.SubmitDate >= oneHourAgo);
            }
            else
            {
                // Kiểm tra cho guest user
                return !_db.Feedbacks.Any(f => f.Email == email && f.UserID == null && f.SubmitDate >= oneHourAgo);
            }
        }

        // API để kiểm tra thời gian gửi phản hồi cuối cùng
        public JsonResult GetLastFeedbackTime()
        {
            try
            {
                int? userId = Session["CurrentUserId"] as int?;
                string userEmail = null;

                // Lấy email từ session nếu có user đăng nhập
                if (Session["CurrentUser"] != null)
                {
                    var user = Session["CurrentUser"] as sem3.Models.ModelViews.UserM;
                    userEmail = user?.Email;
                }

                DateTime oneHourAgo = DateTime.Now.AddHours(-1);
                Feedback lastFeedback = null;

                if (userId.HasValue)
                {
                    // Kiểm tra cho registered user
                    lastFeedback = _db.Feedbacks
                        .Where(f => f.UserID == userId)
                        .OrderByDescending(f => f.SubmitDate)
                        .FirstOrDefault();
                }
                else if (!string.IsNullOrEmpty(userEmail))
                {
                    // Kiểm tra cho guest user bằng email
                    lastFeedback = _db.Feedbacks
                        .Where(f => f.Email == userEmail && f.UserID == null)
                        .OrderByDescending(f => f.SubmitDate)
                        .FirstOrDefault();
                }

                if (lastFeedback != null && lastFeedback.SubmitDate >= oneHourAgo)
                {
                    var timePassed = DateTime.Now - lastFeedback.SubmitDate.Value;
                    var minutesLeft = 60 - timePassed.TotalMinutes;

                    return Json(new
                    {
                        canSubmit = false,
                        minutesLeft = minutesLeft,
                        lastSubmitTime = lastFeedback.SubmitDate.Value.ToString("g")
                    }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { canSubmit = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                // Trả về true nếu có lỗi để không block user
                return Json(new { canSubmit = true }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}