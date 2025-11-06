using Microsoft.AspNetCore.Http;

namespace GreenTech.Utils
{
    public static class UrlHelper
    {
        /// <summary>
        /// Gets the base URL for MVC application from environment variable or builds from HttpContext
        /// </summary>
        public static string GetMvcBaseUrl(HttpContext? httpContext = null)
        {
            // First, try to get from environment variable
            var baseUrl = Environment.GetEnvironmentVariable("MVC_BASE_URL");
            if (!string.IsNullOrEmpty(baseUrl))
            {
                return baseUrl.TrimEnd('/');
            }

            // If not found, try to build from HttpContext
            if (httpContext != null)
            {
                var request = httpContext.Request;
                var scheme = request.Scheme;
                var host = request.Host;

                // Default MVC port if we're in Razor Pages project
                var mvcPort = Environment.GetEnvironmentVariable("MVC_PORT");
                if (!string.IsNullOrEmpty(mvcPort) && int.TryParse(mvcPort, out int port))
                {
                    host = new HostString(host.Host, port);
                }
                else if (host.Host == "localhost" || host.Host == "127.0.0.1")
                {
                    // Default to port 5045 for MVC project
                    host = new HostString(host.Host, 5045);
                }

                return $"{scheme}://{host}";
            }

            // Fallback to default (can be overridden by environment variable)
            return "http://localhost:5045";
        }

        /// <summary>
        /// Gets the login URL for MVC application
        /// </summary>
        public static string GetLoginUrl(HttpContext? httpContext = null)
        {
            return $"{GetMvcBaseUrl(httpContext)}/Auth/Login";
        }

        /// <summary>
        /// Gets the home URL for MVC application
        /// </summary>
        public static string GetHomeUrl(HttpContext? httpContext = null)
        {
            return $"{GetMvcBaseUrl(httpContext)}/Home/Index";
        }
    }
}
