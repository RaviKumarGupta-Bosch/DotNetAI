using System.Diagnostics;
using DotNetAI.Core.Interfaces;
using DotNetAI.Core.Testing;
using DotNetAI.RAG.Models;
using DotNetAI.RAG.Services;
using Microsoft.Extensions.AI;

namespace DotNetAI.RAG;

/// <summary>
/// Use Case 04: Retrieval-Augmented Generation (RAG) & Vector Semantic Search.
/// </summary>
public class RAGVectorSearchUseCase : IAIUseCase
{
    public int Id => 4;
    public string Name => "RAG & Vector Semantic Search";
    public string Description => "Demonstrates text chunking with overlap, vector embeddings generation, cosine similarity vector search, grounded context prompt assembly, and source attribution/citations.";
    public IReadOnlyList<string> AIConcepts => new[]
    {
        "Vector Embeddings & Semantic Search",
        "Document Chunking & Sliding Window Overlap",
        "Cosine Similarity & Metric Ranking",
        "Retrieval-Augmented Generation (RAG) Architecture",
        "Grounded Context Injection & Source Citation"
    };

    public async Task<UseCaseExecutionResult> RunAsync(
        IChatClient chatClient, 
        IEmbeddingGenerator<string, Embedding<float>>? embeddingGenerator = null, 
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var result = new UseCaseExecutionResult();
        result.Logs.Add("[UseCase 04] Starting RAG & Vector Semantic Search demo...");

        // Ensure we have an embedding generator (fallback to mock if not supplied)
        var embGen = embeddingGenerator ?? new MockEmbeddingGenerator(384);
        var vectorStore = new InMemoryVectorStore(embGen);

        // 1. Seed Knowledge Base
        var sampleDocs = GetSampleKnowledgeBase();
        result.Logs.Add($"Indexing {sampleDocs.Count} enterprise policy documents into vector store...");
        await vectorStore.AddDocumentsAsync(sampleDocs, cancellationToken);
        result.Logs.Add($"Vector store indexed {vectorStore.TotalChunks} chunks with embeddings.");

        // 2. Query Pipeline
        var ragPipeline = new RAGPipeline(vectorStore, chatClient);

        // Scenario 1: Specific Policy Question
        string query1 = "What is the policy for deploying urgent hotfixes to production environments?";
        result.Logs.Add($"\n--- Query 1: '{query1}' ---");
        var ragResponse1 = await ragPipeline.QueryAsync(query1, topK: 2, cancellationToken: cancellationToken);

        result.Logs.Add($"Retrieved {ragResponse1.RetrievedContexts.Count} context chunks in {ragResponse1.RetrievalDuration.TotalMilliseconds:F1}ms:");
        foreach (var ctx in ragResponse1.RetrievedContexts)
        {
            result.Logs.Add($"  [Match - Score {ctx.SimilarityScore:F3}] ({ctx.Chunk.DocumentTitle}) => {ctx.Chunk.Text.Replace('\n', ' ')}");
        }
        result.Logs.Add($"Generated Answer ({ragResponse1.GenerationDuration.TotalMilliseconds:F1}ms):\n{ragResponse1.Answer}");
        result.Outputs["Scenario1Response"] = ragResponse1;

        // Scenario 2: VPN / Remote Access Question
        string query2 = "How frequently must remote access VPN passwords and certificates be rotated?";
        result.Logs.Add($"\n--- Query 2: '{query2}' ---");
        var ragResponse2 = await ragPipeline.QueryAsync(query2, topK: 2, cancellationToken: cancellationToken);

        result.Logs.Add($"Retrieved {ragResponse2.RetrievedContexts.Count} context chunks in {ragResponse2.RetrievalDuration.TotalMilliseconds:F1}ms:");
        foreach (var ctx in ragResponse2.RetrievedContexts)
        {
            result.Logs.Add($"  [Match - Score {ctx.SimilarityScore:F3}] ({ctx.Chunk.DocumentTitle}) => {ctx.Chunk.Text.Replace('\n', ' ')}");
        }
        result.Logs.Add($"Generated Answer ({ragResponse2.GenerationDuration.TotalMilliseconds:F1}ms):\n{ragResponse2.Answer}");
        result.Outputs["Scenario2Response"] = ragResponse2;

        sw.Stop();
        result.Duration = sw.Elapsed;
        result.Summary = "RAG pipeline executed chunking, embedding generation, top-k vector search, and grounded LLM generation.";
        return result;
    }

    private static List<KnowledgeDocument> GetSampleKnowledgeBase() => new()
    {
        new KnowledgeDocument(
            Id: "DOC-SEC-01",
            Title: "Enterprise Remote Access & VPN Policy",
            Category: "Security",
            Content: """
            All employees accessing internal corporate resources from outside the corporate firewall must use Company Managed VPN.
            Multi-factor authentication (MFA) using FIDO2 hardware keys or Microsoft Authenticator is mandatory on every session.
            User VPN session tokens expire automatically after 8 hours of continuous connection.
            VPN passwords must be at least 16 characters long and rotated every 90 days. User certificates are renewed automatically every 180 days.
            """,
            CreatedDate: DateTime.UtcNow.AddMonths(-3)
        ),
        new KnowledgeDocument(
            Id: "DOC-OPS-02",
            Title: "Production Release & Deployment Guidelines",
            Category: "DevOps",
            Content: """
            Standard production releases are scheduled on Tuesdays and Thursdays between 02:00 UTC and 04:00 UTC to minimize customer disruption.
            All production deployments require an approved Change Advisory Board (CAB) ticket 24 hours prior to rollout.
            For Emergency Hotfixes: When a Severity 1 incident occurs, on-call Incident Commander can approve immediate hotfix deployment with minimum 2 senior engineer peer code reviews and automated smoke test validation.
            All rollbacks must be initiated within 10 minutes if telemetry reveals error rates exceeding 0.05%.
            """,
            CreatedDate: DateTime.UtcNow.AddMonths(-2)
        ),
        new KnowledgeDocument(
            Id: "DOC-DATA-03",
            Title: "Database Backup and Disaster Recovery Procedure",
            Category: "Infrastructure",
            Content: """
            All production PostgreSQL and SQL Server databases undergo continuous WAL/transaction log archiving with 5-minute RPO.
            Full database snapshots are taken nightly at 00:00 UTC and replicated across secondary geographic regions.
            Bi-monthly disaster recovery drill simulations are executed to verify Recovery Time Objective (RTO) stays under 30 minutes.
            Backups are encrypted using AES-256 keys stored in Azure Key Vault with yearly HSM rotation.
            """,
            CreatedDate: DateTime.UtcNow.AddMonths(-1)
        )
    };
}
