namespace ClinicPlatform.Infrastructure.Services.AI;

public record LlmMessage(string Role, string Content);

public record LlmRequest(string Model, List<LlmMessage> Messages, float Temperature = 0.3f, int MaxTokens = 1024);

public record LlmResponse(string Content, int PromptTokens, int CompletionTokens);
