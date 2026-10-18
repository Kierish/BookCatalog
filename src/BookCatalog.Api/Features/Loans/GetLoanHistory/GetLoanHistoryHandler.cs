using BookCatalog.Domain.Common;
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

        public async Task<Page<LoanHistoryResponse>> HandleAsync(
            GetLoanHistoryRequest request,
            CancellationToken cancellationToken = default)
        {
            var pagedLoans = await _loanRepository.GetPagedAsync(
                request.ToQuery(),
                cancellationToken);

            return pagedLoans.Map(loan => loan.ToHistoryResponse());
        }
    }
}
