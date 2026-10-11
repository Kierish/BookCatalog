namespace BookCatalog.Api.Features.Books.UpdateBook
{
    public sealed record UpdateBookRequest(
        string Title,
        Guid AuthorId,
        string? Isbn,
        int PublicationYear
    );
}
