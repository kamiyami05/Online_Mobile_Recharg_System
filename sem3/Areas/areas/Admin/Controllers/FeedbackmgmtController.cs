using sem3.Models.Entities;
using sem3.Models.ModelViews;
using sem3.Models.Repositories;
using System;
using System.Web.Mvc;

namespace sem3.Areas.Admin.Controllers
{
    [AdminAuthorize]
    public class FeedbackmgmtController : Controller
    {
        private readonly FeedbackRepository _repository = new FeedbackRepository();
        public ActionResult Index()
        {
            var feedbacks = _repository.GetAll();
            return View(feedbacks);
        }
        public ActionResult Details(int id)
        {
            var feedback = _repository.GetById(id);
            if (feedback == null)
            {
                return HttpNotFound();
            }

            return Json(new
            {
                success = true,
                data = new
                {
                    name = feedback.Name,
                    email = feedback.Email,
                    date = feedback.SubmitDate.HasValue ? feedback.SubmitDate.Value.ToString("g") : "N/A",
                    text = feedback.FeedbackText
                }
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Delete(int id)
        {
            try
            {
                _repository.Delete(id);
                return Json(new { success = true, message = "Feedback deleted successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error while deleting: {ex.Message}" });
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repository.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}