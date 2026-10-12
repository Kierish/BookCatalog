using BookCatalog.Domain.Entities;

namespace BookCatalog.Domain.Interfaces
{
    public interface ILoanRepository
    {
        Task<bool> HasActiveLoanAsync(Guid bookId);
        Task<bool> TryBorrowAsync(Loan loan);
        Task<Loan?> GetByIdAsync(Guid id);
        Task ReturnAsync(Loan loan, DateTimeOffset returnedAt);
    }
}
