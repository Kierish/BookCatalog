using BookCatalog.Api.Features.Books.CreateBook;
using FluentValidation.TestHelper;

namespace BookCatalog.UnitTests.Features.Books.CreateBook
{
    public sealed class CreateBookValidatorTests
    {
        private readonly CreateBookValidator _validator = new();

        private static CreateBookRequest CreateValidRequest() => new(
            Title: "Clean Architecture",
            AuthorId: Guid.CreateVersion7(),
            Isbn: "978-0134494166",
            PublicationYear: 2017
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
        public void Validate_WhenTitleIsNullOrWhiteSpace_ShouldHaveValidationError(string? invalidTitle)
        {
            var request = CreateValidRequest() with { Title = invalidTitle! };

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
        [InlineData("978-0134494166")]
        [InlineData("9780134494166")]
        [InlineData("0-306-40615-2")]
        [InlineData("0-8044-2957-X")]
        public void Validate_WhenIsbnIsOptionalOrValid_ShouldNotHaveValidationError(string? isbn)
        {
            var request = CreateValidRequest() with { Isbn = isbn };

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveValidationErrorFor(x => x.Isbn);
        }

        [Theory]
        [InlineData("0-306-40615-3")]
        [InlineData("978-0134494165")]
        [InlineData("4006381333931")]
        [InlineData("978-01344A4166")]
        [InlineData("123456789")]
        public void Validate_WhenIsbnIsInvalid_ShouldHaveValidationError(string isbn)
        {
            var request = CreateValidRequest() with { Isbn = isbn };

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.Isbn)
                  .WithErrorMessage("ISBN must be a valid ISBN-10 or ISBN-13.");
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
        public void Validate_WhenPublicationYearIsWithinAllowedRange_ShouldNotHaveValidationError(int validYear)
        {
            var request = CreateValidRequest() with { PublicationYear = validYear };

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
