using BookCatalog.Domain;
using BookCatalog.Infrastructure.Storage;

namespace BookCatalog.Features.Books.UpdateBook
{
    public sealed class UpdateBookHandler
    {
        private readonly InMemoryBookStore _store;
        private readonly ILogger<UpdateBookHandler> _logger;

        public UpdateBookHandler(InMemoryBookStore store, ILogger<UpdateBookHandler> logger)
        {
            _store = store;
            _logger = logger;
        }

        public async Task<bool> HandleAsync(Guid id, UpdateBookRequest request)
        {
            var updatedBook = new Book
            {
                Id = id,
                Title = request.Title,
                Author = request.Author,
                Isbn = request.Isbn,
                PublicationYear = request.PublicationYear
            };

            var isUpdated = await _store.UpdateAsync(updatedBook);

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
