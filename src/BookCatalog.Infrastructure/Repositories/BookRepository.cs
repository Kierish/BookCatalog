using BookCatalog.Domain.Common;
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

        public async Task<PagedResult<Book>> GetPagedAsync(
            string? title,
            string? author,
            int? publicationYear,
            int pageNumber,
            int pageSize)
        {
            IQueryable<Book> query = _dbContext.Books
                .AsNoTracking()
                .Include(book => book.Author);

            if (!string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(book =>
                    book.Title == title);
            }

            if (!string.IsNullOrWhiteSpace(author))
            {
                query = query.Where(book =>
                    book.Author.Name == author);
            }

            if (publicationYear.HasValue)
            {
                query = query.Where(book =>
                    book.PublicationYear == publicationYear.Value);
            }

            var totalCount = await query.CountAsync();

            if (totalCount == 0 || (pageNumber - 1) * pageSize >= totalCount)
            {
                return new PagedResult<Book>([], totalCount, pageNumber, pageSize);
            }

            var books = await query
                .OrderBy(book => book.Title)
                .ThenBy(book => book.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Book>(books, totalCount, pageNumber, pageSize);
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
