using BookCatalog.Domain.Common;
using BookCatalog.Domain.Entities;
using BookCatalog.Domain.Interfaces;
using System.Collections.Concurrent;

namespace BookCatalog.Infrastructure.Repositories
{
    public sealed class InMemoryBookRepository : IBookRepository
    {
        private readonly ConcurrentDictionary<Guid, Book> _books = new();

        public Task<Book?> GetByIdAsync(Guid id)
        {
            _books.TryGetValue(id, out var book);
            return Task.FromResult(book);
        }

        public Task<PagedResult<Book>> GetPagedAsync(
            string? title,
            string? author,
            int? publicationYear,
            int page,
            int pageSize)
        {
            IEnumerable<Book> query = _books.Values;

            if (!string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(b => b.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(author))
            {
                query = query.Where(b => b.Author.Contains(author, StringComparison.OrdinalIgnoreCase));
            }

            if (publicationYear.HasValue)
            {
                query = query.Where(b => b.PublicationYear == publicationYear.Value);
            }

            var totalCount = query.Count();

            if (totalCount == 0 || (page - 1) * pageSize >= totalCount)
            {
                return Task.FromResult(new PagedResult<Book>([], totalCount, page, pageSize));
            }

            var items = query
                .OrderBy(b => b.Title)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var result = new PagedResult<Book>(items, totalCount, page, pageSize);
            return Task.FromResult(result);
        }

        public Task<bool> AddAsync(Book book)
        {
            var added = _books.TryAdd(book.Id, book);
            return Task.FromResult(added);
        }

        public Task<bool> UpdateAsync(Book updatedBook)
        {
            if (!_books.TryGetValue(updatedBook.Id, out var existing))
                return Task.FromResult(false);

            var updated = _books.TryUpdate(updatedBook.Id, updatedBook, existing);
            return Task.FromResult(updated);
        }

        public Task<bool> DeleteAsync(Guid id)
        {
            var removed = _books.TryRemove(id, out _);
            return Task.FromResult(removed);
        }
    }
}
