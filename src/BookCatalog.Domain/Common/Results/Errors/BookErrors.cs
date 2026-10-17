
namespace BookCatalog.Domain.Common.Results.Errors
{
    public static class BookErrors
    {
        public static Error NotFound(Guid id)
        {
            return Error.NotFound(
                "Books.NotFound",
                $"Book with ID '{id}' was not found.");
        }

        public static Error HasLoanHistory(Guid id)
        {
            return Error.Conflict(
                "Books.HasLoanHistory",
                $"Book with ID '{id}' cannot be deleted because it has loan history.");
        }
    }
}
