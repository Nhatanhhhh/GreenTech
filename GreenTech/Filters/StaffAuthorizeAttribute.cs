using System;
using System.Linq;
using GreenTech.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GreenTech.Filters
{
    /// <summary>
    /// Filter to restrict Staff access to only allowed pages:
    /// - Orders (view, update status)
    /// - Reviews (view, reply)
    /// - Blogs (view, create, update, delete)
    /// - Banners (view, create, update, delete, sort)
    /// - Profile (view only, no update/change password)
    /// </summary>
    public class StaffAuthorizeAttribute : Attribute, IPageFilter
    {
        // Allowed routes for Staff
        private static readonly string[] AllowedRoutes = new[]
        {
            "/Orders",
            "/Reviews",
            "/Blogs",
            "/Banners",
            "/Profile",
            "/Index", // Dashboard
            "/Error", // Error page
        };

        // Blocked routes for Staff (only Admin can access)
        private static readonly string[] BlockedRoutes = new[]
        {
            "/Products",
            "/Categories",
            "/CouponTemplates",
            "/Suppliers",
            "/Users",
        };

        public void OnPageHandlerExecuting(PageHandlerExecutingContext context)
        {
            var request = context.HttpContext.Request;
            var isPostRequest = request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase);

            // Skip authorization check for POST requests (handled in page handlers)
            if (isPostRequest)
            {
                return;
            }

            // Only check authorization for GET requests (when entering the page)
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

                // Also skip for AJAX handlers by name
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
                            $"[StaffAuthorize] Session initialization failed: {ex.Message}"
                        );
                        var loginUrl = UrlHelper.GetLoginUrl(httpContext);
                        context.Result = new RedirectResult(loginUrl);
                        return;
                    }
                }

                // Read session values
                isAuthenticated = session.GetString("IsAuthenticated");
                userRoles = session.GetString("UserRoles");

                // Verify session is still available after read
                if (!session.IsAvailable)
                {
                    Console.WriteLine("[StaffAuthorize] Session became unavailable after read");
                    var loginUrl = UrlHelper.GetLoginUrl(httpContext);
                    context.Result = new RedirectResult(loginUrl);
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[StaffAuthorize] Session error: {ex.Message}");
                Console.WriteLine($"[StaffAuthorize] Stack trace: {ex.StackTrace}");

                var loginUrl = UrlHelper.GetLoginUrl(httpContext);
                context.Result = new RedirectResult(loginUrl);
                return;
            }

            // Check if authentication data exists
            if (string.IsNullOrEmpty(isAuthenticated))
            {
                ClearSessionCookie(httpContext);
                var loginUrl = UrlHelper.GetLoginUrl(httpContext);
                context.Result = new RedirectResult(loginUrl);
                return;
            }

            // Check if user is authenticated
            if (isAuthenticated != "true")
            {
                ClearSessionCookie(httpContext);
                var loginUrl = UrlHelper.GetLoginUrl(httpContext);
                context.Result = new RedirectResult(loginUrl);
                return;
            }

            // Check if user has admin or staff role
            if (string.IsNullOrEmpty(userRoles))
            {
                var homeUrl = UrlHelper.GetHomeUrl(httpContext);
                context.Result = new RedirectResult(homeUrl);
                return;
            }

            var isAdmin = userRoles.Contains("ROLE_ADMIN");
            var isStaff = userRoles.Contains("ROLE_STAFF");

            // Admin can access all pages
            if (isAdmin)
            {
                return;
            }

            // If user is Staff, check if they can access this page
            if (isStaff)
            {
                var path = httpContext.Request.Path.Value ?? "";

                // Check if trying to access blocked route
                var isBlockedRoute = BlockedRoutes.Any(route =>
                    path.StartsWith(route, StringComparison.OrdinalIgnoreCase)
                );
                if (isBlockedRoute)
                {
                    Console.WriteLine(
                        $"[StaffAuthorize] Staff tried to access blocked route: {path}"
                    );
                    // Store error message in session for display on next page
                    httpContext.Session.SetString(
                        "ErrorMessage",
                        "Bạn không có quyền truy cập trang này. Chỉ Admin mới có quyền truy cập."
                    );
                    context.Result = new RedirectResult("/Index");
                    return;
                }

                // Check if trying to access allowed route
                var isAllowedRoute = AllowedRoutes.Any(route =>
                    path.StartsWith(route, StringComparison.OrdinalIgnoreCase)
                );
                if (!isAllowedRoute)
                {
                    Console.WriteLine(
                        $"[StaffAuthorize] Staff tried to access unknown route: {path}"
                    );
                    // Store error message in session for display on next page
                    httpContext.Session.SetString(
                        "ErrorMessage",
                        "Bạn không có quyền truy cập trang này."
                    );
                    context.Result = new RedirectResult("/Index");
                    return;
                }
            }
            else
            {
                // Not admin or staff - redirect to home
                var homeUrl = UrlHelper.GetHomeUrl(httpContext);
                context.Result = new RedirectResult(homeUrl);
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
