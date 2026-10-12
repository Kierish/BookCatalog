namespace BookCatalog.Api.Features.Loans
{
    public sealed record LoanResponse(
        Guid Id,
        Guid BookId,
        Guid UserId,
        DateTimeOffset BorrowedAt,
        DateTimeOffset? ReturnedAt
    );
}
