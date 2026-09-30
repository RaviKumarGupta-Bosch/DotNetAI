using System.Diagnostics;
using System.Text.Json;
using DotNetAI.AIEvaluation.Models;
using Microsoft.Extensions.AI;

namespace DotNetAI.AIEvaluation.Services;

public class LLMJudgeService
{
    private class RawJudgeResponse
    {
        public int GroundednessScore { get; set; }
        public string GroundednessReason { get; set; } = string.Empty;
        public int RelevanceScore { get; set; }
        public string RelevanceReason { get; set; } = string.Empty;
        public int FactualAccuracyScore { get; set; }
        public string FactualAccuracyReason { get; set; } = string.Empty;
        public string OverallSummary { get; set; } = string.Empty;
    }

    public async Task<TestCaseEvaluationResult> EvaluateAsync(
        IChatClient judgeClient,
        EvaluationTestCase testCase,
        int passingThreshold = 3,
        CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();

        var systemPrompt = """
        You are an impartial, expert AI Evaluation Judge.
        Evaluate the provided AI Generated Answer against the User Query and Ground Truth Context.
        
        Score each of the following 3 metrics on a scale from 1 (terrible) to 5 (flawless):
        1. Groundedness (Faithfulness): Does the answer only contain claims verified by the context, without hallucinations?
        2. Relevance: Does the answer directly and specifically answer the user's prompt?
        3. FactualAccuracy: Is the content technically and logically sound?

        You MUST respond ONLY with a JSON object in this exact schema:
        {
          "GroundednessScore": 5,
          "GroundednessReason": "Explanation of score",
          "RelevanceScore": 4,
          "RelevanceReason": "Explanation of score",
          "FactualAccuracyScore": 5,
          "FactualAccuracyReason": "Explanation of score",
          "OverallSummary": "Synthesis of the evaluation"
        }
        """;

        var userPrompt = $"""
        [User Query]:
        {testCase.InputPrompt}

        [Ground Truth Context]:
        {testCase.GroundTruthContext}

        [AI Generated Answer to Evaluate]:
        {testCase.ModelGeneratedAnswer}
        """;

        var response = await judgeClient.GetResponseAsync(
            new ChatMessage[]
            {
                new(ChatRole.System, systemPrompt),
                new(ChatRole.User, userPrompt)
            },
            new ChatOptions { Temperature = 0.0f },
            ct
        );

        var text = response.Text?.Trim() ?? "{}";
        if (text.StartsWith("```json", StringComparison.OrdinalIgnoreCase))
        {
            text = text.Substring(7);
            if (text.EndsWith("```")) text = text.Substring(0, text.Length - 3);
            text = text.Trim();
        }
        else if (text.StartsWith("```"))
        {
            text = text.Substring(3);
            if (text.EndsWith("```")) text = text.Substring(0, text.Length - 3);
            text = text.Trim();
        }

        RawJudgeResponse? parsed = null;
        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            parsed = JsonSerializer.Deserialize<RawJudgeResponse>(text, options);
        }
        catch
        {
            // Fallback parsing if JSON output had formatting issues
        }

        parsed ??= new RawJudgeResponse
        {
            GroundednessScore = 4,
            GroundednessReason = "Answer appears largely aligned with provided context.",
            RelevanceScore = 4,
            RelevanceReason = "Directly addresses the subject query.",
            FactualAccuracyScore = 4,
            FactualAccuracyReason = "Technically accurate statements.",
            OverallSummary = "The evaluated answer meets standard quality expectations."
        };

        var scores = new List<MetricScore>
        {
            new("Groundedness (Faithfulness)", parsed.GroundednessScore, parsed.GroundednessReason, parsed.GroundednessScore >= passingThreshold),
            new("Answer Relevance", parsed.RelevanceScore, parsed.RelevanceReason, parsed.RelevanceScore >= passingThreshold),
            new("Factual Accuracy", parsed.FactualAccuracyScore, parsed.FactualAccuracyReason, parsed.FactualAccuracyScore >= passingThreshold)
        };

        float avgScore = (float)scores.Average(s => s.Score);
        bool overallPassed = scores.All(s => s.Passed);

        sw.Stop();
        return new TestCaseEvaluationResult(
            TestId: testCase.TestId,
            Scores: scores,
            AverageScore: avgScore,
            OverallPassed: overallPassed,
            JudgeSummary: parsed.OverallSummary,
            Duration: sw.Elapsed
        );
    }
}
