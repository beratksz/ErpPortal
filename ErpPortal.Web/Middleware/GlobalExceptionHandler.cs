using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace ErpPortal.Web.Middleware
{
    public class GlobalExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var response = new
            {
                error = new
                {
                    message = "An error occurred while processing your request.",
                    detail = exception.Message,
                    code = "INTERNAL_SERVER_ERROR"
                }
            };

            // API çağrıları için JSON response
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
            }
            // MVC sayfaları için error sayfasına yönlendirme
            else
            {
                context.Response.Redirect("/Home/Error");
            }
        }
    }
} 