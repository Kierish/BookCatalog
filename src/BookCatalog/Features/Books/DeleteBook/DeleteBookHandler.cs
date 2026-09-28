using BookCatalog.Infrastructure.Storage;

namespace BookCatalog.Features.Books.DeleteBook
{
    public sealed class DeleteBookHandler
    {
        private readonly InMemoryBookStore _store;
        private readonly ILogger<DeleteBookHandler> _logger;

        public DeleteBookHandler(InMemoryBookStore store, ILogger<DeleteBookHandler> logger)
        {
            _store = store;
            _logger = logger;
        }

        public async Task<bool> HandleAsync(Guid id)
        {
            var isDeleted = await _store.DeleteAsync(id);
            
            if (!isDeleted)
            {
                _logger.LogWarning("Book deletion failed. Book with Id: {BookId} was not found.", id);
                return false;
            }

            _logger.LogInformation("Book deleted successfully. Id: {BookId}", id);
            return true;
        }
    }
}
