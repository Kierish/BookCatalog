using BookCatalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookCatalog.Infrastructure.Persistence.Configurations
{
    public sealed class BookConfiguration : IEntityTypeConfiguration<Book>
    {
        public void Configure(EntityTypeBuilder<Book> builder)
        {
            builder.ToTable(
                "Books",
                tableBuilder => tableBuilder.HasCheckConstraint(
                    "CK_Books_PublicationYear",
                    "\"PublicationYear\" >= 1"));

            builder.HasKey(book => book.Id);

            builder.Property(book => book.Id)
                .ValueGeneratedNever();

            builder.Property(book => book.Title)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(book => book.Isbn)
                .HasMaxLength(17);

            builder.Property(book => book.PublicationYear)
                .IsRequired();

            builder.HasOne(book => book.Author)
                .WithMany(author => author.Books)
                .HasForeignKey(book => book.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
