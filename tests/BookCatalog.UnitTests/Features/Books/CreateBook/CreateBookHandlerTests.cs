using BookCatalog.Api.Features.Books.CreateBook;
using BookCatalog.Domain.Entities;
using BookCatalog.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace BookCatalog.UnitTests.Features.Books.CreateBook
{
    public sealed class CreateBookHandlerTests
    {
        private readonly IBookRepository _bookRepository = Substitute.For<IBookRepository>();
        private readonly ILogger<CreateBookHandler> _logger = Substitute.For<ILogger<CreateBookHandler>>();
        private readonly CreateBookHandler _handler;
        private readonly Guid _authorId = Guid.CreateVersion7();

        private CreateBookRequest BaseRequest => new(
            "Clean Architecture",
            _authorId,
            null,
            2017);

        public CreateBookHandlerTests()
        {
            _handler = new CreateBookHandler(_bookRepository, _logger);
        }

        [Fact]
        public async Task HandleAsync_WhenRequestIsValid_ShouldAddBookWithIdReturnedInResponse()
        {
            _bookRepository
                .AddAsync(Arg.Any<Book>())
                .Returns(callInfo =>
                {
                    var book = callInfo.Arg<Book>();
                    book.Author = new Author
                    {
                        Id = book.AuthorId,
                        Name = "Robert C. Martin"
                    };

                    return Task.FromResult(book);
                });

            var response = await _handler.HandleAsync(BaseRequest);

            response.Id.ShouldNotBe(Guid.Empty);

            await _bookRepository.Received(1).AddAsync(
                Arg.Is<Book>(book =>
                    book.Id == response.Id &&
                    book.AuthorId == _authorId));
        }
    }
}
