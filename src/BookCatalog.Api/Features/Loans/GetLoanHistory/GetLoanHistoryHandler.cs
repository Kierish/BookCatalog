using BookCatalog.Domain.Common;
using BookCatalog.Domain.Common.Queries;
using BookCatalog.Domain.Interfaces;

namespace BookCatalog.Api.Features.Loans.GetLoanHistory
{
    public sealed class GetLoanHistoryHandler
    {
        private readonly ILoanRepository _loanRepository;

        public GetLoanHistoryHandler(ILoanRepository loanRepository)
        {
            _loanRepository = loanRepository;
        }

        public async Task<PagedResult<LoanHistoryResponse>> HandleAsync(
            GetLoanHistoryRequest request)
        {
            var query = new LoanQuery(
                request.BookId,
                request.UserId,
                request.PageNumber,
                request.PageSize);

            var pagedLoans = await _loanRepository.GetPagedAsync(query);

            return pagedLoans.Map(loan => loan.ToHistoryResponse());
        }
    }
}
