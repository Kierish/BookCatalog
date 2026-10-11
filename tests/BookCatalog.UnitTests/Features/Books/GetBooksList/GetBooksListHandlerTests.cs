using BookCatalog.Api.Features.Books;
using BookCatalog.Api.Features.Books.GetBooksList;
using BookCatalog.Domain.Common;
using BookCatalog.Domain.Common.Results.Queries;
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
            var authorId = Guid.CreateVersion7();
            var request = new GetBooksListRequest(
                Search: "Clean",
                AuthorId: authorId,
                PublicationYear: 2008,
                PageNumber: 2,
                PageSize: 25);
            SetupRepository(new PagedResult<Book>([], TotalCount: 0, PageNumber: 1, PageSize: 10));

            await _handler.HandleAsync(request);

            await _bookRepository.Received(1).GetPagedAsync(
                Arg.Is<BookQuery>(query =>
                    query.Search == "Clean" &&
                    query.AuthorId == authorId &&
                    query.PublicationYear == 2008 &&
                    query.PageNumber == 2 &&
                    query.PageSize == 25));
        }

        [Fact]
        public async Task HandleAsync_WhenRepositoryReturnsBooks_ShouldMapToResponse()
        {
            var martinFowler = new Author { Id = Guid.CreateVersion7(), Name = "Martin Fowler" };
            var robertMartin = new Author { Id = Guid.CreateVersion7(), Name = "Robert C. Martin" };

            var books = new List<Book>
            {
                new() { Id = Guid.CreateVersion7(),
                    Title = "Refactoring",
                    AuthorId = martinFowler.Id,
                    Author = martinFowler,
                    PublicationYear = 2018
                },
                new()
                {
                    Id = Guid.CreateVersion7(),
                    Title = "Clean Code",
                    AuthorId = robertMartin.Id,
                    Author = robertMartin,
                    PublicationYear = 2008
                }
            };

            SetupRepository(new PagedResult<Book>(books, TotalCount: 2, PageNumber: 1, PageSize: 10));

            var result = await _handler.HandleAsync(new GetBooksListRequest());

            result.Items.ShouldBe(books.Select(b => b.ToResponse()));
        }

        private void SetupRepository(PagedResult<Book> page)
        {
            _bookRepository
                .GetPagedAsync(Arg.Any<BookQuery>())
                .Returns(page);
        }
    }
}
