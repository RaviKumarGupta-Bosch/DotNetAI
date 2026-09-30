using System.Diagnostics;
using DotNetAI.MultiAgent.Models;
using Microsoft.Extensions.AI;

namespace DotNetAI.MultiAgent.Services;

public class ArchitectAgent
{
    public async Task<AgentMessage> DesignAsync(IChatClient client, string requirements, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var systemPrompt = """
        You are a Principal Software Architect.
        Given product requirements, produce a clean, modular technical architecture specification.
        Specify:
        1. High-level Component Topology
        2. Clean Architecture Layering (Domain, Application, Infrastructure, API)
        3. Core C# Entity and Interface signatures
        Be concise, precise, and practical.
        """;

        var response = await client.GetResponseAsync(
            new ChatMessage[]
            {
                new(ChatRole.System, systemPrompt),
                new(ChatRole.User, $"Requirements: {requirements}")
            },
            new ChatOptions { Temperature = 0.2f },
            ct
        );

        sw.Stop();
        return new AgentMessage(
            AgentRole.SoftwareArchitect,
            "Architect (Sophia)",
            response.Text ?? string.Empty,
            DateTime.UtcNow,
            sw.Elapsed
        );
    }
}

public class DeveloperAgent
{
    public async Task<AgentMessage> ImplementAsync(IChatClient client, string archSpec, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var systemPrompt = """
        You are a Senior .NET / C# Software Engineer.
        Given an architecture specification, write clean, production-ready C# code.
        Rules:
        1. Use modern C# features (records, primary constructors, pattern matching).
        2. Implement core domain logic, interfaces, and service implementations.
        3. Ensure exception handling and cancellation token support are present.
        """;

        var response = await client.GetResponseAsync(
            new ChatMessage[]
            {
                new(ChatRole.System, systemPrompt),
                new(ChatRole.User, $"Architecture Spec:\n{archSpec}\n\nPlease generate the primary C# implementation files.")
            },
            new ChatOptions { Temperature = 0.1f },
            ct
        );

        sw.Stop();
        return new AgentMessage(
            AgentRole.SoftwareEngineer,
            "Developer (Devon)",
            response.Text ?? string.Empty,
            DateTime.UtcNow,
            sw.Elapsed
        );
    }
}

public class SecurityAuditorAgent
{
    public async Task<AgentMessage> AuditAsync(IChatClient client, string code, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var systemPrompt = """
        You are a Cyber Security Specialist & Static Code Reviewer.
        Analyze the provided C# source code for:
        1. Injection flaws (SQL, Command, Log)
        2. Secrets leakage & Insecure defaults
        3. Input validation, concurrency, and resource exhaustion vulnerabilities
        Provide a concise audit scorecard with Severity (Low, Med, High, Critical) and remediations.
        """;

        var response = await client.GetResponseAsync(
            new ChatMessage[]
            {
                new(ChatRole.System, systemPrompt),
                new(ChatRole.User, $"Source Code under review:\n{code}")
            },
            new ChatOptions { Temperature = 0.1f },
            ct
        );

        sw.Stop();
        return new AgentMessage(
            AgentRole.SecurityAuditor,
            "Security Auditor (Sarah)",
            response.Text ?? string.Empty,
            DateTime.UtcNow,
            sw.Elapsed
        );
    }
}
