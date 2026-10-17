using BookCatalog.Api.Features.Loans.BorrowBook;
using BookCatalog.Domain.Entities;
using BookCatalog.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shouldly;

namespace BookCatalog.UnitTests.Features.Loans.BorrowBook
{
    public sealed class BorrowBookHandlerTests
    {
        private readonly IBookRepository _bookRepository = Substitute.For<IBookRepository>();
        private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
        private readonly ILoanRepository _loanRepository = Substitute.For<ILoanRepository>();
        private readonly ILogger<BorrowBookHandler> _logger = Substitute.For<ILogger<BorrowBookHandler>>();

        private readonly Guid _bookId = Guid.CreateVersion7();
        private readonly Guid _userId = Guid.CreateVersion7();
        private readonly DateTimeOffset _borrowedAt = new(2026, 10, 12, 12, 0, 0, TimeSpan.Zero);

        private readonly User _user;
        private readonly BorrowBookHandler _handler;

        private BorrowBookRequest Request => new(_bookId, _userId);

        public BorrowBookHandlerTests()
        {
            _user = new User
            {
                Id = _userId,
                Name = "Alice Johnson"
            };

            _bookRepository.ExistsAsync(_bookId).Returns(true);
            _userRepository.GetByIdAsync(_userId).Returns(_user);
            _loanRepository.HasActiveLoanAsync(_bookId).Returns(false);
            _loanRepository.TryBorrowAsync(Arg.Any<Loan>(), _user).Returns(true);

            _handler = new BorrowBookHandler(
                _bookRepository,
                _userRepository,
                _loanRepository,
                new FixedTimeProvider(_borrowedAt),
                _logger);
        }

        [Fact]
        public async Task HandleAsync_WhenBookDoesNotExist_ShouldReturnBookNotFound()
        {
            _bookRepository.ExistsAsync(_bookId).Returns(false);

            var result = await _handler.HandleAsync(Request);

            result.IsFailure.ShouldBeTrue();
            result.Error.Code.ShouldBe("Books.NotFound");
            await _userRepository.DidNotReceive().GetByIdAsync(Arg.Any<Guid>());
            await _loanRepository.DidNotReceive().TryBorrowAsync(
                Arg.Any<Loan>(),
                Arg.Any<User>());
        }

        [Fact]
        public async Task HandleAsync_WhenUserDoesNotExist_ShouldReturnUserNotFound()
        {
            _userRepository.GetByIdAsync(_userId).Returns((User?)null);

            var result = await _handler.HandleAsync(Request);

            result.IsFailure.ShouldBeTrue();
            result.Error.Code.ShouldBe("Users.NotFound");
            await _loanRepository.DidNotReceive().HasActiveLoanAsync(Arg.Any<Guid>());
            await _loanRepository.DidNotReceive().TryBorrowAsync(
                Arg.Any<Loan>(),
                Arg.Any<User>());
        }

        [Fact]
        public async Task HandleAsync_WhenBookHasActiveLoan_ShouldReturnConflict()
        {
            _loanRepository.HasActiveLoanAsync(_bookId).Returns(true);

            var result = await _handler.HandleAsync(Request);

            result.IsFailure.ShouldBeTrue();
            result.Error.Code.ShouldBe("Loans.BookAlreadyBorrowed");
            await _loanRepository.DidNotReceive().TryBorrowAsync(
                Arg.Any<Loan>(),
                Arg.Any<User>());
        }

        [Fact]
        public async Task HandleAsync_WhenConcurrentBorrowWins_ShouldReturnConflict()
        {
            _loanRepository.TryBorrowAsync(Arg.Any<Loan>(), _user).Returns(false);

            var result = await _handler.HandleAsync(Request);

            result.IsFailure.ShouldBeTrue();
            result.Error.Code.ShouldBe("Loans.BookAlreadyBorrowed");
            await _loanRepository.Received(1).TryBorrowAsync(
                Arg.Any<Loan>(),
                _user);
        }

        [Fact]
        public async Task HandleAsync_WhenBookIsAvailable_ShouldCreateLoan()
        {
            var result = await _handler.HandleAsync(Request);

            result.IsSuccess.ShouldBeTrue();
            result.Value.Id.ShouldNotBe(Guid.Empty);
            result.Value.BookId.ShouldBe(_bookId);
            result.Value.UserId.ShouldBe(_userId);
            result.Value.BorrowedAt.ShouldBe(_borrowedAt);
            result.Value.ReturnedAt.ShouldBeNull();

            await _loanRepository.Received(1).TryBorrowAsync(
                Arg.Is<Loan>(loan =>
                    loan.Id == result.Value.Id &&
                    loan.BookId == _bookId &&
                    loan.UserId == _userId &&
                    loan.BorrowedAt == _borrowedAt &&
                    loan.ReturnedAt == null),
                _user);
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
