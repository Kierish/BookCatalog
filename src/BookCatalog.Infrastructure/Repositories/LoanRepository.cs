using BookCatalog.Domain.Common;
using BookCatalog.Domain.Common.Queries;
using BookCatalog.Domain.Entities;
using BookCatalog.Domain.Interfaces;
using BookCatalog.Infrastructure.Persistence;
using BookCatalog.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BookCatalog.Infrastructure.Repositories
{
    public sealed class LoanRepository : ILoanRepository
    {
        private readonly BookCatalogDbContext _dbContext;

        public LoanRepository(BookCatalogDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<bool> HasActiveLoanAsync(Guid bookId, CancellationToken cancellationToken = default)
        {
            return _dbContext.Loans.AnyAsync(loan =>
                loan.BookId == bookId &&
                loan.ReturnedAt == null,
                cancellationToken);
        }

        public Task<Loan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _dbContext.Loans
                .SingleOrDefaultAsync(loan => loan.Id == id, cancellationToken);
        }

        public async Task<PagedResult<Loan>> GetPagedAsync(
            LoanQuery query,
            CancellationToken cancellationToken = default)
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

            var totalCount = await loansQuery.CountAsync(cancellationToken);
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
                .ToListAsync(cancellationToken);

            return new PagedResult<Loan>(
                loans,
                totalCount,
                query.PageNumber,
                query.PageSize);
        }

        public async Task<bool> TryBorrowAsync(Loan loan, CancellationToken cancellationToken = default)
        {
            var user = await _dbContext.Users.FindAsync([loan.UserId], cancellationToken)
                ?? throw new InvalidOperationException($"User {loan.UserId} not found for loan {loan.Id}.");

            user.ActiveLoanCount++;
            _dbContext.Loans.Add(loan);

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
                return true;
            }
            catch (DbUpdateException exception)
                when (exception.InnerException is PostgresException
                {
                    SqlState: PostgresErrorCodes.UniqueViolation,
                    ConstraintName: LoanConfiguration.ActiveLoanBookIndexName
                })
            {
                _dbContext.Entry(loan).State = EntityState.Detached;
                _dbContext.Entry(user).State = EntityState.Detached;

                return false;
            }
        }

        public async Task ReturnAsync(
            Loan loan,
            DateTimeOffset returnedAt,
            CancellationToken cancellationToken = default)
        {
            var user = await _dbContext.Users.FindAsync([loan.UserId], cancellationToken)
                ?? throw new InvalidOperationException($"User {loan.UserId} not found for loan {loan.Id}.");

            loan.ReturnedAt = returnedAt;
            user.ActiveLoanCount--;

            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
