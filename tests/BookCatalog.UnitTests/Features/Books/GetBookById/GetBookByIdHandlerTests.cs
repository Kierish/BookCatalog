using BookCatalog.Api.Features.Books;
using BookCatalog.Api.Features.Books.GetBookById;
using BookCatalog.Domain.Entities;
using BookCatalog.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace BookCatalog.UnitTests.Features.Books.GetBookById
{
    public sealed class GetBookByIdHandlerTests
    {
        private readonly IBookRepository _bookRepository = Substitute.For<IBookRepository>();
        private readonly ILogger<GetBookByIdHandler> _logger = Substitute.For<ILogger<GetBookByIdHandler>>();
        private readonly GetBookByIdHandler _handler;

        private readonly Author _author = new()
        {
            Id = Guid.CreateVersion7(),
            Name = "Robert C. Martin"
        };

        private Book CreateExistingBook() => new()
        {
            Id = Guid.CreateVersion7(),
            Title = "Clean Code",
            AuthorId = _author.Id,
            Author = _author,
            Isbn = "978-0132350884",
            PublicationYear = 2008
        };

        public GetBookByIdHandlerTests()
        {
            _handler = new GetBookByIdHandler(_bookRepository, _logger);
        }

        [Fact]
        public async Task HandleAsync_WhenBookExists_ShouldReturnMappedBookResponse()
        {
            var existingBook = CreateExistingBook();
            _bookRepository.GetByIdAsync(existingBook.Id).Returns(existingBook);

            var result = await _handler.HandleAsync(existingBook.Id);

            result.IsSuccess.ShouldBeTrue();
            result.Value.ShouldBe(existingBook.ToResponse());
        }

        [Fact]
        public async Task HandleAsync_WhenBookDoesNotExist_ShouldReturnFailure()
        {
            var nonExistentId = Guid.CreateVersion7();
            _bookRepository.GetByIdAsync(nonExistentId).Returns((Book?)null);

            var result = await _handler.HandleAsync(nonExistentId);

            result.IsFailure.ShouldBeTrue();
            result.Error.Code.ShouldBe("Books.NotFound");
        }
    }
}
