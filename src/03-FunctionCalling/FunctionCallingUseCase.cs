using System.Diagnostics;
using DotNetAI.Core.Interfaces;
using DotNetAI.FunctionCalling.Services;
using Microsoft.Extensions.AI;

namespace DotNetAI.FunctionCalling;

/// <summary>
/// Use Case 03: Function Calling & Tool Invocation with Microsoft.Extensions.AI.
/// </summary>
public class FunctionCallingUseCase : IAIUseCase
{
    public int Id => 3;
    public string Name => "Function Calling & Tool Execution";
    public string Description => "Demonstrates tool declaration with AIFunction, schema inference from C# method signatures, multi-turn tool call loop, execution of local C# business logic, and response synthesis.";
    public IReadOnlyList<string> AIConcepts => new[]
    {
        "Function Calling / Tool Calling Protocol",
        "AIFunctionFactory & Reflection Schema Generation",
        "Multi-turn Tool Invocation Loop (LLM -> Tool -> Result -> LLM)",
        "FunctionCallContent & FunctionResultContent in Microsoft.Extensions.AI",
        "Autonomous Tool Argument Marshaling"
    };

    private readonly ToolCallingAgent _agent = new();

    public async Task<UseCaseExecutionResult> RunAsync(
        IChatClient chatClient, 
        IEmbeddingGenerator<string, Embedding<float>>? embeddingGenerator = null, 
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var result = new UseCaseExecutionResult();
        result.Logs.Add("[UseCase 03] Starting Function Calling & Autonomous Tool Execution demo...");
        result.Logs.Add($"Registered {_agent.RegisteredTools.Count} tools: {string.Join(", ", _agent.RegisteredTools.Select(t => t.Name))}");

        // Query 1: Inventory & Logistics Multi-tool Query
        string query1 = "Check if we have SKU-SERVER-01 in stock, and what would it cost to ship 25kg of hardware from 98052 to 10001 via Express?";
        result.Logs.Add($"\n--- Prompt 1: '{query1}' ---");

        var response1 = await _agent.ExecuteAsync(chatClient, query1, maxIterations: 5, cancellationToken);
        result.Logs.Add($"Tool Calls Made ({response1.ToolTraces.Count}):");
        foreach (var trace in response1.ToolTraces)
        {
            result.Logs.Add($"  -> Executed '{trace.ToolName}' in {trace.Duration.TotalMilliseconds:F1}ms with args: {string.Join(", ", trace.Arguments.Select(kv => $"{kv.Key}={kv.Value}"))}");
            result.Logs.Add($"     Result: {trace.ResultJson}");
        }
        result.Logs.Add($"Final Synthesized Answer:\n{response1.FinalAnswer}");
        result.Outputs["Scenario1Response"] = response1;

        // Query 2: DevOps Database Health Query
        string query2 = "What is the health and current load on database cluster 'prod-db-eastus'?";
        result.Logs.Add($"\n--- Prompt 2: '{query2}' ---");

        var response2 = await _agent.ExecuteAsync(chatClient, query2, maxIterations: 5, cancellationToken);
        result.Logs.Add($"Tool Calls Made ({response2.ToolTraces.Count}):");
        foreach (var trace in response2.ToolTraces)
        {
            result.Logs.Add($"  -> Executed '{trace.ToolName}' in {trace.Duration.TotalMilliseconds:F1}ms");
            result.Logs.Add($"     Result: {trace.ResultJson}");
        }
        result.Logs.Add($"Final Synthesized Answer:\n{response2.FinalAnswer}");
        result.Outputs["Scenario2Response"] = response2;

        sw.Stop();
        result.Duration = sw.Elapsed;
        result.Summary = "Function calling agent completed operations with automated multi-turn tool execution.";
        return result;
    }
}
