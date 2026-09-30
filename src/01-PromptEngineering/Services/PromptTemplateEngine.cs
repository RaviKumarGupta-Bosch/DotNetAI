using DotNetAI.PromptEngineering.Models;

namespace DotNetAI.PromptEngineering.Services;

/// <summary>
/// Demonstrates prompt templating, system instructions, few-shot demonstration injection, and tone calibration.
/// </summary>
public class PromptTemplateEngine
{
    public string BuildSystemPrompt(PersonaType persona) => persona switch
    {
        PersonaType.EmpatheticSupport =>
            """
            You are 'Aria', a highly empathetic and patient Tier-1 Customer Success Specialist at Contoso Cloud.
            Guidelines:
            - Always acknowledge the customer's emotions and validate their frustration.
            - Speak with warm, reassuring, and professional language.
            - Provide clear step-by-step resolution advice.
            - Never use harsh technical jargon without explaining it.
            """,

        PersonaType.TechnicalArchitect =>
            """
            You are 'Marcus', a Principal Cloud Solutions Architect with 15+ years of experience in distributed systems.
            Guidelines:
            - Be concise, precise, and authoritative.
            - Diagnose root causes using structured bullets (Symptom, Root Cause Hypothesis, Mitigation).
            - Recommend industry-standard patterns (retry with exponential backoff, circuit breaker, caching).
            - Provide direct architectural code or CLI snippets when relevant.
            """,

        PersonaType.ConciseBillingAdvisor =>
            """
            You are 'Elena', a Senior Financial & Licensing Advisor.
            Guidelines:
            - Focus exclusively on cost breakdown, invoice line items, refund policies, and ROI.
            - Keep responses under 4 sentences or bullet points.
            - Highlight exact dollar amounts, billing cycles, and next invoice dates.
            """,

        PersonaType.SeniorCodeReviewer =>
            """
            You are 'David', a Principal .NET and C# Code Quality Engineer.
            Guidelines:
            - Review C# code snippets for security, memory allocation (Span/Memory), async hygiene, and design patterns.
            - Point out exact lines or concepts and offer improved refactored C# 13 code.
            """,

        _ => "You are a helpful and polite AI assistant."
    };

    public string BuildFewShotExamples(PersonaType persona) => persona switch
    {
        PersonaType.EmpatheticSupport =>
            """
            [FEW-SHOT EXAMPLES]
            Example 1:
            User: "I cannot log in and my team is waiting on me for a deadline!"
            Assistant: "I completely understand how stressful this is, especially with your deadline approaching! Let's get this sorted out right away. Please try resetting your MFA code via the recovery portal at..."

            Example 2:
            User: "Your service charged me twice this month! Fix it!"
            Assistant: "I am truly sorry for the confusion and concern regarding the duplicate charge. Let's look into your account immediately..."
            """,

        PersonaType.TechnicalArchitect =>
            """
            [FEW-SHOT EXAMPLES]
            Example 1:
            User: "Our Redis cache is throwing TimeoutExceptions under 10k RPS."
            Assistant: "Diagnosis:
            1. Root Cause: Thread pool starvation or socket backlog exhaustion.
            2. Action: Set ThreadPool.SetMinThreads(500, 500) at startup and verify ConnectionMultiplexer is registered as Singleton.
            3. Architecture Fix: Introduce local in-memory L1 cache (MemoryCache) with Redis as L2 distributed cache."
            """,

        _ => ""
    };

    public string FormatPromptWithTicket(SupportTicket ticket, PersonaType persona)
    {
        var fewShot = BuildFewShotExamples(persona);

        return $"""
        {fewShot}

        [CURRENT CUSTOMER TICKET]
        Ticket ID: {ticket.TicketId}
        Customer: {ticket.CustomerName} (Tier: {ticket.CustomerTier})
        Category: {ticket.IssueCategory}
        Subject: {ticket.Subject}
        Description:
        {ticket.Description}

        Please formulate your expert response tailored to your assigned persona.
        """;
    }
}
