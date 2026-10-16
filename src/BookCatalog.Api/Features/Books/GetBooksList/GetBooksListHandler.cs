using BookCatalog.Domain.Common;
using BookCatalog.Domain.Common.Queries;
using BookCatalog.Domain.Interfaces;

namespace BookCatalog.Api.Features.Books.GetBooksList
{
    public sealed class GetBooksListHandler
    {
        private readonly IBookRepository _bookRepository;

        public GetBooksListHandler(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<PagedResult<BookResponse>> HandleAsync(
            GetBooksListRequest request,
            CancellationToken cancellationToken = default)
        {
            var query = new BookQuery(
                request.Search,
                request.AuthorId,
                request.PublicationYear,
                request.PageNumber,
                request.PageSize);

            var pagedBooks = await _bookRepository.GetPagedAsync(query, cancellationToken);

            return pagedBooks.Map(b => b.ToResponse());
        }
    }
}
