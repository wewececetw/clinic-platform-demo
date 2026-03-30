namespace ClinicPlatform.Infrastructure.Services.AI;

public interface ILlmClient
{
    Task<LlmResponse> ChatAsync(LlmRequest request, CancellationToken ct = default);
    string ProviderName { get; }
}
