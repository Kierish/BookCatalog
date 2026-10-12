using FluentValidation;

namespace BookCatalog.Api.Features.Loans.BorrowBook
{
    public sealed class BorrowBookValidator : AbstractValidator<BorrowBookRequest>
    {
        public BorrowBookValidator()
        {
            RuleFor(request => request.BookId)
                .NotEmpty()
                .WithMessage("BookId is required.");

            RuleFor(request => request.UserId)
                .NotEmpty()
                .WithMessage("UserId is required.");
        }
    }
}
