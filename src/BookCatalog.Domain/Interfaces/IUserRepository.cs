namespace BookCatalog.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
