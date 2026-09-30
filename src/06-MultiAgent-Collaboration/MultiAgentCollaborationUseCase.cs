using System.Diagnostics;
using DotNetAI.Core.Interfaces;
using DotNetAI.MultiAgent.Models;
using DotNetAI.MultiAgent.Services;
using Microsoft.Extensions.AI;

namespace DotNetAI.MultiAgent;

/// <summary>
/// Use Case 06: Multi-Agent Collaboration & Orchestration.
/// </summary>
public class MultiAgentCollaborationUseCase : IAIUseCase
{
    public int Id => 6;
    public string Name => "Multi-Agent Collaboration";
    public string Description => "Demonstrates a collaborative multi-agent software engineering team (Architect, Developer, Security Reviewer, Team Lead) with persona specialization, artifact passing, and automated peer review.";
    public IReadOnlyList<string> AIConcepts => new[]
    {
        "Multi-Agent Orchestration & Persona Design",
        "Role-Based System Prompts & Capability Partitioning",
        "Inter-Agent Artifact Handoff & Pipeline Flow",
        "Autonomous Automated Code & Security Review",
        "Executive Summary Consolidation"
    };

    private readonly MultiAgentOrchestrator _orchestrator = new();

    public async Task<UseCaseExecutionResult> RunAsync(
        IChatClient chatClient, 
        IEmbeddingGenerator<string, Embedding<float>>? embeddingGenerator = null, 
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var result = new UseCaseExecutionResult();
        result.Logs.Add("[UseCase 06] Starting Multi-Agent Software Engineering Team demo...");

        string requirements = "Build a high-throughput In-Memory Distributed Cache Service with TTL expiration, LRU eviction policy, Thread-safe ConcurrentDictionary storage, and OpenTelemetry metric reporting.";
        result.Logs.Add($"Engineering Requirements:\n{requirements}\n");

        var collaborationResult = await _orchestrator.RunWorkflowAsync(chatClient, requirements, cancellationToken);

        foreach (var msg in collaborationResult.MessageHistory)
        {
            result.Logs.Add($"\n=======================================================");
            result.Logs.Add($"🧑‍💻 Agent: {msg.AgentName} (Role: {msg.Role}, Duration: {msg.Duration.TotalSeconds:F2}s)");
            result.Logs.Add($"=======================================================");
            result.Logs.Add(msg.Content);
        }

        result.Outputs["CollaborationResult"] = collaborationResult;

        sw.Stop();
        result.Duration = sw.Elapsed;
        result.Summary = $"Multi-Agent team completed architecture, implementation, security audit, and executive summary across 4 specialized roles.";
        return result;
    }
}
