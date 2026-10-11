using BookCatalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookCatalog.Infrastructure.Persistence.Configurations
{
    public sealed class LoanConfiguration : IEntityTypeConfiguration<Loan>
    {
        public void Configure(EntityTypeBuilder<Loan> builder)
        {
            builder.ToTable(
                "Loans",
                tableBuilder => tableBuilder.HasCheckConstraint(
                    "CK_Loans_ReturnedAt",
                    "\"ReturnedAt\" IS NULL OR \"ReturnedAt\" >= \"BorrowedAt\""));

            builder.HasKey(loan => loan.Id);

            builder.Property(loan => loan.Id)
                .ValueGeneratedNever();

            builder.Property(loan => loan.BorrowedAt)
                .IsRequired();

            builder.HasOne(loan => loan.Book)
                .WithMany(book => book.Loans)
                .HasForeignKey(loan => loan.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(loan => loan.User)
                .WithMany(user => user.Loans)
                .HasForeignKey(loan => loan.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(loan => loan.BookId)
                .IsUnique()
                .HasFilter("\"ReturnedAt\" IS NULL");
        }
    }
}
