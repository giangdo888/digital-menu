namespace DigitalMenuApi.Configuration;

using DigitalMenuApi.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class WeightHistoryConfiguration : IEntityTypeConfiguration<WeightHistory>
{
    public void Configure(EntityTypeBuilder<WeightHistory> builder)
    {
        builder.ToTable("WeightHistory");

        // Indexes
        builder.HasIndex(wh => new { wh.UserId, wh.RecordedAt });

        // Column configs
        builder.Property(wh => wh.UserId)
            .IsRequired();

        builder.Property(wh => wh.WeightKg)
            .IsRequired()
            .HasPrecision(5, 2);

        builder.Property(wh => wh.RecordedAt)
            .IsRequired();
    }
}
