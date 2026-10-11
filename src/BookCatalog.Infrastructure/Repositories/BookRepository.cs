using BookCatalog.Domain.Common;
using BookCatalog.Domain.Common.Results.Queries;
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

        public Task<Book?> GetByIdAsync(Guid id)
        {
            return _dbContext.Books
                .AsNoTracking()
                .Include(book => book.Author)
                .SingleOrDefaultAsync(book => book.Id == id);
        }

        public async Task<PagedResult<Book>> GetPagedAsync(BookQuery query)
        {
            IQueryable<Book> booksQuery = _dbContext.Books
                .AsNoTracking()
                .Include(book => book.Author);

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

            var totalCount = await booksQuery.CountAsync();
            var itemsToSkip = (query.PageNumber - 1) * query.PageSize;

            if (totalCount == 0 || itemsToSkip >= totalCount)
            {
                return new PagedResult<Book>([], totalCount, query.PageNumber, query.PageSize);
            }

            var books = await booksQuery
                .OrderBy(book => book.Title)
                .ThenBy(book => book.Id)
                .Skip(itemsToSkip)
                .Take(query.PageSize)
                .ToListAsync();

            return new PagedResult<Book>(books, totalCount, query.PageNumber, query.PageSize);
        }

        private static string EscapeLikePattern(string value)
        {
            return value
                .Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_");
        }

        public async Task<Book> AddAsync(Book book)
        {
            _dbContext.Books.Add(book);

            await _dbContext.SaveChangesAsync();

            await _dbContext.Entry(book)
                .Reference(book => book.Author)
                .LoadAsync();

            return book;
        }

        public async Task<bool> UpdateAsync(Book book)
        {
            var existingBook = await _dbContext.Books
                .SingleOrDefaultAsync(existingBook => existingBook.Id == book.Id);

            if (existingBook is null)
            {
                return false;
            }

            existingBook.Title = book.Title;
            existingBook.AuthorId = book.AuthorId;
            existingBook.Isbn = book.Isbn;
            existingBook.PublicationYear = book.PublicationYear;

            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var book = await _dbContext.Books
                .SingleOrDefaultAsync(book => book.Id == id);

            if (book is null)
            {
                return false;
            }

            _dbContext.Books.Remove(book);
            await _dbContext.SaveChangesAsync();
            return true;
        }
    }
}
