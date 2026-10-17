using BookCatalog.Domain.Common;
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
            var pagedBooks = await _bookRepository.GetPagedAsync(
                request.ToQuery(),
                cancellationToken);

            return pagedBooks.Map(b => b.ToResponse());
        }
    }
}
