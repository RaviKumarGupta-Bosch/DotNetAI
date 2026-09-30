using System.Diagnostics;
using DotNetAI.Core.Interfaces;
using DotNetAI.PlanAndSolve.Models;
using DotNetAI.PlanAndSolve.Services;
using Microsoft.Extensions.AI;

namespace DotNetAI.PlanAndSolve;

/// <summary>
/// Use Case 07: Plan-and-Solve (Hierarchical Planning & Execution) Pattern.
/// </summary>
public class PlanAndSolveUseCase : IAIUseCase
{
    public int Id => 7;
    public string Name => "Plan-and-Solve (Hierarchical Planner)";
    public string Description => "Demonstrates two-stage agentic workflow: first decomposing complex multi-step objectives into structured JSON task plans, then executing steps sequentially with context chaining and final synthesis.";
    public IReadOnlyList<string> AIConcepts => new[]
    {
        "Plan-and-Solve Decomposition Architecture",
        "Hierarchical Task Planning & JSON Plan Schemas",
        "Step-by-Step Execution with Context Accumulation",
        "Deterministic State Chaining between Phases",
        "Consolidated Final Synthesis"
    };

    private readonly HierarchicalPlanner _planner = new();

    public async Task<UseCaseExecutionResult> RunAsync(
        IChatClient chatClient, 
        IEmbeddingGenerator<string, Embedding<float>>? embeddingGenerator = null, 
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var result = new UseCaseExecutionResult();
        result.Logs.Add("[UseCase 07] Starting Hierarchical Plan-and-Solve demo...");

        string objective = "Migrate a monolithic legacy ASP.NET 4.8 e-commerce application to microservices on .NET 9 with minimal downtime and zero data loss.";
        result.Logs.Add($"Goal Objective:\n{objective}\n");

        var report = await _planner.ExecutePlanAsync(chatClient, objective, cancellationToken);

        result.Logs.Add($"📋 Generated Plan ({report.OriginalPlan.Count} steps):");
        foreach (var step in report.OriginalPlan)
        {
            result.Logs.Add($"  [Step {step.StepNumber}] {step.Title}: {step.Description} -> (Output: {step.ExpectedOutput})");
        }

        result.Logs.Add("\n⚙️ Step Execution Timeline:");
        foreach (var exec in report.ExecutedSteps)
        {
            result.Logs.Add($"\n--- Finished Step {exec.StepNumber}: '{exec.StepTitle}' in {exec.Duration.TotalSeconds:F2}s ---");
            result.Logs.Add(exec.Output.Length > 300 ? exec.Output.Substring(0, 300) + "... [truncated]" : exec.Output);
        }

        result.Logs.Add("\n🎯 Consolidated Executive Deliverable:");
        result.Logs.Add(report.FinalConsolidatedOutput);

        result.Outputs["PlanReport"] = report;

        sw.Stop();
        result.Duration = sw.Elapsed;
        result.Summary = $"Plan-and-Solve executed {report.OriginalPlan.Count} decomposed steps and synthesized final deliverable in {report.TotalDuration.TotalSeconds:F2}s.";
        return result;
    }
}
