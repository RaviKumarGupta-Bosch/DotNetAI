# 03 - Tool Use & Autonomous Function Calling

## 🎯 Overview
Tool Use (Function Calling) transforms an LLM from a passive text generator into an active software agent capable of querying databases, fetching live API telemetry, creating Jira/Azure DevOps tickets, and orchestrating external cloud infrastructure.

This use case demonstrates how to define strongly typed C# methods, register them as `AIFunction` instances with `Microsoft.Extensions.AI`, and orchestrate multi-turn tool-calling conversation loops.

---

## 🧠 Key Concepts & AI Theory

### 1. How Tool Calling Works
1. **Tool Definition**: The client exposes function signatures, descriptions, and argument schemas (via JSON Schema) to the LLM in the `ChatOptions.Tools` collection.
2. **Model Decision**: The LLM analyzes the user prompt. If external information is required, instead of generating a final text reply, it emits a `FunctionCallContent` specifying the function name and JSON arguments.
3. **Client Execution**: The .NET host intercepts the function call, executes the corresponding C# method locally, and captures the return value.
4. **Tool Response**: The result is appended to the conversation history as a `ChatMessage` with `ChatRole.Tool`.
5. **Synthesis**: The LLM reads the tool output and synthesizes a natural language final response for the user.

```
User Prompt ──▶ LLM ──▶ Emits FunctionCallContent("GetWeather", {"city":"Seattle"})
                         │
                         ▼
             .NET executes GetWeather("Seattle") ──▶ Returns "48°F, Rain"
                         │
                         ▼
        Appends ChatMessage(ChatRole.Tool, "48°F, Rain") ──▶ LLM
                         │
                         ▼
        LLM outputs: "The weather in Seattle is currently 48°F and rainy."
```

---

## 💻 C# Implementation Walkthrough

### 1. Defining SRE / IT Tools: `InfrastructureTools.cs`
```csharp
public class InfrastructureTools
{
    [Description("Retrieves the current CPU and memory health metrics for an Azure microservice.")]
    public string GetServiceHealth(
        [Description("The exact name of the service, e.g. 'order-service'")] string serviceName)
    {
        return serviceName.ToLowerInvariant() switch
        {
            "order-service" => "{\"status\":\"Warning\",\"cpu_percent\":94.2,\"memory_used_mb\":3840,\"active_connections\":1820}",
            "payment-service" => "{\"status\":\"Healthy\",\"cpu_percent\":22.1,\"memory_used_mb\":1024,\"active_connections\":140}",
            _ => $"{{\"status\":\"Unknown\",\"error\":\"Service '{serviceName}' not found.\"}}"
        };
    }

    [Description("Creates an incident ticket in the system.")]
    public string CreateIncidentTicket(string serviceName, string severity, string description)
    {
        string ticketId = $"INC-{Random.Shared.Next(1000, 9999)}";
        return $"{{\"ticket_id\":\"{ticketId}\",\"status\":\"Created\",\"service\":\"{serviceName}\",\"severity\":\"{severity}\"}}";
    }
}
```

### 2. Orchestrator Service: `FunctionCallingOrchestrator.cs`
```csharp
public async Task<string> ExecutePromptWithToolsAsync(string userPrompt)
{
    var tools = new InfrastructureTools();
    var aiFunctions = new List<AIFunction>
    {
        AIFunctionFactory.Create(tools.GetServiceHealth),
        AIFunctionFactory.Create(tools.CreateIncidentTicket)
    };

    var messages = new List<ChatMessage>
    {
        new(ChatRole.System, "You are an SRE incident manager. Use available tools to diagnose issues and file incident tickets when necessary."),
        new(ChatRole.User, userPrompt)
    };

    var options = new ChatOptions { Tools = aiFunctions, Temperature = 0.1f };
    var response = await _chatClient.GetResponseAsync(messages, options);
    return response.Text ?? string.Empty;
}
```

---

## 🧪 Running & Validating

### Run via .NET CLI:
```powershell
dotnet run --project src/03-FunctionCalling -- --mock
```

### Run Unit Tests:
```powershell
dotnet test tests/DotNetAI.UseCases.Tests --filter "FullyQualifiedName~FunctionCallingTests"
```
