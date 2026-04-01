using System.Text.Json;
using ClinicPlatform.Application.Common;
using ClinicPlatform.Application.Features.AI;
using ClinicPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ClinicPlatform.Infrastructure.Services.AI;

public class AiService(
    IEnumerable<ILlmClient> llmClients,
    ClinicDbContext db,
    IConfiguration configuration,
    ILogger<AiService> logger) : IAiService
{
    public async Task<Result<TriageResponse>> TriageAsync(TriageRequest request)
    {
        // 取得該院所的科別清單
        var departments = await db.Departments
            .Where(d => d.ClinicId == request.ClinicId && d.IsActive)
            .Select(d => new DepartmentInfo(d.Id.ToString(), d.Name))
            .ToListAsync();

        if (departments.Count == 0)
            return Result<TriageResponse>.Fail("該院所尚無可用科別");

        var systemPrompt = TriagePromptBuilder.BuildSystemPrompt(departments);
        var userPrompt = TriagePromptBuilder.BuildUserPrompt(request.Symptoms);

        var llmRequest = new LlmRequest(
            Model: null!,
            Messages:
            [
                new LlmMessage("system", systemPrompt),
                new LlmMessage("user", userPrompt),
            ],
            Temperature: 0.2f,
            MaxTokens: 256);

        // 依優先序嘗試各 LLM provider
        var preferredProvider = configuration["AI:Provider"] ?? "Omlx";
        var orderedClients = llmClients
            .OrderByDescending(c => c.ProviderName == preferredProvider)
            .ToList();

        foreach (var client in orderedClients)
        {
            try
            {
                logger.LogInformation("嘗試使用 {Provider} 進行 AI 分流", client.ProviderName);
                var response = await client.ChatAsync(llmRequest);
                logger.LogDebug("LLM 原始回應：{Content}", response.Content[..Math.Min(500, response.Content.Length)]);
                var triage = ParseTriageResponse(response.Content, departments);
                if (triage is not null)
                    return Result<TriageResponse>.Ok(triage);

                logger.LogWarning("{Provider} 回應無法解析，嘗試下一個 provider", client.ProviderName);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "{Provider} 呼叫失敗，嘗試下一個 provider", client.ProviderName);
            }
        }

        return Result<TriageResponse>.Fail("AI 分流服務暫時無法使用，請手動選擇科別");
    }

    public async Task<Result<CommandResponse>> CommandAsync(CommandRequest request)
    {
        var systemPrompt = CommandPromptBuilder.BuildSystemPrompt(request.Role);
        var userPrompt = CommandPromptBuilder.BuildUserPrompt(request.Command);

        var llmRequest = new LlmRequest(
            Model: null!,
            Messages:
            [
                new LlmMessage("system", systemPrompt),
                new LlmMessage("user", userPrompt),
            ],
            Temperature: 0.1f,
            MaxTokens: 256);

        var preferredProvider = configuration["AI:Provider"] ?? "Omlx";
        var orderedClients = llmClients
            .OrderByDescending(c => c.ProviderName == preferredProvider)
            .ToList();

        foreach (var client in orderedClients)
        {
            try
            {
                logger.LogInformation("嘗試使用 {Provider} 進行指令解析", client.ProviderName);
                var response = await client.ChatAsync(llmRequest);
                logger.LogDebug("指令 LLM 原始回應：{Content}", response.Content[..Math.Min(500, response.Content.Length)]);
                var command = ParseCommandResponse(response.Content);
                if (command is not null)
                    return Result<CommandResponse>.Ok(command);

                logger.LogWarning("{Provider} 指令回應無法解析", client.ProviderName);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "{Provider} 指令呼叫失敗", client.ProviderName);
            }
        }

        return Result<CommandResponse>.Fail("指令解析失敗，請換個說法再試一次");
    }

    private static CommandResponse? ParseCommandResponse(string content)
    {
        try
        {
            var json = ExtractJson(content, "action");
            if (json is null) return null;

            var parsed = JsonSerializer.Deserialize<JsonElement>(json);

            var action = parsed.GetProperty("action").GetString() ?? "unknown";
            var message = parsed.TryGetProperty("message", out var m) ? m.GetString() ?? "" : "";

            Dictionary<string, object>? parameters = null;
            if (parsed.TryGetProperty("params", out var p) && p.ValueKind == JsonValueKind.Object)
            {
                parameters = new Dictionary<string, object>();
                foreach (var prop in p.EnumerateObject())
                {
                    parameters[prop.Name] = prop.Value.ValueKind switch
                    {
                        JsonValueKind.String => prop.Value.GetString()!,
                        JsonValueKind.Number => prop.Value.GetDouble(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        _ => prop.Value.ToString()
                    };
                }
            }

            var needsConfirm = parsed.TryGetProperty("needsConfirm", out var nc) && nc.GetBoolean();
            var result = needsConfirm ? "confirm" : "done";

            return new CommandResponse(action, parameters, result, message);
        }
        catch
        {
            return null;
        }
    }

    private static TriageResponse? ParseTriageResponse(string content, List<DepartmentInfo> departments)
    {
        try
        {
            // 從回應中提取包含 "department" 的 JSON（跳過 thinking process）
            var json = ExtractJson(content, "department");
            if (json is null) return null;

            var parsed = JsonSerializer.Deserialize<JsonElement>(json);

            var department = parsed.GetProperty("department").GetString() ?? "";
            var reasoning = parsed.GetProperty("reasoning").GetString() ?? "";
            var priority = parsed.TryGetProperty("priority", out var p) ? p.GetInt32() : 0;
            var wait = parsed.TryGetProperty("estimatedWaitMinutes", out var w) ? w.GetInt32() : 15;

            // 嘗試匹配科別 ID
            Guid? deptId = null;
            if (parsed.TryGetProperty("departmentId", out var dId) &&
                Guid.TryParse(dId.GetString(), out var parsedId))
            {
                deptId = parsedId;
            }
            else
            {
                var match = departments.FirstOrDefault(d => d.Name == department);
                if (match is not null) deptId = Guid.Parse(match.Id);
            }

            return new TriageResponse(department, deptId, priority, wait, reasoning);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// 從 LLM 回應中提取含指定 key 的 JSON 物件
    /// </summary>
    private static string? ExtractJson(string content, string requiredKey = "department")
    {
        var idx = content.Length - 1;
        while (idx >= 0)
        {
            var end = content.LastIndexOf('}', idx);
            if (end < 0) break;

            var depth = 0;
            for (var i = end; i >= 0; i--)
            {
                if (content[i] == '}') depth++;
                else if (content[i] == '{') depth--;

                if (depth == 0)
                {
                    var candidate = content[i..(end + 1)];
                    if (candidate.Contains(requiredKey))
                        return candidate;
                    break;
                }
            }
            idx = end - 1;
        }
        return null;
    }
}
