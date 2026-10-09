using BookCatalog.Domain.Entities;
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

        public DbSet<Book> Books => Set<Book>();

        public DbSet<Author> Authors => Set<Author>();

        public DbSet<User> Users => Set<User>();

        public DbSet<Loan> Loans => Set<Loan>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(BookCatalogDbContext).Assembly);
        }
    }
}
