namespace DotNetAI.PromptEngineering.Models;

public enum PersonaType
{
    EmpatheticSupport,
    TechnicalArchitect,
    ConciseBillingAdvisor,
    SeniorCodeReviewer
}

public record SupportTicket(
    string TicketId,
    string CustomerName,
    string CustomerTier,
    string IssueCategory,
    string Subject,
    string Description
);

public record PromptEngineeringResult(
    string TicketId,
    PersonaType Persona,
    float Temperature,
    string FormattedPrompt,
    string ResponseText,
    int EstimatedTokensUsed,
    TimeSpan ResponseDuration
);
