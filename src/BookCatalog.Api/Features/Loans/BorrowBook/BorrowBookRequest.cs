namespace BookCatalog.Api.Features.Loans.BorrowBook
{
    public sealed record BorrowBookRequest(
        Guid BookId,
        Guid UserId
    );
}
