using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ClinicPlatform.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "clinics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(UUID())"),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Slug = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "longtext", nullable: true),
                    Address = table.Column<string>(type: "longtext", nullable: true),
                    SettingsJson = table.Column<string>(type: "json", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clinics", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(UUID())"),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    NormalizedName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "departments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(UUID())"),
                    ClinicId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_departments_clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "medications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(UUID())"),
                    ClinicId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Code = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Unit = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    DefaultDosage = table.Column<string>(type: "longtext", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_medications_clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "notification_templates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(UUID())"),
                    ClinicId = table.Column<Guid>(type: "char(36)", nullable: false),
                    StepCode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Channel = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    TitleTemplate = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    BodyTemplate = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_templates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_notification_templates_clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "otp_verifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(UUID())"),
                    ClinicId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    OtpCode = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    IsUsed = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_otp_verifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_otp_verifications_clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "patients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(UUID())"),
                    ClinicId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Phone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    FullName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Gender = table.Column<string>(type: "longtext", nullable: true),
                    NationalId = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    Notes = table.Column<string>(type: "longtext", nullable: true),
                    IsAnonymous = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_patients_clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "queue_configs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(UUID())"),
                    ClinicId = table.Column<Guid>(type: "char(36)", nullable: false),
                    QueueType = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    PriorityWeight = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_queue_configs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_queue_configs_clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(UUID())"),
                    ClinicId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Email = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    DisplayName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "longtext", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_users_clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "workflow_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(UUID())"),
                    ClinicId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    IsDefault = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_definitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_definitions_clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "rooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(UUID())"),
                    ClinicId = table.Column<Guid>(type: "char(36)", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    RoomType = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rooms_clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_rooms_departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "patient_devices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(UUID())"),
                    ClinicId = table.Column<Guid>(type: "char(36)", nullable: false),
                    PatientId = table.Column<Guid>(type: "char(36)", nullable: false),
                    DeviceToken = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    P256dh = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    AuthKey = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    UserAgent = table.Column<string>(type: "longtext", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_patient_devices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_patient_devices_clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_patient_devices_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "appointments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(UUID())"),
                    ClinicId = table.Column<Guid>(type: "char(36)", nullable: false),
                    PatientId = table.Column<Guid>(type: "char(36)", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "char(36)", nullable: false),
                    DoctorId = table.Column<Guid>(type: "char(36)", nullable: true),
                    AppointmentDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    QrCodeToken = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "longtext", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_appointments_clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_appointments_departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_appointments_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_appointments_users_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(UUID())"),
                    ClinicId = table.Column<Guid>(type: "char(36)", nullable: false),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    EntityType = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    OldValues = table.Column<string>(type: "json", nullable: true),
                    NewValues = table.Column<string>(type: "json", nullable: true),
                    IpAddress = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_audit_logs_clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_audit_logs_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    RoleId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ClinicId = table.Column<Guid>(type: "char(36)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_user_roles_clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_roles_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_roles_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "workflow_steps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(UUID())"),
                    ClinicId = table.Column<Guid>(type: "char(36)", nullable: false),
                    WorkflowDefinitionId = table.Column<Guid>(type: "char(36)", nullable: false),
                    StepCode = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    StepOrder = table.Column<int>(type: "int", nullable: false),
                    RequiredRole = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    IsSkippable = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    AutoAdvance = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_steps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_steps_clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_workflow_steps_workflow_definitions_WorkflowDefinitionId",
                        column: x => x.WorkflowDefinitionId,
                        principalTable: "workflow_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "schedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(UUID())"),
                    ClinicId = table.Column<Guid>(type: "char(36)", nullable: false),
                    UserId = table.Column<Guid>(type: "char(36)", nullable: false),
                    DepartmentId = table.Column<Guid>(type: "char(36)", nullable: false),
                    RoomId = table.Column<Guid>(type: "char(36)", nullable: true),
                    DayOfWeekFlags = table.Column<int>(type: "int", nullable: false),
                    TimeSlot = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_schedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_schedules_clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_schedules_departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_schedules_rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_schedules_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "visits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(UUID())"),
                    ClinicId = table.Column<Guid>(type: "char(36)", nullable: false),
                    PatientId = table.Column<Guid>(type: "char(36)", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "char(36)", nullable: true),
                    WorkflowDefinitionId = table.Column<Guid>(type: "char(36)", nullable: false),
                    CurrentStepId = table.Column<Guid>(type: "char(36)", nullable: true),
                    DoctorId = table.Column<Guid>(type: "char(36)", nullable: true),
                    RoomId = table.Column<Guid>(type: "char(36)", nullable: true),
                    QueueNumber = table.Column<int>(type: "int", nullable: true),
                    CheckinMethod = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    NeedsMedication = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CheckedInAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_visits_appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_visits_clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_visits_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_visits_rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_visits_users_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_visits_workflow_definitions_WorkflowDefinitionId",
                        column: x => x.WorkflowDefinitionId,
                        principalTable: "workflow_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_visits_workflow_steps_CurrentStepId",
                        column: x => x.CurrentStepId,
                        principalTable: "workflow_steps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "workflow_transitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(UUID())"),
                    ClinicId = table.Column<Guid>(type: "char(36)", nullable: false),
                    WorkflowDefinitionId = table.Column<Guid>(type: "char(36)", nullable: false),
                    FromStepId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ToStepId = table.Column<Guid>(type: "char(36)", nullable: false),
                    ConditionJson = table.Column<string>(type: "json", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_transitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_workflow_transitions_clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_workflow_transitions_workflow_definitions_WorkflowDefinition~",
                        column: x => x.WorkflowDefinitionId,
                        principalTable: "workflow_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_workflow_transitions_workflow_steps_FromStepId",
                        column: x => x.FromStepId,
                        principalTable: "workflow_steps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_workflow_transitions_workflow_steps_ToStepId",
                        column: x => x.ToStepId,
                        principalTable: "workflow_steps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "notification_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(UUID())"),
                    ClinicId = table.Column<Guid>(type: "char(36)", nullable: false),
                    VisitId = table.Column<Guid>(type: "char(36)", nullable: true),
                    PatientId = table.Column<Guid>(type: "char(36)", nullable: true),
                    Channel = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    ErrorMessage = table.Column<string>(type: "longtext", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notification_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_notification_logs_clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notification_logs_patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_notification_logs_visits_VisitId",
                        column: x => x.VisitId,
                        principalTable: "visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "prescriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(UUID())"),
                    ClinicId = table.Column<Guid>(type: "char(36)", nullable: false),
                    VisitId = table.Column<Guid>(type: "char(36)", nullable: false),
                    DoctorId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "longtext", nullable: true),
                    PrescribedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SentToPharmacyAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    DispensedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    PickedUpAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prescriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prescriptions_clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_prescriptions_users_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_prescriptions_visits_VisitId",
                        column: x => x.VisitId,
                        principalTable: "visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "queue_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(UUID())"),
                    ClinicId = table.Column<Guid>(type: "char(36)", nullable: false),
                    VisitId = table.Column<Guid>(type: "char(36)", nullable: false),
                    QueueType = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    QueueNumber = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    CalledAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SkippedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_queue_entries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_queue_entries_clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_queue_entries_visits_VisitId",
                        column: x => x.VisitId,
                        principalTable: "visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "visit_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(UUID())"),
                    ClinicId = table.Column<Guid>(type: "char(36)", nullable: false),
                    VisitId = table.Column<Guid>(type: "char(36)", nullable: false),
                    FromStepId = table.Column<Guid>(type: "char(36)", nullable: true),
                    ToStepId = table.Column<Guid>(type: "char(36)", nullable: true),
                    TriggerType = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    TriggeredByUserId = table.Column<Guid>(type: "char(36)", nullable: true),
                    Notes = table.Column<string>(type: "longtext", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_visit_events", x => x.Id);
                    table.ForeignKey(
                        name: "FK_visit_events_clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_visit_events_users_TriggeredByUserId",
                        column: x => x.TriggeredByUserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_visit_events_visits_VisitId",
                        column: x => x.VisitId,
                        principalTable: "visits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_visit_events_workflow_steps_FromStepId",
                        column: x => x.FromStepId,
                        principalTable: "workflow_steps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_visit_events_workflow_steps_ToStepId",
                        column: x => x.ToStepId,
                        principalTable: "workflow_steps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "prescription_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValueSql: "(UUID())"),
                    ClinicId = table.Column<Guid>(type: "char(36)", nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "char(36)", nullable: false),
                    MedicationId = table.Column<Guid>(type: "char(36)", nullable: false),
                    Dosage = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Frequency = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    DurationDays = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Instructions = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prescription_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_prescription_items_clinics_ClinicId",
                        column: x => x.ClinicId,
                        principalTable: "clinics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_prescription_items_medications_MedicationId",
                        column: x => x.MedicationId,
                        principalTable: "medications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_prescription_items_prescriptions_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "prescriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.InsertData(
                table: "clinics",
                columns: new[] { "Id", "Address", "CreatedAt", "IsActive", "Name", "Phone", "SettingsJson", "Slug", "UpdatedAt" },
                values: new object[] { new Guid("10000000-0000-0000-0000-000000000001"), "台北市信義區示範路1號", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "示範診所", "02-1234-5678", "{\"allowed_checkin_methods\":[\"Otp\",\"QrCode\",\"Manual\"],\"business_hours\":{\"start\":\"09:00\",\"end\":\"18:00\"}}", "demo-clinic", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "Id", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), "Admin", "ADMIN" },
                    { new Guid("00000000-0000-0000-0000-000000000002"), "Nurse", "NURSE" },
                    { new Guid("00000000-0000-0000-0000-000000000003"), "Doctor", "DOCTOR" },
                    { new Guid("00000000-0000-0000-0000-000000000004"), "Pharmacist", "PHARMACIST" }
                });

            migrationBuilder.InsertData(
                table: "workflow_definitions",
                columns: new[] { "Id", "ClinicId", "CreatedAt", "Description", "IsActive", "IsDefault", "Name", "UpdatedAt" },
                values: new object[] { new Guid("20000000-0000-0000-0000-000000000001"), new Guid("10000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "報到→候診→叫號→看診→開處方→傳藥局→配藥→叫領藥→離院", true, true, "一般門診流程", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "workflow_steps",
                columns: new[] { "Id", "ClinicId", "CreatedAt", "DisplayName", "RequiredRole", "StepCode", "StepOrder", "WorkflowDefinitionId" },
                values: new object[,]
                {
                    { new Guid("30000000-0000-0000-0000-000000000001"), new Guid("10000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "報到", "Nurse", "check_in", 1, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("30000000-0000-0000-0000-000000000002"), new Guid("10000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "候診", null, "waiting", 2, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("30000000-0000-0000-0000-000000000003"), new Guid("10000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "叫號進診間", "Nurse", "called", 3, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("30000000-0000-0000-0000-000000000004"), new Guid("10000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "醫師看診", "Doctor", "consulting", 4, new Guid("20000000-0000-0000-0000-000000000001") }
                });

            migrationBuilder.InsertData(
                table: "workflow_steps",
                columns: new[] { "Id", "ClinicId", "CreatedAt", "DisplayName", "IsSkippable", "RequiredRole", "StepCode", "StepOrder", "WorkflowDefinitionId" },
                values: new object[] { new Guid("30000000-0000-0000-0000-000000000005"), new Guid("10000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "開立處方", true, "Doctor", "prescribed", 5, new Guid("20000000-0000-0000-0000-000000000001") });

            migrationBuilder.InsertData(
                table: "workflow_steps",
                columns: new[] { "Id", "AutoAdvance", "ClinicId", "CreatedAt", "DisplayName", "IsSkippable", "RequiredRole", "StepCode", "StepOrder", "WorkflowDefinitionId" },
                values: new object[] { new Guid("30000000-0000-0000-0000-000000000006"), true, new Guid("10000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "處方傳藥局", true, null, "sent_to_pharmacy", 6, new Guid("20000000-0000-0000-0000-000000000001") });

            migrationBuilder.InsertData(
                table: "workflow_steps",
                columns: new[] { "Id", "ClinicId", "CreatedAt", "DisplayName", "IsSkippable", "RequiredRole", "StepCode", "StepOrder", "WorkflowDefinitionId" },
                values: new object[,]
                {
                    { new Guid("30000000-0000-0000-0000-000000000007"), new Guid("10000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "藥劑師配藥", true, "Pharmacist", "dispensing", 7, new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("30000000-0000-0000-0000-000000000008"), new Guid("10000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "叫領藥號", true, "Nurse", "ready_for_pickup", 8, new Guid("20000000-0000-0000-0000-000000000001") }
                });

            migrationBuilder.InsertData(
                table: "workflow_steps",
                columns: new[] { "Id", "ClinicId", "CreatedAt", "DisplayName", "RequiredRole", "StepCode", "StepOrder", "WorkflowDefinitionId" },
                values: new object[] { new Guid("30000000-0000-0000-0000-000000000009"), new Guid("10000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "離院", null, "completed", 9, new Guid("20000000-0000-0000-0000-000000000001") });

            migrationBuilder.InsertData(
                table: "workflow_transitions",
                columns: new[] { "Id", "ClinicId", "ConditionJson", "CreatedAt", "FromStepId", "IsActive", "ToStepId", "WorkflowDefinitionId" },
                values: new object[,]
                {
                    { new Guid("40000000-0000-0000-0000-000000000001"), new Guid("10000000-0000-0000-0000-000000000001"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("30000000-0000-0000-0000-000000000001"), true, new Guid("30000000-0000-0000-0000-000000000002"), new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("40000000-0000-0000-0000-000000000002"), new Guid("10000000-0000-0000-0000-000000000001"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("30000000-0000-0000-0000-000000000002"), true, new Guid("30000000-0000-0000-0000-000000000003"), new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("40000000-0000-0000-0000-000000000003"), new Guid("10000000-0000-0000-0000-000000000001"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("30000000-0000-0000-0000-000000000003"), true, new Guid("30000000-0000-0000-0000-000000000004"), new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("40000000-0000-0000-0000-000000000004"), new Guid("10000000-0000-0000-0000-000000000001"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("30000000-0000-0000-0000-000000000004"), true, new Guid("30000000-0000-0000-0000-000000000005"), new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("40000000-0000-0000-0000-000000000005"), new Guid("10000000-0000-0000-0000-000000000001"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("30000000-0000-0000-0000-000000000005"), true, new Guid("30000000-0000-0000-0000-000000000006"), new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("40000000-0000-0000-0000-000000000006"), new Guid("10000000-0000-0000-0000-000000000001"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("30000000-0000-0000-0000-000000000006"), true, new Guid("30000000-0000-0000-0000-000000000007"), new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("40000000-0000-0000-0000-000000000007"), new Guid("10000000-0000-0000-0000-000000000001"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("30000000-0000-0000-0000-000000000007"), true, new Guid("30000000-0000-0000-0000-000000000008"), new Guid("20000000-0000-0000-0000-000000000001") },
                    { new Guid("40000000-0000-0000-0000-000000000008"), new Guid("10000000-0000-0000-0000-000000000001"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("30000000-0000-0000-0000-000000000008"), true, new Guid("30000000-0000-0000-0000-000000000009"), new Guid("20000000-0000-0000-0000-000000000001") }
                });

            migrationBuilder.InsertData(
                table: "workflow_transitions",
                columns: new[] { "Id", "ClinicId", "ConditionJson", "CreatedAt", "FromStepId", "IsActive", "Priority", "ToStepId", "WorkflowDefinitionId" },
                values: new object[] { new Guid("40000000-0000-0000-0000-000000000009"), new Guid("10000000-0000-0000-0000-000000000001"), "{\"skip_when\":{\"field\":\"visit.needs_medication\",\"operator\":\"eq\",\"value\":false}}", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("30000000-0000-0000-0000-000000000004"), true, 10, new Guid("30000000-0000-0000-0000-000000000009"), new Guid("20000000-0000-0000-0000-000000000001") });

            migrationBuilder.CreateIndex(
                name: "IX_appointments_ClinicId_AppointmentDate",
                table: "appointments",
                columns: new[] { "ClinicId", "AppointmentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_appointments_ClinicId_PatientId",
                table: "appointments",
                columns: new[] { "ClinicId", "PatientId" });

            migrationBuilder.CreateIndex(
                name: "IX_appointments_DepartmentId",
                table: "appointments",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_DoctorId",
                table: "appointments",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_PatientId",
                table: "appointments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_appointments_QrCodeToken",
                table: "appointments",
                column: "QrCodeToken",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_ClinicId_CreatedAt",
                table: "audit_logs",
                columns: new[] { "ClinicId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_ClinicId_EntityType_EntityId",
                table: "audit_logs",
                columns: new[] { "ClinicId", "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_UserId",
                table: "audit_logs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_clinics_IsActive",
                table: "clinics",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_clinics_Slug",
                table: "clinics",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_departments_ClinicId_Code",
                table: "departments",
                columns: new[] { "ClinicId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_medications_ClinicId_Code",
                table: "medications",
                columns: new[] { "ClinicId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_notification_logs_ClinicId_CreatedAt",
                table: "notification_logs",
                columns: new[] { "ClinicId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_logs_ClinicId_VisitId",
                table: "notification_logs",
                columns: new[] { "ClinicId", "VisitId" });

            migrationBuilder.CreateIndex(
                name: "IX_notification_logs_PatientId",
                table: "notification_logs",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_notification_logs_VisitId",
                table: "notification_logs",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_notification_templates_ClinicId_StepCode",
                table: "notification_templates",
                columns: new[] { "ClinicId", "StepCode" });

            migrationBuilder.CreateIndex(
                name: "IX_otp_verifications_ClinicId",
                table: "otp_verifications",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_otp_verifications_ExpiresAt",
                table: "otp_verifications",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_otp_verifications_Phone_OtpCode",
                table: "otp_verifications",
                columns: new[] { "Phone", "OtpCode" });

            migrationBuilder.CreateIndex(
                name: "IX_patient_devices_ClinicId_PatientId",
                table: "patient_devices",
                columns: new[] { "ClinicId", "PatientId" });

            migrationBuilder.CreateIndex(
                name: "IX_patient_devices_PatientId",
                table: "patient_devices",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_patients_ClinicId_Phone",
                table: "patients",
                columns: new[] { "ClinicId", "Phone" });

            migrationBuilder.CreateIndex(
                name: "IX_prescription_items_ClinicId",
                table: "prescription_items",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_prescription_items_MedicationId",
                table: "prescription_items",
                column: "MedicationId");

            migrationBuilder.CreateIndex(
                name: "IX_prescription_items_PrescriptionId",
                table: "prescription_items",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_prescriptions_ClinicId_Status",
                table: "prescriptions",
                columns: new[] { "ClinicId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_prescriptions_ClinicId_VisitId",
                table: "prescriptions",
                columns: new[] { "ClinicId", "VisitId" });

            migrationBuilder.CreateIndex(
                name: "IX_prescriptions_DoctorId",
                table: "prescriptions",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_prescriptions_VisitId",
                table: "prescriptions",
                column: "VisitId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_queue_configs_ClinicId_QueueType",
                table: "queue_configs",
                columns: new[] { "ClinicId", "QueueType" });

            migrationBuilder.CreateIndex(
                name: "IX_queue_entries_ClinicId_QueueType_Status",
                table: "queue_entries",
                columns: new[] { "ClinicId", "QueueType", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_queue_entries_ClinicId_VisitId",
                table: "queue_entries",
                columns: new[] { "ClinicId", "VisitId" });

            migrationBuilder.CreateIndex(
                name: "IX_queue_entries_VisitId",
                table: "queue_entries",
                column: "VisitId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_NormalizedName",
                table: "roles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rooms_ClinicId_DepartmentId",
                table: "rooms",
                columns: new[] { "ClinicId", "DepartmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_rooms_DepartmentId",
                table: "rooms",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_schedules_ClinicId_UserId",
                table: "schedules",
                columns: new[] { "ClinicId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_schedules_DepartmentId",
                table: "schedules",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_schedules_RoomId",
                table: "schedules",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_schedules_UserId",
                table: "schedules",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_ClinicId",
                table: "user_roles",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_RoleId",
                table: "user_roles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_users_ClinicId",
                table: "users",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_users_ClinicId_Email",
                table: "users",
                columns: new[] { "ClinicId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_visit_events_ClinicId_VisitId_CreatedAt",
                table: "visit_events",
                columns: new[] { "ClinicId", "VisitId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_visit_events_FromStepId",
                table: "visit_events",
                column: "FromStepId");

            migrationBuilder.CreateIndex(
                name: "IX_visit_events_ToStepId",
                table: "visit_events",
                column: "ToStepId");

            migrationBuilder.CreateIndex(
                name: "IX_visit_events_TriggeredByUserId",
                table: "visit_events",
                column: "TriggeredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_visit_events_VisitId",
                table: "visit_events",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_visits_AppointmentId",
                table: "visits",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_visits_ClinicId_CheckedInAt",
                table: "visits",
                columns: new[] { "ClinicId", "CheckedInAt" });

            migrationBuilder.CreateIndex(
                name: "IX_visits_ClinicId_PatientId",
                table: "visits",
                columns: new[] { "ClinicId", "PatientId" });

            migrationBuilder.CreateIndex(
                name: "IX_visits_ClinicId_Status",
                table: "visits",
                columns: new[] { "ClinicId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_visits_CurrentStepId",
                table: "visits",
                column: "CurrentStepId");

            migrationBuilder.CreateIndex(
                name: "IX_visits_DoctorId",
                table: "visits",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_visits_PatientId",
                table: "visits",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_visits_RoomId",
                table: "visits",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_visits_WorkflowDefinitionId",
                table: "visits",
                column: "WorkflowDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_definitions_ClinicId_IsDefault",
                table: "workflow_definitions",
                columns: new[] { "ClinicId", "IsDefault" });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_steps_ClinicId_WorkflowDefinitionId",
                table: "workflow_steps",
                columns: new[] { "ClinicId", "WorkflowDefinitionId" });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_steps_WorkflowDefinitionId_StepOrder",
                table: "workflow_steps",
                columns: new[] { "WorkflowDefinitionId", "StepOrder" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_workflow_transitions_ClinicId",
                table: "workflow_transitions",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_transitions_FromStepId",
                table: "workflow_transitions",
                column: "FromStepId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_transitions_ToStepId",
                table: "workflow_transitions",
                column: "ToStepId");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_transitions_WorkflowDefinitionId_FromStepId",
                table: "workflow_transitions",
                columns: new[] { "WorkflowDefinitionId", "FromStepId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "notification_logs");

            migrationBuilder.DropTable(
                name: "notification_templates");

            migrationBuilder.DropTable(
                name: "otp_verifications");

            migrationBuilder.DropTable(
                name: "patient_devices");

            migrationBuilder.DropTable(
                name: "prescription_items");

            migrationBuilder.DropTable(
                name: "queue_configs");

            migrationBuilder.DropTable(
                name: "queue_entries");

            migrationBuilder.DropTable(
                name: "schedules");

            migrationBuilder.DropTable(
                name: "user_roles");

            migrationBuilder.DropTable(
                name: "visit_events");

            migrationBuilder.DropTable(
                name: "workflow_transitions");

            migrationBuilder.DropTable(
                name: "medications");

            migrationBuilder.DropTable(
                name: "prescriptions");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "visits");

            migrationBuilder.DropTable(
                name: "appointments");

            migrationBuilder.DropTable(
                name: "rooms");

            migrationBuilder.DropTable(
                name: "workflow_steps");

            migrationBuilder.DropTable(
                name: "patients");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "departments");

            migrationBuilder.DropTable(
                name: "workflow_definitions");

            migrationBuilder.DropTable(
                name: "clinics");
        }
    }
}
