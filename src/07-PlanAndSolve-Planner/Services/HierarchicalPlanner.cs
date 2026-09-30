using System.Diagnostics;
using System.Text;
using System.Text.Json;
using DotNetAI.PlanAndSolve.Models;
using Microsoft.Extensions.AI;

namespace DotNetAI.PlanAndSolve.Services;

public class HierarchicalPlanner
{
    public async Task<List<PlanStep>> GeneratePlanAsync(
        IChatClient client,
        string objective,
        CancellationToken ct = default)
    {
        var systemPrompt = """
        You are a Strategic Task Planner. Given a high-level goal, decompose it into 3 to 5 discrete, sequential, executable steps.
        You MUST respond ONLY with a valid JSON array of objects with the following schema:
        [
          {
            "StepNumber": 1,
            "Title": "Short step title",
            "Description": "Detailed instructions on what to accomplish",
            "ExpectedOutput": "Specific deliverable expected from this step"
          }
        ]
        Do NOT wrap in markdown ticks other than standard JSON.
        """;

        var response = await client.GetResponseAsync(
            new ChatMessage[]
            {
                new(ChatRole.System, systemPrompt),
                new(ChatRole.User, $"Objective: {objective}")
            },
            new ChatOptions { Temperature = 0.1f },
            ct
        );

        var text = response.Text?.Trim() ?? "[]";
        if (text.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            text = text.Substring(7);
            if (text.EndsWith("```")) text = text.Substring(0, text.Length - 3);
            text = text.Trim();
        }
        else if (text.StartsWith("```"))
        {
            text = text.Substring(3);
            if (text.EndsWith("```")) text = text.Substring(0, text.Length - 3);
            text = text.Trim();
        }

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var steps = JsonSerializer.Deserialize<List<PlanStep>>(text, options);
            if (steps != null && steps.Count > 0)
                return steps;
        }
        catch
        {
            // Fallback deterministic default plan if JSON parsing fails
        }

        return new List<PlanStep>
        {
            new(1, "Requirements & Threat Modeling", "Analyze the target system and identify security risks and constraints.", "Threat model report"),
            new(2, "Architecture & Safeguard Design", "Design authentication, rate limiting, and encryption controls.", "Security architecture design"),
            new(3, "Validation & Automated Testing", "Define verification tests and penetration testing scenarios.", "Test plan documentation")
        };
    }

    public async Task<PlanExecutionReport> ExecutePlanAsync(
        IChatClient client,
        string objective,
        CancellationToken ct = default)
    {
        var totalSw = Stopwatch.StartNew();

        // 1. Generate Structured Plan
        var plan = await GeneratePlanAsync(client, objective, ct);
        var executedSteps = new List<StepExecutionResult>();
        var accumulatedContext = new StringBuilder();

        // 2. Sequentially execute each plan step
        foreach (var step in plan)
        {
            var stepSw = Stopwatch.StartNew();

            var executorPrompt = $"""
            You are an Expert Task Executor.
            Overall Objective: {objective}
            
            Prior Completed Work Context:
            {(accumulatedContext.Length > 0 ? accumulatedContext.ToString() : "None (Initial Step)")}

            Current Step to Execute:
            Step {step.StepNumber}: {step.Title}
            Description: {step.Description}
            Expected Output: {step.ExpectedOutput}

            Please produce the detailed deliverable for this specific step.
            """;

            var stepResponse = await client.GetResponseAsync(
                new ChatMessage[]
                {
                    new(ChatRole.System, "Execute the assigned step with rigorous engineering precision."),
                    new(ChatRole.User, executorPrompt)
                },
                new ChatOptions { Temperature = 0.2f },
                ct
            );

            stepSw.Stop();
            var outputText = stepResponse.Text ?? string.Empty;

            executedSteps.Add(new StepExecutionResult(step.StepNumber, step.Title, outputText, stepSw.Elapsed));

            accumulatedContext.AppendLine($"\n=== Output of Step {step.StepNumber} ({step.Title}) ===");
            accumulatedContext.AppendLine(outputText);
        }

        // 3. Synthesize Final Consolidated Output
        var synthPrompt = $"""
        You are a Solutions Delivery Lead.
        Consolidate the completed plan execution steps into a cohesive, production-ready deliverable report.
        
        Original Objective: {objective}

        Step Deliverables:
        {accumulatedContext}
        """;

        var finalResponse = await client.GetResponseAsync(
            new ChatMessage[]
            {
                new(ChatRole.System, "Produce a comprehensive consolidated solution report."),
                new(ChatRole.User, synthPrompt)
            },
            new ChatOptions { Temperature = 0.2f },
            ct
        );

        totalSw.Stop();

        return new PlanExecutionReport(
            Objective: objective,
            OriginalPlan: plan,
            ExecutedSteps: executedSteps,
            FinalConsolidatedOutput: finalResponse.Text ?? string.Empty,
            TotalDuration: totalSw.Elapsed
        );
    }
}
