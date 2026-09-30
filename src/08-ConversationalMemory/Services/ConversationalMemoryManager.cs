using System.Diagnostics;
using System.Text;
using DotNetAI.ConversationalMemory.Models;
using Microsoft.Extensions.AI;

namespace DotNetAI.ConversationalMemory.Services;

public class ConversationalMemoryManager
{
    private readonly SlidingWindowMemoryStore _windowStore;
    private readonly SummarizingMemoryStore _summaryStore;
    private readonly EntityMemoryStore _entityStore;

    public ConversationalMemoryManager(int windowSize = 4)
    {
        _windowStore = new SlidingWindowMemoryStore(windowSize);
        _summaryStore = new SummarizingMemoryStore();
        _entityStore = new EntityMemoryStore();
    }

    public MemoryDiagnostics GetDiagnostics()
    {
        var recent = _windowStore.GetRecentMessages();
        return new MemoryDiagnostics(
            TotalMessagesStored: _windowStore.TotalStoredMessages,
            WindowSize: 4,
            ActiveWindowMessages: recent.Count,
            RunningSummary: string.IsNullOrWhiteSpace(_summaryStore.CurrentSummary) ? null : _summaryStore.CurrentSummary,
            KnownEntities: _entityStore.Entities
        );
    }

    public async Task<string> ProcessUserTurnAsync(IChatClient client, string userMessage, CancellationToken ct = default)
    {
        // 1. Extract and store any new entities
        await _entityStore.ExtractEntitiesAsync(client, userMessage, ct);

        // 2. Build Memory Context for Prompt Injection
        var memoryContext = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(_summaryStore.CurrentSummary))
        {
            memoryContext.AppendLine("--- Summary of Earlier Conversation ---");
            memoryContext.AppendLine(_summaryStore.CurrentSummary);
            memoryContext.AppendLine();
        }

        if (_entityStore.Entities.Count > 0)
        {
            memoryContext.AppendLine("--- Known Entities & User Facts ---");
            foreach (var e in _entityStore.Entities.Values)
            {
                memoryContext.AppendLine($"- {e.Key}: {e.Value} ({e.Category})");
            }
            memoryContext.AppendLine();
        }

        var systemPrompt = $"""
        You are an intelligent conversational assistant with long-term memory.
        Use the following memory context to answer personal or context-dependent queries seamlessly.

        {memoryContext}
        """;

        var conversation = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt)
        };

        // Add recent sliding window turns
        var recentTurns = _windowStore.GetRecentMessages();
        foreach (var turn in recentTurns)
        {
            var role = turn.Role.Equals("User", StringComparison.OrdinalIgnoreCase) ? ChatRole.User : ChatRole.Assistant;
            conversation.Add(new ChatMessage(role, turn.Content));
        }

        // Add current user turn
        conversation.Add(new ChatMessage(ChatRole.User, userMessage));

        // 3. Generate response
        var response = await client.GetResponseAsync(conversation, new ChatOptions { Temperature = 0.2f }, ct);
        var assistantReply = response.Text ?? string.Empty;

        // 4. Update memory stores
        _windowStore.AddMessage("User", userMessage);
        _windowStore.AddMessage("Assistant", assistantReply);

        // Periodically update summary if message count exceeds threshold
        if (_windowStore.TotalStoredMessages > 6 && _windowStore.TotalStoredMessages % 4 == 0)
        {
            var oldest = _windowStore.GetRecentMessages().Take(2);
            await _summaryStore.UpdateSummaryAsync(client, oldest, ct);
        }

        return assistantReply;
    }
}
