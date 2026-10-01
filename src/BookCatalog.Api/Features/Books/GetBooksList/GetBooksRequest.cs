namespace BookCatalog.Api.Features.Books.GetBooksList
{
    public sealed record GetBooksRequest(
        string? Title = null,
        string? Author = null,
        int? PublicationYear = null,
        int Page = 1,
        int PageSize = 10
    );
}
