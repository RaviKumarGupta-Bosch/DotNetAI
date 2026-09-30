using DotNetAI.Core.Testing;
using Microsoft.Extensions.AI;
using Xunit;

namespace DotNetAI.Core.Tests;

public class MockClientsTests
{
    [Fact]
    public async Task MockChatClient_ReturnsConfiguredResponse()
    {
        var mock = new MockChatClient("Preconfigured test answer");
        var response = await mock.GetResponseAsync(new[] { new ChatMessage(ChatRole.User, "What is AI?") });

        Assert.NotNull(response);
        Assert.Equal("Preconfigured test answer", response.Text);
    }

    [Fact]
    public async Task MockChatClient_MatchesPredicateResponse()
    {
        var mock = new MockChatClient()
            .WhenUserMessageContains("inventory", """{"inStock": true, "quantity": 42}""")
            .WhenUserMessageContains("shipping", """{"shippingCost": 15.50}""");

        var response1 = await mock.GetResponseAsync(new[] { new ChatMessage(ChatRole.User, "Check inventory for SKU-100") });
        var response2 = await mock.GetResponseAsync(new[] { new ChatMessage(ChatRole.User, "Calculate shipping to 90210") });

        Assert.Contains("quantity", response1.Text);
        Assert.Contains("shippingCost", response2.Text);
    }

    [Fact]
    public async Task MockEmbeddingGenerator_GeneratesDeterministicVectors()
    {
        var generator = new MockEmbeddingGenerator(dimensions: 128);
        var embeddings1 = await generator.GenerateAsync(new[] { "DotNet AI Framework" });
        var embeddings2 = await generator.GenerateAsync(new[] { "DotNet AI Framework" });

        Assert.Single(embeddings1);
        Assert.Single(embeddings2);
        Assert.Equal(128, embeddings1[0].Vector.Length);
        Assert.Equal(embeddings1[0].Vector.ToArray(), embeddings2[0].Vector.ToArray());
    }

    [Fact]
    public async Task MockEmbeddingGenerator_DifferentInputs_ProduceDifferentVectors()
    {
        var generator = new MockEmbeddingGenerator(dimensions: 64);
        var embeddings = await generator.GenerateAsync(new[] { "Apple", "Microservices" });

        Assert.Equal(2, embeddings.Count);
        Assert.NotEqual(embeddings[0].Vector.ToArray(), embeddings[1].Vector.ToArray());
    }
}
