using ClinicPlatform.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicPlatform.Infrastructure.Persistence.Configurations;

public class WorkflowStepConfiguration : IEntityTypeConfiguration<WorkflowStep>
{
    public void Configure(EntityTypeBuilder<WorkflowStep> builder)
    {
        builder.ToTable("workflow_steps");

        builder.Property(x => x.Id).HasDefaultValueSql("(UUID())");
        builder.Property(x => x.StepCode).IsRequired().HasMaxLength(50);
        builder.Property(x => x.DisplayName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.RequiredRole).HasMaxLength(50);
        builder.Property(x => x.IsSkippable).HasDefaultValue(false);
        builder.Property(x => x.AutoAdvance).HasDefaultValue(false);
        builder.Property(x => x.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

        builder.HasIndex(x => new { x.WorkflowDefinitionId, x.StepOrder }).IsUnique();
        builder.HasIndex(x => new { x.ClinicId, x.WorkflowDefinitionId });

        builder.HasOne(x => x.WorkflowDefinition).WithMany(x => x.Steps)
            .HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Clinic).WithMany()
            .HasForeignKey(x => x.ClinicId).OnDelete(DeleteBehavior.Restrict);
    }
}
