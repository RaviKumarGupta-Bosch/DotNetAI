using DotNetAI.AgenticReAct.Services;
using DotNetAI.AgenticReAct.Tools;
using DotNetAI.Core.Testing;
using Xunit;

namespace DotNetAI.UseCases.Tests;

public class AgenticReActTests
{
    [Fact]
    public async Task SystemMetricsTool_ExecuteAsync_ReturnsNodeTelemetry()
    {
        var tool = new SystemMetricsTool();
        var telemetryJson = await tool.ExecuteAsync("web-node-01");

        Assert.Contains("web-node-01", telemetryJson);
        Assert.Contains("98.4%", telemetryJson);
        Assert.Contains("DEGRADED", telemetryJson);
    }

    [Fact]
    public async Task ProcessActionTool_RestartService_ReturnsSuccess()
    {
        var tool = new ProcessActionTool();
        var resultJson = await tool.ExecuteAsync("BillingService");

        Assert.Contains("BillingService", resultJson);
        Assert.Contains("restarted successfully", resultJson);
    }

    [Fact]
    public async Task ReActAgent_RunsThoughtActionObservationLoop()
    {
        var agent = new ReActAgent();
        int step = 0;

        var mock = new MockChatClient(prompt =>
        {
            step++;
            if (step == 1)
            {
                return """
                Thought: I should inspect the system metrics of web-node-01 first.
                Action: get_system_metrics
                Action Input: web-node-01
                """;
            }
            if (step == 2)
            {
                return """
                Thought: web-node-01 has 98.4% CPU usage and is degraded. Let me restart the billing service.
                Action: restart_service
                Action Input: BillingService
                """;
            }

            return """
            Thought: The service was restarted successfully and memory buffer was reclaimed.
            Final Answer: web-node-01 was degraded due to OutOfMemoryException and thread pool starvation; restarting BillingService reclaimed 11.4GB RAM and restored healthy operations.
            """;
        });

        var result = await agent.SolveAsync(mock, "Diagnose and resolve web-node-01 degradation");

        Assert.True(result.IsSuccess);
        Assert.Contains("web-node-01 was degraded", result.FinalAnswer);
        Assert.True(result.Steps.Count >= 2);
    }
}
