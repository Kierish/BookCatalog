using BookCatalog.Domain.Common;
using BookCatalog.Domain.Entities;

namespace BookCatalog.Domain.Interfaces
{
    public interface IBookRepository
    {
        Task<Book?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<Book>> GetAllAsync();
        Task<PagedResult<Book>> GetPagedAsync(
            string? title,
            string? author,
            int? publicationYear,
            int page,
            int pageSize);
        Task<bool> AddAsync(Book book);
        Task<bool> UpdateAsync(Book updatedBook);
        Task<bool> DeleteAsync(Guid id);
    }
}
