namespace DigitalMenuApi.Configuration;

using DigitalMenuApi.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        // Indexes
        builder.HasIndex(r => r.Name).IsUnique();

        // Column configs
        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.Description)
            .IsRequired()
            .HasMaxLength(200);

        // Seed data
        builder.HasData(
            new Role { Id = 1, Name = "system_admin", Description = "System Administrator" },
            new Role { Id = 2, Name = "restaurant_admin", Description = "Restaurant Administrator" },
            new Role { Id = 3, Name = "customer", Description = "Customer" }
        );
    }
}
