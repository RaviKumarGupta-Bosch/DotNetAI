using Microsoft.Extensions.AI;

namespace DotNetAI.AgenticAI.Samples.Models;

public sealed record AgenticSample(
    string Id,
    string Name,
    string Description,
    string Goal,
    IReadOnlyList<AIFunction> Tools,
    string MockToolName,
    IDictionary<string, object?> MockArguments,
    string MockFinalAnswer);

public sealed record AgenticRunResult(
    string FinalAnswer,
    IReadOnlyList<string> ToolCalls,
    int Turns,
    bool ReachedFinalAnswer);