namespace DigitalMenuApi.Configuration;

using DigitalMenuApi.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DishIngredientConfiguration : IEntityTypeConfiguration<DishIngredient>
{
    public void Configure(EntityTypeBuilder<DishIngredient> builder)
    {
        builder.ToTable("DishIngredients");

        // Indexes
        builder.HasIndex(di => di.DishId);
        builder.HasIndex(di => new { di.DishId, di.AfcdItemId }).IsUnique();

        // Column configs
        builder.Property(di => di.DishId)
            .IsRequired();

        builder.Property(di => di.AfcdItemId)
            .IsRequired();

        builder.Property(di => di.Amount)
            .IsRequired()
            .HasPrecision(8, 2);

        // Relationships
        builder.HasOne(di => di.AfcdItem)
            .WithMany(a => a.DishIngredients)
            .HasForeignKey(di => di.AfcdItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
