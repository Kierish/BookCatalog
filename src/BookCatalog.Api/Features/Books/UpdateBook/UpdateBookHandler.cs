using BookCatalog.Domain.Interfaces;

namespace BookCatalog.Api.Features.Books.UpdateBook
{
    public sealed class UpdateBookHandler
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILogger<UpdateBookHandler> _logger;

        public UpdateBookHandler(IBookRepository bookRepository, ILogger<UpdateBookHandler> logger)
        {
            _bookRepository = bookRepository;
            _logger = logger;
        }

        public async Task<bool> HandleAsync(Guid id, UpdateBookRequest request)
        {
            var book = request.ToEntity(id);

            var isUpdated = await _bookRepository.UpdateAsync(book);

            if (!isUpdated)
            {
                _logger.LogWarning("Book update failed. Book with Id: {BookId} was not found.", id);
                return false;
            }

            _logger.LogInformation(
                "Book updated successfully. Id: {BookId}, Title: {Title}, AuthorId: {AuthorId}, Isbn: {Isbn}, PublicationYear: {PublicationYear}"
                , book.Id, book.Title, book.AuthorId, book.Isbn, book.PublicationYear);

            return true;
        }
    }
}
