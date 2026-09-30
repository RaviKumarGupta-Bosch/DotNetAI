using DotNetAI.ConversationalMemory.Services;
using DotNetAI.Core.Testing;
using Xunit;

namespace DotNetAI.UseCases.Tests;

public class ConversationalMemoryTests
{
    [Fact]
    public void SlidingWindowMemoryStore_PreservesOnlyLastNItems()
    {
        var store = new SlidingWindowMemoryStore(windowSize: 4);

        store.AddMessage("User", "Msg 1");
        store.AddMessage("Assistant", "Reply 1");
        store.AddMessage("User", "Msg 2");
        store.AddMessage("Assistant", "Reply 2");
        store.AddMessage("User", "Msg 3");

        var recent = store.GetRecentMessages();

        Assert.Equal(4, recent.Count);
        Assert.Equal("Reply 1", recent[0].Content);
        Assert.Equal("Msg 3", recent[3].Content);
        Assert.Equal(5, store.TotalStoredMessages);
    }

    [Fact]
    public async Task EntityMemoryStore_ExtractsJsonEntities()
    {
        string entityJson = """
        [
          {"Key": "Database", "Value": "CockroachDB", "Category": "Infrastructure"},
          {"Key": "DeployEnv", "Value": "Staging", "Category": "Environment"}
        ]
        """;

        var mock = new MockChatClient(entityJson);
        var entityStore = new EntityMemoryStore();

        await entityStore.ExtractEntitiesAsync(mock, "We are configuring CockroachDB on Staging.");

        Assert.Equal(2, entityStore.Entities.Count);
        Assert.Equal("CockroachDB", entityStore.Entities["Database"].Value);
        Assert.Equal("Staging", entityStore.Entities["DeployEnv"].Value);
    }

    [Fact]
    public async Task ConversationalMemoryManager_InjectsContextAcrossTurns()
    {
        var mock = new MockChatClient("I remember your preference for CockroachDB and staging environment.");
        var memoryManager = new ConversationalMemoryManager(windowSize: 6);

        var reply = await memoryManager.ProcessUserTurnAsync(mock, "What database was I talking about earlier?");

        Assert.Contains("CockroachDB", reply);
        var diag = memoryManager.GetDiagnostics();
        Assert.Equal(2, diag.TotalMessagesStored);
    }
}
