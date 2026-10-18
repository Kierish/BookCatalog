using BookCatalog.Domain.Entities;
using BookCatalog.Domain.Interfaces;
using BookCatalog.Infrastructure.Persistence;

namespace BookCatalog.Infrastructure.Repositories
{
    public sealed class UserRepository : IUserRepository
    {
        private readonly BookCatalogDbContext _dbContext;

        public UserRepository(BookCatalogDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users.FindAsync([id], cancellationToken);
        }
    }
}
