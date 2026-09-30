using System.Diagnostics;
using DotNetAI.MultiAgent.Models;
using Microsoft.Extensions.AI;

namespace DotNetAI.MultiAgent.Services;

public class MultiAgentOrchestrator
{
    private readonly ArchitectAgent _architect = new();
    private readonly DeveloperAgent _developer = new();
    private readonly SecurityAuditorAgent _auditor = new();

    public async Task<MultiAgentCollaborationResult> RunWorkflowAsync(
        IChatClient client,
        string projectRequirements,
        CancellationToken ct = default)
    {
        var totalSw = Stopwatch.StartNew();
        var history = new List<AgentMessage>();

        // Phase 1: Architecture Design
        var archMsg = await _architect.DesignAsync(client, projectRequirements, ct);
        history.Add(archMsg);

        // Phase 2: Code Implementation
        var devMsg = await _developer.ImplementAsync(client, archMsg.Content, ct);
        history.Add(devMsg);

        // Phase 3: Security Review
        var secMsg = await _auditor.AuditAsync(client, devMsg.Content, ct);
        history.Add(secMsg);

        // Phase 4: Team Lead Synthesis
        var leadSw = Stopwatch.StartNew();
        var summaryPrompt = """
        You are the Technical Team Lead.
        Consolidate the deliverables from the Architect, Developer, and Security Auditor into an executive engineering summary.
        Highlight architectural decisions, code delivery status, and key security sign-offs.
        """;

        var leadResponse = await client.GetResponseAsync(
            new ChatMessage[]
            {
                new(ChatRole.System, summaryPrompt),
                new(ChatRole.User, $"Team Artifacts:\nArchitect:\n{archMsg.Content}\n\nSecurity Review:\n{secMsg.Content}")
            },
            new ChatOptions { Temperature = 0.2f },
            ct
        );
        leadSw.Stop();

        var leadMsg = new AgentMessage(
            AgentRole.TeamLead,
            "Team Lead (Alex)",
            leadResponse.Text ?? string.Empty,
            DateTime.UtcNow,
            leadSw.Elapsed
        );
        history.Add(leadMsg);

        totalSw.Stop();

        return new MultiAgentCollaborationResult(
            ProjectGoal: projectRequirements,
            ArchitectureSpecification: archMsg.Content,
            GeneratedCode: devMsg.Content,
            SecurityAuditReport: secMsg.Content,
            ExecutiveSummary: leadMsg.Content,
            MessageHistory: history,
            TotalDuration: totalSw.Elapsed
        );
    }
}
