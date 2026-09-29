using BookCatalog.Domain.Interfaces;
using BookCatalog.Infrastructure.Storage;

namespace BookCatalog.Features.Books.GetBooksList
{
    public sealed class GetBooksListHandler
    {
        private readonly IBookRepository _bookRepository;

        public GetBooksListHandler(IBookRepository store)
        {
            _bookRepository = store;
        }

        public async Task<IReadOnlyList<BookResponse>> HandleAsync()
        {
            var books = await _bookRepository.GetAllAsync();

            return books
                .Select(b => new BookResponse(b.Id, b.Title, b.Author, b.Isbn, b.PublicationYear))
                .ToList();
        }
    }
}
