using BookCatalog.Api.Features.Loans.BorrowBook;
using BookCatalog.Domain.Entities;

namespace BookCatalog.Api.Features.Loans
{
    public static class LoanMappings
    {
        public static Loan ToEntity(this BorrowBookRequest request, DateTimeOffset borrowedAt)
        {
            return new Loan
            {
                BookId = request.BookId,
                UserId = request.UserId,
                BorrowedAt = borrowedAt
            };
        }

        public static LoanResponse ToResponse(this Loan loan)
        {
            return new LoanResponse(
                loan.Id,
                loan.BookId,
                loan.UserId,
                loan.BorrowedAt,
                loan.ReturnedAt
            );
        }
    }
}
