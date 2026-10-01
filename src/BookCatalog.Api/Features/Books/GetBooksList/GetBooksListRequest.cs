namespace BookCatalog.Api.Features.Books.GetBooksList
{
    public sealed record GetBooksListRequest(
        string? Title = null,
        string? Author = null,
        int? PublicationYear = null,
        int PageNumber = 1,
        int PageSize = 10
    );
}
