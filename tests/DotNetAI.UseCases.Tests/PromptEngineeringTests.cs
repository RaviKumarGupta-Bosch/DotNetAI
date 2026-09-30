using DotNetAI.Core.Testing;
using DotNetAI.PromptEngineering.Models;
using DotNetAI.PromptEngineering.Services;
using Xunit;

namespace DotNetAI.UseCases.Tests;

public class PromptEngineeringTests
{
    private readonly PromptTemplateEngine _templateEngine = new();

    [Theory]
    [InlineData(PersonaType.EmpatheticSupport, "Aria")]
    [InlineData(PersonaType.TechnicalArchitect, "Marcus")]
    [InlineData(PersonaType.ConciseBillingAdvisor, "Elena")]
    [InlineData(PersonaType.SeniorCodeReviewer, "David")]
    public void BuildSystemPrompt_ContainsPersonaKeywords(PersonaType persona, string expectedSnippet)
    {
        string prompt = _templateEngine.BuildSystemPrompt(persona);
        Assert.Contains(expectedSnippet, prompt);
    }

    [Fact]
    public void FormatPromptWithTicket_IncludesTicketDetails()
    {
        var ticket = new SupportTicket(
            "TCK-999",
            "EnterpriseCorp",
            "Platinum",
            "Database",
            "PostgreSQL DB Connection Timeout",
            "System fails to connect after 30s during peak traffic."
        );

        string formatted = _templateEngine.FormatPromptWithTicket(ticket, PersonaType.TechnicalArchitect);

        Assert.Contains("TCK-999", formatted);
        Assert.Contains("EnterpriseCorp", formatted);
        Assert.Contains("Platinum", formatted);
        Assert.Contains("PostgreSQL", formatted);
    }

    [Fact]
    public async Task SupportChatService_ProcessTicket_ReturnsValidResult()
    {
        var mockClient = new MockChatClient("Root Cause: Connection pool exhaustion. Remediation: Increase max_connections to 250.");
        var service = new SupportChatService(_templateEngine);

        var ticket = new SupportTicket(
            "TCK-101",
            "Acme Corp",
            "Gold",
            "Billing",
            "Deadlock in payments",
            "Transactions failing under load"
        );

        var result = await service.ProcessTicketAsync(mockClient, ticket, PersonaType.TechnicalArchitect, temperature: 0.2f);

        Assert.NotNull(result);
        Assert.Equal("TCK-101", result.TicketId);
        Assert.Equal(PersonaType.TechnicalArchitect, result.Persona);
        Assert.Contains("Connection pool", result.ResponseText);
        Assert.True(result.EstimatedTokensUsed > 0);
    }
}
