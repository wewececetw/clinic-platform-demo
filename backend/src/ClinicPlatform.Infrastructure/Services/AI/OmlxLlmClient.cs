using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;

namespace ClinicPlatform.Infrastructure.Services.AI;

public class OmlxLlmClient(HttpClient httpClient, IConfiguration configuration) : ILlmClient
{
    private readonly string _endpoint = configuration["AI:Omlx:Endpoint"] ?? "http://localhost:9000/v1/chat/completions";
    private readonly string _apiKey = configuration["AI:Omlx:ApiKey"] ?? "";
    private readonly string _model = configuration["AI:Omlx:Model"] ?? "Qwen3.5-9B-MLX-4bit";

    public string ProviderName => "Omlx";

    public async Task<LlmResponse> ChatAsync(LlmRequest request, CancellationToken ct = default)
    {
        var payload = new
        {
            model = request.Model ?? _model,
            messages = request.Messages.Select(m => new { role = m.Role, content = m.Content }),
            temperature = request.Temperature,
            max_tokens = request.MaxTokens
        };

        var json = JsonSerializer.Serialize(payload);
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, _endpoint)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        if (!string.IsNullOrEmpty(_apiKey))
            httpRequest.Headers.Add("Authorization", $"Bearer {_apiKey}");

        var response = await httpClient.SendAsync(httpRequest, ct);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<OpenAiChatResponse>(responseJson, JsonOptions);

        if (result?.Choices is not { Count: > 0 })
            throw new InvalidOperationException("LLM 未回傳任何內容");

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

// OpenAI 相容格式的回應模型（OMLX 和 Groq 共用）
public class OpenAiChatResponse
{
    [JsonPropertyName("choices")]
    public List<OpenAiChoice> Choices { get; set; } = [];

    [JsonPropertyName("usage")]
    public OpenAiUsage? Usage { get; set; }
}

public class OpenAiChoice
{
    [JsonPropertyName("message")]
    public OpenAiMessage Message { get; set; } = new();
}

public class OpenAiMessage
{
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}

public class OpenAiUsage
{
    [JsonPropertyName("prompt_tokens")]
    public int PromptTokens { get; set; }

    [JsonPropertyName("completion_tokens")]
    public int CompletionTokens { get; set; }
}
