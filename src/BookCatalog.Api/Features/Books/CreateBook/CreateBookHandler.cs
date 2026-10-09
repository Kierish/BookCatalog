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

            var createdBook = await _bookRepository.AddAsync(book);

            _logger.LogInformation(
                "Book created successfully. Id: {BookId}, Title: {Title}, AuthorId: {AuthorId}, Isbn: {Isbn}, PublicationYear: {PublicationYear}"
                , createdBook.Id, createdBook.Title, createdBook.AuthorId, createdBook.Isbn, createdBook.PublicationYear);

            return createdBook.ToResponse();
        }
    }
}
