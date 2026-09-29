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
            var updatedBook = request.ToEntity(id);

            var isUpdated = await _bookRepository.UpdateAsync(updatedBook);

            if (!isUpdated)
            {
                _logger.LogWarning("Book update failed. Book with Id: {BookId} was not found.", id);
                return false;
            }

            _logger.LogInformation(
                "Book updated successfully. Id: {BookId}, Title: {Title}, Author: {Author}, Isbn: {Isbn}, PublicationYear: {PublicationYear}"
                , updatedBook.Id, updatedBook.Title, updatedBook.Author, updatedBook.Isbn, updatedBook.PublicationYear);

            return true;
        }
    }
}
