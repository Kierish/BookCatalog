using BookCatalog.Domain.Interfaces;
using BookCatalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookCatalog.Infrastructure.Repositories
{
    public sealed class UserRepository : IUserRepository
    {
        private readonly BookCatalogDbContext _dbContext;

        public UserRepository(BookCatalogDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _dbContext.Users.AnyAsync(user => user.Id == id, cancellationToken);
        }
    }
}
