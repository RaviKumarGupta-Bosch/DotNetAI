using System.Text.Json;
using DotNetAI.Core.Testing;
using DotNetAI.FunctionCalling.Services;
using DotNetAI.FunctionCalling.Tools;
using Microsoft.Extensions.AI;
using Xunit;

namespace DotNetAI.UseCases.Tests;

public class FunctionCallingTests
{
    [Fact]
    public void InventoryTools_CheckInventory_KnownSku_ReturnsStockData()
    {
        var inventory = new InventoryTools();
        var resultJson = inventory.CheckInventory("SKU-SERVER-01");

        Assert.Contains("SKU-SERVER-01", resultJson);
        Assert.Contains("IN_STOCK", resultJson);
        Assert.Contains("Dell PowerEdge", resultJson);
    }

    [Fact]
    public void LogisticsTools_CalculateShipping_ValidInput_ReturnsCost()
    {
        var logistics = new LogisticsTools();
        var quoteJson = logistics.CalculateShippingRate("10001", "90210", 5.5, "Express");

        Assert.Contains("Express", quoteJson);
        Assert.Contains("estimatedCostUsd", quoteJson);
        Assert.Contains("transitDays", quoteJson);
    }

    [Fact]
    public void DevOpsTools_GetMetrics_ReturnsClusterTelemetry()
    {
        var devOps = new DevOpsTools();
        var metricsJson = devOps.GetDatabaseClusterMetrics("prod-aurora-cluster-01");

        Assert.Contains("prod-aurora-cluster-01", metricsJson);
        Assert.Contains("cpuUtilizationPercent", metricsJson);
        Assert.Contains("activeConnections", metricsJson);
    }

    [Fact]
    public async Task ToolCallingAgent_ExecutesToolAndReturnsFinalAnswer()
    {
        var agent = new ToolCallingAgent();
        Assert.Equal(3, agent.RegisteredTools.Count);

        int turn = 0;
        var mockClient = new MockChatClient(messages =>
        {
            turn++;
            if (turn == 1)
            {
                var functionCall = new FunctionCallContent("call_123", "CheckInventory", new Dictionary<string, object?> { ["sku"] = "SKU-SERVER-01" });
                return new ChatResponse(new ChatMessage(ChatRole.Assistant, [functionCall]));
            }

            return new ChatResponse(new ChatMessage(ChatRole.Assistant, "The Dell PowerEdge server (SKU-SERVER-01) is currently in stock with 14 units in Austin."));
        });

        var response = await agent.ExecuteAsync(mockClient, "Do we have servers in stock?");

        Assert.NotNull(response);
        Assert.Contains("14 units", response.FinalAnswer);
        Assert.Single(response.ToolTraces);
        Assert.Equal("CheckInventory", response.ToolTraces[0].ToolName);
    }
}
