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

        private readonly Book _existingBook = new()
        {
            Id = Guid.CreateVersion7(),
            Title = "Clean Code",
            Author = "Robert C. Martin",
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
            _bookRepository.GetByIdAsync(_existingBook.Id).Returns(_existingBook);

            var response = await _handler.HandleAsync(_existingBook.Id);

            response.ShouldBe(_existingBook.ToResponse());
        }

        [Fact]
        public async Task HandleAsync_WhenBookDoesNotExist_ShouldReturnNull()
        {
            var nonExistentId = Guid.CreateVersion7();
            _bookRepository.GetByIdAsync(nonExistentId).Returns((Book?)null);

            var response = await _handler.HandleAsync(nonExistentId);

            response.ShouldBeNull();
        }
    }
}