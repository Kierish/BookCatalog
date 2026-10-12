using BookCatalog.Domain.Common;
using BookCatalog.Domain.Common.Queries;
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

        public Task<Loan?> GetByIdAsync(Guid id)
        {
            return _dbContext.Loans
                .SingleOrDefaultAsync(loan => loan.Id == id);
        }

        public async Task<PagedResult<Loan>> GetPagedAsync(LoanQuery query)
        {
            IQueryable<Loan> loansQuery = _dbContext.Loans
                .AsNoTracking()
                .Include(loan => loan.Book)
                .Include(loan => loan.User);

            if (query.BookId.HasValue)
            {
                loansQuery = loansQuery.Where(
                    loan => loan.BookId == query.BookId.Value);
            }

            if (query.UserId.HasValue)
            {
                loansQuery = loansQuery.Where(
                    loan => loan.UserId == query.UserId.Value);
            }

            var totalCount = await loansQuery.CountAsync();
            var itemsToSkip = (long)(query.PageNumber - 1) * query.PageSize;

            if (totalCount == 0 || itemsToSkip >= totalCount)
            {
                return new PagedResult<Loan>(
                    [],
                    totalCount,
                    query.PageNumber,
                    query.PageSize);
            }

            var loans = await loansQuery
                .OrderByDescending(loan => loan.BorrowedAt)
                .ThenByDescending(loan => loan.Id)
                .Skip((int)itemsToSkip)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<Loan>(
                loans,
                totalCount,
                query.PageNumber,
                query.PageSize);
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

        public async Task ReturnAsync(Loan loan, DateTimeOffset returnedAt)
        {
            loan.ReturnedAt = returnedAt;
            await _dbContext.SaveChangesAsync();
        }
    }
}
