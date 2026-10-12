namespace BookCatalog.Domain.Common.Queries
{
    public sealed record LoanQuery(
        Guid? BookId,
        Guid? UserId,
        int PageNumber,
        int PageSize);
}
