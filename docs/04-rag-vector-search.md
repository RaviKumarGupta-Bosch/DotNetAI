# 04 - Retrieval-Augmented Generation (RAG) & Vector Search

## 🎯 Overview
LLMs lack private organizational knowledge. **Retrieval-Augmented Generation (RAG)** overcomes this limitation by retrieving relevant document snippets from a vector database and injecting them into the LLM's prompt as grounded context.

This use case demonstrates how to build a complete RAG pipeline in .NET 9: recursive document chunking with overlap, vector embedding generation, in-memory cosine similarity search, and grounded response synthesis with source citations.

---

## 🧠 Key Concepts & AI Theory

### 1. The RAG Pipeline Architecture
```
[Document Corpus (.md, .pdf)]
           │
           ▼ (1. Chunking: 250 tokens, 50 token overlap)
[Chunks: C1, C2, ... Cn]
           │
           ▼ (2. Embedding Generation via nomic-embed-text)
[Vectors: V1, V2, ... Vn] ──▶ Stored in InMemoryVectorStore
           │
[User Query: "What is the token expiration policy?"]
           │
           ▼ (3. Query Embedding & Cosine Similarity)
[Top-K Nearest Document Chunks]
           │
           ▼ (4. Grounded Prompt Formulation)
"Answer the user query using ONLY the following facts. Cite your sources.
Facts:
[Doc 1]: The JWT token expiration is 15 minutes...
Query: What is the token expiration policy?"
           │
           ▼ (5. LLM Synthesis)
[Grounded Answer: "According to our security policy, JWT tokens expire in 15 minutes."]
```

### 2. Recursive Chunking with Overlap
Splitting documents purely by fixed character counts can cut words or sentences in half, destroying semantic meaning.
**Recursive Chunking**:
- First splits on Markdown headers (`#`, `##`)
- Then splits on double newlines (paragraphs)
- Then splits on sentences
- Maintains a **Sliding Overlap** (e.g., 50 tokens) between adjacent chunks to preserve continuity across boundaries.

### 3. Vector Similarity Math
Cosine similarity measures the angular alignment between query vector $\mathbf{Q}$ and document vector $\mathbf{D}$:
$$\text{Sim}(\mathbf{Q}, \mathbf{D}) = \frac{\sum_{i=1}^n Q_i D_i}{\sqrt{\sum_{i=1}^n Q_i^2} \sqrt{\sum_{i=1}^n D_i^2}}$$

---

## 💻 C# Implementation Walkthrough

### 1. Recursive Document Chunker: `DocumentChunker.cs`
Located at `src/04-RAG-VectorSearch/Services/DocumentChunker.cs`:
Splits Markdown text into chunks of maximum token size while preserving paragraph structure and adding sliding window overlap.

### 2. In-Memory Vector Store: `InMemoryVectorStore.cs`
```csharp
public class InMemoryVectorStore
{
    private readonly List<VectorDocument> _documents = new();

    public void Add(VectorDocument doc) => _documents.Add(doc);

    public List<ScoredDocument> SearchTopK(ReadOnlyMemory<float> queryEmbedding, int topK = 3)
    {
        return _documents
            .Select(doc => new ScoredDocument(
                doc, 
                VectorMath.CosineSimilarity(queryEmbedding.Span, doc.Embedding.Span)))
            .OrderByDescending(x => x.Score)
            .Take(topK)
            .ToList();
    }
}
```

### 3. Grounded RAG Pipeline: `RagPipelineService.cs`
```csharp
public async Task<RagResponse> QueryAsync(string userQuery, int topK = 3)
{
    // 1. Vectorize query
    var queryEmbedding = await _embeddingGenerator.GenerateEmbeddingVectorAsync(userQuery);

    // 2. Retrieve top-k nearest chunks
    var relevantDocs = _vectorStore.SearchTopK(queryEmbedding, topK);

    // 3. Construct grounded prompt
    var contextText = string.Join("\n\n", relevantDocs.Select((d, idx) => 
        $"[Source {idx+1}: {d.Document.SourceFile}]\n{d.Document.Content}"));

    var prompt = $"Use the following reference context to answer the question. If the answer cannot be found, say 'I don't have enough information'.\n\nContext:\n{contextText}\n\nQuestion: {userQuery}";

    // 4. Generate grounded synthesis
    var response = await _chatClient.GetResponseAsync(
        new ChatMessage[] { new(ChatRole.User, prompt) },
        new ChatOptions { Temperature = 0.1f }
    );

    return new RagResponse(response.Text, relevantDocs);
}
```

---

## 🧪 Running & Validating

### Run via .NET CLI:
```powershell
dotnet run --project src/04-RAG-VectorSearch -- --mock
```

### Run Unit Tests:
```powershell
dotnet test tests/DotNetAI.UseCases.Tests --filter "FullyQualifiedName~RagVectorSearchTests"
```
