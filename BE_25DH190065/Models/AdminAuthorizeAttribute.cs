using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace BE_25DH190065.Models
{
    public class AdminAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            string role = session["UserRole"] != null ? session["UserRole"].ToString().Trim() : "";

            bool isAdmin = (session["AdminUser"] != null) ||
                           role.Equals("A", StringComparison.OrdinalIgnoreCase) ||
                           role.Equals("Admin", StringComparison.OrdinalIgnoreCase);

            if (!isAdmin)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new
                    {
                        area = "Admin",
                        controller = "Users",
                        action = "Login"
                    })
                );
            }

            base.OnActionExecuting(filterContext);
        }
    }
}