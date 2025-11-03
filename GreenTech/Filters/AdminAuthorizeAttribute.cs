using System;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GreenTech.Filters
{
    public class AdminAuthorizeAttribute : Attribute, IPageFilter
    {
        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            var request = context.HttpContext.Request;
            var isPostRequest = request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase);

            if (isPostRequest)
            {
                return;
            }

            // Only check authorization for GET requests (when entering the page)
            // Skip authorization if handler has [IgnoreAntiforgeryToken]
            var handlerMethod = context.HandlerMethod;
            if (handlerMethod?.MethodInfo != null)
            {
                // Check if handler method has [IgnoreAntiforgeryToken]
                var handlerIgnoreAntiforgery = handlerMethod
                    .MethodInfo.GetCustomAttributes(typeof(IgnoreAntiforgeryTokenAttribute), false)
                    .Any();

                // Check if page model class has [IgnoreAntiforgeryToken]
                var pageModelType = context.HandlerInstance?.GetType();
                var classIgnoreAntiforgery =
                    pageModelType != null
                    && pageModelType
                        .GetCustomAttributes(typeof(IgnoreAntiforgeryTokenAttribute), false)
                        .Any();

                // Also skip for AJAX handlers by name (e.g., ValidateDiscountValue)
                var handlerName = handlerMethod.Name;
                var isAjaxHandler =
                    handlerName != null
                    && handlerName.Contains("Validate")
                    && handlerName != "OnPostAsync"
                    && handlerName != "OnGetAsync";

                if (handlerIgnoreAntiforgery || isAjaxHandler)
                {
                    return;
                }
            }

            // Only perform session check for GET requests
            var session = context.HttpContext.Session;
            var httpContext = context.HttpContext;

            string? isAuthenticated = null;
            string? userRoles = null;

            try
            {
                // Load session if not available
                if (!session.IsAvailable)
                {
                    try
                    {
                        _ = session.Id; // Force session initialization
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(
                            $"[AdminAuthorize] Session initialization failed: {ex.Message}"
                        );
                        context.Result = new RedirectResult("http://localhost:5045/Auth/Login");
                        return;
                    }
                }

                // Read session values
                isAuthenticated = session.GetString("IsAuthenticated");
                userRoles = session.GetString("UserRoles");

                // Verify session is still available after read
                if (!session.IsAvailable)
                {
                    Console.WriteLine("[AdminAuthorize] Session became unavailable after read");
                    context.Result = new RedirectResult("http://localhost:5045/Auth/Login");
                    return;
                }
            }
            catch (Exception ex)
            {
                // Log exception for debugging
                Console.WriteLine($"[AdminAuthorize] Session error: {ex.Message}");
                Console.WriteLine($"[AdminAuthorize] Stack trace: {ex.StackTrace}");

                // Redirect to login if session read fails
                context.Result = new RedirectResult("http://localhost:5045/Auth/Login");
                return;
            }

            // Check if authentication data exists
            if (string.IsNullOrEmpty(isAuthenticated))
            {
                ClearSessionCookie(httpContext);
                context.Result = new RedirectResult("http://localhost:5045/Auth/Login");
                return;
            }

            // Check if user is authenticated
            if (isAuthenticated != "true")
            {
                ClearSessionCookie(httpContext);
                context.Result = new RedirectResult("http://localhost:5045/Auth/Login");
                return;
            }

            // Check if user has admin role
            if (string.IsNullOrEmpty(userRoles) || !userRoles.Contains("ROLE_ADMIN"))
            {
                context.Result = new RedirectResult("http://localhost:5045/Home/Index");
                return;
            }
        }

        private static void ClearSessionCookie(HttpContext httpContext)
        {
            httpContext.Session.Clear();
            httpContext.Response.Cookies.Delete(".GreenTech.Session");
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
