using BookCatalog.Domain.Interfaces;

using BookCatalog.Domain.Common.Results;
using BookCatalog.Domain.Errors;

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

        public async Task<Result> HandleAsync(Guid id, UpdateBookRequest request)
        {
            var authorExists = await _authorRepository.ExistsAsync(request.AuthorId);

            if (!authorExists)
            {
                return Result.Failure(
                    AuthorErrors.NotFound(request.AuthorId));
            }

            var book = request.ToEntity(id);

            var isUpdated = await _bookRepository.UpdateAsync(book);

            if (!isUpdated)
            {
                _logger.LogWarning("Book update failed. Book with Id: {BookId} was not found.", id);
                return Result.Failure(
                    BookErrors.NotFound(id));
            }

            _logger.LogInformation(
                "Book updated successfully. Id: {BookId}, Title: {Title}, AuthorId: {AuthorId}, Isbn: {Isbn}, PublicationYear: {PublicationYear}"
                , book.Id, book.Title, book.AuthorId, book.Isbn, book.PublicationYear);

            return Result.Success();
        }
    }
}
