using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using ILogger = Integration.Util.Logging.ILogger; // Assuming this is the correct namespace alias



namespace Integrate.EmailVerification.Api.Middlewares
{
    [ExcludeFromCodeCoverage]
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Unhandled exception occurred.");

                var (statusCode, message) = ex switch
                {
                    CustomException customEx => (customEx.StatusCode, customEx.Message),
                    _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
                };

                var response = new ErrorResponse
                {
                    StatusCode = (int)statusCode,
                    Message = message,
                    Detailed = ex.InnerException?.Message ?? ex.Message
                };

                context.Response.StatusCode = response.StatusCode;
                context.Response.ContentType = "application/json";

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var json = JsonSerializer.Serialize(response, options);

                await context.Response.WriteAsync(json);
            }
        }
    }

    [ExcludeFromCodeCoverage]
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Detailed { get; set; } = string.Empty;
    }
}
