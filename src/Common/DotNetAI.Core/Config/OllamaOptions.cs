namespace DotNetAI.Core.Config;

/// <summary>
/// Configuration settings for local Ollama instance and model selections.
/// </summary>
public class OllamaOptions
{
    public const string SectionName = "Ollama";

    /// <summary>
    /// Base URL for Ollama API (default: http://localhost:11434)
    /// </summary>
    public string Endpoint { get; set; } = "http://localhost:11434";

    /// <summary>
    /// Default chat completion model name (e.g. qwen2.5:7b-instruct, mistral:latest, llama3:latest)
    /// </summary>
    public string ChatModel { get; set; } = "qwen2.5:7b-instruct";

    /// <summary>
    /// Default embedding model name (e.g. nomic-embed-text:latest, all-minilm:latest)
    /// </summary>
    public string EmbeddingModel { get; set; } = "nomic-embed-text:latest";

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// If true, uses mock clients for testing when Ollama is unreachable
    /// </summary>
    public bool EnableFallbackToMock { get; set; } = true;
}
