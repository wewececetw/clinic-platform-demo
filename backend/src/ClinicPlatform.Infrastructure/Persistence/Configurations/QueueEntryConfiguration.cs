using ClinicPlatform.Domain.Entities;
using ClinicPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicPlatform.Infrastructure.Persistence.Configurations;

public class QueueEntryConfiguration : IEntityTypeConfiguration<QueueEntry>
{
    public void Configure(EntityTypeBuilder<QueueEntry> builder)
    {
        builder.ToTable("queue_entries");

        builder.Property(x => x.Id).HasDefaultValueSql("(UUID())");
        builder.Property(x => x.QueueType).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.Priority).HasDefaultValue(0);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

        builder.HasIndex(x => new { x.ClinicId, x.QueueType, x.Status });
        builder.HasIndex(x => new { x.ClinicId, x.VisitId });

        builder.HasOne(x => x.Visit).WithOne(x => x.QueueEntry)
            .HasForeignKey<QueueEntry>(x => x.VisitId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Clinic).WithMany()
            .HasForeignKey(x => x.ClinicId).OnDelete(DeleteBehavior.Restrict);
    }
}
