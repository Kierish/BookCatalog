using BookCatalog.Domain.Interfaces;
using BookCatalog.Domain.Common.Results;
using BookCatalog.Domain.Common.Results.Errors;

namespace BookCatalog.Api.Features.Books.UpdateBook
{
    public sealed class UpdateBookHandler
    {
        private readonly IBookRepository _bookRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly ILogger<UpdateBookHandler> _logger;

        public UpdateBookHandler(
            IBookRepository bookRepository,
            IAuthorRepository authorRepository,
            ILogger<UpdateBookHandler> logger)
        {
            _bookRepository = bookRepository;
            _authorRepository = authorRepository;
            _logger = logger;
        }

        public async Task<Result> HandleAsync(
            Guid id,
            UpdateBookRequest request,
            CancellationToken cancellationToken = default)
        {
            var authorExists = await _authorRepository.ExistsAsync(request.AuthorId, cancellationToken);

            if (!authorExists)
            {
                _logger.LogWarning(
                    "Book update rejected. AuthorId: {AuthorId} was not found. BookId: {BookId}", request.AuthorId, id);

                return Result.Failure(
                    AuthorErrors.NotFound(request.AuthorId));
            }

            var book = request.ToEntity(id);

            var isUpdated = await _bookRepository.UpdateAsync(book, cancellationToken);

            if (!isUpdated)
            {
                _logger.LogInformation("Book update rejected. BookId: {BookId} was not found.", id);

                return Result.Failure(
                    BookErrors.NotFound(id));
            }

            _logger.LogInformation(
                "Book updated. BookId: {BookId}, AuthorId: {AuthorId}", book.Id, book.AuthorId);

            return Result.Success();
        }
    }
}
