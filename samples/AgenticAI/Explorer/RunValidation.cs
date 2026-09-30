using DotNetAI.AgenticAI.Samples.Models;

namespace DotNetAI.AgenticAI.Explorer;

public sealed record ValidationCheck(string Name, bool Passed, string Detail);

public static class RunValidation
{
    public static IReadOnlyList<ValidationCheck> Validate(AgenticRunResult result, AgenticSample sample, int maxTurns)
    {
        var availableTools = sample.Tools.Select(tool => tool.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var usedRegisteredTool = result.ToolCalls.Count > 0
            && result.ToolCalls.All(availableTools.Contains);

        return
        [
            new(
                "Final answer",
                result.ReachedFinalAnswer && !string.IsNullOrWhiteSpace(result.FinalAnswer),
                result.ReachedFinalAnswer && !string.IsNullOrWhiteSpace(result.FinalAnswer)
                    ? "The agent returned a non-empty final response."
                    : "The agent did not produce a final response."),
            new(
                "Registered tool use",
                usedRegisteredTool,
                usedRegisteredTool
                    ? $"Called {string.Join(", ", result.ToolCalls)} from this sample's allowlist."
                    : "No registered sample tool was called, or an unregistered tool was requested."),
            new(
                "Turn limit",
                result.Turns <= maxTurns && result.ReachedFinalAnswer,
                result.Turns <= maxTurns && result.ReachedFinalAnswer
                    ? $"Completed in {result.Turns} of {maxTurns} allowed turns."
                    : $"Reached {result.Turns} turns; limit is {maxTurns}.")
        ];
    }
}

public sealed record RunRequest(string SampleId, string Mode, int? MaxTurns);

public sealed record ExplorerRunResponse(
    string SampleId,
    string SampleName,
    string Mode,
    string Model,
    string FinalAnswer,
    IReadOnlyList<string> ToolCalls,
    int Turns,
    int MaxTurns,
    bool Valid,
    IReadOnlyList<ValidationCheck> Checks,
    long DurationMilliseconds);