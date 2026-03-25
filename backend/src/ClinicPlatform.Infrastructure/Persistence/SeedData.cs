using ClinicPlatform.Domain.Entities;
using ClinicPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ClinicPlatform.Infrastructure.Persistence;

public static class SeedData
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        // Roles
        var adminRoleId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var nurseRoleId = Guid.Parse("00000000-0000-0000-0000-000000000002");
        var doctorRoleId = Guid.Parse("00000000-0000-0000-0000-000000000003");
        var pharmacistRoleId = Guid.Parse("00000000-0000-0000-0000-000000000004");

        modelBuilder.Entity<Role>().HasData(
            new Role { Id = adminRoleId, Name = "Admin", NormalizedName = "ADMIN" },
            new Role { Id = nurseRoleId, Name = "Nurse", NormalizedName = "NURSE" },
            new Role { Id = doctorRoleId, Name = "Doctor", NormalizedName = "DOCTOR" },
            new Role { Id = pharmacistRoleId, Name = "Pharmacist", NormalizedName = "PHARMACIST" }
        );

        // Demo Clinic
        var clinicId = Guid.Parse("10000000-0000-0000-0000-000000000001");

        modelBuilder.Entity<Clinic>().HasData(new Clinic
        {
            Id = clinicId,
            Name = "示範診所",
            Slug = "demo-clinic",
            Phone = "02-1234-5678",
            Address = "台北市信義區示範路1號",
            SettingsJson = """{"allowed_checkin_methods":["Otp","QrCode","Manual"],"business_hours":{"start":"09:00","end":"18:00"}}""",
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        // Default Workflow: 一般門診流程 (9 steps)
        var workflowId = Guid.Parse("20000000-0000-0000-0000-000000000001");

        modelBuilder.Entity<WorkflowDefinition>().HasData(new WorkflowDefinition
        {
            Id = workflowId,
            ClinicId = clinicId,
            Name = "一般門診流程",
            Description = "報到→候診→叫號→看診→開處方→傳藥局→配藥→叫領藥→離院",
            IsDefault = true,
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        // Workflow Steps
        var steps = new (string code, string name, int order, string? role, Guid id)[]
        {
            ("check_in", "報到", 1, "Nurse", Guid.Parse("30000000-0000-0000-0000-000000000001")),
            ("waiting", "候診", 2, null, Guid.Parse("30000000-0000-0000-0000-000000000002")),
            ("called", "叫號進診間", 3, "Nurse", Guid.Parse("30000000-0000-0000-0000-000000000003")),
            ("consulting", "醫師看診", 4, "Doctor", Guid.Parse("30000000-0000-0000-0000-000000000004")),
            ("prescribed", "開立處方", 5, "Doctor", Guid.Parse("30000000-0000-0000-0000-000000000005")),
            ("sent_to_pharmacy", "處方傳藥局", 6, null, Guid.Parse("30000000-0000-0000-0000-000000000006")),
            ("dispensing", "藥劑師配藥", 7, "Pharmacist", Guid.Parse("30000000-0000-0000-0000-000000000007")),
            ("ready_for_pickup", "叫領藥號", 8, "Nurse", Guid.Parse("30000000-0000-0000-0000-000000000008")),
            ("completed", "離院", 9, null, Guid.Parse("30000000-0000-0000-0000-000000000009")),
        };

        var stepEntities = steps.Select(s => new WorkflowStep
        {
            Id = s.id,
            ClinicId = clinicId,
            WorkflowDefinitionId = workflowId,
            StepCode = s.code,
            DisplayName = s.name,
            StepOrder = s.order,
            RequiredRole = s.role,
            IsSkippable = s.code is "prescribed" or "sent_to_pharmacy" or "dispensing" or "ready_for_pickup",
            AutoAdvance = s.code is "sent_to_pharmacy",
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        }).ToArray();

        modelBuilder.Entity<WorkflowStep>().HasData(stepEntities);

        // Workflow Transitions (linear: step N → step N+1)
        var transitions = new List<WorkflowTransition>();
        for (int i = 0; i < steps.Length - 1; i++)
        {
            transitions.Add(new WorkflowTransition
            {
                Id = Guid.Parse($"40000000-0000-0000-0000-00000000000{i + 1}"),
                ClinicId = clinicId,
                WorkflowDefinitionId = workflowId,
                FromStepId = steps[i].id,
                ToStepId = steps[i + 1].id,
                ConditionJson = null,
                Priority = 0,
                IsActive = true,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        }

        // Skip transition: prescribed → completed (when no medication needed)
        transitions.Add(new WorkflowTransition
        {
            Id = Guid.Parse("40000000-0000-0000-0000-000000000009"),
            ClinicId = clinicId,
            WorkflowDefinitionId = workflowId,
            FromStepId = steps[3].id, // consulting
            ToStepId = steps[8].id,   // completed
            ConditionJson = """{"skip_when":{"field":"visit.needs_medication","operator":"eq","value":false}}""",
            Priority = 10,
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });

        modelBuilder.Entity<WorkflowTransition>().HasData(transitions);
    }
}
