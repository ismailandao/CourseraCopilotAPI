using System.Diagnostics;

namespace CopilotApiProject.Middleware
{
    /// <summary>
    /// Middleware to log HTTP method, request path, and response status code for each request
    /// </summary>
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;

        public RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var request = context.Request;
            var requestStartTime = DateTime.UtcNow;

            // Log the incoming request
            _logger.LogInformation("Request started: {Method} {Path} at {StartTime}",
                request.Method,
                request.Path,
                requestStartTime.ToString("yyyy-MM-dd HH:mm:ss.fff UTC"));

            try
            {
                // Continue to next middleware
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                var response = context.Response;
                var requestEndTime = DateTime.UtcNow;

                // Log the response with timing information
                _logger.LogInformation("Request completed: {Method} {Path} -> {StatusCode} in {ElapsedMs}ms at {EndTime}",
                    request.Method,
                    request.Path,
                    response.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    requestEndTime.ToString("yyyy-MM-dd HH:mm:ss.fff UTC"));

                // Log warning for slow requests (over 1 second)
                if (stopwatch.ElapsedMilliseconds > 1000)
                {
                    _logger.LogWarning("Slow request detected: {Method} {Path} took {ElapsedMs}ms",
                        request.Method,
                        request.Path,
                        stopwatch.ElapsedMilliseconds);
                }

                // Log error for failed requests
                if (response.StatusCode >= 400)
                {
                    var logLevel = response.StatusCode >= 500 ? LogLevel.Error : LogLevel.Warning;
                    _logger.Log(logLevel, "Request failed: {Method} {Path} -> {StatusCode}",
                        request.Method,
                        request.Path,
                        response.StatusCode);
                }
            }
        }
    }

    /// <summary>
    /// Extension method to register the logging middleware
    /// </summary>
    public static class RequestResponseLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestResponseLogging(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestResponseLoggingMiddleware>();
        }
    }
}