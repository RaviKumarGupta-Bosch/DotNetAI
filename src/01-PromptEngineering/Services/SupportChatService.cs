using System.Diagnostics;
using DotNetAI.Core.Utils;
using DotNetAI.PromptEngineering.Models;
using Microsoft.Extensions.AI;

namespace DotNetAI.PromptEngineering.Services;

/// <summary>
/// Executes prompts against an IChatClient with tuned parameters (temperature, topP) and token measurement.
/// </summary>
public class SupportChatService(PromptTemplateEngine templateEngine)
{
    private readonly PromptTemplateEngine _templateEngine = templateEngine;

    public async Task<PromptEngineeringResult> ProcessTicketAsync(
        IChatClient chatClient,
        SupportTicket ticket,
        PersonaType persona,
        float temperature = 0.3f,
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();

        var systemPrompt = _templateEngine.BuildSystemPrompt(persona);
        var userPrompt = _templateEngine.FormatPromptWithTicket(ticket, persona);

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt),
            new(ChatRole.User, userPrompt)
        };

        var options = new ChatOptions
        {
            Temperature = temperature,
            TopP = 0.9f,
            MaxOutputTokens = 800
        };

        var response = await chatClient.GetResponseAsync(messages, options, ct);
        sw.Stop();

        var responseText = response.Text ?? string.Empty;
        var totalPromptText = systemPrompt + "\n" + userPrompt + "\n" + responseText;
        var estimatedTokens = TokenEstimator.EstimateTokenCount(totalPromptText);

        return new PromptEngineeringResult(
            ticket.TicketId,
            persona,
            temperature,
            userPrompt,
            responseText,
            estimatedTokens,
            sw.Elapsed
        );
    }
}
