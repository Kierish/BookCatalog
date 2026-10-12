namespace BookCatalog.Domain.Common.Results.Errors
{
    public static class LoanErrors
    {
        public static Error NotFound(Guid id)
        {
            return Error.NotFound(
                "Loans.NotFound",
                $"Loan with ID '{id}' was not found.");
        }

        public static Error BookAlreadyBorrowed(Guid bookId)
        {
            return Error.Conflict(
                "Loans.BookAlreadyBorrowed",
                $"Book with ID '{bookId}' is already borrowed.");
        }

        public static Error AlreadyReturned(Guid id)
        {
            return Error.Conflict(
                "Loans.AlreadyReturned",
                $"Loan with ID '{id}' has already been returned.");
        }
    }
}
