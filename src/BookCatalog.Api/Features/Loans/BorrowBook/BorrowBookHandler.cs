using BookCatalog.Domain.Common.Results;
using BookCatalog.Domain.Common.Results.Errors;
using BookCatalog.Domain.Interfaces;

namespace BookCatalog.Api.Features.Loans.BorrowBook
{
    public sealed class BorrowBookHandler
    {
        private readonly IBookRepository _bookRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILoanRepository _loanRepository;
        private readonly TimeProvider _timeProvider;
        private readonly ILogger<BorrowBookHandler> _logger;

        public BorrowBookHandler(
            IBookRepository bookRepository,
            IUserRepository userRepository,
            ILoanRepository loanRepository,
            TimeProvider timeProvider,
            ILogger<BorrowBookHandler> logger)
        {
            _bookRepository = bookRepository;
            _userRepository = userRepository;
            _loanRepository = loanRepository;
            _timeProvider = timeProvider;
            _logger = logger;
        }

        public async Task<Result<LoanResponse>> HandleAsync(
            BorrowBookRequest request)
        {
            if (!await _bookRepository.ExistsAsync(request.BookId))
            {
                _logger.LogWarning(
                    "Book borrowing rejected. BookId: {BookId} was not found.",
                    request.BookId);

                return Result<LoanResponse>.Failure(
                    BookErrors.NotFound(request.BookId));
            }

            if (!await _userRepository.ExistsAsync(request.UserId))
            {
                _logger.LogWarning(
                    "Book borrowing rejected. UserId: {UserId} was not found.",
                    request.UserId);

                return Result<LoanResponse>.Failure(
                    UserErrors.NotFound(request.UserId));
            }

            if (await _loanRepository.HasActiveLoanAsync(request.BookId))
            {
                _logger.LogInformation(
                    "Book borrowing rejected. BookId: {BookId} is already borrowed.",
                    request.BookId);

                return Result<LoanResponse>.Failure(
                    LoanErrors.BookAlreadyBorrowed(request.BookId));
            }

            var loan = request.ToEntity(_timeProvider.GetUtcNow());

            if (!await _loanRepository.TryBorrowAsync(loan))
            {
                _logger.LogWarning(
                    "Book borrowing rejected due to a concurrent request. BookId: {BookId} is already borrowed.",
                    request.BookId);

                return Result<LoanResponse>.Failure(
                    LoanErrors.BookAlreadyBorrowed(request.BookId));
            }

            _logger.LogInformation(
                "Book borrowed. LoanId: {LoanId}, BookId: {BookId}, UserId: {UserId}",
                loan.Id,
                loan.BookId,
                loan.UserId);

            return Result<LoanResponse>.Success(loan.ToResponse());
        }
    }
}
