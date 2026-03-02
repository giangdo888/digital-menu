namespace DigitalMenuApi.Configuration;

using DigitalMenuApi.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AFCDItemConfiguration : IEntityTypeConfiguration<AFCDItem>
{
    public void Configure(EntityTypeBuilder<AFCDItem> builder)
    {
        builder.ToTable("AFCDItems");

        // Indexes
        builder.HasIndex(a => a.Name);
        builder.HasIndex(a => a.PublicFoodKey).IsUnique();

        // PublicFoodKey config
        builder.Property(a => a.PublicFoodKey)
            .HasMaxLength(50);

        // Column configs
        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Variant)
            .HasMaxLength(300);

        // Nutrition per 100g
        builder.Property(a => a.Calories)
            .IsRequired()
            .HasPrecision(8, 2);

        builder.Property(a => a.ProteinG)
            .IsRequired()
            .HasPrecision(8, 2);

        builder.Property(a => a.CarbsG)
            .IsRequired()
            .HasPrecision(8, 2);

        builder.Property(a => a.FatG)
            .IsRequired()
            .HasPrecision(8, 2);

        // Full nutrition JSON for future use
        builder.Property(a => a.FullNutritionJson)
            .HasColumnType("nvarchar(max)");
    }
}
