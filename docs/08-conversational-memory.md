# 08 - Conversational Memory & State Management

## 🎯 Overview
In long-running conversations, passing every previous message verbatim quickly overflows the LLM's **Context Window** and escalates latency and compute costs.

This use case demonstrates how to architect an enterprise-grade **Hybrid Memory Manager** combining:
1. **Sliding Window Buffer**: Keeps the most recent $K$ turns verbatim.
2. **Periodic Conversational Summarizer**: Compresses historical turns into a concise executive summary.
3. **Structured Entity Store**: Tracks persistent key-value facts across turns (e.g., user preferences, project variables).

---

## 🧠 Key Concepts & AI Theory

```
┌────────────────────────────────────────────────────────┐
│               CONVERSATIONAL MEMORY MANAGER            │
│                                                        │
│  ┌──────────────────────┐    ┌──────────────────────┐  │
│  │ Structured Entity    │    │ Running Summary      │  │
│  │ State Store          │    │ (Distilled History)  │  │
│  │ - UserName: Sarah    │    │ "User is configuring │  │
│  │ - Cloud: Azure       │    │  Redis cache..."     │  │
│  └──────────────────────┘    └──────────────────────┘  │
│             │                            │             │
│             └─────────────┬──────────────┘             │
│                           │                            │
│                           ▼                            │
│              [System Instructions Context]             │
│                           │                            │
│                           ▼                            │
│  ┌──────────────────────────────────────────────────┐  │
│  │ Sliding Window Buffer (Last K=4 raw messages)    │  │
│  │ Turn 7 (User): "Can we enable clustering?"       │  │
│  │ Turn 7 (AI): "Yes, here is the ARM template..."  │  │
│  │ Turn 8 (User): "What port does it use?"          │  │
│  └──────────────────────────────────────────────────┘  │
└────────────────────────────────────────────────────────┘
```

---

## 💻 C# Implementation Walkthrough

Located at `src/08-ConversationalMemory/Services/MemoryManagerService.cs`:
```csharp
public class MemoryManagerService
{
    private readonly int _windowSize;
    private readonly List<ChatMessage> _slidingBuffer = new();
    private readonly Dictionary<string, string> _entityStore = new();
    private string _runningSummary = string.Empty;

    public void AddTurn(ChatRole role, string content)
    {
        _slidingBuffer.Add(new ChatMessage(role, content));
        if (_slidingBuffer.Count > _windowSize * 2)
        {
            // Trigger background summarization of older messages
            CompressHistory();
        }
    }

    public List<ChatMessage> BuildPrompt(string newUserInput)
    {
        var messages = new List<ChatMessage>();
        
        var memoryHeader = new StringBuilder();
        if (!string.IsNullOrEmpty(_runningSummary))
            memoryHeader.AppendLine($"[Conversation Summary]: {_runningSummary}");
        if (_entityStore.Count > 0)
            memoryHeader.AppendLine($"[Known Entities]: {JsonSerializer.Serialize(_entityStore)}");

        if (memoryHeader.Length > 0)
            messages.Add(new ChatMessage(ChatRole.System, memoryHeader.ToString()));

        messages.AddRange(_slidingBuffer);
        messages.Add(new ChatMessage(ChatRole.User, newUserInput));
        return messages;
    }
}
```

---

## 🧪 Running & Validating

### Run via .NET CLI:
```powershell
dotnet run --project src/08-ConversationalMemory -- --mock
```

### Run Unit Tests:
```powershell
dotnet test tests/DotNetAI.UseCases.Tests --filter "FullyQualifiedName~ConversationalMemoryTests"
```
