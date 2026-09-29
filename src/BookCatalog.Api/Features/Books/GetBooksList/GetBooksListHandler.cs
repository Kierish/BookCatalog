using BookCatalog.Infrastructure.Storage;

namespace BookCatalog.Features.Books.GetBooksList
{
    public sealed class GetBooksListHandler
    {
        private readonly InMemoryBookStore _store;

        public GetBooksListHandler(InMemoryBookStore store)
        {
            _store = store;
        }

        public async Task<IReadOnlyList<BookResponse>> HandleAsync()
        {
            var books = await _store.GetAllAsync();

            return books
                .Select(b => new BookResponse(b.Id, b.Title, b.Author, b.Isbn, b.PublicationYear))
                .ToList();
        }
    }
}
