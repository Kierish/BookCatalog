using BookCatalog.Api.Features.Loans.ReturnBook;
using BookCatalog.Domain.Entities;
using BookCatalog.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace BookCatalog.UnitTests.Features.Loans.ReturnBook
{
    public sealed class ReturnBookHandlerTests
    {
        private readonly ILoanRepository _loanRepository = Substitute.For<ILoanRepository>();
        private readonly ILogger<ReturnBookHandler> _logger = Substitute.For<ILogger<ReturnBookHandler>>();

        private readonly Guid _loanId = Guid.CreateVersion7();
        private readonly DateTimeOffset _returnedAt = new(2026, 10, 12, 15, 0, 0, TimeSpan.Zero);

        private readonly Loan _loan;
        private readonly ReturnBookHandler _handler;

        public ReturnBookHandlerTests()
        {
            _loan = new Loan
            {
                Id = _loanId,
                BookId = Guid.CreateVersion7(),
                UserId = Guid.CreateVersion7(),
                BorrowedAt = _returnedAt.AddDays(-1)
            };

            _loanRepository.GetByIdAsync(_loanId).Returns(_loan);

            _handler = new ReturnBookHandler(
                _loanRepository,
                new FixedTimeProvider(_returnedAt),
                _logger);
        }

        [Fact]
        public async Task HandleAsync_WhenLoanDoesNotExist_ShouldReturnNotFound()
        {
            _loanRepository.GetByIdAsync(_loanId).Returns((Loan?)null);

            var result = await _handler.HandleAsync(_loanId);

            result.IsFailure.ShouldBeTrue();
            result.Error.Code.ShouldBe("Loans.NotFound");
            await _loanRepository.DidNotReceive().ReturnAsync(
                Arg.Any<Loan>(),
                Arg.Any<DateTimeOffset>());
        }

        [Fact]
        public async Task HandleAsync_WhenLoanWasAlreadyReturned_ShouldReturnConflict()
        {
            var originalReturnedAt = _returnedAt.AddHours(-1);
            _loan.ReturnedAt = originalReturnedAt;

            var result = await _handler.HandleAsync(_loanId);

            result.IsFailure.ShouldBeTrue();
            result.Error.Code.ShouldBe("Loans.AlreadyReturned");
            _loan.ReturnedAt.ShouldBe(originalReturnedAt);
            await _loanRepository.DidNotReceive().ReturnAsync(
                Arg.Any<Loan>(),
                Arg.Any<DateTimeOffset>());
        }

        [Fact]
        public async Task HandleAsync_WhenLoanIsActive_ShouldReturnBook()
        {
            var result = await _handler.HandleAsync(_loanId);

            result.IsSuccess.ShouldBeTrue();
            await _loanRepository.Received(1).ReturnAsync(
                Arg.Is<Loan>(loan => loan.Id == _loanId),
                _returnedAt);
        }

        private sealed class FixedTimeProvider : TimeProvider
        {
            private readonly DateTimeOffset _utcNow;

            public FixedTimeProvider(DateTimeOffset utcNow)
            {
                _utcNow = utcNow;
            }

            public override DateTimeOffset GetUtcNow()
            {
                return _utcNow;
            }
        }
    }
}
