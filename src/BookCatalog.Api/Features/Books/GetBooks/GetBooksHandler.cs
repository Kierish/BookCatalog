using BookCatalog.Domain.Common;
using BookCatalog.Domain.Interfaces;

namespace BookCatalog.Api.Features.Books.GetBooks
{
    public sealed class GetBooksHandler
    {
        private readonly IBookRepository _bookRepository;

        public GetBooksHandler(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
        }

        public async Task<Page<BookResponse>> HandleAsync(
            GetBooksRequest request,
            CancellationToken cancellationToken = default)
        {
            var pagedBooks = await _bookRepository.GetPagedAsync(
                request.ToQuery(),
                cancellationToken);

            return pagedBooks.Map(b => b.ToResponse());
        }
    }
}
