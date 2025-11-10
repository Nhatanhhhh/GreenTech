using System;
using System.Linq;
using System.Threading;
using GreenTech.Utils;
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
                    Console.WriteLine("[AdminAuthorize] Session became unavailable after read");
                    var loginUrl = UrlHelper.GetLoginUrl(httpContext);
                    context.Result = new RedirectResult(loginUrl);
                    return;
                }
            }
            catch (Exception ex)
            {
                // Log exception for debugging
                Console.WriteLine($"[AdminAuthorize] Session error: {ex.Message}");
                Console.WriteLine($"[AdminAuthorize] Stack trace: {ex.StackTrace}");

                // Redirect to login if session read fails
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

            // If not admin or staff, redirect to home
            if (!isAdmin && !isStaff)
            {
                var homeUrl = UrlHelper.GetHomeUrl(httpContext);
                context.Result = new RedirectResult(homeUrl);
                return;
            }

            // If Staff, check if they can access this page
            if (isStaff && !isAdmin)
            {
                var path = httpContext.Request.Path.Value ?? "";

                // Block Dashboard (Index page) for Staff
                if (
                    path.Equals("/Index", StringComparison.OrdinalIgnoreCase)
                    || path.Equals("/", StringComparison.OrdinalIgnoreCase)
                )
                {
                    Console.WriteLine("[AdminAuthorize] Staff tried to access Dashboard");
                    httpContext.Session.SetString(
                        "ErrorMessage",
                        "Bạn không có quyền truy cập trang Dashboard. Vui lòng sử dụng trang Quản lý đơn hàng."
                    );
                    context.Result = new RedirectResult("/Orders/Index");
                    return;
                }

                // Blocked routes for Staff (only Admin can access)
                var blockedRoutes = new[]
                {
                    "/Products",
                    "/Categories",
                    "/CouponTemplates",
                    "/Suppliers",
                    "/Users",
                };

                var isBlockedRoute = blockedRoutes.Any(route =>
                    path.StartsWith(route, StringComparison.OrdinalIgnoreCase)
                );

                if (isBlockedRoute)
                {
                    Console.WriteLine(
                        $"[AdminAuthorize] Staff tried to access blocked route: {path}"
                    );
                    httpContext.Session.SetString(
                        "ErrorMessage",
                        "Bạn không có quyền truy cập trang này. Chỉ Admin mới có quyền truy cập."
                    );
                    context.Result = new RedirectResult("/Orders/Index");
                    return;
                }
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
