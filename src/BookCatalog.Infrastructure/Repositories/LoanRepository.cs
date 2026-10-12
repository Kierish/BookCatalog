using BookCatalog.Domain.Entities;
using BookCatalog.Domain.Interfaces;
using BookCatalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BookCatalog.Infrastructure.Repositories
{
    public sealed class LoanRepository : ILoanRepository
    {
        private const string ActiveLoanBookIndex = "IX_Loans_BookId";

        private readonly BookCatalogDbContext _dbContext;

        public LoanRepository(BookCatalogDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<bool> HasActiveLoanAsync(Guid bookId)
        {
            return _dbContext.Loans.AnyAsync(loan =>
                loan.BookId == bookId &&
                loan.ReturnedAt == null);
        }

        public async Task<bool> TryBorrowAsync(Loan loan)
        {
            _dbContext.Loans.Add(loan);

            try
            {
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException exception)
                when (exception.InnerException is PostgresException
                {
                    SqlState: PostgresErrorCodes.UniqueViolation,
                    ConstraintName: ActiveLoanBookIndex
                })
            {
                _dbContext.Entry(loan).State = EntityState.Detached;
                return false;
            }
        }
    }
}
