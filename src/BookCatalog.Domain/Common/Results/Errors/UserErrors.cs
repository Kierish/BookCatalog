namespace BookCatalog.Domain.Common.Results.Errors
{
    public static class UserErrors
    {
        public static Error NotFound(Guid id)
        {
            return Error.NotFound(
                "Users.NotFound",
                $"User with ID '{id}' was not found.");
        }
    }
}
