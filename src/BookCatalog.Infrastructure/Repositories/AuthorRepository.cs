using BookCatalog.Domain.Interfaces;
using BookCatalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookCatalog.Infrastructure.Repositories
{
    public sealed class AuthorRepository : IAuthorRepository
    {
        private readonly BookCatalogDbContext _dbContext;

        public AuthorRepository(BookCatalogDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<bool> ExistsAsync(Guid id)
        {
            return _dbContext.Authors.AnyAsync(author => author.Id == id);
        }
    }
}
