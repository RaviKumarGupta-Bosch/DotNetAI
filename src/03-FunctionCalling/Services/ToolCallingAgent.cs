using System.Diagnostics;
using DotNetAI.FunctionCalling.Tools;
using Microsoft.Extensions.AI;

namespace DotNetAI.FunctionCalling.Services;

public record ToolExecutionTrace(
    string ToolName,
    string CallId,
    IDictionary<string, object?> Arguments,
    string ResultJson,
    TimeSpan Duration
);

public record ToolCallingAgentResponse(
    string FinalAnswer,
    List<ToolExecutionTrace> ToolTraces,
    int TotalTurns,
    TimeSpan TotalDuration
);

/// <summary>
/// Autonomous Tool-Calling Agent that registers C# AIFunctions, manages the multi-turn function call loop,
/// executes matching tools, and returns final synthesized answers.
/// </summary>
public class ToolCallingAgent
{
    private readonly List<AIFunction> _tools = new();

    public ToolCallingAgent()
    {
        var inventory = new InventoryTools();
        var logistics = new LogisticsTools();
        var devOps = new DevOpsTools();

        // Register C# methods as AIFunctions using Microsoft.Extensions.AI reflection
        _tools.Add(AIFunctionFactory.Create(inventory.CheckInventory, nameof(inventory.CheckInventory)));
        _tools.Add(AIFunctionFactory.Create(logistics.CalculateShippingRate, nameof(logistics.CalculateShippingRate)));
        _tools.Add(AIFunctionFactory.Create(devOps.GetDatabaseClusterMetrics, nameof(devOps.GetDatabaseClusterMetrics)));
    }

    public IReadOnlyList<AIFunction> RegisteredTools => _tools;

    public async Task<ToolCallingAgentResponse> ExecuteAsync(
        IChatClient chatClient,
        string userPrompt,
        int maxIterations = 5,
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var traces = new List<ToolExecutionTrace>();

        var systemPrompt = """
        You are an Enterprise Operations Copilot with direct access to live enterprise tools.
        When asked about inventory, shipping, or database health, ALWAYS call the appropriate tool.
        After receiving tool outputs, synthesize a helpful, comprehensive response for the user.
        """;

        var conversation = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt),
            new(ChatRole.User, userPrompt)
        };

        var options = new ChatOptions
        {
            Tools = _tools.Cast<AITool>().ToList(),
            Temperature = 0.0f
        };

        int turnCount = 0;
        while (turnCount < maxIterations)
        {
            turnCount++;
            var response = await chatClient.GetResponseAsync(conversation, options, ct);
            var assistantMsg = response.Messages.FirstOrDefault() ?? new ChatMessage(ChatRole.Assistant, response.Text ?? string.Empty);
            conversation.Add(assistantMsg);

            // Check if model requested function calls
            var functionCalls = assistantMsg.Contents.OfType<FunctionCallContent>().ToList();
            if (functionCalls.Count == 0)
            {
                // Final answer generated
                sw.Stop();
                return new ToolCallingAgentResponse(
                    assistantMsg.Text ?? response.Text ?? string.Empty,
                    traces,
                    turnCount,
                    sw.Elapsed
                );
            }

            // Execute each requested tool call and add results to conversation
            var toolResults = new List<AIContent>();

            foreach (var call in functionCalls)
            {
                var toolSw = Stopwatch.StartNew();
                var tool = _tools.FirstOrDefault(t => string.Equals(t.Name, call.Name, StringComparison.OrdinalIgnoreCase));

                string resultText;
                if (tool != null)
                {
                    try
                    {
                        var args = call.Arguments != null ? new AIFunctionArguments(call.Arguments) : null;
                        var invokeResult = await tool.InvokeAsync(args, ct);
                        resultText = invokeResult?.ToString() ?? "null";
                    }
                    catch (Exception ex)
                    {
                        resultText = $$"""{"error": "{{ex.Message}}"}""";
                    }
                }
                else
                {
                    resultText = $$"""{"error": "Tool '{{call.Name}}' not found."}""";
                }

                toolSw.Stop();
                traces.Add(new ToolExecutionTrace(call.Name, call.CallId, call.Arguments ?? new Dictionary<string, object?>(), resultText, toolSw.Elapsed));
                toolResults.Add(new FunctionResultContent(call.CallId, resultText));
            }

            conversation.Add(new ChatMessage(ChatRole.Tool, toolResults));
        }

        sw.Stop();
        return new ToolCallingAgentResponse(
            conversation.LastOrDefault(m => m.Role == ChatRole.Assistant)?.Text ?? "Maximum iterations reached.",
            traces,
            turnCount,
            sw.Elapsed
        );
    }
}
