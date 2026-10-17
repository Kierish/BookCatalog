using BookCatalog.Domain.Interfaces;
using BookCatalog.Domain.Common.Results;
using BookCatalog.Domain.Common.Results.Errors;

namespace BookCatalog.Api.Features.Books.DeleteBook
{
    public sealed class DeleteBookHandler
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly ILogger<DeleteBookHandler> _logger;

        public DeleteBookHandler(
            IBookRepository bookRepository,
            ILoanRepository loanRepository,
            ILogger<DeleteBookHandler> logger)
        {
            _bookRepository = bookRepository;
            _loanRepository = loanRepository;
            _logger = logger;
        }

        public async Task<Result> HandleAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            if (await _loanRepository.HasLoanHistoryAsync(id, cancellationToken))
            {
                _logger.LogInformation(
                    "Book deletion rejected. BookId: {BookId} has loan history.",
                    id);

                return Result.Failure(
                    BookErrors.HasLoanHistory(id));
            }

            var isDeleted = await _bookRepository.DeleteAsync(id, cancellationToken);
            
            if (!isDeleted)
            {
                _logger.LogInformation("Book deletion rejected. BookId: {BookId} was not found.", id);

                return Result.Failure(
                    BookErrors.NotFound(id));
            }

            _logger.LogInformation("Book deleted. BookId: {BookId}", id);

            return Result.Success();
        }
    }
}
