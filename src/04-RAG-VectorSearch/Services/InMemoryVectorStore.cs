using DotNetAI.Core.Utils;
using DotNetAI.RAG.Models;
using Microsoft.Extensions.AI;

namespace DotNetAI.RAG.Services;

public interface IVectorStore
{
    Task AddDocumentsAsync(IEnumerable<KnowledgeDocument> documents, CancellationToken ct = default);
    Task<IReadOnlyList<SearchResult>> SearchAsync(string query, int topK = 3, float minScore = 0.4f, CancellationToken ct = default);
    int TotalChunks { get; }
}

/// <summary>
/// High-performance in-memory vector store computing exact Cosine Similarity across document embeddings.
/// </summary>
public class InMemoryVectorStore : IVectorStore
{
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly IDocumentChunker _chunker;
    private readonly List<DocumentChunk> _chunks = new();
    private readonly object _lock = new();

    public InMemoryVectorStore(
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        IDocumentChunker? chunker = null)
    {
        _embeddingGenerator = embeddingGenerator;
        _chunker = chunker ?? new RecursiveDocumentChunker();
    }

    public int TotalChunks
    {
        get
        {
            lock (_lock) { return _chunks.Count; }
        }
    }

    public async Task AddDocumentsAsync(IEnumerable<KnowledgeDocument> documents, CancellationToken ct = default)
    {
        var allChunks = new List<DocumentChunk>();
        foreach (var doc in documents)
        {
            var docChunks = _chunker.Chunk(doc);
            allChunks.AddRange(docChunks);
        }

        if (allChunks.Count == 0) return;

        // Batch generate embeddings
        var texts = allChunks.Select(c => c.Text).ToList();
        var embeddings = await _embeddingGenerator.GenerateAsync(texts, cancellationToken: ct);

        lock (_lock)
        {
            for (int i = 0; i < allChunks.Count; i++)
            {
                var chunk = allChunks[i] with { Embedding = embeddings[i].Vector.ToArray() };
                _chunks.Add(chunk);
            }
        }
    }

    public async Task<IReadOnlyList<SearchResult>> SearchAsync(string query, int topK = 3, float minScore = 0.4f, CancellationToken ct = default)
    {
        var queryEmbeddings = await _embeddingGenerator.GenerateAsync(new[] { query }, cancellationToken: ct);
        var queryVector = queryEmbeddings[0].Vector.ToArray();

        List<DocumentChunk> snapshot;
        lock (_lock)
        {
            snapshot = _chunks.ToList();
        }

        var results = new List<SearchResult>();
        foreach (var chunk in snapshot)
        {
            if (chunk.Embedding == null) continue;
            float similarity = VectorMath.CosineSimilarity(queryVector, chunk.Embedding);
            if (similarity >= minScore)
            {
                results.Add(new SearchResult(chunk, similarity));
            }
        }

        return results.OrderByDescending(r => r.SimilarityScore).Take(topK).ToList();
    }
}
