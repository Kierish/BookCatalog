namespace BookCatalog.Domain.Entities
{
    public sealed class Book
    {
        public Guid Id { get; init; } = Guid.CreateVersion7();
        public required string Title { get; set; }
        public required string Author { get; set; }
        public string? Isbn { get; set; }
        public required int PublicationYear { get; set; }
    }
}
