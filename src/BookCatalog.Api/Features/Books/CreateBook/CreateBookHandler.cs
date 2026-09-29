using BookCatalog.Domain.Interfaces;

namespace BookCatalog.Api.Features.Books.CreateBook
{
    public sealed class CreateBookHandler
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILogger<CreateBookHandler> _logger;

        public CreateBookHandler(IBookRepository bookRepository, ILogger<CreateBookHandler> logger)
        {
            _bookRepository = bookRepository;
            _logger = logger;
        }

        public async Task<BookResponse> HandleAsync(CreateBookRequest request)
        {
            var book = request.ToEntity();

            await _bookRepository.AddAsync(book);

            _logger.LogInformation(
                "Book created successfully. Id: {BookId}, Title: {Title}, Author: {Author}, Isbn: {Isbn}, PublicationYear: {PublicationYear}"
                , book.Id, book.Title, book.Author, book.Isbn, book.PublicationYear);

            return book.ToResponse();
        }
    }
}
