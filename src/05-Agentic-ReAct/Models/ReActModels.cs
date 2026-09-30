namespace DotNetAI.AgenticReAct.Models;

public record ReActStep(
    int StepNumber,
    string? Thought,
    string? ActionName,
    string? ActionInput,
    string? Observation,
    TimeSpan Duration
);

public record ReActAgentResult(
    string Goal,
    string FinalAnswer,
    List<ReActStep> Steps,
    bool IsSuccess,
    int TotalSteps,
    TimeSpan TotalDuration
);
