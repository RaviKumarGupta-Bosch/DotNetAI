namespace DotNetAI.Core.Utils;

/// <summary>
/// High-performance vector math operations for embedding similarity and search.
/// </summary>
public static class VectorMath
{
    /// <summary>
    /// Calculates the cosine similarity between two float vectors.
    /// Returns a value between -1.0 and +1.0 (where 1.0 means identical direction).
    /// </summary>
    public static float CosineSimilarity(ReadOnlySpan<float> vectorA, ReadOnlySpan<float> vectorB)
    {
        if (vectorA.Length != vectorB.Length)
        {
            throw new ArgumentException($"Vector dimension mismatch: {vectorA.Length} vs {vectorB.Length}");
        }

        if (vectorA.IsEmpty)
        {
            return 0f;
        }

        float dotProduct = 0f;
        float normA = 0f;
        float normB = 0f;

        for (int i = 0; i < vectorA.Length; i++)
        {
            float a = vectorA[i];
            float b = vectorB[i];

            dotProduct += a * b;
            normA += a * a;
            normB += b * b;
        }

        if (normA <= 0f || normB <= 0f)
        {
            return 0f;
        }

        return dotProduct / (MathF.Sqrt(normA) * MathF.Sqrt(normB));
    }

    /// <summary>
    /// Normalizes a vector in-place to unit length (L2 norm = 1.0).
    /// </summary>
    public static float[] Normalize(float[] vector)
    {
        float sumSquares = 0f;
        for (int i = 0; i < vector.Length; i++)
        {
            sumSquares += vector[i] * vector[i];
        }

        float norm = MathF.Sqrt(sumSquares);
        if (norm <= 0f) return vector;

        var result = new float[vector.Length];
        for (int i = 0; i < vector.Length; i++)
        {
            result[i] = vector[i] / norm;
        }

        return result;
    }

    /// <summary>
    /// Computes Euclidean distance between two vectors.
    /// </summary>
    public static float EuclideanDistance(ReadOnlySpan<float> vectorA, ReadOnlySpan<float> vectorB)
    {
        if (vectorA.Length != vectorB.Length)
        {
            throw new ArgumentException($"Vector dimension mismatch: {vectorA.Length} vs {vectorB.Length}");
        }

        float sum = 0f;
        for (int i = 0; i < vectorA.Length; i++)
        {
            float diff = vectorA[i] - vectorB[i];
            sum += diff * diff;
        }

        return MathF.Sqrt(sum);
    }
}
