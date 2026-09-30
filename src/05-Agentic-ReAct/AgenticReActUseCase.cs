using System.Diagnostics;
using DotNetAI.AgenticReAct.Models;
using DotNetAI.AgenticReAct.Services;
using DotNetAI.Core.Interfaces;
using Microsoft.Extensions.AI;

namespace DotNetAI.AgenticReAct;

/// <summary>
/// Use Case 05: Agentic AI with the ReAct (Reasoning + Acting) Framework.
/// </summary>
public class AgenticReActUseCase : IAIUseCase
{
    public int Id => 5;
    public string Name => "Agentic AI - ReAct Pattern";
    public string Description => "Demonstrates autonomous goal decomposition via ReAct (Thought -> Action -> Observation -> Final Answer), telemetry inspection, diagnostic error log querying, and remediation action triggering.";
    public IReadOnlyList<string> AIConcepts => new[]
    {
        "ReAct Pattern (Reasoning + Acting Framework)",
        "Autonomous Goal-Seeking Loops",
        "Dynamic Observation Ingestion & State Updating",
        "Deterministic Thought/Action Regex Parsing",
        "Autonomous Incident Remediation"
    };

    private readonly ReActAgent _agent = new();

    public async Task<UseCaseExecutionResult> RunAsync(
        IChatClient chatClient, 
        IEmbeddingGenerator<string, Embedding<float>>? embeddingGenerator = null, 
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var result = new UseCaseExecutionResult();
        result.Logs.Add("[UseCase 05] Starting Agentic ReAct Pattern Incident Resolution demo...");

        string goal = "Production alert fired: Service 'BillingService' on node 'web-node-01' is reporting 500 errors. Investigate node metrics, search error logs for root cause, restart or remediate if necessary, and report status.";
        result.Logs.Add($"Task Goal:\n{goal}\n");

        var agentResult = await _agent.SolveAsync(chatClient, goal, maxSteps: 6, ct: cancellationToken);

        result.Logs.Add($"ReAct Loop completed in {agentResult.TotalSteps} steps ({agentResult.TotalDuration.TotalSeconds:F2}s):");
        foreach (var step in agentResult.Steps)
        {
            result.Logs.Add($"\n[Step {step.StepNumber}] ({step.Duration.TotalMilliseconds:F1}ms)");
            if (!string.IsNullOrEmpty(step.Thought))
                result.Logs.Add($"  💭 Thought: {step.Thought}");
            if (!string.IsNullOrEmpty(step.ActionName))
                result.Logs.Add($"  ⚡ Action: {step.ActionName}({step.ActionInput})");
            if (!string.IsNullOrEmpty(step.Observation))
                result.Logs.Add($"  👁️ Observation: {step.Observation}");
        }

        result.Logs.Add($"\n🎯 Final Answer:\n{agentResult.FinalAnswer}");
        result.Outputs["AgentResult"] = agentResult;

        sw.Stop();
        result.Duration = sw.Elapsed;
        result.Summary = $"ReAct agent resolved goal in {agentResult.TotalSteps} iterations with final status: {(agentResult.IsSuccess ? "Success" : "Incomplete")}.";
        return result;
    }
}
