using System.Text.Json;
using System.Text.RegularExpressions;
using DotNetAI.StructuredOutputs.Models;
using Microsoft.Extensions.AI;

namespace DotNetAI.StructuredOutputs.Services;

/// <summary>
/// Service that extracts structured POCO records from unstructured text with self-correction retry capabilities.
/// </summary>
public class StructuredDataExtractorService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public async Task<ExtractionResult<T>> ExtractAsync<T>(
        IChatClient chatClient,
        string rawDocumentText,
        Func<T, List<string>>? customValidator = null,
        int maxRetries = 2,
        CancellationToken ct = default) where T : class
    {
        var result = new ExtractionResult<T>();
        var schemaDescription = JsonSchemaExtractor.GenerateSchemaDescription<T>();

        var systemPrompt = $"""
        You are an expert Data Extraction Engine.
        Your task is to parse raw text and return a strictly valid JSON object matching the requested schema.
        Do NOT wrap the output in markdown code blocks like ```json or ```. Return ONLY the raw JSON string.
        JSON Schema template:
        {schemaDescription}
        """;

        var conversation = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt),
            new(ChatRole.User, $"Extract structured data from the following document:\n\n{rawDocumentText}")
        };

        for (int attempt = 1; attempt <= maxRetries + 1; attempt++)
        {
            result.AttemptCount = attempt;

            var response = await chatClient.GetResponseAsync(conversation, new ChatOptions
            {
                Temperature = 0.0f // Low temperature for high schema adherence
            }, ct);

            var rawJson = CleanJsonOutput(response.Text ?? string.Empty);
            result.RawJson = rawJson;

            try
            {
                var parsedData = JsonSerializer.Deserialize<T>(rawJson, JsonOptions);
                if (parsedData != null)
                {
                    // Run custom domain validation (e.g. subtotal + tax == total)
                    var validationErrors = customValidator?.Invoke(parsedData) ?? new List<string>();

                    if (validationErrors.Count == 0)
                    {
                        result.IsSuccess = true;
                        result.Data = parsedData;
                        return result;
                    }

                    result.ValidationErrors = validationErrors;
                    result.RequiredSelfCorrection = true;

                    // Feed validation errors back to LLM for self-correction
                    conversation.Add(new ChatMessage(ChatRole.Assistant, rawJson));
                    conversation.Add(new ChatMessage(ChatRole.User, 
                        $"The previous JSON had semantic/validation errors:\n- {string.Join("\n- ", validationErrors)}\nPlease fix the errors and output the corrected JSON."));
                }
            }
            catch (JsonException ex)
            {
                result.ValidationErrors.Add($"JSON Syntax Error: {ex.Message}");
                result.RequiredSelfCorrection = true;

                // Feed syntax error back to LLM for self-correction
                conversation.Add(new ChatMessage(ChatRole.Assistant, rawJson));
                conversation.Add(new ChatMessage(ChatRole.User, 
                    $"The JSON failed to parse due to syntax errors: {ex.Message}. Please return ONLY valid, correctly formatted JSON."));
            }
        }

        return result;
    }

    private static string CleanJsonOutput(string text)
    {
        var cleaned = text.Trim();

        // Strip markdown ```json ... ``` code fence if present
        if (cleaned.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            cleaned = cleaned[7..];
        }
        else if (cleaned.StartsWith("```"))
        {
            cleaned = cleaned[3..];
        }

        if (cleaned.EndsWith("```"))
        {
            cleaned = cleaned[..^3];
        }

        // Locate first '{' and last '}'
        int firstBrace = cleaned.IndexOf('{');
        int lastBrace = cleaned.LastIndexOf('}');
        if (firstBrace >= 0 && lastBrace > firstBrace)
        {
            cleaned = cleaned.Substring(firstBrace, lastBrace - firstBrace + 1);
        }

        return cleaned.Trim();
    }
}
