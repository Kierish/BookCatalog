using BookCatalog.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookCatalog.Infrastructure.Persistence.Configurations
{
    public sealed class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(user => user.Id);

            builder.Property(user => user.Id)
                .ValueGeneratedNever();

            builder.Property(user => user.Name)
                .HasMaxLength(200)
                .IsRequired();

            builder.HasData(
                new User
                {
                    Id = Guid.Parse(" "),
                    Name = "Alice Johnson"
                },
                new User
                {
                    Id = Guid.Parse("0199c500-0000-7000-8000-000000000102"),
                    Name = "Bob Smith"
                });
        }
    }
}
