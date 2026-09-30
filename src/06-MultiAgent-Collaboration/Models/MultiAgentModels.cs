namespace DotNetAI.MultiAgent.Models;

public enum AgentRole
{
    SoftwareArchitect,
    SoftwareEngineer,
    SecurityAuditor,
    TeamLead
}

public record AgentMessage(
    AgentRole Role,
    string AgentName,
    string Content,
    DateTime Timestamp,
    TimeSpan Duration
);

public record MultiAgentCollaborationResult(
    string ProjectGoal,
    string ArchitectureSpecification,
    string GeneratedCode,
    string SecurityAuditReport,
    string ExecutiveSummary,
    List<AgentMessage> MessageHistory,
    TimeSpan TotalDuration
);
