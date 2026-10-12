using BookCatalog.Api.Features.Loans.GetLoanHistory;
using FluentValidation.TestHelper;

namespace BookCatalog.UnitTests.Features.Loans.GetLoanHistory
{
    public sealed class GetLoanHistoryValidatorTests
    {
        private readonly GetLoanHistoryValidator _validator = new();

        [Fact]
        public void Validate_WhenRequestIsValid_ShouldNotHaveValidationErrors()
        {
            var request = new GetLoanHistoryRequest(
                BookId: Guid.CreateVersion7(),
                UserId: Guid.CreateVersion7(),
                PageNumber: 2,
                PageSize: 25);

            var result = _validator.TestValidate(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WhenFilterIdsAreEmpty_ShouldHaveValidationErrors()
        {
            var request = new GetLoanHistoryRequest(
                BookId: Guid.Empty,
                UserId: Guid.Empty);

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.BookId)
                .WithErrorMessage("BookId must not be empty.");
            result.ShouldHaveValidationErrorFor(x => x.UserId)
                .WithErrorMessage("UserId must not be empty.");
        }

        [Fact]
        public void Validate_WhenPaginationIsOutOfRange_ShouldHaveValidationErrors()
        {
            var request = new GetLoanHistoryRequest(
                PageNumber: 0,
                PageSize: 51);

            var result = _validator.TestValidate(request);

            result.ShouldHaveValidationErrorFor(x => x.PageNumber)
                .WithErrorMessage("PageNumber must be at least 1.");
            result.ShouldHaveValidationErrorFor(x => x.PageSize)
                .WithErrorMessage("PageSize must be between 1 and 50.");
        }
    }
}
