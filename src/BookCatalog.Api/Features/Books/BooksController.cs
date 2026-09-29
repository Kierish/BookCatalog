using BookCatalog.Api.Features.Books.CreateBook;
using BookCatalog.Api.Features.Books.DeleteBook;
using BookCatalog.Api.Features.Books.GetBookById;
using BookCatalog.Api.Features.Books.GetBooksList;
using BookCatalog.Api.Features.Books.UpdateBook;
using Microsoft.AspNetCore.Mvc;

namespace BookCatalog.Api.Features.Books
{
    [ApiController]
    [Route("api/books")]
    public sealed class BooksController : ControllerBase
    {
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookResponse>> GetById(
            Guid id,
            [FromServices] GetBookByIdHandler handler)
        {
            var book = await handler.HandleAsync(id);
            return book is not null ? Ok(book) : NotFound();
        }

        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<BookResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<BookResponse>>> GetAll(
            [FromServices] GetBooksListHandler handler)
        {
            var books = await handler.HandleAsync();
            return Ok(books);
        }

        [HttpPost]
        [ProducesResponseType(typeof(BookResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BookResponse>> Create(
            [FromBody] CreateBookRequest request,
            [FromServices] CreateBookHandler handler)
        {
            var response = await handler.HandleAsync(request);

            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(HttpValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateBookRequest request,
            [FromServices] UpdateBookHandler handler)
        {
            var isUpdated = await handler.HandleAsync(id, request);
            return isUpdated ? NoContent() : NotFound();
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(
            Guid id,
            [FromServices] DeleteBookHandler handler)
        {
            var isDeleted = await handler.HandleAsync(id);
            return isDeleted ? NoContent() : NotFound();
        }
    }
}
