# 01 - Prompt Engineering, Personas & Temperature Tuning

## 🎯 Overview
Prompt engineering is the foundational skill of modern AI development. This use case demonstrates how to structure prompts, assign distinct personas (e.g., Software Architect vs. SRE Incident Responder vs. Security Analyst), apply Chain-of-Thought reasoning, and tune generation temperature for optimal determinism or creativity.

---

## 🧠 Key Concepts & AI Theory

### 1. Personas & System Messages
A **System Message** acts as the foundational operating instruction for an LLM. It defines:
- **Role Identity**: Who the AI represents (e.g., Senior Cloud Architect).
- **Domain Boundaries**: What topics the AI should focus on or refuse.
- **Tone & Style**: Direct, concise, technical, or exploratory.

### 2. Zero-Shot vs Few-Shot Prompting
- **Zero-Shot**: Prompting the model to solve a problem without prior examples.
- **Few-Shot**: Including 1–3 concrete example inputs and desired outputs directly in the prompt to enforce structure and style.

### 3. Chain-of-Thought (CoT) Prompting
By instructing the model to `"Think step-by-step before answering"`, we enable autoregressive reasoning where intermediate reasoning tokens act as working memory for subsequent conclusions.

### 4. Temperature Impact
| Temperature | Behavior | Typical Use Case |
| :--- | :--- | :--- |
| **0.0 - 0.2** | Deterministic, focused, highly reproducible | Code generation, data extraction, JSON |
| **0.5 - 0.7** | Balanced, conversational, clear | Technical explanations, chatbots |
| **0.8 - 1.2** | High variance, creative, explorative | Brainstorming, architectural trade-off exploration |

---

## 💻 C# Implementation Walkthrough

### Core Service: `PromptEngineeringService.cs`
Located at `src/01-PromptEngineering/Services/PromptEngineeringService.cs`:

```csharp
public async Task<string> GenerateWithPersonaAsync(
    string personaRole, 
    string systemInstructions, 
    string userPrompt, 
    float temperature = 0.2f)
{
    var messages = new List<ChatMessage>
    {
        new(ChatRole.System, $"You are a {personaRole}. {systemInstructions}"),
        new(ChatRole.User, userPrompt)
    };

    var options = new ChatOptions
    {
        Temperature = temperature,
        MaxOutputTokens = 1000
    };

    var response = await _chatClient.GetResponseAsync(messages, options);
    return response.Text ?? string.Empty;
}
```

---

## 🧪 Running & Validating

### Run via .NET CLI:
```powershell
# Run with local Ollama
dotnet run --project src/01-PromptEngineering -- --ollama

# Run with offline Mock mode
dotnet run --project src/01-PromptEngineering -- --mock
```

### Run Unit Tests:
```powershell
dotnet test tests/DotNetAI.UseCases.Tests --filter "FullyQualifiedName~PromptEngineeringTests"
```
