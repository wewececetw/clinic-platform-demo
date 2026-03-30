using ClinicPlatform.Application.Features.AI;
using ClinicPlatform.Application.Features.CheckIn;
using ClinicPlatform.Application.Features.Prescription;
using ClinicPlatform.Application.Features.Queue;
using ClinicPlatform.Application.Features.Visit;
using ClinicPlatform.Application.Features.Workflow;
using ClinicPlatform.Infrastructure.Persistence;
using ClinicPlatform.Infrastructure.Services;
using ClinicPlatform.Infrastructure.Services.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicPlatform.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")!;

        services.AddDbContext<ClinicDbContext>(options =>
            options.UseMySQL(connectionString));

        services.AddScoped<ICheckInService, CheckInService>();
        services.AddScoped<IQueueService, QueueService>();
        services.AddScoped<IVisitService, VisitService>();
        services.AddScoped<IPrescriptionService, PrescriptionService>();
        services.AddScoped<IWorkflowEngine, WorkflowEngine>();

        // AI 服務
        services.AddHttpClient<OmlxLlmClient>();
        services.AddHttpClient<GroqLlmClient>();
        services.AddScoped<ILlmClient, OmlxLlmClient>();
        services.AddScoped<ILlmClient, GroqLlmClient>();
        services.AddScoped<IAiService, AiService>();

        return services;
    }
}
