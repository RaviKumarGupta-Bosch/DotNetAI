using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using DotNetAI.AgenticReAct.Models;
using DotNetAI.AgenticReAct.Tools;
using Microsoft.Extensions.AI;

namespace DotNetAI.AgenticReAct.Services;

public class ReActAgent
{
    private readonly Dictionary<string, IDiagnosticTool> _tools = new(StringComparer.OrdinalIgnoreCase);

    public ReActAgent(IEnumerable<IDiagnosticTool>? tools = null)
    {
        var toolList = tools ?? new IDiagnosticTool[]
        {
            new SystemMetricsTool(),
            new LogAnalyzerTool(),
            new ProcessActionTool()
        };

        foreach (var t in toolList)
        {
            _tools[t.Name] = t;
        }
    }

    public async Task<ReActAgentResult> SolveAsync(
        IChatClient chatClient,
        string taskGoal,
        int maxSteps = 6,
        CancellationToken ct = default)
    {
        var totalSw = Stopwatch.StartNew();
        var steps = new List<ReActStep>();

        var toolDescriptions = new StringBuilder();
        foreach (var tool in _tools.Values)
        {
            toolDescriptions.AppendLine($"- {tool.Name}: {tool.Description}");
        }

        var systemPrompt = $$"""
        Answer the following question and resolve the issue by using the ReAct (Reasoning + Acting) cycle.
        You have access to the following diagnostic tools:
        {{toolDescriptions}}

        Use the following strict format:

        Thought: Describe your step-by-step reasoning about what is happening or what to check next.
        Action: The name of the tool to use (must be one of: {{string.Join(", ", _tools.Keys)}})
        Action Input: The specific input parameter to pass to the tool
        Observation: The result from executing the action (this will be provided to you)

        ... (this Thought/Action/Action Input/Observation can repeat up to {{maxSteps}} times)

        When you have gathered enough information to fully resolve the issue, output:
        Thought: I now have enough information to resolve the task.
        Final Answer: The complete explanation of the problem, root cause, actions taken, and final status.
        """;

        var conversation = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt),
            new(ChatRole.User, $"Task Goal: {taskGoal}")
        };

        int stepNum = 0;
        string finalAnswer = string.Empty;
        bool isSuccess = false;

        while (stepNum < maxSteps)
        {
            stepNum++;
            var stepSw = Stopwatch.StartNew();

            var response = await chatClient.GetResponseAsync(conversation, new ChatOptions { Temperature = 0.1f }, ct);
            var assistantText = response.Text?.Trim() ?? string.Empty;
            conversation.Add(new ChatMessage(ChatRole.Assistant, assistantText));

            // Check if Final Answer is reached
            if (assistantText.Contains("Final Answer:", StringComparison.OrdinalIgnoreCase))
            {
                int faIdx = assistantText.IndexOf("Final Answer:", StringComparison.OrdinalIgnoreCase);
                finalAnswer = assistantText.Substring(faIdx + "Final Answer:".Length).Trim();
                
                string thought = assistantText.Substring(0, faIdx).Trim();
                if (thought.StartsWith("Thought:", StringComparison.OrdinalIgnoreCase))
                    thought = thought.Substring("Thought:".Length).Trim();

                stepSw.Stop();
                steps.Add(new ReActStep(stepNum, thought, null, null, null, stepSw.Elapsed));
                isSuccess = true;
                break;
            }

            // Parse Thought, Action, Action Input
            var match = Regex.Match(assistantText, @"Thought:\s*(.*?)\s*Action:\s*([a-zA-Z0-9_\-]+)\s*Action Input:\s*(.*)", RegexOptions.Singleline | RegexOptions.IgnoreCase);

            if (match.Success)
            {
                string thought = match.Groups[1].Value.Trim();
                string action = match.Groups[2].Value.Trim();
                string actionInput = match.Groups[3].Value.Trim();

                string observation;
                if (_tools.TryGetValue(action, out var tool))
                {
                    try
                    {
                        observation = await tool.ExecuteAsync(actionInput, ct);
                    }
                    catch (Exception ex)
                    {
                        observation = $"Tool execution error: {ex.Message}";
                    }
                }
                else
                {
                    observation = $"Error: Tool '{action}' does not exist. Available tools: {string.Join(", ", _tools.Keys)}";
                }

                stepSw.Stop();
                steps.Add(new ReActStep(stepNum, thought, action, actionInput, observation, stepSw.Elapsed));

                // Feed observation back to LLM
                conversation.Add(new ChatMessage(ChatRole.User, $"Observation: {observation}"));
            }
            else
            {
                // Unrecognized format or raw response; fallback parse
                stepSw.Stop();
                steps.Add(new ReActStep(stepNum, assistantText, null, null, null, stepSw.Elapsed));
                finalAnswer = assistantText;
                isSuccess = true;
                break;
            }
        }

        totalSw.Stop();
        if (string.IsNullOrWhiteSpace(finalAnswer))
        {
            finalAnswer = "Agent reached maximum step limit before producing a Final Answer.";
        }

        return new ReActAgentResult(
            Goal: taskGoal,
            FinalAnswer: finalAnswer,
            Steps: steps,
            IsSuccess: isSuccess,
            TotalSteps: stepNum,
            TotalDuration: totalSw.Elapsed
        );
    }
}
