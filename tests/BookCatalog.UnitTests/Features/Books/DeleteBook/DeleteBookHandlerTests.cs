using BookCatalog.Api.Features.Books.DeleteBook;
using BookCatalog.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace BookCatalog.UnitTests.Features.Books.DeleteBook
{
    public sealed class DeleteBookHandlerTests
    {
        private readonly IBookRepository _bookRepository = Substitute.For<IBookRepository>();
        private readonly ILogger<DeleteBookHandler> _logger = Substitute.For<ILogger<DeleteBookHandler>>();
        private readonly DeleteBookHandler _handler;

        public DeleteBookHandlerTests()
        {
            _handler = new DeleteBookHandler(_bookRepository, _logger);
        }

        [Fact]
        public async Task HandleAsync_WhenBookExists_ShouldDeleteBook()
        {
            var bookId = Guid.CreateVersion7();
            _bookRepository.DeleteAsync(bookId).Returns(true);

            var result = await _handler.HandleAsync(bookId);

            result.IsSuccess.ShouldBeTrue();
            await _bookRepository.Received(1).DeleteAsync(bookId);
        }

        [Fact]
        public async Task HandleAsync_WhenBookDoesNotExist_ShouldReturnFailure()
        {
            var nonExistentId = Guid.CreateVersion7();
            _bookRepository.DeleteAsync(nonExistentId).Returns(false);

            var result = await _handler.HandleAsync(nonExistentId);

            result.IsFailure.ShouldBeTrue();
            result.Error.Code.ShouldBe("Books.NotFound");
        }
    }
}
