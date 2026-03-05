namespace DigitalMenuApi.Configuration;

using DigitalMenuApi.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MealLogConfiguration : IEntityTypeConfiguration<MealLog>
{
    public void Configure(EntityTypeBuilder<MealLog> builder)
    {
        builder.ToTable("MealLogs");

        // Indexes
        builder.HasIndex(ml => new { ml.UserId, ml.CreatedAt });

        // Column configs
        builder.Property(ml => ml.UserId)
            .IsRequired();

        builder.Property(ml => ml.DishId)
            .IsRequired();

        // Nutrition snapshot
        builder.Property(ml => ml.Calories)
            .IsRequired()
            .HasPrecision(8, 2);

        builder.Property(ml => ml.ProteinG)
            .IsRequired()
            .HasPrecision(8, 2);

        builder.Property(ml => ml.CarbsG)
            .IsRequired()
            .HasPrecision(8, 2);

        builder.Property(ml => ml.FatG)
            .IsRequired()
            .HasPrecision(8, 2);

        builder.Property(ml => ml.CreatedAt)
            .IsRequired();
    }
}
