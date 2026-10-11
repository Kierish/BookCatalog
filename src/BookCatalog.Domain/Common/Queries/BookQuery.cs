namespace BookCatalog.Domain.Common.Results.Queries
{
    public sealed record BookQuery(
        string? Search,
        Guid? AuthorId,
        int? PublicationYear,
        int PageNumber,
        int PageSize);
}
