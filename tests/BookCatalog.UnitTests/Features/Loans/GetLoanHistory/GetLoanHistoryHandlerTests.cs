using BookCatalog.Api.Features.Loans.GetLoanHistory;
using BookCatalog.Domain.Common;
using BookCatalog.Domain.Common.Queries;
using BookCatalog.Domain.Entities;
using BookCatalog.Domain.Interfaces;
using NSubstitute;
using Shouldly;

namespace BookCatalog.UnitTests.Features.Loans.GetLoanHistory
{
    public sealed class GetLoanHistoryHandlerTests
    {
        private readonly ILoanRepository _loanRepository = Substitute.For<ILoanRepository>();
        private readonly GetLoanHistoryHandler _handler;

        public GetLoanHistoryHandlerTests()
        {
            _handler = new GetLoanHistoryHandler(_loanRepository);
        }

        [Fact]
        public async Task HandleAsync_WhenCalled_ShouldPassRequestParametersToRepository()
        {
            var bookId = Guid.CreateVersion7();
            var userId = Guid.CreateVersion7();
            var request = new GetLoanHistoryRequest(
                BookId: bookId,
                UserId: userId,
                PageNumber: 2,
                PageSize: 25);
            SetupRepository(new Page<Loan>(
                [],
                TotalCount: 0,
                PageNumber: 2,
                PageSize: 25));

            await _handler.HandleAsync(request);

            await _loanRepository.Received(1).GetPagedAsync(
                Arg.Is<LoanHistoryQuery>(query =>
                    query.BookId == bookId &&
                    query.UserId == userId &&
                    query.PageNumber == 2 &&
                    query.PageSize == 25));
        }

        [Fact]
        public async Task HandleAsync_WhenRepositoryReturnsLoans_ShouldMapPagedHistory()
        {
            var book = new Book
            {
                Id = Guid.CreateVersion7(),
                Title = "Clean Code",
                PublicationYear = 2008,
                AuthorId = Guid.CreateVersion7()
            };
            var user = new User
            {
                Id = Guid.CreateVersion7(),
                Name = "Alice Johnson"
            };
            var loan = new Loan
            {
                Id = Guid.CreateVersion7(),
                BookId = book.Id,
                Book = book,
                UserId = user.Id,
                User = user,
                BorrowedAt = new DateTimeOffset(2026, 10, 10, 12, 0, 0, TimeSpan.Zero),
                ReturnedAt = new DateTimeOffset(2026, 10, 12, 12, 0, 0, TimeSpan.Zero)
            };
            SetupRepository(new Page<Loan>(
                [loan],
                TotalCount: 11,
                PageNumber: 2,
                PageSize: 10));

            var result = await _handler.HandleAsync(
                new GetLoanHistoryRequest(PageNumber: 2));

            result.TotalCount.ShouldBe(11);
            result.PageNumber.ShouldBe(2);
            result.PageSize.ShouldBe(10);

            var item = result.Items.ShouldHaveSingleItem();
            item.Id.ShouldBe(loan.Id);
            item.BookId.ShouldBe(book.Id);
            item.BookTitle.ShouldBe(book.Title);
            item.UserId.ShouldBe(user.Id);
            item.UserName.ShouldBe(user.Name);
            item.BorrowedAt.ShouldBe(loan.BorrowedAt);
            item.ReturnedAt.ShouldBe(loan.ReturnedAt);
        }

        private void SetupRepository(Page<Loan> page)
        {
            _loanRepository
                .GetPagedAsync(Arg.Any<LoanHistoryQuery>())
                .Returns(page);
        }
    }
}
