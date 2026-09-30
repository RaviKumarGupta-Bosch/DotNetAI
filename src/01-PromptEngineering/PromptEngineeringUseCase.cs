using System.Diagnostics;
using DotNetAI.Core.Interfaces;
using DotNetAI.PromptEngineering.Models;
using DotNetAI.PromptEngineering.Services;
using Microsoft.Extensions.AI;

namespace DotNetAI.PromptEngineering;

/// <summary>
/// Use Case 01: Prompt Engineering & Parameter Tuning for Local LLMs.
/// Demonstrates System Prompts, Few-Shot Demonstrations, Temperature adjustments, and Persona steering.
/// </summary>
public class PromptEngineeringUseCase : IAIUseCase
{
    public int Id => 1;
    public string Name => "Prompt Engineering & Local LLM Tuning";
    public string Description => "Demonstrates role-based system prompting, few-shot demonstration learning, parameter tuning (temperature/top_p), and token estimation for Customer Support tickets.";
    public IReadOnlyList<string> AIConcepts => new[]
    {
        "System Prompting & Role Specialization",
        "Few-Shot In-Context Learning",
        "Dynamic Prompt Templates",
        "Hyperparameter Tuning (Temperature, TopP, MaxTokens)",
        "Token Estimation & Cost Budgeting",
        "IChatClient Abstraction in Microsoft.Extensions.AI"
    };

    private readonly SupportChatService _chatService;

    public PromptEngineeringUseCase()
    {
        var templateEngine = new PromptTemplateEngine();
        _chatService = new SupportChatService(templateEngine);
    }

    public async Task<UseCaseExecutionResult> RunAsync(
        IChatClient chatClient, 
        IEmbeddingGenerator<string, Embedding<float>>? embeddingGenerator = null, 
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var result = new UseCaseExecutionResult();
        result.Logs.Add("[UseCase 01] Initializing Prompt Engineering demo with sample tickets...");

        var ticket1 = new SupportTicket(
            TicketId: "TCK-1092",
            CustomerName: "Sarah Jenkins",
            CustomerTier: "Enterprise",
            IssueCategory: "Outage / Escalation",
            Subject: "Production database cluster connection timeout spikes",
            Description: "Our primary PostgreSQL cluster in East US started dropping 15% of inbound queries during peak traffic. Our deployment pipeline is blocked."
        );

        var ticket2 = new SupportTicket(
            TicketId: "TCK-8812",
            CustomerName: "TechCorp Labs",
            CustomerTier: "Developer",
            IssueCategory: "Billing & Invoicing",
            Subject: "Unexpected bandwidth charges on monthly statement",
            Description: "We noticed our invoice is $400 higher than expected due to outbound egress traffic. Please explain this line item and suggest optimization."
        );

        // Scenario 1: Technical Architect Persona with low temperature (deterministic)
        result.Logs.Add("--- Scenario 1: Processing with Technical Architect Persona (Temp=0.1) ---");
        var res1 = await _chatService.ProcessTicketAsync(chatClient, ticket1, PersonaType.TechnicalArchitect, 0.1f, cancellationToken);
        result.Logs.Add($"[Ticket 1 Response]\n{res1.ResponseText}");
        result.Outputs["TechnicalArchitectResult"] = res1;

        // Scenario 2: Empathetic Support Persona with medium temperature (creative/warm)
        result.Logs.Add("--- Scenario 2: Processing with Empathetic Support Persona (Temp=0.6) ---");
        var res2 = await _chatService.ProcessTicketAsync(chatClient, ticket1, PersonaType.EmpatheticSupport, 0.6f, cancellationToken);
        result.Logs.Add($"[Ticket 1 Empathetic Response]\n{res2.ResponseText}");
        result.Outputs["EmpatheticSupportResult"] = res2;

        // Scenario 3: Billing Advisor Persona
        result.Logs.Add("--- Scenario 3: Processing with Concise Billing Advisor Persona (Temp=0.2) ---");
        var res3 = await _chatService.ProcessTicketAsync(chatClient, ticket2, PersonaType.ConciseBillingAdvisor, 0.2f, cancellationToken);
        result.Logs.Add($"[Ticket 2 Billing Response]\n{res3.ResponseText}");
        result.Outputs["BillingAdvisorResult"] = res3;

        sw.Stop();
        result.Duration = sw.Elapsed;
        result.Summary = $"Successfully processed {result.Outputs.Count} prompt scenarios with different personas, temperatures, and token estimates.";
        return result;
    }
}
