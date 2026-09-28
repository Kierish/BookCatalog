using BookCatalog.Domain;
using BookCatalog.Infrastructure.Storage;

namespace BookCatalog.Features.Books.CreateBook
{
    public sealed class CreateBookHandler
    {
        private readonly InMemoryBookStore _store;
        private readonly ILogger<CreateBookHandler> _logger;

        public CreateBookHandler(InMemoryBookStore store, ILogger<CreateBookHandler> logger)
        {
            _store = store;
            _logger = logger;
        }

        public async Task<BookResponse> HandleAsync(CreateBookRequest request)
        {
            var book = new Book
            {
                Title = request.Title,
                Author = request.Author,
                Isbn = request.Isbn,
                PublicationYear = request.PublicationYear
            };

            await _store.AddAsync(book);

            _logger.LogInformation(
                "Book created successfully. Id: {BookId}, Title: {Title}, Author: {Author}, Isbn: {Isbn}, PublicationYear: {PublicationYear}"
                , book.Id, book.Title, book.Author, book.Isbn, book.PublicationYear);

            return new BookResponse(book.Id, book.Title, book.Author, book.Isbn, book.PublicationYear);
        }
    }
}
