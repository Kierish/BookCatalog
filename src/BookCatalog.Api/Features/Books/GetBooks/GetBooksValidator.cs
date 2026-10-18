using FluentValidation;

namespace BookCatalog.Api.Features.Books.GetBooks
{
    public sealed class GetBooksValidator : AbstractValidator<GetBooksRequest>
    {
        public GetBooksValidator(TimeProvider timeProvider)
        {
            var currentYear = timeProvider.GetUtcNow().Year;

            RuleFor(request => request.Search)
                .MaximumLength(200)
                .When(request => !string.IsNullOrWhiteSpace(request.Search))
                .WithMessage("Search must not exceed 200 characters.");

            RuleFor(request => request.AuthorId)
                .NotEqual(Guid.Empty)
                .When(request => request.AuthorId.HasValue)
                .WithMessage("AuthorId must not be empty.");

            RuleFor(x => x.PageNumber)
                .GreaterThanOrEqualTo(1)
                .WithMessage("PageNumber must be at least 1.");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 50)
                .WithMessage("PageSize must be between 1 and 50.");

            RuleFor(x => x.PublicationYear)
                .InclusiveBetween(1, currentYear)
                .When(x => x.PublicationYear.HasValue)
                .WithMessage($"Publication year must be between 1 and {currentYear}.");
        }
    }
}
