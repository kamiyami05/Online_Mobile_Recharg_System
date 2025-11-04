using System.Web.Mvc;
using sem3.Models.ModelViews;

namespace sem3.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var currentUser = Session["CurrentUser"] as UserM;
            ViewBag.CurrentUser = currentUser;
            return View();
        }
    }
}
