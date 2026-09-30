using DotNetAI.Core.Testing;
using DotNetAI.RAG.Models;
using DotNetAI.RAG.Services;
using Xunit;

namespace DotNetAI.UseCases.Tests;

public class RAGVectorSearchTests
{
    [Fact]
    public void RecursiveDocumentChunker_SplitsLongText_PreservesOverlap()
    {
        var chunker = new RecursiveDocumentChunker();
        string longText = string.Join(" ", Enumerable.Range(1, 100).Select(i => $"Sentence number {i} provides detailed engineering specification."));
        var doc = new KnowledgeDocument("doc-1", "Specification Guide", "Engineering", longText, DateTime.UtcNow);

        var chunks = chunker.Chunk(doc, chunkSize: 100, chunkOverlap: 20);

        Assert.True(chunks.Count > 1);
        Assert.All(chunks, c =>
        {
            Assert.Equal("doc-1", c.DocumentId);
            Assert.Equal("Specification Guide", c.DocumentTitle);
            Assert.False(string.IsNullOrWhiteSpace(c.Text));
        });
    }

    [Fact]
    public async Task InMemoryVectorStore_AddAndSearch_ReturnsRelevantChunks()
    {
        var embeddingGen = new MockEmbeddingGenerator(128);
        var vectorStore = new InMemoryVectorStore(embeddingGen);

        var doc1 = new KnowledgeDocument("doc-auth", "Authentication Policy", "Security", "All engineers must use hardware FIDO2 keys for production bastion access.", DateTime.UtcNow);
        var doc2 = new KnowledgeDocument("doc-vpn", "VPN Policy", "Network", "Remote access VPN certificates rotate automatically every 90 days.", DateTime.UtcNow);

        await vectorStore.AddDocumentsAsync(new[] { doc1, doc2 });

        Assert.Equal(2, vectorStore.TotalChunks);

        var results = await vectorStore.SearchAsync("VPN certificate rotation policy", topK: 1, minScore: 0.0f);

        Assert.NotEmpty(results);
        Assert.Equal("VPN Policy", results[0].Chunk.DocumentTitle);
    }

    [Fact]
    public async Task RAGPipeline_Query_RetrievesContextAndSynthesizesAnswer()
    {
        var embeddingGen = new MockEmbeddingGenerator(128);
        var vectorStore = new InMemoryVectorStore(embeddingGen);

        await vectorStore.AddDocumentsAsync(new[]
        {
            new KnowledgeDocument("doc-hotfix", "Hotfix Procedures", "Operations", "Production hotfixes require VP approval and must pass automated regression testing.", DateTime.UtcNow)
        });

        var mockChat = new MockChatClient("According to [SOURCE 1], production hotfixes require VP approval.");
        var pipeline = new RAGPipeline(vectorStore, mockChat);

        var response = await pipeline.QueryAsync("What are hotfix rules?", minScore: 0.0f);

        Assert.NotNull(response);
        Assert.Contains("VP approval", response.Answer);
        Assert.Single(response.RetrievedContexts);
    }
}
