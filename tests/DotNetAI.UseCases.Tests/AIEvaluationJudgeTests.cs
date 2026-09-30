using DotNetAI.AIEvaluation.Models;
using DotNetAI.AIEvaluation.Services;
using DotNetAI.Core.Testing;
using Xunit;

namespace DotNetAI.UseCases.Tests;

public class AIEvaluationJudgeTests
{
    private readonly LLMJudgeService _judgeService = new();

    [Fact]
    public async Task LLMJudgeService_EvaluateAsync_FaithfulAnswer_ReturnsPassingScore()
    {
        string judgeJson = """
        {
          "GroundednessScore": 5,
          "GroundednessReason": "The response directly quotes the provided context without introducing ungrounded assertions.",
          "RelevanceScore": 5,
          "RelevanceReason": "Directly answers the upload limit question.",
          "FactualAccuracyScore": 5,
          "FactualAccuracyReason": "Fully aligned with official specs.",
          "OverallSummary": "Accurate, grounded, and concise."
        }
        """;

        var mock = new MockChatClient(judgeJson);

        var testCase = new EvaluationTestCase(
            "Test-001",
            "What is the maximum file upload limit?",
            "The file upload limit is 50MB per batch.",
            "Files up to 50MB can be uploaded per batch."
        );

        var result = await _judgeService.EvaluateAsync(mock, testCase, passingThreshold: 3);

        Assert.NotNull(result);
        Assert.True(result.OverallPassed);
        Assert.Equal(5.0f, result.AverageScore, precision: 2);
        Assert.Equal(3, result.Scores.Count);
    }

    [Fact]
    public async Task LLMJudgeService_EvaluateAsync_Hallucination_ReturnsFailingScore()
    {
        string judgeJson = """
        {
          "GroundednessScore": 1,
          "GroundednessReason": "The response claimed 100GB limit when context stated 50MB.",
          "RelevanceScore": 4,
          "RelevanceReason": "Answers the topic but provides incorrect numbers.",
          "FactualAccuracyScore": 1,
          "FactualAccuracyReason": "Incorrect data stated as fact.",
          "OverallSummary": "Severe hallucination on critical metric."
        }
        """;

        var mock = new MockChatClient(judgeJson);

        var testCase = new EvaluationTestCase(
            "Test-002",
            "What is the maximum file upload limit?",
            "The file upload limit is 50MB per batch.",
            "You can upload up to 100GB of files per batch without restrictions."
        );

        var result = await _judgeService.EvaluateAsync(mock, testCase, passingThreshold: 3);

        Assert.NotNull(result);
        Assert.False(result.OverallPassed);
        Assert.True(result.AverageScore < 3.0f);
    }
}
