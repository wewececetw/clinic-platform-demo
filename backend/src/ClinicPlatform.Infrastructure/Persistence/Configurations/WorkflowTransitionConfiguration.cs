using ClinicPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicPlatform.Infrastructure.Persistence.Configurations;

public class WorkflowTransitionConfiguration : IEntityTypeConfiguration<WorkflowTransition>
{
    public void Configure(EntityTypeBuilder<WorkflowTransition> builder)
    {
        builder.ToTable("workflow_transitions");

        builder.Property(x => x.Id).HasDefaultValueSql("(UUID())");
        builder.Property(x => x.ConditionJson).HasColumnType("json");
        builder.Property(x => x.Priority).HasDefaultValue(0);
        builder.Property(x => x.IsActive).HasDefaultValue(true);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

        builder.HasIndex(x => new { x.WorkflowDefinitionId, x.FromStepId });

        builder.HasOne(x => x.WorkflowDefinition).WithMany()
            .HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.FromStep).WithMany(x => x.OutgoingTransitions)
            .HasForeignKey(x => x.FromStepId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ToStep).WithMany(x => x.IncomingTransitions)
            .HasForeignKey(x => x.ToStepId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Clinic).WithMany()
            .HasForeignKey(x => x.ClinicId).OnDelete(DeleteBehavior.Restrict);
    }
}
