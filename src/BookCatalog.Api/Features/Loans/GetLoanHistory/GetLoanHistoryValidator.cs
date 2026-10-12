using FluentValidation;

namespace BookCatalog.Api.Features.Loans.GetLoanHistory
{
    public sealed class GetLoanHistoryValidator : AbstractValidator<GetLoanHistoryRequest>
    {
        public GetLoanHistoryValidator()
        {
            RuleFor(request => request.BookId)
                .NotEqual(Guid.Empty)
                .When(request => request.BookId.HasValue)
                .WithMessage("BookId must not be empty.");

            RuleFor(request => request.UserId)
                .NotEqual(Guid.Empty)
                .When(request => request.UserId.HasValue)
                .WithMessage("UserId must not be empty.");

            RuleFor(request => request.PageNumber)
                .GreaterThanOrEqualTo(1)
                .WithMessage("PageNumber must be at least 1.");

            RuleFor(request => request.PageSize)
                .InclusiveBetween(1, 50)
                .WithMessage("PageSize must be between 1 and 50.");
        }
    }
}
