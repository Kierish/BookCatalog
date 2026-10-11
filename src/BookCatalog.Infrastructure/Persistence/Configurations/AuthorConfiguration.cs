using BookCatalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookCatalog.Infrastructure.Persistence.Configurations
{
    public sealed class AuthorConfiguration : IEntityTypeConfiguration<Author>
    {
        public void Configure(EntityTypeBuilder<Author> builder)
        {
            builder.ToTable("Authors");

            builder.HasKey(author => author.Id);

            builder.Property(author => author.Id)
                .ValueGeneratedNever();

            builder.Property(author => author.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.HasData(
                new Author
                {
                    Id = Guid.Parse("0199c500-0000-7000-8000-000000000001"),
                    Name = "George Orwell"
                },
                new Author
                {
                    Id = Guid.Parse("0199c500-0000-7000-8000-000000000002"),
                    Name = "J. R. R. Tolkien"
                },
                new Author
                {
                    Id = Guid.Parse("0199c500-0000-7000-8000-000000000003"),
                    Name = "Fyodor Dostoevsky"
                });
        }
    }
}
