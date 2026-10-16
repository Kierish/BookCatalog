using BookCatalog.Domain.Common;
using BookCatalog.Domain.Common.Queries;
using BookCatalog.Domain.Entities;

namespace BookCatalog.Domain.Interfaces
{
    public interface IBookRepository
    {
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<PagedResult<Book>> GetPagedAsync(BookQuery query, CancellationToken cancellationToken = default);
        Task<Book> AddAsync(Book book, CancellationToken cancellationToken = default);
        Task<bool> UpdateAsync(Book book, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
