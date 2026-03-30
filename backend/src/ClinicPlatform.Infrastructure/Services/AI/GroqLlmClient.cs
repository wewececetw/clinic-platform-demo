using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace ClinicPlatform.Infrastructure.Services.AI;

public class GroqLlmClient(HttpClient httpClient, IConfiguration configuration) : ILlmClient
{
    private const string Endpoint = "https://api.groq.com/openai/v1/chat/completions";
    private readonly string _apiKey = configuration["AI:Groq:ApiKey"] ?? "";
    private readonly string _model = configuration["AI:Groq:Model"] ?? "llama-3.3-70b-versatile";

    public string ProviderName => "Groq";

    public async Task<LlmResponse> ChatAsync(LlmRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(_apiKey))
            throw new InvalidOperationException("未設定 Groq API Key（AI:Groq:ApiKey）");

        var payload = new
        {
            model = request.Model ?? _model,
            messages = request.Messages.Select(m => new { role = m.Role, content = m.Content }),
            temperature = request.Temperature,
            max_tokens = request.MaxTokens
        };

        var json = JsonSerializer.Serialize(payload);
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, Endpoint)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        httpRequest.Headers.Add("Authorization", $"Bearer {_apiKey}");

        var response = await httpClient.SendAsync(httpRequest, ct);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<OpenAiChatResponse>(responseJson, JsonOptions);

        if (result?.Choices is not { Count: > 0 })
            throw new InvalidOperationException("Groq 未回傳任何內容");

        return new LlmResponse(
            Content: result.Choices[0].Message.Content,
            PromptTokens: result.Usage?.PromptTokens ?? 0,
            CompletionTokens: result.Usage?.CompletionTokens ?? 0);
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };
}
