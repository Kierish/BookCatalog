using BookCatalog.Api.Controllers;
using BookCatalog.Domain.Common.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shouldly;

namespace BookCatalog.UnitTests.Controllers
{
    public sealed class ApiControllerBaseTests
    {
        [Fact]
        public void HandleFailure_WhenErrorIsNotFound_ShouldReturnNotFoundProblemDetails()
        {
            var controller = new TestController();
            var result = Result.Failure(
                Error.NotFound(
                    "Books.NotFound",
                    "Book was not found."));

            var response = controller.Handle(result);

            response.StatusCode.ShouldBe(StatusCodes.Status404NotFound);

            var problemDetails = response.Value.ShouldBeOfType<ProblemDetails>();
            problemDetails.Title.ShouldBe("Books.NotFound");
            problemDetails.Detail.ShouldBe("Book was not found.");
        }

        private sealed class TestController : ApiControllerBase
        {
            public ObjectResult Handle(Result result)
            {
                return HandleFailure(result);
            }
        }
    }
}
