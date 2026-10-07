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

        private readonly CreateBookRequest _baseRequest = new(
            "Clean Architecture",
            "Robert C. Martin",
            null,
            2017
        );

        public CreateBookHandlerTests()
        {
            _handler = new CreateBookHandler(_bookRepository, _logger);
        }

        [Fact]
        public async Task HandleAsync_WhenRequestIsValid_ShouldSaveBookWithIdReturnedInResponse()
        {
            var response = await _handler.HandleAsync(_baseRequest);

            response.Id.ShouldNotBe(Guid.Empty);
            await _bookRepository.Received(1).AddAsync(Arg.Is<Book>(b => b.Id == response.Id));
        }
    }
}