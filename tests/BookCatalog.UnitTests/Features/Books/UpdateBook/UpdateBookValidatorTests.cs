using BookCatalog.Api.Features.Books.UpdateBook;
using FluentValidation.TestHelper;

namespace BookCatalog.UnitTests.Features.Books.UpdateBook
{
    public sealed class UpdateBookValidatorTests
    {
        private readonly UpdateBookValidator _validator = new();

        private static UpdateBookRequest CreateValidRequest() => new(
            Title: "Refactoring: Improving the Design of Existing Code",
            AuthorId: Guid.CreateVersion7(),
            Isbn: "978-0134757599",
            PublicationYear: 2018
        );

        [Fact]
        public void Validate_WhenRequestIsValid_ShouldNotHaveAnyValidationErrors()
        {
            var request = CreateValidRequest();

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        #region Title

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_WhenTitleIsNullOrWhiteSpace_ShouldHaveValidationError(string? title)
        {
            var request = CreateValidRequest() with { Title = title! };

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage("Title is required.");
        }

        [Fact]
        public void Validate_WhenTitleExceeds200Characters_ShouldHaveValidationError()
        {
            var request = CreateValidRequest() with { Title = new string('a', 201) };

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Title)
                  .WithErrorMessage("Title must be between 1 and 200 characters.");
        }

        #endregion

        #region AuthorId

        [Fact]
        public void Validate_WhenAuthorIdIsEmpty_ShouldHaveValidationError()
        {
            var request = CreateValidRequest() with { AuthorId = Guid.Empty };

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.AuthorId)
                  .WithErrorMessage("AuthorId is required.");
        }

        #endregion

        #region ISBN

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("978-0134757599")]
        [InlineData("12345678901234567")] 
        public void Validate_WhenIsbnIsOptionalOrWithin17Characters_ShouldNotHaveValidationError(string? isbn)
        {
            var request = CreateValidRequest() with { Isbn = isbn };

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(x => x.Isbn);
        }

        [Fact]
        public void Validate_WhenIsbnExceeds17Characters_ShouldHaveValidationError()
        {
            var request = CreateValidRequest() with { Isbn = new string('X', 18) };

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Isbn)
                  .WithErrorMessage("ISBN must not exceed 17 characters.");
        }

        #endregion

        #region PublicationYear

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WhenPublicationYearIsLessThanOne_ShouldHaveValidationError(int year)
        {
            var request = CreateValidRequest() with { PublicationYear = year };

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.PublicationYear);
        }

        [Fact]
        public void Validate_WhenPublicationYearIsInFuture_ShouldHaveValidationError()
        {
            var futureYear = DateTime.UtcNow.Year + 1;
            var request = CreateValidRequest() with { PublicationYear = futureYear };

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.PublicationYear);
        }

        [Theory]
        [MemberData(nameof(GetValidPublicationYears))]
        public void Validate_WhenPublicationYearIsWithinAllowedRange_ShouldNotHaveValidationError(int year)
        {
            var request = CreateValidRequest() with { PublicationYear = year };

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(x => x.PublicationYear);
        }

        public static TheoryData<int> GetValidPublicationYears() => new()
    {
        1,
        DateTime.UtcNow.Year
    };

        #endregion
    }
}
