namespace DotNetAI.SafetyGuardrails.Models;

public enum SafetyViolationType
{
    None,
    PromptInjection,
    PIIExposed,
    ToxicContent,
    SystemPromptLeakage,
    ProhibitedTopic
}

public record PIIMaskingResult(
    string SanitizedText,
    List<string> RedactedItems,
    int TotalRedactionsCount
);

public record GuardrailCheckResult(
    bool IsSafe,
    SafetyViolationType ViolationType,
    string Reason,
    string? SanitizedInput = null
);

public record GuardrailedChatResponse(
    string FinalOutput,
    bool BlockedAtInput,
    bool BlockedAtOutput,
    List<GuardrailCheckResult> GuardrailLogs,
    TimeSpan TotalDuration
);
