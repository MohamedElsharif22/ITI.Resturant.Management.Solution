using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ITI.Resturant.Management.MVC.Middleware
{
    public class BusinessHoursMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<BusinessHoursMiddleware> _logger;

        public BusinessHoursMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<BusinessHoursMiddleware> logger)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var currentTime = TimeOnly.FromDateTime(DateTime.Now);
                var openingTime = TimeOnly.Parse(_configuration["BusinessHours:OpeningTime"] ?? "09:00");
                var closingTime = TimeOnly.Parse(_configuration["BusinessHours:ClosingTime"] ?? "22:00");
                var happyHourStart = TimeOnly.Parse("15:00"); // 3 PM
                var happyHourEnd = TimeOnly.Parse("17:00"); // 5 PM

                // Add business hours state to HttpContext for use in services and views
                context.Items["IsHappyHour"] = currentTime >= happyHourStart && currentTime <= happyHourEnd;
                context.Items["IsBusinessHours"] = currentTime >= openingTime && currentTime <= closingTime;

                // Skip middleware for static files, admin routes, API endpoints, or the business page itself
                if (ShouldSkipMiddleware(context))
                {
                    await _next(context);
                    return;
                }

                // Check if we're processing an order outside business hours
                bool isOrderRequest = context.Request.Path.StartsWithSegments("/Order") &&
                    context.Request.Method.Equals("POST", StringComparison.OrdinalIgnoreCase);

                if (!((bool)context.Items["IsBusinessHours"]!))
                {
                    // Build redirect URL to the Business Closed page with helpful query parameters
                    var qs = new QueryString($"?opening={Uri.EscapeDataString(openingTime.ToString("hh\\:mm"))}&closing={Uri.EscapeDataString(closingTime.ToString("hh\\:mm"))}&happy={( (bool)context.Items["IsHappyHour"] ? "1" : "0" )}&isOrder={(isOrderRequest ? "1" : "0")}&returnUrl={Uri.EscapeDataString(context.Request.Path + context.Request.QueryString)}");

                    var redirectPath = "/business/closed" + qs;

                    // For order POSTs prefer returning 400 for API-like requests, but redirect for user-facing pages
                    if (isOrderRequest)
                    {
                        // If the request originated from a browser form POST, redirect to a friendly page
                        context.Response.Redirect(redirectPath);
                        return;
                    }
                    else
                    {
                        context.Response.Redirect(redirectPath);
                        return;
                    }
                }

                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                _logger.LogInformation(
                    "Request {RequestPath} completed in {ElapsedMilliseconds}ms",
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds);
            }
        }

        private bool ShouldSkipMiddleware(HttpContext context)
        {
            return context.Request.Path.StartsWithSegments("/admin") ||
                   context.Request.Path.StartsWithSegments("/business") ||
                   context.Request.Path.StartsWithSegments("/css") ||
                   context.Request.Path.StartsWithSegments("/js") ||
                   context.Request.Path.StartsWithSegments("/lib") ||
                   context.Request.Path.StartsWithSegments("/api") ||
                   context.Request.Path.StartsWithSegments("/swagger");
        }
    }

    public static class BusinessHoursMiddlewareExtensions
    {
        public static IApplicationBuilder UseBusinessHours(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<BusinessHoursMiddleware>();
        }
    }
}