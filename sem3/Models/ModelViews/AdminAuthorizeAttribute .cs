using System.Web.Mvc;
using sem3.Models.ModelViews;

namespace sem3.Models.ModelViews
{
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var user = filterContext.HttpContext.Session["CurrentUser"] as UserM;
            if (user == null || user.RoleID != 1)
            {
                filterContext.Result = new RedirectResult("/Login/Login");
            }
            base.OnActionExecuting(filterContext);
        }
    }
}
