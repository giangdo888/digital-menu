namespace DigitalMenuApi.Configuration;

using DigitalMenuApi.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DishConfiguration : IEntityTypeConfiguration<Dish>
{
    public void Configure(EntityTypeBuilder<Dish> builder)
    {
        builder.ToTable("Dishes");

        // Column configs
        builder.Property(d => d.CategoryId)
            .IsRequired();

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Price)
            .IsRequired()
            .HasPrecision(10, 2);

        builder.Property(d => d.ImageUrl)
            .HasMaxLength(500);

        // Cached nutrition values
        builder.Property(d => d.Calories)
            .HasPrecision(8, 2)
            .HasDefaultValue(0);

        builder.Property(d => d.ProteinG)
            .HasPrecision(8, 2)
            .HasDefaultValue(0);

        builder.Property(d => d.CarbsG)
            .HasPrecision(8, 2)
            .HasDefaultValue(0);

        builder.Property(d => d.FatG)
            .HasPrecision(8, 2)
            .HasDefaultValue(0);

        builder.Property(d => d.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(d => d.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        // Relationships
        builder.HasMany(d => d.DishIngredients)
            .WithOne(di => di.Dish)
            .HasForeignKey(di => di.DishId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(d => d.MealLogs)
            .WithOne(ml => ml.Dish)
            .HasForeignKey(ml => ml.DishId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
