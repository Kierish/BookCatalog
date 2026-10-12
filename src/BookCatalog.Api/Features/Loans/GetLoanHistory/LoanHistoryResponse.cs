namespace BookCatalog.Api.Features.Loans.GetLoanHistory
{
    public sealed record LoanHistoryResponse(
        Guid Id,
        Guid BookId,
        string BookTitle,
        Guid UserId,
        string UserName,
        DateTimeOffset BorrowedAt,
        DateTimeOffset? ReturnedAt);
}
