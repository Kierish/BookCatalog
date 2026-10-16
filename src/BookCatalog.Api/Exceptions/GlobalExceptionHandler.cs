using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BookCatalog.Api.Exceptions
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            if (exception is OperationCanceledException &&
                httpContext.RequestAborted.IsCancellationRequested)
            {
                _logger.LogInformation(
                    "Request canceled by the client. Method: {Method}, Path: {Path}",
                    httpContext.Request.Method,
                    httpContext.Request.Path);

                if (!httpContext.Response.HasStarted)
                {
                    httpContext.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
                }

                return true;
            }

            var traceId = System.Diagnostics.Activity.Current?.TraceId.ToString()
                ?? httpContext.TraceIdentifier;

            _logger.LogError(
                exception,
                "Unhandled exception while processing {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Server Error",
                Detail = "An unexpected system error occurred. Please try again later.",
                Instance = httpContext.Request.Path
            };

            problemDetails.Extensions["traceId"] = traceId;

            httpContext.Response.StatusCode = problemDetails.Status.Value;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
