using BookCatalog.Domain.Common.Results;
using BookCatalog.Domain.Common.Results.Errors;
using BookCatalog.Domain.Interfaces;

namespace BookCatalog.Api.Features.Loans.ReturnBook
{
    public sealed class ReturnBookHandler
    {
        private readonly ILoanRepository _loanRepository;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<ReturnBookHandler> _logger;

        public ReturnBookHandler(
            ILoanRepository loanRepository,
            TimeProvider timeProvider,
            ILogger<ReturnBookHandler> logger)
        {
            _loanRepository = loanRepository;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        public async Task<Result> HandleAsync(Guid loanId)
        {
            var loan = await _loanRepository.GetByIdAsync(loanId);

            if (loan is null)
            {
                _logger.LogInformation(
                    "Book return rejected. LoanId: {LoanId} was not found.",
                    loanId);

                return Result.Failure(
                    LoanErrors.NotFound(loanId));
            }

            if (loan.ReturnedAt.HasValue)
            {
                _logger.LogInformation(
                    "Book return rejected. LoanId: {LoanId} was already returned.",
                    loanId);

                return Result.Failure(
                    LoanErrors.AlreadyReturned(loanId));
            }

            var returnedAt = _timeProvider.GetUtcNow();

            await _loanRepository.ReturnAsync(
                loan,
                returnedAt);

            _logger.LogInformation(
                "Book returned. LoanId: {LoanId}, BookId: {BookId}, UserId: {UserId}",
                loan.Id,
                loan.BookId,
                loan.UserId);

            return Result.Success();
        }
    }
}
