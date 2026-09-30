using DotNetAI.AgenticAI.Samples.Models;
using Microsoft.Extensions.AI;

namespace DotNetAI.AgenticAI.Samples.Services;

public sealed class AgenticToolAgent
{
    public async Task<AgenticRunResult> RunAsync(
        IChatClient chatClient,
        AgenticSample sample,
        int maxTurns = 5,
        CancellationToken cancellationToken = default)
    {
        if (maxTurns < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxTurns), "At least one model turn is required.");
        }

        var history = new List<ChatMessage>
        {
            new(ChatRole.System, """
                You are a bounded .NET engineering assistant. Use the supplied tools to gather evidence for the user's goal.
                Never claim an action was completed unless a tool confirms it. Tools in this demo are read-only or draft-only.
                Do not execute deployments, merge pull requests, approve payments, change databases, or contact customers.
                Summarize evidence, uncertainty, and recommended next steps. Ask for human approval before consequential actions.
                """),
            new(ChatRole.User, $"{sample.Description}\n\nGoal: {sample.Goal}")
        };

        var options = new ChatOptions
        {
            Tools = sample.Tools.Cast<AITool>().ToList(),
            Temperature = 0.1f
        };
        var toolCalls = new List<string>();

        for (var turn = 1; turn <= maxTurns; turn++)
        {
            var response = await chatClient.GetResponseAsync(history, options, cancellationToken);
            var assistantMessage = response.Messages.FirstOrDefault()
                ?? new ChatMessage(ChatRole.Assistant, response.Text ?? string.Empty);
            history.Add(assistantMessage);

            var functionCalls = assistantMessage.Contents.OfType<FunctionCallContent>().ToList();
            if (functionCalls.Count == 0)
            {
                return new AgenticRunResult(
                    assistantMessage.Text ?? response.Text ?? string.Empty,
                    toolCalls,
                    turn,
                    true);
            }

            var functionResults = new List<AIContent>();
            foreach (var functionCall in functionCalls)
            {
                var tool = sample.Tools.FirstOrDefault(candidate =>
                    string.Equals(candidate.Name, functionCall.Name, StringComparison.OrdinalIgnoreCase));

                string result;
                if (tool is null)
                {
                    result = "Tool is not registered for this sample.";
                }
                else
                {
                    try
                    {
                        var arguments = functionCall.Arguments is null
                            ? null
                            : new AIFunctionArguments(functionCall.Arguments);
                        result = (await tool.InvokeAsync(arguments, cancellationToken))?.ToString() ?? "No data returned.";
                    }
                    catch
                    {
                        result = "Tool execution failed. Check its input and try again.";
                    }
                }

                toolCalls.Add(functionCall.Name);
                functionResults.Add(new FunctionResultContent(functionCall.CallId, result));
            }

            history.Add(new ChatMessage(ChatRole.Tool, functionResults));
        }

        return new AgenticRunResult(
            "The agent reached its turn limit before producing a final answer.",
            toolCalls,
            maxTurns,
            false);
    }
}