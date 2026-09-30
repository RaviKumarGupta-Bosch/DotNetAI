using DotNetAI.AgenticAI.Samples;
using DotNetAI.AgenticAI.Samples.Services;
using DotNetAI.Core.Testing;
using Microsoft.Extensions.AI;

namespace DotNetAI.UseCases.Tests;

public class AgenticAISamplesTests
{
    [Fact]
    public void Catalog_ContainsTenDistinctPracticalSamples()
    {
        var samples = AgenticSampleCatalog.Create();

        Assert.Equal(10, samples.Count);
        Assert.Equal(10, samples.Select(sample => sample.Id).Distinct().Count());
        Assert.All(samples, sample => Assert.NotEmpty(sample.Tools));
    }

    [Fact]
    public async Task EverySample_ExecutesItsMockToolAndReturnsFinalAnswer()
    {
        var agent = new AgenticToolAgent();

        foreach (var sample in AgenticSampleCatalog.Create())
        {
            var mockClient = AgenticSampleCatalog.CreateMockClient(sample);
            var result = await agent.RunAsync(mockClient, sample);

            Assert.True(result.ReachedFinalAnswer, sample.Name);
            Assert.Equal(2, result.Turns);
            Assert.Equal(sample.MockToolName, Assert.Single(result.ToolCalls));
            Assert.Contains(mockClient.ReceivedHistories.Last(), message => message.Role == ChatRole.Tool);
            Assert.False(string.IsNullOrWhiteSpace(result.FinalAnswer));
        }
    }

    [Fact]
    public async Task Agent_RejectsZeroTurnLimit()
    {
        var sample = AgenticSampleCatalog.Create()[0];
        var agent = new AgenticToolAgent();

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            agent.RunAsync(AgenticSampleCatalog.CreateMockClient(sample), sample, maxTurns: 0));
    }

    [Fact]
    public async Task Agent_StopsWhenModelKeepsRequestingTools()
    {
        var sample = AgenticSampleCatalog.Create()[0];
        var agent = new AgenticToolAgent();
        var mockClient = new MockChatClient(_ => new ChatResponse(new ChatMessage(
            ChatRole.Assistant,
            [new FunctionCallContent("repeated-call", sample.MockToolName, sample.MockArguments)])));

        var result = await agent.RunAsync(mockClient, sample, maxTurns: 2);

        Assert.False(result.ReachedFinalAnswer);
        Assert.Equal(2, result.Turns);
        Assert.Equal(2, result.ToolCalls.Count);
        Assert.Contains("turn limit", result.FinalAnswer, StringComparison.OrdinalIgnoreCase);
    }
}