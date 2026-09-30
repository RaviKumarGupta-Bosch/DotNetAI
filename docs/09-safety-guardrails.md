# 09 - AI Safety Guardrails, PII Masking & Injection Defense

## 🎯 Overview
Deploying AI in enterprise environments requires robust **Defense-in-Depth** safety measures. Without guardrails, applications are vulnerable to:
1. **PII Data Leakage**: Accidentally sending SSNs, Credit Cards, or API tokens to cloud LLMs.
2. **Prompt Injection & Jailbreaks**: Adversarial inputs tricking the model into ignoring system rules.
3. **Harmful Content Generation**: Outputting unverified, toxic, or unauthorized content.

This use case demonstrates how to implement pre-execution and post-execution guardrail pipelines in C#.

---

## 🧠 Key Concepts & AI Theory

```
[Raw User Input]
       │
       ▼ (1. Pre-Guardrail: PII Masking)
[Replaces SSN/CC/Keys with [REDACTED_PII]]
       │
       ▼ (2. Pre-Guardrail: Prompt Injection Classifier)
[Blocks "Ignore instructions", "DAN Mode", Delimiter attacks]
       │
       ▼ (Passed Safe?) ──▶ If Malicious ──▶ [Immediate Safety Refusal]
       │
       ▼ (3. LLM Execution)
[Raw AI Response]
       │
       ▼ (4. Post-Guardrail: Output Sanitizer)
[Filters Toxic Terms, Redacts residual keys, Enforces tone]
       │
       ▼
[Sanitized Enterprise Response Delivered]
```

---

## 💻 C# Implementation Walkthrough

### Guardrail Service: `GuardrailServices.cs`
Located at `src/09-SafetyGuardrails/Services/GuardrailServices.cs`:
```csharp
public class GuardrailService
{
    private static readonly Regex SsnRegex = new(@"\b\d{3}-\d{2}-\d{4}\b", RegexOptions.Compiled);
    private static readonly Regex CreditCardRegex = new(@"\b(?:\d{4}[ -]?){3}\d{4}\b", RegexOptions.Compiled);
    private static readonly Regex ApiKeyRegex = new(@"\b(?:sk|key|token)_[a-zA-Z0-9_\-]{16,}\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public PiiMaskingResult MaskPii(string rawInput)
    {
        var masked = SsnRegex.Replace(rawInput, "[REDACTED_SSN]");
        masked = CreditCardRegex.Replace(masked, "[REDACTED_CREDIT_CARD]");
        masked = ApiKeyRegex.Replace(masked, "[REDACTED_API_KEY]");
        return new PiiMaskingResult(masked, rawInput != masked);
    }

    public InjectionCheckResult CheckPromptInjection(string input)
    {
        string[] maliciousSignatures = {
            "ignore previous instructions", "system override", "dan mode",
            "you are now unfiltered", "disregard all prior rules"
        };

        foreach (var sig in maliciousSignatures)
        {
            if (input.Contains(sig, StringComparison.OrdinalIgnoreCase))
                return new InjectionCheckResult(true, sig, "High risk of prompt injection.");
        }
        return new InjectionCheckResult(false, null, "Input is clean.");
    }
}
```

---

## 🧪 Running & Validating

### Run via .NET CLI:
```powershell
dotnet run --project src/09-SafetyGuardrails -- --mock
```

### Run Unit Tests:
```powershell
dotnet test tests/DotNetAI.UseCases.Tests --filter "FullyQualifiedName~SafetyGuardrailsTests"
```
