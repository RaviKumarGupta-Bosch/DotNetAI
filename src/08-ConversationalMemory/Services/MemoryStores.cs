using System.Diagnostics;
using System.Text;
using System.Text.Json;
using DotNetAI.ConversationalMemory.Models;
using DotNetAI.Core.Utils;
using Microsoft.Extensions.AI;

namespace DotNetAI.ConversationalMemory.Services;

/// <summary>
/// Sliding Window Memory maintains the last N messages verbatim to ensure conversation locality.
/// </summary>
public class SlidingWindowMemoryStore
{
    private readonly int _windowSize;
    private readonly List<MemoryItem> _allHistory = new();

    public SlidingWindowMemoryStore(int windowSize = 4)
    {
        _windowSize = windowSize;
    }

    public void AddMessage(string role, string content)
    {
        _allHistory.Add(new MemoryItem(role, content, DateTime.UtcNow, TokenEstimator.EstimateTokens(content)));
    }

    public IReadOnlyList<MemoryItem> GetRecentMessages()
    {
        return _allHistory.TakeLast(_windowSize).ToList();
    }

    public int TotalStoredMessages => _allHistory.Count;
}

/// <summary>
/// Semantic Summarizing Memory compresses evicted turns into a consolidated running summary using the LLM.
/// </summary>
public class SummarizingMemoryStore
{
    private string _runningSummary = string.Empty;
    private int _summarizedCount = 0;

    public string CurrentSummary => _runningSummary;
    public int SummarizedTurnCount => _summarizedCount;

    public async Task UpdateSummaryAsync(IChatClient client, IEnumerable<MemoryItem> evictedItems, CancellationToken ct = default)
    {
        var itemsToSummarize = evictedItems.ToList();
        if (itemsToSummarize.Count == 0) return;

        var sb = new StringBuilder();
        foreach (var item in itemsToSummarize)
        {
            sb.AppendLine($"{item.Role}: {item.Content}");
        }

        var prompt = $"""
        You are a Conversation Memory Summarizer. Update the existing running summary with the new conversation turns.
        Preserve crucial facts, user preferences, technical choices, and active tasks.

        Existing Summary:
        {(string.IsNullOrWhiteSpace(_runningSummary) ? "None (start of conversation)" : _runningSummary)}

        New Turns to Compress:
        {sb}

        Provide ONLY the updated concise summary text.
        """;

        var response = await client.GetResponseAsync(
            new ChatMessage[]
            {
                new(ChatRole.System, "Summarize concisely while preserving facts."),
                new(ChatRole.User, prompt)
            },
            new ChatOptions { Temperature = 0.0f },
            ct
        );

        _runningSummary = response.Text?.Trim() ?? _runningSummary;
        _summarizedCount += itemsToSummarize.Count;
    }
}

/// <summary>
/// Entity Memory extracts key-value facts (user name, preferences, project config) and persists them across turns.
/// </summary>
public class EntityMemoryStore
{
    private readonly Dictionary<string, ExtractedEntity> _entities = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyDictionary<string, ExtractedEntity> Entities => _entities;

    public void SetEntity(string key, string value, string category)
    {
        _entities[key] = new ExtractedEntity(key, value, category, DateTime.UtcNow);
    }

    public async Task ExtractEntitiesAsync(IChatClient client, string userMessage, CancellationToken ct = default)
    {
        var prompt = $$"""
        Extract any distinct personal facts, preferences, project names, technical stack choices, or system identifiers from the user message.
        Respond ONLY with a JSON array of objects:
        [
          {"Key": "UserName", "Value": "Alex", "Category": "Identity"},
          {"Key": "DatabaseChoice", "Value": "PostgreSQL", "Category": "TechStack"}
        ]
        If no distinct entities are present, respond with [].
        
        User Message:
        {{userMessage}}
        """;

        var response = await client.GetResponseAsync(
            new ChatMessage[]
            {
                new(ChatRole.System, "Extract structured key-value entities in valid JSON."),
                new(ChatRole.User, prompt)
            },
            new ChatOptions { Temperature = 0.0f },
            ct
        );

        var text = response.Text?.Trim() ?? "[]";
        if (text.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            text = text.Substring(7);
            if (text.EndsWith("```")) text = text.Substring(0, text.Length - 3);
            text = text.Trim();
        }
        else if (text.StartsWith("```"))
        {
            text = text.Substring(3);
            if (text.EndsWith("```")) text = text.Substring(0, text.Length - 3);
            text = text.Trim();
        }

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var list = JsonSerializer.Deserialize<List<ExtractedEntity>>(text, options);
            if (list != null)
            {
                foreach (var item in list)
                {
                    if (!string.IsNullOrWhiteSpace(item.Key) && !string.IsNullOrWhiteSpace(item.Value))
                    {
                        SetEntity(item.Key, item.Value, item.Category ?? "General");
                    }
                }
            }
        }
        catch
        {
            // Silently ignore extraction parsing noise
        }
    }
}
