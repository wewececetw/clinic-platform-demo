using ClinicPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicPlatform.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.Property(x => x.Id).HasDefaultValueSql("(UUID())");
        builder.Property(x => x.EntityType).IsRequired().HasMaxLength(100);
        builder.Property(x => x.EntityId).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Action).IsRequired().HasMaxLength(50);
        builder.Property(x => x.OldValues).HasColumnType("json");
        builder.Property(x => x.NewValues).HasColumnType("json");
        builder.Property(x => x.IpAddress).HasMaxLength(50);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

        builder.HasIndex(x => new { x.ClinicId, x.EntityType, x.EntityId });
        builder.HasIndex(x => new { x.ClinicId, x.CreatedAt });

        builder.HasOne(x => x.Clinic).WithMany()
            .HasForeignKey(x => x.ClinicId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.User).WithMany()
            .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
    }
}
