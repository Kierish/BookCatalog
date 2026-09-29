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

        public Task<IReadOnlyList<Book>> GetAllAsync()
        {
            return Task.FromResult<IReadOnlyList<Book>>(_books.Values.ToList());
        }

        public Task<bool> AddAsync(Book book)
        {
            var added = _books.TryAdd(book.Id, book);
            return Task.FromResult(added);
        }

        public Task<bool> UpdateAsync(Book updatedBook)
        {
            if (!_books.ContainsKey(updatedBook.Id))
                return Task.FromResult(false);

            _books[updatedBook.Id] = updatedBook;
            return Task.FromResult(true);
        }

        public Task<bool> DeleteAsync(Guid id)
        {
            var removed = _books.TryRemove(id, out _);
            return Task.FromResult(removed);
        }
    }
}
