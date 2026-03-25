using ClinicPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicPlatform.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.Property(x => x.Id).HasDefaultValueSql("(UUID())");
        builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
        builder.Property(x => x.NormalizedName).IsRequired().HasMaxLength(50);

        builder.HasIndex(x => x.NormalizedName).IsUnique();
    }
}
