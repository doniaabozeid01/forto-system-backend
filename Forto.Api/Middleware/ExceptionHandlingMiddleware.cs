using Forto.Api.Common;
using System.Net;
using System.Text.Json;

namespace Forto.Api.Middleware
{
    public class ExceptionHandlingMiddleware : IMiddleware
    {
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                var traceId = context.TraceIdentifier;

                _logger.LogError(ex, "Unhandled exception. TraceId={TraceId}", traceId);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var payload = ApiResponse<object>.Fail(
                    message: "Something went wrong",
                    errors: null,
                    traceId: traceId
                );

                await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
            }
        }
    }

}



