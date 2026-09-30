using DotNetAI.Core.Testing;
using DotNetAI.MultiAgent.Models;
using DotNetAI.MultiAgent.Services;
using Xunit;

namespace DotNetAI.UseCases.Tests;

public class MultiAgentCollaborationTests
{
    [Fact]
    public async Task ArchitectAgent_ProducesArchitectureSpec()
    {
        var mock = new MockChatClient("## Clean Architecture Specification\n1. Core Domain\n2. Application Layer\n3. Infrastructure");
        var architect = new ArchitectAgent();

        var message = await architect.DesignAsync(mock, "Build multi-tenant SaaS billing");

        Assert.Equal(AgentRole.SoftwareArchitect, message.Role);
        Assert.Contains("Clean Architecture", message.Content);
    }

    [Fact]
    public async Task MultiAgentOrchestrator_CoordinatesAllAgents()
    {
        var mock = new MockChatClient(prompt =>
        {
            if (prompt.Contains("Principal Software Architect", StringComparison.OrdinalIgnoreCase))
                return "Architecture: Hexagonal architecture with MediatR CQRS.";
            if (prompt.Contains("Senior .NET", StringComparison.OrdinalIgnoreCase))
                return "```csharp\npublic record ProcessPaymentCommand(Guid Id, decimal Amount);\n```";
            if (prompt.Contains("Cyber Security", StringComparison.OrdinalIgnoreCase))
                return "Security Scorecard: 0 Critical, 0 High. Remediation: Add input validation on Amount.";
            return "Executive Summary: The architecture, C# code, and security review have been successfully completed.";
        });

        var orchestrator = new MultiAgentOrchestrator();
        var result = await orchestrator.RunWorkflowAsync(mock, "Implement secure Stripe webhook listener");

        Assert.NotNull(result);
        Assert.False(string.IsNullOrWhiteSpace(result.ArchitectureSpecification));
        Assert.False(string.IsNullOrWhiteSpace(result.GeneratedCode));
        Assert.False(string.IsNullOrWhiteSpace(result.SecurityAuditReport));
        Assert.False(string.IsNullOrWhiteSpace(result.ExecutiveSummary));
        Assert.Equal(4, result.MessageHistory.Count);
    }
}
