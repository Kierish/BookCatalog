using Microsoft.EntityFrameworkCore;

namespace BookCatalog.Infrastructure.Persistence
{
    public sealed class BookCatalogDbContext : DbContext
    {
        public BookCatalogDbContext(
            DbContextOptions<BookCatalogDbContext> options)
            : base(options)
        {
        }
    }
}
