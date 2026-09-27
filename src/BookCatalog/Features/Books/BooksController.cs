using BookCatalog.Features.Books.CreateBook;
using BookCatalog.Features.Books.DeleteBook;
using BookCatalog.Features.Books.GetBookById;
using BookCatalog.Features.Books.GetBooksList;
using BookCatalog.Features.Books.UpdateBook;
using Microsoft.AspNetCore.Mvc;

namespace BookCatalog.Features.Books
{
    [ApiController]
    [Route("api/books")]
    public sealed class BooksController : ControllerBase
    {
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(
            Guid id,
            [FromServices] GetBookByIdHandler handler)
        {
            var book = await handler.HandleAsync(id);
            return book is not null ? Ok(book) : NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromServices] GetBooksListHandler handler)
        {
            var books = await handler.HandleAsync();
            return Ok(books);
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateBookRequest request,
            [FromServices] CreateBookHandler handler)
        {
            var response = await handler.HandleAsync(request);

            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateBookRequest request,
            [FromServices] UpdateBookHandler handler)
        {
            var isUpdated = await handler.HandleAsync(id, request);
            return isUpdated ? NoContent() : NotFound();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(
            Guid id,
            [FromServices] DeleteBookHandler handler)
        {
            var isDeleted = await handler.HandleAsync(id);
            return isDeleted ? NoContent() : NotFound();
        }
    }
}
