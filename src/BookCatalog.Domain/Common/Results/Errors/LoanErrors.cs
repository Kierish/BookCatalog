namespace BookCatalog.Domain.Common.Results.Errors
{
    public static class LoanErrors
    {
        public static Error BookAlreadyBorrowed(Guid bookId)
        {
            return Error.Conflict(
                "Loans.BookAlreadyBorrowed",
                $"Book with ID '{bookId}' is already borrowed.");
        }
    }
}
