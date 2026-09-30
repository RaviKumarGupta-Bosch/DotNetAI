using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.AI;

namespace DotNetAI.Core.Testing;

/// <summary>
/// Deterministic mock embedding generator for testing vector search and RAG pipelines without requiring a remote model.
/// Produces consistent, normalized vector embeddings derived from input text tokens.
/// </summary>
public class MockEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    private readonly int _dimensions;

    public EmbeddingGeneratorMetadata Metadata { get; }

    public MockEmbeddingGenerator(int dimensions = 384, string modelId = "mock-embed")
    {
        _dimensions = dimensions;
        Metadata = new EmbeddingGeneratorMetadata("MockEmbeddingGenerator", new Uri("http://localhost:11434"), modelId, dimensions);
    }

    public Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values, 
        EmbeddingGenerationOptions? options = null, 
        CancellationToken cancellationToken = default)
    {
        var list = new List<Embedding<float>>();

        foreach (var text in values)
        {
            var vector = GenerateVectorForText(text ?? string.Empty, _dimensions);
            list.Add(new Embedding<float>(vector));
        }

        var result = new GeneratedEmbeddings<Embedding<float>>(list);
        return Task.FromResult(result);
    }

    /// <summary>
    /// Generates a deterministic, semantic-approximating pseudo-embedding based on word frequencies and hashing.
    /// </summary>
    private static float[] GenerateVectorForText(string text, int dimensions)
    {
        var vector = new float[dimensions];
        var words = text.ToLowerInvariant().Split(new[] { ' ', '\t', '\r', '\n', '.', ',', '!', '?', ';', ':', '-', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);

        if (words.Length == 0)
        {
            return vector;
        }

        // Feature hashing / bag-of-words projection
        foreach (var word in words)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(word));
            for (int i = 0; i < 4; i++)
            {
                int index = Math.Abs(BitConverter.ToInt32(hash, i * 4)) % dimensions;
                float sign = (hash[i * 4 + 3] % 2 == 0) ? 1.0f : -1.0f;
                vector[index] += sign * (1.0f / MathF.Sqrt(words.Length));
            }
        }

        // Normalize vector to unit length (L2 norm)
        float sumSquares = 0f;
        for (int i = 0; i < dimensions; i++)
        {
            sumSquares += vector[i] * vector[i];
        }

        float norm = MathF.Sqrt(sumSquares);
        if (norm > 0f)
        {
            for (int i = 0; i < dimensions; i++)
            {
                vector[i] /= norm;
            }
        }

        return vector;
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        if (serviceType == typeof(EmbeddingGeneratorMetadata)) return Metadata;
        return null;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
