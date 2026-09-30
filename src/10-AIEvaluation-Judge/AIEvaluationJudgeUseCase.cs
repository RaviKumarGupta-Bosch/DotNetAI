using System.Diagnostics;
using DotNetAI.AIEvaluation.Models;
using DotNetAI.AIEvaluation.Services;
using DotNetAI.Core.Interfaces;
using Microsoft.Extensions.AI;

namespace DotNetAI.AIEvaluation;

/// <summary>
/// Use Case 10: AI Evaluation & Automated LLM-as-a-Judge Benchmarking.
/// </summary>
public class AIEvaluationJudgeUseCase : IAIUseCase
{
    public int Id => 10;
    public string Name => "AI Evaluation & LLM-as-a-Judge";
    public string Description => "Demonstrates quantitative evaluation of AI outputs using LLM-as-a-Judge architecture: scoring Groundedness/Faithfulness, Relevance, and Factual Accuracy with rubric reasoning.";
    public IReadOnlyList<string> AIConcepts => new[]
    {
        "LLM-as-a-Judge Architecture",
        "Groundedness & Faithfulness Scoring (Hallucination Detection)",
        "Answer Relevance & Semantic Alignment Metrics",
        "Automated Test Rubrics & Quantitative Scoring",
        "Continuous AI Regression Benchmarking"
    };

    private readonly LLMJudgeService _judgeService = new();

    public async Task<UseCaseExecutionResult> RunAsync(
        IChatClient chatClient, 
        IEmbeddingGenerator<string, Embedding<float>>? embeddingGenerator = null, 
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var result = new UseCaseExecutionResult();
        result.Logs.Add("[UseCase 10] Starting AI Evaluation & LLM-as-a-Judge Benchmarking demo...");

        var testCases = GetBenchmarkTestCases();
        var evalResults = new List<TestCaseEvaluationResult>();

        foreach (var tc in testCases)
        {
            result.Logs.Add($"\n-----------------------------------------------------------");
            result.Logs.Add($"Evaluating Test Case [{tc.TestId}]: \"{tc.InputPrompt}\"");
            result.Logs.Add($"Model Output under test: \"{tc.ModelGeneratedAnswer}\"");

            var eval = await _judgeService.EvaluateAsync(chatClient, tc, passingThreshold: 3, cancellationToken);
            evalResults.Add(eval);

            result.Logs.Add($"Judge Scores (Avg: {eval.AverageScore:F1}/5.0 | Overall: {(eval.OverallPassed ? "PASSED ✅" : "FAILED ❌")}):");
            foreach (var score in eval.Scores)
            {
                result.Logs.Add($"  - {score.MetricName}: {score.Score}/5 [{(score.Passed ? "PASS" : "FAIL")}] -> {score.Justification}");
            }
            result.Logs.Add($"Judge Summary: {eval.JudgeSummary}");
        }

        var overallAvg = (float)evalResults.Average(e => e.AverageScore);
        int passedCount = evalResults.Count(e => e.OverallPassed);
        int failedCount = evalResults.Count(e => !e.OverallPassed);

        var benchmarkReport = new BenchmarkEvaluationReport(
            SuiteName: "Enterprise AI Quality Benchmark",
            TestResults: evalResults,
            OverallAverageScore: overallAvg,
            TotalPassed: passedCount,
            TotalFailed: failedCount,
            TotalDuration: sw.Elapsed
        );

        result.Logs.Add($"\n===========================================================");
        result.Logs.Add($"📊 Benchmark Summary: {passedCount}/{testCases.Count} Passed | Avg Quality Score: {overallAvg:F2}/5.0");
        result.Logs.Add($"===========================================================");

        result.Outputs["BenchmarkReport"] = benchmarkReport;

        sw.Stop();
        result.Duration = sw.Elapsed;
        result.Summary = $"LLM Judge evaluated {testCases.Count} test scenarios with overall score of {overallAvg:F2}/5.0 ({passedCount} passed, {failedCount} failed).";
        return result;
    }

    private static List<EvaluationTestCase> GetBenchmarkTestCases() => new()
    {
        new EvaluationTestCase(
            TestId: "TC-01-HIGH-QUALITY",
            InputPrompt: "What is the timeout duration for database connections in cluster EastUS?",
            GroundTruthContext: "In database cluster EastUS, the connection timeout is strictly configured to 15 seconds, and connection retry limit is 3 attempts.",
            ModelGeneratedAnswer: "The database connection timeout for the EastUS cluster is 15 seconds, with a maximum of 3 retry attempts."
        ),
        new EvaluationTestCase(
            TestId: "TC-02-HALLUCINATION",
            InputPrompt: "What is the maximum file upload size for the customer portal?",
            GroundTruthContext: "Customer portal supports file uploads up to 50 megabytes per attachment in PDF or PNG formats.",
            ModelGeneratedAnswer: "The customer portal allows uploading files up to 5 gigabytes and supports any video format."
        )
    };
}
