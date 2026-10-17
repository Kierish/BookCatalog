using BookCatalog.Domain.Common;
using BookCatalog.Domain.Common.Queries;
using BookCatalog.Domain.Entities;

namespace BookCatalog.Domain.Interfaces
{
    public interface ILoanRepository
    {
        Task<bool> HasLoanHistoryAsync(Guid bookId, CancellationToken cancellationToken = default);
        Task<bool> HasActiveLoanAsync(Guid bookId, CancellationToken cancellationToken = default);
        Task<bool> TryBorrowAsync(Loan loan, User user, CancellationToken cancellationToken = default);
        Task<Loan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task ReturnAsync(Loan loan, DateTimeOffset returnedAt, CancellationToken cancellationToken = default);
        Task<PagedResult<Loan>> GetPagedAsync(LoanQuery query, CancellationToken cancellationToken = default);
    }
}
