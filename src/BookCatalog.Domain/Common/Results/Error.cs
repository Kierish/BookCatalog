namespace BookCatalog.Domain.Common.Results
{
    public sealed record Error(string Code, string Message, ErrorType Type)
    {
        public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);

        public static Error NotFound(string code, string message) =>
            new(code, message, ErrorType.NotFound);
    }
}
