namespace BookCatalog.Domain.Entities
{
    public sealed class Author
    {
        public Guid Id { get; init; } = Guid.CreateVersion7();

        public required string Name { get; set; }

        public ICollection<Book> Books { get; set; } = [];
    }
}
