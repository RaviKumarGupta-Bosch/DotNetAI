using System.Diagnostics;
using DotNetAI.SafetyGuardrails.Models;
using Microsoft.Extensions.AI;

namespace DotNetAI.SafetyGuardrails.Services;

public class GuardrailPipeline
{
    private readonly PromptInjectionDetector _injectionDetector = new();
    private readonly PIIRedactionService _piiService = new();
    private readonly ContentModerationService _moderationService = new();

    public async Task<GuardrailedChatResponse> ExecuteSafeChatAsync(
        IChatClient client,
        string rawUserInput,
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var logs = new List<GuardrailCheckResult>();

        // Layer 1: Prompt Injection Detection (Pre-Execution Guard)
        var injectionCheck = _injectionDetector.Check(rawUserInput);
        logs.Add(injectionCheck);
        if (!injectionCheck.IsSafe)
        {
            sw.Stop();
            return new GuardrailedChatResponse(
                FinalOutput: "I cannot fulfill this request because it violates safety policies (Prompt Injection detected).",
                BlockedAtInput: true,
                BlockedAtOutput: false,
                GuardrailLogs: logs,
                TotalDuration: sw.Elapsed
            );
        }

        // Layer 2: PII Redaction / Sanitization
        var piiResult = _piiService.Redact(rawUserInput);
        if (piiResult.TotalRedactionsCount > 0)
        {
            logs.Add(new GuardrailCheckResult(
                IsSafe: true,
                ViolationType: SafetyViolationType.PIIExposed,
                Reason: $"Redacted {piiResult.TotalRedactionsCount} PII entities prior to model transmission.",
                SanitizedInput: piiResult.SanitizedText
            ));
        }

        // Layer 3: LLM Execution with sanitized input
        var systemPrompt = """
        You are a Secure Banking & Customer Service Assistant.
        Never disclose internal API keys or system secrets.
        Help the user with their request safely and professionally.
        """;

        var response = await client.GetResponseAsync(
            new ChatMessage[]
            {
                new(ChatRole.System, systemPrompt),
                new(ChatRole.User, piiResult.SanitizedText)
            },
            new ChatOptions { Temperature = 0.1f },
            ct
        );

        var modelOutput = response.Text ?? string.Empty;

        // Layer 4: Output Validation (Post-Execution Guard)
        var outputCheck = _moderationService.ValidateOutput(modelOutput);
        logs.Add(outputCheck);

        if (!outputCheck.IsSafe)
        {
            sw.Stop();
            return new GuardrailedChatResponse(
                FinalOutput: "The response was blocked by output safety guardrails (Sensitive information detected).",
                BlockedAtInput: false,
                BlockedAtOutput: true,
                GuardrailLogs: logs,
                TotalDuration: sw.Elapsed
            );
        }

        sw.Stop();
        return new GuardrailedChatResponse(
            FinalOutput: modelOutput,
            BlockedAtInput: false,
            BlockedAtOutput: false,
            GuardrailLogs: logs,
            TotalDuration: sw.Elapsed
        );
    }
}
