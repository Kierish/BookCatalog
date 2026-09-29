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

        public async Task<IReadOnlyList<BookResponse>> HandleAsync()
        {
            var books = await _bookRepository.GetAllAsync();

            return books
                .Select(b => b.ToResponse())
                .ToList();
        }
    }
}
