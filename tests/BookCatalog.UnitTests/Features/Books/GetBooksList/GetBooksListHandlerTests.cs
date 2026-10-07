using BookCatalog.Api.Features.Books;
using BookCatalog.Api.Features.Books.GetBooksList;
using BookCatalog.Domain.Common;
using BookCatalog.Domain.Entities;
using BookCatalog.Domain.Interfaces;
using NSubstitute;
using Shouldly;

namespace BookCatalog.UnitTests.Features.Books.GetBooksList
{
    public sealed class GetBooksListHandlerTests
    {
        private readonly IBookRepository _bookRepository = Substitute.For<IBookRepository>();
        private readonly GetBooksListHandler _handler;

        public GetBooksListHandlerTests()
        {
            _handler = new GetBooksListHandler(_bookRepository);
        }

        [Fact]
        public async Task HandleAsync_WhenCalled_ShouldPassRequestParametersToRepository()
        {
            var request = new GetBooksListRequest("Clean", "Martin", 2008, PageNumber: 2, PageSize: 25);
            SetupRepository(new PagedResult<Book>([], TotalCount: 0, PageNumber: 1, PageSize: 10));

            await _handler.HandleAsync(request);

            await _bookRepository.Received(1).GetPagedAsync("Clean", "Martin", 2008, 2, 25);
        }

        [Fact]
        public async Task HandleAsync_WhenRepositoryReturnsBooks_ShouldMapToResponse()
        {
            var books = new List<Book>
            {
                new() { Id = Guid.CreateVersion7(), Title = "Refactoring", Author = "Martin Fowler", PublicationYear = 2018 },
                new() { Id = Guid.CreateVersion7(), Title = "Clean Code", Author = "Robert C. Martin", PublicationYear = 2008 }
            };

            SetupRepository(new PagedResult<Book>(books, TotalCount: 2, PageNumber: 1, PageSize: 10));

            var result = await _handler.HandleAsync(new GetBooksListRequest());

            result.Items.ShouldBe(books.Select(b => b.ToResponse()));
        }

        private void SetupRepository(PagedResult<Book> page)
        {
            _bookRepository
                .GetPagedAsync(
                    Arg.Any<string?>(),
                    Arg.Any<string?>(),
                    Arg.Any<int?>(),
                    Arg.Any<int>(),
                    Arg.Any<int>())
                .Returns(page);
        }
    }
}