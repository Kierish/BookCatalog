namespace BookCatalog.Api.Features.Loans.GetLoanHistory
{
    public sealed record GetLoanHistoryRequest(
        Guid? BookId = null,
        Guid? UserId = null,
        int PageNumber = 1,
        int PageSize = 10);
}
