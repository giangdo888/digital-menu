namespace DigitalMenuApi.Configuration;

using DigitalMenuApi.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class RestaurantConfiguration : IEntityTypeConfiguration<Restaurant>
{
    public void Configure(EntityTypeBuilder<Restaurant> builder)
    {
        builder.ToTable("Restaurants");

        //indexes
        builder.HasIndex(r => r.UserId).IsUnique();

        //column configs
        builder.Property(r => r.UserId)
            .IsRequired();
        
        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(r => r.Address)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(r => r.LogoUrl)
            .HasMaxLength(255);
        
        builder.Property(r => r.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
            
        //relationships
        builder.HasOne(r => r.User)
            .WithOne(u => u.Restaurant)
            .HasForeignKey<Restaurant>(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(r => r.Categories)
            .WithOne(c => c.Restaurant)
            .HasForeignKey(c => c.RestaurantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}