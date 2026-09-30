using DotNetAI.Core.Config;
using DotNetAI.Core.Factories;
using Xunit;

namespace DotNetAI.Core.Tests;

public class ConfigurationAndFactoryTests
{
    [Fact]
    public void ConfigurationLoader_ReturnsDefaultOptionsWhenNoConfigFile()
    {
        var options = ConfigurationLoader.LoadOllamaOptions();

        Assert.NotNull(options);
        Assert.False(string.IsNullOrWhiteSpace(options.Endpoint));
        Assert.False(string.IsNullOrWhiteSpace(options.ChatModel));
        Assert.False(string.IsNullOrWhiteSpace(options.EmbeddingModel));
    }

    [Fact]
    public void AIClientFactory_CreateMockChatClient_ReturnsConfiguredMock()
    {
        var mock = AIClientFactory.CreateMockChatClient("Mocked Output");
        Assert.NotNull(mock);
    }

    [Fact]
    public void AIClientFactory_CreateMockEmbeddingGenerator_ReturnsConfiguredMock()
    {
        var mock = AIClientFactory.CreateMockEmbeddingGenerator(256);
        Assert.NotNull(mock);
    }

    [Fact]
    public void AIClientFactory_CreateChatClient_WithOllamaOptions_ReturnsValidInstance()
    {
        var options = new OllamaOptions
        {
            Endpoint = "http://localhost:11434",
            ChatModel = "qwen2.5:7b-instruct"
        };

        var client = AIClientFactory.CreateChatClient(options);
        Assert.NotNull(client);
    }
}
