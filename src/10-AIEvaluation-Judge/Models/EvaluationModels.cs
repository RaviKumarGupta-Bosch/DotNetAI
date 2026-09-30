namespace DotNetAI.AIEvaluation.Models;

public record EvaluationTestCase(
    string TestId,
    string InputPrompt,
    string GroundTruthContext,
    string ModelGeneratedAnswer
);

public record MetricScore(
    string MetricName,
    int Score, // 1 to 5
    string Justification,
    bool Passed
);

public record TestCaseEvaluationResult(
    string TestId,
    List<MetricScore> Scores,
    float AverageScore,
    bool OverallPassed,
    string JudgeSummary,
    TimeSpan Duration
);

public record BenchmarkEvaluationReport(
    string SuiteName,
    List<TestCaseEvaluationResult> TestResults,
    float OverallAverageScore,
    int TotalPassed,
    int TotalFailed,
    TimeSpan TotalDuration
);
