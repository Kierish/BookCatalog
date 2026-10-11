using BookCatalog.Domain.Interfaces;
using BookCatalog.Domain.Common.Results;
using BookCatalog.Domain.Common.Results.Errors;

namespace BookCatalog.Api.Features.Books.CreateBook
{
    public sealed class CreateBookHandler
    {
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly ILogger<CreateBookHandler> _logger;

        public CreateBookHandler(
            IBookRepository bookRepository,
            IAuthorRepository authorRepository,
            ILogger<CreateBookHandler> logger)
        {
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
            _logger = logger;
        }

        public async Task<Result<BookResponse>> HandleAsync(CreateBookRequest request)
        {
            var authorExists = await _authorRepository.ExistsAsync(request.AuthorId);

            if (!authorExists)
            {
                _logger.LogInformation(
                    "Book creation rejected. AuthorId: {AuthorId} was not found.", request.AuthorId);

                return Result<BookResponse>.Failure(
                    AuthorErrors.NotFound(request.AuthorId));
            }

            var book = request.ToEntity();

            var createdBook = await _bookRepository.AddAsync(book);

            _logger.LogInformation(
                "Book created. BookId: {BookId}, AuthorId: {AuthorId}", createdBook.Id, createdBook.AuthorId);

            return Result<BookResponse>.Success(createdBook.ToResponse());
        }
    }
}
