using ClinicPlatform.Domain.Entities;
using ClinicPlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ClinicPlatform.Infrastructure.Persistence;

public static class SeedData
{
    // ── 固定 ID，讓 HasData 與 SeedAsync 共用 ──
    private static readonly Guid AdminRoleId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    private static readonly Guid NurseRoleId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    private static readonly Guid DoctorRoleId = Guid.Parse("00000000-0000-0000-0000-000000000003");
    private static readonly Guid PharmacistRoleId = Guid.Parse("00000000-0000-0000-0000-000000000004");

    private static readonly Guid ClinicId = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid WorkflowId = Guid.Parse("20000000-0000-0000-0000-000000000001");

    // Steps
    private static readonly Guid StepCheckinId = Guid.Parse("30000000-0000-0000-0000-000000000001");
    private static readonly Guid StepWaitingId = Guid.Parse("30000000-0000-0000-0000-000000000002");
    private static readonly Guid StepCalledId = Guid.Parse("30000000-0000-0000-0000-000000000003");
    private static readonly Guid StepConsultingId = Guid.Parse("30000000-0000-0000-0000-000000000004");
    private static readonly Guid StepPrescribedId = Guid.Parse("30000000-0000-0000-0000-000000000005");
    private static readonly Guid StepSentToPharmacyId = Guid.Parse("30000000-0000-0000-0000-000000000006");
    private static readonly Guid StepDispensingId = Guid.Parse("30000000-0000-0000-0000-000000000007");
    private static readonly Guid StepReadyForPickupId = Guid.Parse("30000000-0000-0000-0000-000000000008");
    private static readonly Guid StepCompletedId = Guid.Parse("30000000-0000-0000-0000-000000000009");

    // Departments
    private static readonly Guid DeptGenId = Guid.Parse("50000000-0000-0000-0000-000000000001");
    private static readonly Guid DeptDenId = Guid.Parse("50000000-0000-0000-0000-000000000002");

    // Rooms
    private static readonly Guid Room1Id = Guid.Parse("60000000-0000-0000-0000-000000000001");
    private static readonly Guid Room2Id = Guid.Parse("60000000-0000-0000-0000-000000000002");

    private static readonly DateTime SeedTime = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    /// <summary>
    /// EF Core HasData 種子資料（在 OnModelCreating 中呼叫）
    /// </summary>
    public static void Seed(ModelBuilder modelBuilder)
    {
        // ── 角色 ──
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = AdminRoleId, Name = "Admin", NormalizedName = "ADMIN" },
            new Role { Id = NurseRoleId, Name = "Nurse", NormalizedName = "NURSE" },
            new Role { Id = DoctorRoleId, Name = "Doctor", NormalizedName = "DOCTOR" },
            new Role { Id = PharmacistRoleId, Name = "Pharmacist", NormalizedName = "PHARMACIST" }
        );

        // ── 示範診所 ──
        modelBuilder.Entity<Clinic>().HasData(new Clinic
        {
            Id = ClinicId,
            Name = "示範診所",
            Slug = "demo-clinic",
            Phone = "02-1234-5678",
            Address = "台北市信義區示範路1號",
            SettingsJson = """{"timezone":"Asia/Taipei","allowed_checkin_methods":["otp","qrcode","manual"]}""",
            IsActive = true,
            CreatedAt = SeedTime,
            UpdatedAt = SeedTime
        });

        // ── 一般門診流程 ──
        modelBuilder.Entity<WorkflowDefinition>().HasData(new WorkflowDefinition
        {
            Id = WorkflowId,
            ClinicId = ClinicId,
            Name = "一般門診流程",
            Description = "報到→候診→叫號→看診→已開處方→傳藥局→配藥中→待領藥→完成",
            IsDefault = true,
            IsActive = true,
            CreatedAt = SeedTime,
            UpdatedAt = SeedTime
        });

        // ── 9 個 Workflow Steps ──
        modelBuilder.Entity<WorkflowStep>().HasData(
            MakeStep(StepCheckinId, "checkin", "報到", 1, null, false),
            MakeStep(StepWaitingId, "waiting", "候診", 2, null, false),
            MakeStep(StepCalledId, "called", "叫號", 3, "nurse", false),
            MakeStep(StepConsultingId, "consulting", "看診", 4, "doctor", false),
            MakeStep(StepPrescribedId, "prescribed", "已開處方", 5, "doctor", true),
            MakeStep(StepSentToPharmacyId, "sent_to_pharmacy", "傳藥局", 6, "system", true),
            MakeStep(StepDispensingId, "dispensing", "配藥中", 7, "pharmacist", false),
            MakeStep(StepReadyForPickupId, "ready_for_pickup", "待領藥", 8, "pharmacist", false),
            MakeStep(StepCompletedId, "completed", "完成", 9, null, false)
        );

        // ── Workflow Transitions（有向圖）──
        modelBuilder.Entity<WorkflowTransition>().HasData(
            MakeTransition(Guid.Parse("40000000-0000-0000-0000-000000000001"),
                StepCheckinId, StepWaitingId, 0, null),
            MakeTransition(Guid.Parse("40000000-0000-0000-0000-000000000002"),
                StepWaitingId, StepCalledId, 0, null),
            MakeTransition(Guid.Parse("40000000-0000-0000-0000-000000000003"),
                StepCalledId, StepConsultingId, 0, null),
            MakeTransition(Guid.Parse("40000000-0000-0000-0000-000000000004"),
                StepConsultingId, StepPrescribedId, 0, null),
            // prescribed → sent_to_pharmacy（預設，需要藥物時）
            MakeTransition(Guid.Parse("40000000-0000-0000-0000-000000000005"),
                StepPrescribedId, StepSentToPharmacyId, 0,
                """{"condition":{"field":"visit.needs_medication","operator":"eq","value":true}}"""),
            // prescribed → completed（不需藥物時跳過）
            MakeTransition(Guid.Parse("40000000-0000-0000-0000-000000000006"),
                StepPrescribedId, StepCompletedId, 10,
                """{"skip_when":{"field":"visit.needs_medication","operator":"eq","value":false}}"""),
            MakeTransition(Guid.Parse("40000000-0000-0000-0000-000000000007"),
                StepSentToPharmacyId, StepDispensingId, 0, null),
            MakeTransition(Guid.Parse("40000000-0000-0000-0000-000000000008"),
                StepDispensingId, StepReadyForPickupId, 0, null),
            MakeTransition(Guid.Parse("40000000-0000-0000-0000-000000000009"),
                StepReadyForPickupId, StepCompletedId, 0, null)
        );

        // ── 示範科別 ──
        modelBuilder.Entity<Department>().HasData(
            new Department { Id = DeptGenId, ClinicId = ClinicId, Name = "一般內科", Code = "GEN", IsActive = true, SortOrder = 1 },
            new Department { Id = DeptDenId, ClinicId = ClinicId, Name = "牙科", Code = "DEN", IsActive = true, SortOrder = 2 }
        );

        // ── 示範診間 ──
        modelBuilder.Entity<Room>().HasData(
            new Room { Id = Room1Id, ClinicId = ClinicId, DepartmentId = DeptGenId, Name = "第一診", RoomType = RoomType.Consulting, IsActive = true, SortOrder = 1 },
            new Room { Id = Room2Id, ClinicId = ClinicId, DepartmentId = DeptGenId, Name = "第二診", RoomType = RoomType.Consulting, IsActive = true, SortOrder = 2 }
        );
    }

    /// <summary>
    /// 執行期種子資料（可在應用程式啟動時呼叫，僅在資料不存在時新增）
    /// </summary>
    public static async Task SeedAsync(ClinicDbContext context)
    {
        var changed = false;

        // ── 角色 ──
        if (!context.Roles.Any())
        {
            context.Roles.Add(new Role { Id = AdminRoleId, Name = "Admin", NormalizedName = "ADMIN" });
            context.Roles.Add(new Role { Id = NurseRoleId, Name = "Nurse", NormalizedName = "NURSE" });
            context.Roles.Add(new Role { Id = DoctorRoleId, Name = "Doctor", NormalizedName = "DOCTOR" });
            context.Roles.Add(new Role { Id = PharmacistRoleId, Name = "Pharmacist", NormalizedName = "PHARMACIST" });
            changed = true;
        }

        // ── 示範診所 ──
        if (!context.Clinics.Any(c => c.Slug == "demo-clinic"))
        {
            context.Clinics.Add(new Clinic
            {
                Id = ClinicId,
                Name = "示範診所",
                Slug = "demo-clinic",
                Phone = "02-1234-5678",
                Address = "台北市信義區示範路1號",
                SettingsJson = """{"timezone":"Asia/Taipei","allowed_checkin_methods":["otp","qrcode","manual"]}""",
                IsActive = true,
                CreatedAt = SeedTime,
                UpdatedAt = SeedTime
            });
            changed = true;
        }

        // ── 一般門診流程 ──
        if (!context.WorkflowDefinitions.Any(w => w.ClinicId == ClinicId))
        {
            context.WorkflowDefinitions.Add(new WorkflowDefinition
            {
                Id = WorkflowId,
                ClinicId = ClinicId,
                Name = "一般門診流程",
                Description = "報到→候診→叫號→看診→已開處方→傳藥局→配藥中→待領藥→完成",
                IsDefault = true,
                IsActive = true,
                CreatedAt = SeedTime,
                UpdatedAt = SeedTime
            });
            changed = true;
        }

        // ── Workflow Steps ──
        if (!context.WorkflowSteps.Any(s => s.WorkflowDefinitionId == WorkflowId))
        {
            context.WorkflowSteps.Add(MakeStep(StepCheckinId, "checkin", "報到", 1, null, false));
            context.WorkflowSteps.Add(MakeStep(StepWaitingId, "waiting", "候診", 2, null, false));
            context.WorkflowSteps.Add(MakeStep(StepCalledId, "called", "叫號", 3, "nurse", false));
            context.WorkflowSteps.Add(MakeStep(StepConsultingId, "consulting", "看診", 4, "doctor", false));
            context.WorkflowSteps.Add(MakeStep(StepPrescribedId, "prescribed", "已開處方", 5, "doctor", true));
            context.WorkflowSteps.Add(MakeStep(StepSentToPharmacyId, "sent_to_pharmacy", "傳藥局", 6, "system", true));
            context.WorkflowSteps.Add(MakeStep(StepDispensingId, "dispensing", "配藥中", 7, "pharmacist", false));
            context.WorkflowSteps.Add(MakeStep(StepReadyForPickupId, "ready_for_pickup", "待領藥", 8, "pharmacist", false));
            context.WorkflowSteps.Add(MakeStep(StepCompletedId, "completed", "完成", 9, null, false));
            changed = true;
        }

        // ── Workflow Transitions ──
        if (!context.WorkflowTransitions.Any(t => t.WorkflowDefinitionId == WorkflowId))
        {
            context.WorkflowTransitions.Add(MakeTransition(Guid.NewGuid(), StepCheckinId, StepWaitingId, 0, null));
            context.WorkflowTransitions.Add(MakeTransition(Guid.NewGuid(), StepWaitingId, StepCalledId, 0, null));
            context.WorkflowTransitions.Add(MakeTransition(Guid.NewGuid(), StepCalledId, StepConsultingId, 0, null));
            context.WorkflowTransitions.Add(MakeTransition(Guid.NewGuid(), StepConsultingId, StepPrescribedId, 0, null));
            context.WorkflowTransitions.Add(MakeTransition(Guid.NewGuid(),
                StepPrescribedId, StepSentToPharmacyId, 0,
                """{"condition":{"field":"visit.needs_medication","operator":"eq","value":true}}"""));
            context.WorkflowTransitions.Add(MakeTransition(Guid.NewGuid(),
                StepPrescribedId, StepCompletedId, 10,
                """{"skip_when":{"field":"visit.needs_medication","operator":"eq","value":false}}"""));
            context.WorkflowTransitions.Add(MakeTransition(Guid.NewGuid(), StepSentToPharmacyId, StepDispensingId, 0, null));
            context.WorkflowTransitions.Add(MakeTransition(Guid.NewGuid(), StepDispensingId, StepReadyForPickupId, 0, null));
            context.WorkflowTransitions.Add(MakeTransition(Guid.NewGuid(), StepReadyForPickupId, StepCompletedId, 0, null));
            changed = true;
        }

        // ── 示範科別 ──
        if (!context.Departments.Any(d => d.ClinicId == ClinicId))
        {
            context.Departments.Add(new Department { Id = DeptGenId, ClinicId = ClinicId, Name = "一般內科", Code = "GEN", IsActive = true, SortOrder = 1 });
            context.Departments.Add(new Department { Id = DeptDenId, ClinicId = ClinicId, Name = "牙科", Code = "DEN", IsActive = true, SortOrder = 2 });
            changed = true;
        }

        // ── 示範診間 ──
        if (!context.Rooms.Any(r => r.ClinicId == ClinicId))
        {
            context.Rooms.Add(new Room { Id = Room1Id, ClinicId = ClinicId, DepartmentId = DeptGenId, Name = "第一診", RoomType = RoomType.Consulting, IsActive = true, SortOrder = 1 });
            context.Rooms.Add(new Room { Id = Room2Id, ClinicId = ClinicId, DepartmentId = DeptGenId, Name = "第二診", RoomType = RoomType.Consulting, IsActive = true, SortOrder = 2 });
            changed = true;
        }

        if (changed)
        {
            await context.SaveChangesAsync();
        }
    }

    // ── Helper Methods ──

    private static WorkflowStep MakeStep(Guid id, string code, string displayName, int order, string? requiredRole, bool autoAdvance)
    {
        return new WorkflowStep
        {
            Id = id,
            ClinicId = ClinicId,
            WorkflowDefinitionId = WorkflowId,
            StepCode = code,
            DisplayName = displayName,
            StepOrder = order,
            RequiredRole = requiredRole,
            AutoAdvance = autoAdvance,
            IsSkippable = false,
            CreatedAt = SeedTime
        };
    }

    private static WorkflowTransition MakeTransition(Guid id, Guid fromStepId, Guid toStepId, int priority, string? conditionJson)
    {
        return new WorkflowTransition
        {
            Id = id,
            ClinicId = ClinicId,
            WorkflowDefinitionId = WorkflowId,
            FromStepId = fromStepId,
            ToStepId = toStepId,
            Priority = priority,
            ConditionJson = conditionJson,
            IsActive = true,
            CreatedAt = SeedTime
        };
    }
}
