using BookCatalog.Domain.Common;
using BookCatalog.Domain.Entities;

namespace BookCatalog.Domain.Interfaces
{
    public interface IBookRepository
    {
        Task<Book?> GetByIdAsync(Guid id);
        Task<PagedResult<Book>> GetPagedAsync(
            string? title,
            string? author,
            int? publicationYear,
            int pageNumber,
            int pageSize);
        Task<Book> AddAsync(Book book);
        Task<bool> UpdateAsync(Book book);
        Task<bool> DeleteAsync(Guid id);
    }
}
