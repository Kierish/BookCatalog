using System;
using System.Collections.Generic;
using System.Text;

namespace BookCatalog.Domain.Interfaces
{
    public interface IBookRepository
    {
        Task<Book?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<Book>> GetAllAsync();
        Task<bool> AddAsync(Book book);
        Task<bool> UpdateAsync(Book updatedBook);
        Task<bool> DeleteAsync(Guid id);
    }
}
