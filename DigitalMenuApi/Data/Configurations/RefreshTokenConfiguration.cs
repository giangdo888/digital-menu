using DigitalMenuApi.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DigitalMenuApi.Configuration;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        //indexes
        builder.HasIndex(rt => rt.Token).IsUnique();

        //column configs
        builder.Property(rt => rt.UserId)
            .IsRequired();
        
        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(rt => rt.ExpiresAt)
            .IsRequired();
        
        builder.Property(rt => rt.RevokedAt)
            .IsRequired(false);
        
        builder.Property(rt => rt.ReplacedByToken)
            .HasMaxLength(255);
    }
}