using ClinicPlatform.Application.Features.Notifications;
using ClinicPlatform.Infrastructure;
using ClinicPlatform.WebAPI.Notifications;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddSignalR();
builder.Services.AddSingleton<INotificationPublisher, SignalRNotificationPublisher>();

builder.Services.AddCors(options =>
{
    // SignalR 需要 AllowCredentials，因此必須指定明確 origin（不能用 AllowAnyOrigin）
    options.AddPolicy("AllowFrontend", policy =>
        policy.WithOrigins("http://localhost:5173", "http://localhost:4173")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials());
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ClinicPlatform.Infrastructure.Persistence.ClinicDbContext>();
    await ClinicPlatform.Infrastructure.Persistence.SeedData.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowFrontend");
app.UseHttpsRedirection();
app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck");

app.MapHub<ClinicPlatform.WebAPI.Hubs.VisitHub>("/hubs/visit");

app.Run();
