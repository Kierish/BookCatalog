using BookCatalog.Domain.Common.Results;

namespace BookCatalog.Domain.Errors
{
    public static class BookErrors
    {
        public static Error NotFound(Guid id)
        {
            return Error.NotFound(
                "Books.NotFound",
                $"Book with ID '{id}' was not found.");
        }
    }
}
