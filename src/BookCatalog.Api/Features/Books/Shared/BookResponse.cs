namespace BookCatalog.Api.Features.Books
{
    public sealed record BookResponse(
        Guid Id,
        string Title,
        Guid AuthorId,
        string AuthorName,
        string? Isbn,
        int PublicationYear
    );
}
