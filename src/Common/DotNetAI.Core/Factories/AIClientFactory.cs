using DotNetAI.Core.Config;
using DotNetAI.Core.Testing;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace DotNetAI.Core.Factories;

/// <summary>
/// Factory for creating IChatClient and IEmbeddingGenerator instances for local Ollama or test mocks.
/// </summary>
public static class AIClientFactory
{
    private static readonly HttpClient SharedHttpClient = new() { Timeout = TimeSpan.FromSeconds(120) };

    /// <summary>
    /// Checks if the local Ollama instance is accessible.
    /// </summary>
    public static async Task<bool> IsOllamaReachableAsync(string endpoint = "http://localhost:11434", CancellationToken ct = default)
    {
        try
        {
            var uri = new Uri(new Uri(endpoint), "api/tags");
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(3));

            var response = await SharedHttpClient.GetAsync(uri, cts.Token);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Creates a MockChatClient with scripted response for unit testing.
    /// </summary>
    public static MockChatClient CreateMockChatClient(string responseText = "Mock response")
    {
        return new MockChatClient(responseText);
    }

    /// <summary>
    /// Creates a MockEmbeddingGenerator for testing.
    /// </summary>
    public static MockEmbeddingGenerator CreateMockEmbeddingGenerator(int dimensions = 384, string modelId = "mock-embedding")
    {
        return new MockEmbeddingGenerator(dimensions, modelId);
    }
    public static IChatClient CreateChatClient(
        OllamaOptions? options = null, 
        ILogger? logger = null, 
        bool forceMock = false)
    {
        options ??= ConfigurationLoader.LoadOllamaOptions();

        if (forceMock)
        {
            logger?.LogInformation("Using MockChatClient (forced).");
            return new MockChatClient(options.ChatModel, new Uri(options.Endpoint));
        }

        var uri = new Uri(options.Endpoint);
        if (options.EnableFallbackToMock)
        {
            try
            {
                var isReachable = IsOllamaReachableAsync(options.Endpoint).GetAwaiter().GetResult();
                if (!isReachable)
                {
                    logger?.LogInformation("Ollama endpoint at {Endpoint} is offline or unreachable. Initializing MockChatClient fallback.", options.Endpoint);
                    return new MockChatClient(options.ChatModel, uri);
                }
            }
            catch
            {
                return new MockChatClient(options.ChatModel, uri);
            }
        }

        try
        {
            // Create real Ollama chat client
            var client = new OllamaChatClient(uri, options.ChatModel);
            return client;
        }
        catch (Exception ex)
        {
            if (options.EnableFallbackToMock)
            {
                logger?.LogWarning(ex, "Failed to initialize OllamaChatClient. Falling back to MockChatClient.");
                return new MockChatClient(options.ChatModel, uri);
            }
            throw;
        }
    }

    /// <summary>
    /// Creates an IEmbeddingGenerator configured for Ollama or fallback mock.
    /// </summary>
    public static IEmbeddingGenerator<string, Embedding<float>> CreateEmbeddingGenerator(
        OllamaOptions? options = null, 
        ILogger? logger = null, 
        bool forceMock = false)
    {
        options ??= ConfigurationLoader.LoadOllamaOptions();

        if (forceMock)
        {
            logger?.LogInformation("Using MockEmbeddingGenerator (forced).");
            return new MockEmbeddingGenerator(384, options.EmbeddingModel);
        }

        var uri = new Uri(options.Endpoint);
        if (options.EnableFallbackToMock)
        {
            try
            {
                var isReachable = IsOllamaReachableAsync(options.Endpoint).GetAwaiter().GetResult();
                if (!isReachable)
                {
                    logger?.LogInformation("Ollama endpoint at {Endpoint} is offline or unreachable. Initializing MockEmbeddingGenerator fallback.", options.Endpoint);
                    return new MockEmbeddingGenerator(384, options.EmbeddingModel);
                }
            }
            catch
            {
                return new MockEmbeddingGenerator(384, options.EmbeddingModel);
            }
        }

        try
        {
            // Create real Ollama embedding generator
            var generator = new OllamaEmbeddingGenerator(uri, options.EmbeddingModel);
            return generator;
        }
        catch (Exception ex)
        {
            if (options.EnableFallbackToMock)
            {
                logger?.LogWarning(ex, "Failed to initialize OllamaEmbeddingGenerator. Falling back to MockEmbeddingGenerator.");
                return new MockEmbeddingGenerator(384, options.EmbeddingModel);
            }
            throw;
        }
    }
}
