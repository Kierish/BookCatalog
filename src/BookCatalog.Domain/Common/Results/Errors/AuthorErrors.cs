
namespace BookCatalog.Domain.Common.Results.Errors
{
    public static class AuthorErrors
    {
        public static Error NotFound(Guid id)
        {
            return Error.NotFound(
                "Authors.NotFound",
                $"Author with ID '{id}' was not found.");
        }
    }
}
