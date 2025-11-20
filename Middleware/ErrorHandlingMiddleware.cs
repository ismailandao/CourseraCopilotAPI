using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace CopilotApiProject.Middleware
{
    /// <summary>
    /// Middleware to catch unhandled exceptions and return consistent JSON error responses
    /// </summary>
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _environment;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger, IWebHostEnvironment environment)
        {
            _next = next;
            _logger = logger;
            _environment = environment;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            // Log the exception with full details
            _logger.LogError(exception, "Unhandled exception occurred. Request: {Method} {Path} from {RemoteIP}",
                context.Request.Method,
                context.Request.Path,
                context.Connection.RemoteIpAddress);

            // Determine response based on exception type
            var response = CreateErrorResponse(exception);

            // Set response content type and status code
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)response.StatusCode;

            // Create consistent error response format
            var errorResponse = new
            {
                error = response.Message,
                statusCode = (int)response.StatusCode,
                timestamp = DateTime.UtcNow,
                path = context.Request.Path.Value,
                method = context.Request.Method,
                traceId = context.TraceIdentifier,
                details = _environment.IsDevelopment() ? response.Details : null
            };

            var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await context.Response.WriteAsync(jsonResponse);
        }

        private ErrorResponse CreateErrorResponse(Exception exception)
        {
            return exception switch
            {
                ArgumentNullException => new ErrorResponse(
                    HttpStatusCode.BadRequest,
                    "Invalid request: Required parameter is missing.",
                    GetExceptionDetails(exception)
                ),
                ArgumentException => new ErrorResponse(
                    HttpStatusCode.BadRequest,
                    "Invalid request: Invalid parameter value.",
                    GetExceptionDetails(exception)
                ),
                KeyNotFoundException => new ErrorResponse(
                    HttpStatusCode.NotFound,
                    "Requested resource was not found.",
                    GetExceptionDetails(exception)
                ),
                UnauthorizedAccessException => new ErrorResponse(
                    HttpStatusCode.Unauthorized,
                    "Access denied: Authentication required.",
                    GetExceptionDetails(exception)
                ),
                InvalidOperationException => new ErrorResponse(
                    HttpStatusCode.BadRequest,
                    "Invalid operation: The requested action cannot be performed.",
                    GetExceptionDetails(exception)
                ),
                TimeoutException => new ErrorResponse(
                    HttpStatusCode.RequestTimeout,
                    "Request timeout: The operation took too long to complete.",
                    GetExceptionDetails(exception)
                ),
                NotImplementedException => new ErrorResponse(
                    HttpStatusCode.NotImplemented,
                    "Feature not implemented: This functionality is not yet available.",
                    GetExceptionDetails(exception)
                ),
                ValidationException validationEx => new ErrorResponse(
                    HttpStatusCode.BadRequest,
                    $"Validation failed: {validationEx.Message}",
                    GetExceptionDetails(exception)
                ),
                _ => new ErrorResponse(
                    HttpStatusCode.InternalServerError,
                    "Internal server error: An unexpected error occurred.",
                    GetExceptionDetails(exception)
                )
            };
        }

        private object? GetExceptionDetails(Exception exception)
        {
            if (!_environment.IsDevelopment())
                return null;

            return new
            {
                exceptionType = exception.GetType().Name,
                message = exception.Message,
                stackTrace = exception.StackTrace,
                innerException = exception.InnerException?.Message
            };
        }

        private record ErrorResponse(HttpStatusCode StatusCode, string Message, object? Details);
    }

    /// <summary>
    /// Custom validation exception for business logic validation errors
    /// </summary>
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
        public ValidationException(string message, Exception innerException) : base(message, innerException) { }
    }

    /// <summary>
    /// Extension method to register the error handling middleware
    /// </summary>
    public static class ErrorHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseErrorHandling(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ErrorHandlingMiddleware>();
        }
    }
}