namespace BookCatalog.Domain.Interfaces
{
    public interface IAuthorRepository
    {
        Task<bool> ExistsAsync(Guid id);
    }
}
