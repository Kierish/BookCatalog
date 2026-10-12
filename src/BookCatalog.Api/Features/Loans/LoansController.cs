using BookCatalog.Api.Controllers;
using BookCatalog.Api.Features.Loans.BorrowBook;
using Microsoft.AspNetCore.Mvc;

namespace BookCatalog.Api.Features.Loans
{
    [ApiController]
    [Route("api/loans")]
    public sealed class LoansController : ApiControllerBase
    {
        [HttpPost]
        [ProducesResponseType(typeof(LoanResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<LoanResponse>> Borrow(
            [FromBody] BorrowBookRequest request,
            [FromServices] BorrowBookHandler handler)
        {
            var result = await handler.HandleAsync(request);

            if (result.IsFailure)
            {
                return HandleFailure(result);
            }

            return StatusCode(
                StatusCodes.Status201Created,
                result.Value);
        }
    }
}
