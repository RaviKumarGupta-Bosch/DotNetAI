# 02 - Structured Outputs & Self-Correcting Extraction Loops

## 🎯 Overview
In real-world enterprise applications, AI output must be reliably converted into strongly-typed C# objects (e.g., invoices, telemetry incident reports, customer records) rather than unpredictable unstructured text.

This use case demonstrates how to extract structured JSON matching C# records with automatic schema constraints and a **self-correcting retry loop** that fixes malformed JSON on the fly.

---

## 🧠 Key Concepts & AI Theory

### 1. JSON Mode & Schema Constrained Decoding
LLMs can be instructed to output strictly valid JSON conforming to a specific JSON schema. In `Microsoft.Extensions.AI`, this is supported via `ChatResponseFormat.Json`.

### 2. Self-Correcting Error Loops
Even with JSON mode, smaller local models may occasionally produce truncated or syntactically invalid JSON.
A **Self-Correcting Loop** handles this autonomously:
1. Attempt `System.Text.Json.JsonSerializer.Deserialize<T>(rawText)`.
2. If a `JsonException` occurs, capture the exception message.
3. Formulate a correction prompt:
   ```
   "The previous JSON output failed validation with error: {error}.
   Original malformed text: {rawText}
   Please output ONLY the corrected, valid JSON matching the schema."
   ```
4. Re-prompt the model and deserialize the corrected payload.

---

## 💻 C# Implementation Walkthrough

### Typed Models: `IncidentReport.cs` & `InvoiceData.cs`
```csharp
public record IncidentReport(
    string IncidentId,
    string Severity,
    string AffectedService,
    string RootCauseSummary,
    List<string> RecommendedMitigations,
    double EstimatedDowntimeMinutes
);
```

### Self-Correcting Extractor: `StructuredDataExtractorService.cs`
```csharp
public async Task<ExtractionResult<T>> ExtractStructuredDataAsync<T>(
    string unstructuredText, 
    string schemaDescription, 
    int maxRetries = 2)
{
    var currentPrompt = $"Extract the following information into valid JSON conforming to: {schemaDescription}\n\nInput Text:\n{unstructuredText}";
    
    for (int attempt = 1; attempt <= maxRetries + 1; attempt++)
    {
        var response = await _chatClient.GetResponseAsync(
            new ChatMessage[] { new(ChatRole.User, currentPrompt) },
            new ChatOptions { Temperature = 0.1f, ResponseFormat = ChatResponseFormat.Json }
        );

        string rawJson = CleanJsonMarkdown(response.Text);
        try
        {
            var data = JsonSerializer.Deserialize<T>(rawJson, _jsonOptions);
            return new ExtractionResult<T>(true, data, attempt, rawJson, null);
        }
        catch (JsonException ex) when (attempt <= maxRetries)
        {
            currentPrompt = $"Your previous output produced a JSON parsing error: {ex.Message}\nRaw output was: {rawJson}\nPlease correct the syntax and return ONLY valid JSON.";
        }
    }
    return new ExtractionResult<T>(false, default, maxRetries + 1, string.Empty, "Exceeded retries.");
}
```

---

## 🧪 Running & Validating

### Run via .NET CLI:
```powershell
dotnet run --project src/02-StructuredOutputs -- --mock
```

### Run Unit Tests:
```powershell
dotnet test tests/DotNetAI.UseCases.Tests --filter "FullyQualifiedName~StructuredOutputsTests"
```
