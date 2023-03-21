using Microsoft.AspNetCore.Http;

namespace OrderApp.Server
{

    public class ErrorInterceptorMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorInterceptorMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception exception)
            {
                httpContext.Response.StatusCode = 400;
                await httpContext.Response.WriteAsync(exception.Message);
            }
        }
    }

    public static class ErrorInterceptorMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorInterceptorMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorInterceptorMiddleware>();
        }
    }
}
