using ClinicPlatform.Domain.Entities;
using ClinicPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicPlatform.Infrastructure.Persistence.Configurations;

public class VisitEventConfiguration : IEntityTypeConfiguration<VisitEvent>
{
    public void Configure(EntityTypeBuilder<VisitEvent> builder)
    {
        builder.ToTable("visit_events");

        builder.Property(x => x.Id).HasDefaultValueSql("(UUID())");
        builder.Property(x => x.TriggerType).HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

        builder.HasIndex(x => new { x.ClinicId, x.VisitId, x.CreatedAt });

        builder.HasOne(x => x.Visit).WithMany(x => x.VisitEvents)
            .HasForeignKey(x => x.VisitId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.FromStep).WithMany()
            .HasForeignKey(x => x.FromStepId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.ToStep).WithMany()
            .HasForeignKey(x => x.ToStepId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.TriggeredByUser).WithMany()
            .HasForeignKey(x => x.TriggeredByUserId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(x => x.Clinic).WithMany()
            .HasForeignKey(x => x.ClinicId).OnDelete(DeleteBehavior.Restrict);
    }
}
