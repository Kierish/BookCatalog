using FluentValidation;

namespace BookCatalog.Features.Books.CreateBook
{
    public sealed class CreateBookValidator : AbstractValidator<CreateBookRequest>
    {
        public CreateBookValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                .WithMessage("Title is required.")
                .Length(1, 200)
                .WithMessage("Title must be between 1 and 200 characters.");

            RuleFor(x => x.Author)
                .NotEmpty()
                .WithMessage("Author is required.")
                .Length(1, 200)
                .WithMessage("Author must be between 1 and 200 characters.");

            RuleFor(x => x.Isbn)
                .MaximumLength(17)
                .WithMessage("ISBN must not exceed 17 characters.")
                .When(x => !string.IsNullOrWhiteSpace(x.Isbn));

            RuleFor(x => x.PublicationYear)
                .InclusiveBetween(1, DateTime.UtcNow.Year)
                .WithMessage($"Publication year must be between 1 and {DateTime.UtcNow.Year}.");
        }
    }
}
