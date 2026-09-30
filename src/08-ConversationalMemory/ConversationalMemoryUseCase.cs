using System.Diagnostics;
using DotNetAI.ConversationalMemory.Services;
using DotNetAI.Core.Interfaces;
using Microsoft.Extensions.AI;

namespace DotNetAI.ConversationalMemory;

/// <summary>
/// Use Case 08: Conversational Memory & State Management.
/// </summary>
public class ConversationalMemoryUseCase : IAIUseCase
{
    public int Id => 8;
    public string Name => "Conversational Memory & State Tracking";
    public string Description => "Demonstrates multi-turn conversation memory architectures: Sliding Window buffering, Semantic Running Summarization, and Structured Entity Fact Extraction across extended dialogues.";
    public IReadOnlyList<string> AIConcepts => new[]
    {
        "Stateless LLM Memory Architectures",
        "Sliding Window Context Buffering",
        "Semantic Summary Compression (Token Conservation)",
        "Structured Key-Value Entity Memory Extraction",
        "Context Injection & Long-Term Recall"
    };

    public async Task<UseCaseExecutionResult> RunAsync(
        IChatClient chatClient, 
        IEmbeddingGenerator<string, Embedding<float>>? embeddingGenerator = null, 
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var result = new UseCaseExecutionResult();
        result.Logs.Add("[UseCase 08] Starting Conversational Memory & State Tracking demo...");

        var memoryManager = new ConversationalMemoryManager(windowSize: 4);

        // Turn 1: User introduces themselves and tech choices
        string turn1 = "Hi! My name is Jordan and I am building an automated trading engine using C# 13 and RabbitMQ on AWS.";
        result.Logs.Add($"\n--- Turn 1 (User) ---\n{turn1}");
        var reply1 = await memoryManager.ProcessUserTurnAsync(chatClient, turn1, cancellationToken);
        result.Logs.Add($"🤖 (Assistant):\n{reply1}");

        // Turn 2: User adds database decision
        string turn2 = "We decided our main storage will be PostgreSQL with TimescaleDB for telemetry data.";
        result.Logs.Add($"\n--- Turn 2 (User) ---\n{turn2}");
        var reply2 = await memoryManager.ProcessUserTurnAsync(chatClient, turn2, cancellationToken);
        result.Logs.Add($"🤖 (Assistant):\n{reply2}");

        // Turn 3: Unrelated topic to push window
        string turn3 = "By the way, what is the current temperature recommended for financial algorithmic models?";
        result.Logs.Add($"\n--- Turn 3 (User) ---\n{turn3}");
        var reply3 = await memoryManager.ProcessUserTurnAsync(chatClient, turn3, cancellationToken);
        result.Logs.Add($"🤖 (Assistant):\n{reply3}");

        // Turn 4: Memory Recall Test
        string turn4 = "Can you recap my name, what language and message broker I am using, and what database we picked?";
        result.Logs.Add($"\n--- Turn 4 (User - Memory Recall Test) ---\n{turn4}");
        var reply4 = await memoryManager.ProcessUserTurnAsync(chatClient, turn4, cancellationToken);
        result.Logs.Add($"🤖 (Assistant):\n{reply4}");

        // Memory Diagnostics
        var diag = memoryManager.GetDiagnostics();
        result.Logs.Add("\n🧠 Final Memory Diagnostics:");
        result.Logs.Add($"  Total Messages Stored: {diag.TotalMessagesStored}");
        result.Logs.Add($"  Active Window Messages: {diag.ActiveWindowMessages}");
        result.Logs.Add($"  Extracted Entities ({diag.KnownEntities.Count}):");
        foreach (var entity in diag.KnownEntities.Values)
        {
            result.Logs.Add($"    * [{entity.Category}] {entity.Key} = '{entity.Value}'");
        }

        result.Outputs["MemoryDiagnostics"] = diag;

        sw.Stop();
        result.Duration = sw.Elapsed;
        result.Summary = $"Conversational Memory maintained {diag.TotalMessagesStored} turns and tracked {diag.KnownEntities.Count} entities across multi-turn session.";
        return result;
    }
}
