namespace DotNetAI.RAG.Models;

public record KnowledgeDocument(
    string Id,
    string Title,
    string Category,
    string Content,
    DateTime CreatedDate
);

public record DocumentChunk(
    string ChunkId,
    string DocumentId,
    string DocumentTitle,
    string Text,
    int ChunkIndex,
    int StartOffset,
    int EndOffset,
    float[]? Embedding = null
);

public record SearchResult(
    DocumentChunk Chunk,
    float SimilarityScore
);

public record RAGResponse(
    string Query,
    string Answer,
    List<SearchResult> RetrievedContexts,
    float ConfidenceScore,
    TimeSpan RetrievalDuration,
    TimeSpan GenerationDuration
);
