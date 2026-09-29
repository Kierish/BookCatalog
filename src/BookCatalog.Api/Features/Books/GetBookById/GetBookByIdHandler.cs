using BookCatalog.Domain.Interfaces;
using BookCatalog.Infrastructure.Storage;

namespace BookCatalog.Features.Books.GetBookById
{
    public sealed class GetBookByIdHandler
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILogger<GetBookByIdHandler> _logger;

        public GetBookByIdHandler(IBookRepository store, ILogger<GetBookByIdHandler> logger)
        {
            _bookRepository = store;
            _logger = logger;
        }

        public async Task<BookResponse?> HandleAsync(Guid id)
        {
            var book = await _bookRepository.GetByIdAsync(id);

            if (book is null)
            {
                _logger.LogWarning("Book with ID {Id} was not found", id);
                return null;
            }

            return new BookResponse(book.Id, book.Title, book.Author, book.Isbn, book.PublicationYear);
        }
    }
}
