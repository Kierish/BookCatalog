using BookCatalog.Domain.Interfaces;

namespace BookCatalog.Api.Features.Books.DeleteBook
{
    public sealed class DeleteBookHandler
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILogger<DeleteBookHandler> _logger;

        public DeleteBookHandler(IBookRepository bookRepository, ILogger<DeleteBookHandler> logger)
        {
            _bookRepository = bookRepository;
            _logger = logger;
        }

        public async Task<bool> HandleAsync(Guid id)
        {
            var isDeleted = await _bookRepository.DeleteAsync(id);
            
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
