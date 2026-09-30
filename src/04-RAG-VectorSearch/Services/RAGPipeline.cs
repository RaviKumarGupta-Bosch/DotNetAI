using System.Diagnostics;
using System.Text;
using DotNetAI.RAG.Models;
using Microsoft.Extensions.AI;

namespace DotNetAI.RAG.Services;

public class RAGPipeline
{
    private readonly IVectorStore _vectorStore;
    private readonly IChatClient _chatClient;

    public RAGPipeline(IVectorStore vectorStore, IChatClient chatClient)
    {
        _vectorStore = vectorStore;
        _chatClient = chatClient;
    }

    public async Task<RAGResponse> QueryAsync(
        string query,
        int topK = 3,
        float minScore = 0.3f,
        CancellationToken cancellationToken = default)
    {
        // 1. Vector Search Retrieval
        var retrievalSw = Stopwatch.StartNew();
        var searchResults = await _vectorStore.SearchAsync(query, topK, minScore, cancellationToken);
        retrievalSw.Stop();

        // 2. Format Context & Citations
        var contextBuilder = new StringBuilder();
        if (searchResults.Count == 0)
        {
            contextBuilder.AppendLine("No relevant internal documents found.");
        }
        else
        {
            for (int i = 0; i < searchResults.Count; i++)
            {
                var r = searchResults[i];
                contextBuilder.AppendLine($"--- [SOURCE {i + 1}] (Document: {r.Chunk.DocumentTitle}, Score: {r.SimilarityScore:F3}) ---");
                contextBuilder.AppendLine(r.Chunk.Text);
                contextBuilder.AppendLine();
            }
        }

        // 3. Grounded Prompt Construction
        var systemPrompt = """
        You are an enterprise knowledge assistant. Answer the user's question accurately using ONLY the provided Source context.
        Rules:
        1. Always cite source numbers like [SOURCE 1] or [SOURCE 2] when referencing information.
        2. If the context does not contain the answer, explicitly state that the provided documentation does not contain enough information.
        3. Do NOT invent facts or hallucinate external information.
        """;

        var userPrompt = $"""
        Context Information:
        {contextBuilder}

        User Question:
        {query}
        """;

        var conversation = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt),
            new(ChatRole.User, userPrompt)
        };

        // 4. LLM Generation
        var genSw = Stopwatch.StartNew();
        var response = await _chatClient.GetResponseAsync(conversation, new ChatOptions { Temperature = 0.1f }, cancellationToken);
        genSw.Stop();

        float confidence = searchResults.Count > 0 ? searchResults.Average(s => s.SimilarityScore) : 0f;

        return new RAGResponse(
            Query: query,
            Answer: response.Text ?? string.Empty,
            RetrievedContexts: searchResults.ToList(),
            ConfidenceScore: confidence,
            RetrievalDuration: retrievalSw.Elapsed,
            GenerationDuration: genSw.Elapsed
        );
    }
}
