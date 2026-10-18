using BookCatalog.Domain.Common;
using BookCatalog.Domain.Common.Queries;
using BookCatalog.Domain.Entities;
using BookCatalog.Domain.Interfaces;
using BookCatalog.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BookCatalog.Infrastructure.Repositories
{
    public sealed class BookRepository : IBookRepository
    {
        private readonly BookCatalogDbContext _dbContext;

        public BookRepository(BookCatalogDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _dbContext.Books.AnyAsync(book => book.Id == id, cancellationToken);
        }

        public Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return _dbContext.Books
                .AsNoTracking()
                .Include(book => book.Author)
                .Include(book => book.Loans.Where(loan => loan.ReturnedAt == null))
                .SingleOrDefaultAsync(book => book.Id == id, cancellationToken);
        }

        public async Task<Page<Book>> GetPagedAsync(
            BookQuery query,
            CancellationToken cancellationToken = default)
        {
            IQueryable<Book> booksQuery = _dbContext.Books
                .AsNoTracking()
                .Include(book => book.Author)
                .Include(book => book.Loans.Where(loan => loan.ReturnedAt == null));

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.Trim();
                var escapedSearch = EscapeLikePattern(search);
                var pattern = $"%{escapedSearch}%";

                booksQuery = booksQuery.Where(book =>
                    EF.Functions.ILike(book.Title, pattern, "\\") ||
                    EF.Functions.ILike(book.Author.Name, pattern, "\\"));
            }

            if (query.AuthorId.HasValue)
            {
                booksQuery = booksQuery.Where(book =>
                    book.AuthorId == query.AuthorId.Value);
            }

            if (query.PublicationYear.HasValue)
            {
                booksQuery = booksQuery.Where(book =>
                    book.PublicationYear == query.PublicationYear.Value);
            }

            var totalCount = await booksQuery.CountAsync(cancellationToken);
            var itemsToSkip = (long)(query.PageNumber - 1) * query.PageSize;

            if (totalCount == 0 || itemsToSkip >= totalCount)
            {
                return new Page<Book>([], totalCount, query.PageNumber, query.PageSize);
            }

            var books = await booksQuery
                .OrderBy(book => book.Title)
                .ThenBy(book => book.Id)
                .Skip((int)itemsToSkip)
                .Take(query.PageSize)
                .ToListAsync(cancellationToken);

            return new Page<Book>(books, totalCount, query.PageNumber, query.PageSize);
        }

        private static string EscapeLikePattern(string value)
        {
            return value
                .Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_");
        }

        public async Task<Book> AddAsync(Book book, CancellationToken cancellationToken = default)
        {
            _dbContext.Books.Add(book);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _dbContext.Entry(book)
                .Reference(book => book.Author)
                .LoadAsync(cancellationToken);

            return book;
        }

        public async Task<bool> UpdateAsync(Book book, CancellationToken cancellationToken = default)
        {
            var updatedCount = await _dbContext.Books
                .Where(existingBook => existingBook.Id == book.Id)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(existingBook => existingBook.Title, book.Title)
                        .SetProperty(existingBook => existingBook.AuthorId, book.AuthorId)
                        .SetProperty(existingBook => existingBook.Isbn, book.Isbn)
                        .SetProperty(
                            existingBook => existingBook.PublicationYear,
                            book.PublicationYear),
                    cancellationToken);

            return updatedCount > 0;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var deletedCount = await _dbContext.Books
                .Where(book => book.Id == id)
                .ExecuteDeleteAsync(cancellationToken);

            return deletedCount > 0;
        }
    }
}
