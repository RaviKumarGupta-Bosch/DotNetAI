namespace DotNetAI.PlanAndSolve.Models;

public record PlanStep(
    int StepNumber,
    string Title,
    string Description,
    string ExpectedOutput,
    bool IsCompleted = false
);

public record StepExecutionResult(
    int StepNumber,
    string StepTitle,
    string Output,
    TimeSpan Duration
);

public record PlanExecutionReport(
    string Objective,
    List<PlanStep> OriginalPlan,
    List<StepExecutionResult> ExecutedSteps,
    string FinalConsolidatedOutput,
    TimeSpan TotalDuration
);
