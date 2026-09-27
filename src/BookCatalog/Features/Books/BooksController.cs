using BookCatalog.Features.Books.CreateBook;
using Microsoft.AspNetCore.Mvc;

namespace BookCatalog.Features.Books
{
    [ApiController]
    [Route("api/books")]
    public sealed class BooksController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateBookRequest request,
            [FromServices] CreateBookHandler handler)
        {
            var response = await handler.HandleAsync(request);

            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        [HttpGet("{id:guid}")]
        public IActionResult GetById(Guid id) => Ok();
    }
}
