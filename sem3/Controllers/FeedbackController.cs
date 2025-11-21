using sem3.Models.Entities;
using sem3.Models.ModelViews;
using System;
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(FeedbackM model)
        {
            if (ModelState.IsValid)
            {
                var newFeedback = new Feedback
                {
                    Name = model.Name,
                    Email = model.Email,
                    FeedbackText = model.FeedbackText,
                    SubmitDate = DateTime.Now
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
    }
}