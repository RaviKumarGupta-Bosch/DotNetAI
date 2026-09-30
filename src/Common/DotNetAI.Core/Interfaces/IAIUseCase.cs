using Microsoft.Extensions.AI;

namespace DotNetAI.Core.Interfaces;

/// <summary>
/// Common contract for all AI Use Case implementations.
/// </summary>
public interface IAIUseCase
{
    /// <summary>
    /// Sequential number of the use case (1 to 10)
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Friendly title of the use case
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Detailed description of what this use case solves
    /// </summary>
    string Description { get; }

    /// <summary>
    /// List of AI concepts and architectural patterns demonstrated
    /// </summary>
    IReadOnlyList<string> AIConcepts { get; }

    /// <summary>
    /// Executes the interactive or sample workflow for this use case.
    /// </summary>
    Task<UseCaseExecutionResult> RunAsync(
        IChatClient chatClient, 
        IEmbeddingGenerator<string, Embedding<float>>? embeddingGenerator = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result data returned from executing a use case.
/// </summary>
public class UseCaseExecutionResult
{
    public bool Success { get; set; } = true;
    public string Summary { get; set; } = string.Empty;
    public Dictionary<string, object?> Outputs { get; set; } = new();
    public List<string> Logs { get; set; } = new();
    public TimeSpan Duration { get; set; }
}
