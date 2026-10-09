namespace BookCatalog.Domain.Entities
{
    public sealed class User
    {
        public Guid Id { get; init; } = Guid.CreateVersion7();

        public required string Name { get; set; }

        public ICollection<Loan> Loans { get; set; } = [];
    }
}
