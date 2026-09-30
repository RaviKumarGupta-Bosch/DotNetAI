# 10 - AI Evaluation & LLM-as-a-Judge Quality Systems

## 🎯 Overview
In traditional software, quality is verified with deterministic unit tests. In generative AI, outputs are non-deterministic, making manual inspection impossible at scale.

**LLM-as-a-Judge** utilizes a high-capability LLM to systematically evaluate candidate outputs against rigorous scoring rubrics across the **RAG Triad**:
1. **Faithfulness / Groundedness**: Is the answer factually grounded in the reference documents without hallucination?
2. **Answer Relevance**: Does the response directly address the user's specific prompt?
3. **Semantic Coherence**: Is the explanation well-structured, logically sound, and clear?

---

## 🧠 Key Concepts & AI Theory

### The RAG Triad Evaluation Metrics
```
              [Reference Context Documents]
                     ▲           ▲
                     │           │
       Faithfulness  │           │  Context Relevance
                     │           │
                     ▼           ▼
             [Generated] ◀──────▶ [User Query]
              [Answer]      Answer
                          Relevance
```

### The Evaluation Rubric (1 to 5 Scale)
- **Score 1 (Critical Failure)**: Severe hallucination, completely off-topic, or logically contradictory.
- **Score 3 (Marginal / Flawed)**: Partially answered, minor unverified claims, or poor clarity.
- **Score 5 (Exemplary)**: 100% grounded in reference facts, concise, directly relevant, and clear.

---

## 💻 C# Implementation Walkthrough

### 1. Evaluation Score Model: `EvaluationResult.cs`
Located at `src/10-AIEvaluation-Judge/Models/EvaluationModels.cs`:
```csharp
public record MetricScore(
    string MetricName,
    int Score, // 1 to 5
    string Reasoning,
    bool Passed
);

public record EvaluationReport(
    string UserPrompt,
    string CandidateResponse,
    string? ReferenceContext,
    List<MetricScore> Metrics,
    double OverallScore,
    bool OverallPassed
);
```

### 2. Evaluator Service: `AIEvaluationJudgeService.cs`
Located at `src/10-AIEvaluation-Judge/Services/AIEvaluationJudgeService.cs`:
```csharp
public async Task<EvaluationReport> EvaluateResponseAsync(
    string userPrompt, 
    string candidateResponse, 
    string? referenceContext = null)
{
    var judgePrompt = $@"You are an expert AI quality judge. Evaluate the following Candidate Response.
User Prompt: {userPrompt}
Reference Context: {referenceContext ?? "None"}
Candidate Response: {candidateResponse}

Output JSON with scores (1-5) and detailed reasoning for:
1. Faithfulness (Groundedness)
2. Relevance
3. Coherence";

    var response = await _judgeChatClient.GetResponseAsync(
        new ChatMessage[] { new(ChatRole.User, judgePrompt) },
        new ChatOptions { Temperature = 0.0f, ResponseFormat = ChatResponseFormat.Json }
    );

    return ParseEvaluationReport(response.Text);
}
```

---

## 🧪 Running & Validating

### Run via .NET CLI:
```powershell
dotnet run --project src/10-AIEvaluation-Judge -- --mock
```

### Run Unit Tests:
```powershell
dotnet test tests/DotNetAI.UseCases.Tests --filter "FullyQualifiedName~AIEvaluationJudgeTests"
```
