using ClinicPlatform.Domain.Entities;
using ClinicPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicPlatform.Infrastructure.Persistence.Configurations;

public class QueueConfigConfiguration : IEntityTypeConfiguration<QueueConfig>
{
    public void Configure(EntityTypeBuilder<QueueConfig> builder)
    {
        builder.ToTable("queue_configs");

        builder.Property(x => x.Id).HasDefaultValueSql("(UUID())");
        builder.Property(x => x.QueueType).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.PriorityWeight).HasDefaultValue(0);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

        builder.HasIndex(x => new { x.ClinicId, x.QueueType });

        builder.HasOne(x => x.Clinic).WithMany()
            .HasForeignKey(x => x.ClinicId).OnDelete(DeleteBehavior.Restrict);
    }
}
