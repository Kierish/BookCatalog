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

        public async Task<PagedResult<BookResponse>> HandleAsync(GetBooksListRequest request)
        {
            var pagedBooks = await _bookRepository.GetPagedAsync(
                request.Title,
                request.Author,
                request.PublicationYear,
                request.PageNumber,
                request.PageSize);

            return pagedBooks.Map(b => b.ToResponse());
        }
    }
}
