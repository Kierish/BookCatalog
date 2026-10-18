using BookCatalog.Domain.Entities;

namespace BookCatalog.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
