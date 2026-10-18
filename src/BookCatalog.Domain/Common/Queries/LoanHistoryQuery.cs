namespace BookCatalog.Domain.Common.Queries
{
    public sealed record LoanHistoryQuery(
        Guid? BookId,
        Guid? UserId,
        int PageNumber,
        int PageSize);
}
