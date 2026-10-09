using BookCatalog.Api.Features.Books.UpdateBook;
using BookCatalog.Domain.Entities;
using BookCatalog.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace BookCatalog.UnitTests.Features.Books.UpdateBook
{
    public sealed class UpdateBookHandlerTests
    {
        private readonly IBookRepository _bookRepository = Substitute.For<IBookRepository>();
        private readonly ILogger<UpdateBookHandler> _logger = Substitute.For<ILogger<UpdateBookHandler>>();
        private readonly UpdateBookHandler _handler;

        private readonly UpdateBookRequest _baseRequest = new(
            "Refactoring (2nd Edition)",
            Guid.CreateVersion7(),
            null,
            2018
        );

        public UpdateBookHandlerTests()
        {
            _handler = new UpdateBookHandler(_bookRepository, _logger);
        }

        [Fact]
        public async Task HandleAsync_WhenBookExists_ShouldUpdateBookAndReturnTrue()
        {
            var bookId = Guid.CreateVersion7();
            _bookRepository.UpdateAsync(Arg.Any<Book>()).Returns(true);

            var result = await _handler.HandleAsync(bookId, _baseRequest);

            result.ShouldBeTrue();
            await _bookRepository.Received(1).UpdateAsync(Arg.Is<Book>(b => b.Id == bookId));
        }

        [Fact]
        public async Task HandleAsync_WhenBookDoesNotExist_ShouldReturnFalse()
        {
            var nonExistentId = Guid.CreateVersion7();
            _bookRepository.UpdateAsync(Arg.Any<Book>()).Returns(false);

            var result = await _handler.HandleAsync(nonExistentId, _baseRequest);

            result.ShouldBeFalse();
        }
    }
}
