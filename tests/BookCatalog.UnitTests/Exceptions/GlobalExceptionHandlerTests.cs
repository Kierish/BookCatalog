using System.Text.Json;
using BookCatalog.Api.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace BookCatalog.UnitTests.Exceptions
{
    public sealed class GlobalExceptionHandlerTests
    {
        private readonly ILogger<GlobalExceptionHandler> _logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
        private readonly GlobalExceptionHandler _handler;

        public GlobalExceptionHandlerTests()
        {
            _handler = new GlobalExceptionHandler(_logger);
        }

        [Fact]
        public async Task TryHandleAsync_WhenExceptionOccurs_ShouldWriteServerErrorResponse()
        {
            using var responseBody = new MemoryStream();
            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = responseBody;
            httpContext.Request.Path = "/api/books";
            httpContext.TraceIdentifier = "test-trace-id";
            var exception = new Exception("Unexpected failure");

            var handled = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

            handled.ShouldBeTrue();
            httpContext.Response.StatusCode.ShouldBe(StatusCodes.Status500InternalServerError);

            responseBody.Position = 0;
            using var response = await JsonDocument.ParseAsync(responseBody);
            response.RootElement.GetProperty("status").GetInt32().ShouldBe(500);
            response.RootElement.GetProperty("title").GetString().ShouldBe("Server Error");
            response.RootElement.GetProperty("detail").GetString()
                .ShouldBe("An unexpected system error occurred. Please try again later.");
            response.RootElement.GetProperty("instance").GetString().ShouldBe("/api/books");
            response.RootElement.GetProperty("traceId").GetString().ShouldBe("test-trace-id");
        }

        [Fact]
        public async Task TryHandleAsync_WhenExceptionOccurs_ShouldNotExposeExceptionDetails()
        {
            using var responseBody = new MemoryStream();
            var httpContext = new DefaultHttpContext();
            httpContext.Response.Body = responseBody;
            Action throwException = () => throw new InvalidOperationException("Database password: secret123");
            var exception = Record.Exception(throwException);
            exception.ShouldNotBeNull();
            exception.StackTrace.ShouldNotBeNullOrWhiteSpace();

            await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

            responseBody.Position = 0;
            using var reader = new StreamReader(responseBody);
            var responseJson = await reader.ReadToEndAsync();
            responseJson.ShouldNotContain("secret123");
            responseJson.ShouldNotContain(nameof(InvalidOperationException));
            responseJson.ShouldNotContain(nameof(TryHandleAsync_WhenExceptionOccurs_ShouldNotExposeExceptionDetails));

            using var response = JsonDocument.Parse(responseJson);
            response.RootElement.TryGetProperty("stackTrace", out _).ShouldBeFalse();
        }
    }
}
