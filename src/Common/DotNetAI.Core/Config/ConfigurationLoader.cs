using Microsoft.Extensions.Configuration;

namespace DotNetAI.Core.Config;

/// <summary>
/// Helper for loading application settings from appsettings.json and environment variables.
/// </summary>
public static class ConfigurationLoader
{
    public static IConfiguration LoadConfiguration(string? basePath = null)
    {
        basePath ??= AppContext.BaseDirectory;

        var builder = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables("DOTNETAI_");

        return builder.Build();
    }

    public static OllamaOptions LoadOllamaOptions(string? basePath = null)
    {
        var config = LoadConfiguration(basePath);
        var options = new OllamaOptions();
        config.GetSection(OllamaOptions.SectionName).Bind(options);

        // Fallback to environment variables if present
        var envEndpoint = Environment.GetEnvironmentVariable("OLLAMA_ENDPOINT");
        if (!string.IsNullOrWhiteSpace(envEndpoint))
        {
            options.Endpoint = envEndpoint;
        }

        var envChatModel = Environment.GetEnvironmentVariable("OLLAMA_CHAT_MODEL");
        if (!string.IsNullOrWhiteSpace(envChatModel))
        {
            options.ChatModel = envChatModel;
        }

        var envEmbedModel = Environment.GetEnvironmentVariable("OLLAMA_EMBED_MODEL");
        if (!string.IsNullOrWhiteSpace(envEmbedModel))
        {
            options.EmbeddingModel = envEmbedModel;
        }

        return options;
    }
}
