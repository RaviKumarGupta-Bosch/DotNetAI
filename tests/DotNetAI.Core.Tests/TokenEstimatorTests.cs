using DotNetAI.Core.Utils;
using Xunit;

namespace DotNetAI.Core.Tests;

public class TokenEstimatorTests
{
    [Fact]
    public void EstimateTokenCount_NullOrEmpty_ReturnsZero()
    {
        Assert.Equal(0, TokenEstimator.EstimateTokenCount(null));
        Assert.Equal(0, TokenEstimator.EstimateTokenCount(string.Empty));
    }

    [Theory]
    [InlineData("Hello, World!", 3)]
    [InlineData("The quick brown fox jumps over the lazy dog.", 12)]
    public void EstimateTokenCount_NormalText_ReturnsReasonableApproximation(string text, int minTokens)
    {
        int estimated = TokenEstimator.EstimateTokenCount(text);
        Assert.True(estimated >= 1, "Token estimate should be positive.");
        Assert.True(estimated >= minTokens / 2 && estimated <= minTokens * 3, $"Estimate {estimated} for text '{text}' is within reasonable bounds.");
    }

    [Fact]
    public void EstimateTokens_Alias_MatchesEstimateTokenCount()
    {
        string sample = "Testing alias consistency across helper methods in DotNetAI.Core.";
        Assert.Equal(TokenEstimator.EstimateTokenCount(sample), TokenEstimator.EstimateTokens(sample));
    }
}
