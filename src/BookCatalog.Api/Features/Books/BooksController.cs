using BookCatalog.Api.Features.Books.CreateBook;
using BookCatalog.Api.Features.Books.DeleteBook;
using BookCatalog.Api.Features.Books.GetBookById;
using BookCatalog.Api.Features.Books.GetBooksList;
using BookCatalog.Api.Features.Books.UpdateBook;
using BookCatalog.Domain.Common;
using Microsoft.AspNetCore.Mvc;
using BookCatalog.Api.Controllers;

namespace BookCatalog.Api.Features.Books
{
    [ApiController]
    [Route("api/books")]
    public sealed class BooksController : ApiControllerBase
    {
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BookResponse>> GetById(
            Guid id,
            [FromServices] GetBookByIdHandler handler)
        {
            var result = await handler.HandleAsync(id);

            if (result.IsFailure)
            {
                return HandleFailure(result);
            }

            return Ok(result.Value);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<BookResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PagedResult<BookResponse>>> GetBooks(
            [FromQuery] GetBooksListRequest request,
            [FromServices] GetBooksListHandler handler)
        {
            var pagedBooks = await handler.HandleAsync(request);
            return Ok(pagedBooks);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BookResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookResponse>> Create(
            [FromBody] CreateBookRequest request,
            [FromServices] CreateBookHandler handler)
        {
            var result = await handler.HandleAsync(request);

            if (result.IsFailure)
            {
                return HandleFailure(result);
            }

            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Value.Id },
                result.Value);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateBookRequest request,
            [FromServices] UpdateBookHandler handler)
        {
            var result = await handler.HandleAsync(id, request);

            if (result.IsFailure)
            {
                return HandleFailure(result);
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Delete(
            Guid id,
            [FromServices] DeleteBookHandler handler)
        {
            var result = await handler.HandleAsync(id);

            if (result.IsFailure)
            {
                return HandleFailure(result);
            }

            return NoContent();
        }
    }
}
