namespace DigitalMenuApi.Configuration;

using DigitalMenuApi.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RestaurantConfiguration : IEntityTypeConfiguration<Restaurant>
{
    public void Configure(EntityTypeBuilder<Restaurant> builder)
    {
        builder.ToTable("Restaurants");

        // Indexes
        builder.HasIndex(r => r.UserId);  // Not unique - one owner can have many restaurants
        builder.HasIndex(r => r.Slug).IsUnique();

        // Column configs
        builder.Property(r => r.UserId)
            .IsRequired();

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(r => r.Address)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(r => r.Phone)
            .HasMaxLength(20);

        builder.Property(r => r.Email)
            .HasMaxLength(100);

        builder.Property(r => r.Description)
            .HasMaxLength(500);

        builder.Property(r => r.OpeningHours)
            .HasMaxLength(200);

        builder.Property(r => r.LogoUrl)
            .HasMaxLength(255);

        builder.Property(r => r.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // Relationships
        builder.HasOne(r => r.User)
            .WithMany(u => u.Restaurants)  // One user can have many restaurants
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Categories)
            .WithOne(c => c.Restaurant)
            .HasForeignKey(c => c.RestaurantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}