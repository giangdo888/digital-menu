namespace DigitalMenuApi.Configuration;

using DigitalMenuApi.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles");

        // Indexes
        builder.HasIndex(up => up.UserId).IsUnique();

        // Column configs
        builder.Property(up => up.UserId)
            .IsRequired();

        builder.Property(up => up.Gender)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(up => up.DateOfBirth)
            .IsRequired();

        builder.Property(up => up.HeightCm)
            .IsRequired()
            .HasPrecision(5, 2);

        builder.Property(up => up.CurrentWeightKg)
            .IsRequired()
            .HasPrecision(5, 2);

        builder.Property(up => up.BmiGoal)
            .HasPrecision(5, 2);

        builder.Property(up => up.LastWeightUpdate)
            .IsRequired();

        // Relationships defined in UserConfiguration
    }
}
