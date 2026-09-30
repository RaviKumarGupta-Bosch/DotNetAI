using DotNetAI.Core.Utils;
using Xunit;

namespace DotNetAI.Core.Tests;

public class VectorMathTests
{
    [Fact]
    public void CosineSimilarity_IdenticalVectors_ReturnsOne()
    {
        ReadOnlySpan<float> v1 = [1.0f, 2.0f, 3.0f];
        ReadOnlySpan<float> v2 = [1.0f, 2.0f, 3.0f];

        float similarity = VectorMath.CosineSimilarity(v1, v2);

        Assert.Equal(1.0f, similarity, precision: 5);
    }

    [Fact]
    public void CosineSimilarity_OrthogonalVectors_ReturnsZero()
    {
        ReadOnlySpan<float> v1 = [1.0f, 0.0f, 0.0f];
        ReadOnlySpan<float> v2 = [0.0f, 1.0f, 0.0f];

        float similarity = VectorMath.CosineSimilarity(v1, v2);

        Assert.Equal(0.0f, similarity, precision: 5);
    }

    [Fact]
    public void CosineSimilarity_OppositeVectors_ReturnsNegativeOne()
    {
        ReadOnlySpan<float> v1 = [1.0f, 0.0f];
        ReadOnlySpan<float> v2 = [-1.0f, 0.0f];

        float similarity = VectorMath.CosineSimilarity(v1, v2);

        Assert.Equal(-1.0f, similarity, precision: 5);
    }

    [Fact]
    public void CosineSimilarity_ZeroVector_ReturnsZero()
    {
        ReadOnlySpan<float> v1 = [0.0f, 0.0f, 0.0f];
        ReadOnlySpan<float> v2 = [1.0f, 2.0f, 3.0f];

        float similarity = VectorMath.CosineSimilarity(v1, v2);

        Assert.Equal(0.0f, similarity);
    }

    [Fact]
    public void CosineSimilarity_MismatchedDimensions_ThrowsArgumentException()
    {
        float[] v1 = [1.0f, 2.0f];
        float[] v2 = [1.0f, 2.0f, 3.0f];

        Assert.Throws<ArgumentException>(() => VectorMath.CosineSimilarity(v1, v2));
    }

    [Fact]
    public void Normalize_ScalesVectorToUnitLength()
    {
        float[] vector = [3.0f, 4.0f];
        var normalized = VectorMath.Normalize(vector);

        float norm = MathF.Sqrt(normalized[0] * normalized[0] + normalized[1] * normalized[1]);
        Assert.Equal(1.0f, norm, precision: 5);
        Assert.Equal(0.6f, normalized[0], precision: 5);
        Assert.Equal(0.8f, normalized[1], precision: 5);
    }
}
