using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;

namespace GreenTech.Filters
{
    public class AdminAuthorizeAttribute : Attribute, IPageFilter
    {
        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            var session = context.HttpContext.Session;
            
            // Check if session is available
            if (!session.IsAvailable)
            {
                context.Result = new RedirectResult("https://localhost:7135/Auth/Login");
                return;
            }
            
            var isAuthenticated = session.GetString("IsAuthenticated");
            var userRoles = session.GetString("UserRoles");

            // Check if user is authenticated
            if (string.IsNullOrEmpty(isAuthenticated) || isAuthenticated != "true")
            {
                context.Result = new RedirectResult("https://localhost:7135/Auth/Login");
                return;
            }

            // Check if user has admin role
            if (string.IsNullOrEmpty(userRoles) || !userRoles.Contains("ROLE_ADMIN"))
            {
                context.Result = new RedirectResult("https://localhost:7135/Home/Index");
                return;
            }
        }

        public void OnPageHandlerExecuted(PageHandlerExecutedContext context)
        {
            // Nothing to do here
        }

        public void OnPageHandlerSelected(PageHandlerSelectedContext context)
        {
            // Nothing to do here
        }
    }
}

