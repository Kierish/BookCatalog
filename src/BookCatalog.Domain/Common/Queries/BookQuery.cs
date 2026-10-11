namespace BookCatalog.Domain.Common.Queries
{
    public sealed record BookQuery(
        string? Search,
        Guid? AuthorId,
        int? PublicationYear,
        int PageNumber,
        int PageSize);
}
