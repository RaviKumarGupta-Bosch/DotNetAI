using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.AI;

namespace DotNetAI.Core.Testing;

/// <summary>
/// A flexible, observable IChatClient implementation for deterministic unit and integration tests.
/// Supports scripted responses, rule-based generation, tool call simulations, and call recording.
/// </summary>
public class MockChatClient : IChatClient
{
    private readonly List<List<ChatMessage>> _receivedHistory = new();
    private readonly List<ChatOptions?> _receivedOptions = new();
    private readonly Queue<Func<IEnumerable<ChatMessage>, ChatOptions?, ChatResponse>> _responseQueue = new();
    private readonly List<(Func<string, bool> Predicate, Func<IEnumerable<ChatMessage>, ChatOptions?, ChatResponse> Handler)> _rules = new();
    private readonly Func<IEnumerable<ChatMessage>, ChatOptions?, ChatResponse>? _responderFunc;

    public ChatClientMetadata Metadata { get; }

    public IReadOnlyList<IReadOnlyList<ChatMessage>> ReceivedHistories => _receivedHistory;
    public IReadOnlyList<ChatOptions?> ReceivedOptions => _receivedOptions;
    public int CallCount => _receivedHistory.Count;

    public MockChatClient(string modelIdOrResponse = "mock-model", Uri? endpoint = null)
    {
        Metadata = new ChatClientMetadata("MockChatClient", endpoint ?? new Uri("http://localhost:11434"), modelIdOrResponse);
        if (!string.IsNullOrWhiteSpace(modelIdOrResponse) && modelIdOrResponse != "mock-model")
        {
            _responderFunc = (msgs, opts) => new ChatResponse(new ChatMessage(ChatRole.Assistant, modelIdOrResponse));
        }
    }

    public MockChatClient(Func<string, string> responder)
    {
        Metadata = new ChatClientMetadata("MockChatClient", new Uri("http://localhost:11434"), "mock-model");
        _responderFunc = (msgs, opts) =>
        {
            var prompt = string.Join("\n", msgs.Select(m => m.Text));
            var reply = responder(prompt);
            return new ChatResponse(new ChatMessage(ChatRole.Assistant, reply));
        };
    }

    public MockChatClient(Func<IEnumerable<ChatMessage>, ChatResponse> responder)
    {
        Metadata = new ChatClientMetadata("MockChatClient", new Uri("http://localhost:11434"), "mock-model");
        _responderFunc = (msgs, opts) => responder(msgs);
    }

    /// <summary>
    /// Enqueues a specific text response.
    /// </summary>
    public MockChatClient EnqueueResponse(string text)
    {
        _responseQueue.Enqueue((msgs, opts) => new ChatResponse(new ChatMessage(ChatRole.Assistant, text)));
        return this;
    }

    /// <summary>
    /// Enqueues a response generator function.
    /// </summary>
    public MockChatClient EnqueueResponse(Func<IEnumerable<ChatMessage>, ChatOptions?, ChatResponse> handler)
    {
        _responseQueue.Enqueue(handler);
        return this;
    }

    /// <summary>
    /// Adds a conditional rule for matching prompts and generating responses.
    /// </summary>
    public MockChatClient WhenContains(string substring, string responseText)
    {
        _rules.Add((
            prompt => prompt.Contains(substring, StringComparison.OrdinalIgnoreCase),
            (msgs, opts) => new ChatResponse(new ChatMessage(ChatRole.Assistant, responseText))
        ));
        return this;
    }

    /// <summary>
    /// Alias for WhenContains.
    /// </summary>
    public MockChatClient WhenUserMessageContains(string substring, string responseText) => WhenContains(substring, responseText);

    /// <summary>
    /// Adds a rule with custom handler.
    /// </summary>
    public MockChatClient When(Func<string, bool> predicate, Func<IEnumerable<ChatMessage>, ChatOptions?, ChatResponse> handler)
    {
        _rules.Add((predicate, handler));
        return this;
    }

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> chatMessages, 
        ChatOptions? options = null, 
        CancellationToken cancellationToken = default)
    {
        var messagesList = chatMessages.ToList();
        _receivedHistory.Add(messagesList);
        _receivedOptions.Add(options);

        // 1. Check queued responses
        if (_responseQueue.Count > 0)
        {
            var handler = _responseQueue.Dequeue();
            return Task.FromResult(handler(messagesList, options));
        }

        // 2. Check rule-based handlers
        var lastUserMsg = messagesList.LastOrDefault(m => m.Role == ChatRole.User)?.Text ?? string.Empty;
        var fullText = string.Join("\n", messagesList.Select(m => m.Text));

        foreach (var (predicate, handler) in _rules)
        {
            if (predicate(fullText) || predicate(lastUserMsg))
            {
                return Task.FromResult(handler(messagesList, options));
            }
        }

        // 3. Check persistent responder delegate if configured
        if (_responderFunc != null)
        {
            return Task.FromResult(_responderFunc(messagesList, options));
        }

        // 4. Fallback smart synthetic generator
        return Task.FromResult(GenerateDefaultResponse(messagesList, options));
    }

    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> chatMessages, 
        ChatOptions? options = null, 
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var response = await GetResponseAsync(chatMessages, options, cancellationToken);
        var text = response.Text ?? string.Empty;
        var words = text.Split(' ');

        for (int i = 0; i < words.Length; i++)
        {
            var updateText = (i == 0 ? "" : " ") + words[i];
            yield return new ChatResponseUpdate
            {
                Role = ChatRole.Assistant,
                Contents = [new TextContent(updateText)]
            };
            await Task.Yield();
        }
    }

    private ChatResponse GenerateDefaultResponse(List<ChatMessage> messages, ChatOptions? options)
    {
        var lastUserMsg = messages.LastOrDefault(m => m.Role == ChatRole.User)?.Text ?? "";
        var systemMsg = messages.FirstOrDefault(m => m.Role == ChatRole.System)?.Text ?? "";

        // Check if tool/function calling is requested or enabled in options
        if (options?.Tools != null && options.Tools.Count > 0 && lastUserMsg.Contains("calculate", StringComparison.OrdinalIgnoreCase))
        {
            // Simulate tool call response if applicable
            var tool = options.Tools.FirstOrDefault();
            if (tool is AIFunction aiFunc)
            {
                var callContent = new FunctionCallContent(
                    callId: "call_mock_123",
                    name: aiFunc.Name,
                    arguments: new Dictionary<string, object?> { ["query"] = "sample", ["value"] = 100 }
                );
                var msg = new ChatMessage(ChatRole.Assistant, [callContent]);
                return new ChatResponse(msg);
            }
        }

        // Check for JSON schema/structured output requests
        if (options?.ResponseFormat != null || lastUserMsg.Contains("json", StringComparison.OrdinalIgnoreCase) || systemMsg.Contains("JSON", StringComparison.OrdinalIgnoreCase))
        {
            var sampleJson = """
            {
              "status": "success",
              "invoiceNumber": "INV-2026-001",
              "vendorName": "Contoso Cloud Services",
              "invoiceDate": "2026-09-30",
              "totalAmount": 1450.75,
              "currency": "USD",
              "lineItems": [
                { "description": "Cloud Hosting", "quantity": 1, "unitPrice": 1200.00, "total": 1200.00 },
                { "description": "Support SLA", "quantity": 1, "unitPrice": 250.75, "total": 250.75 }
              ],
              "confidenceScore": 0.98
            }
            """;
            return new ChatResponse(new ChatMessage(ChatRole.Assistant, sampleJson));
        }

        var defaultText = $"[Mock AI Response for: '{lastUserMsg}'] - Successfully processed query with {messages.Count} context messages.";
        return new ChatResponse(new ChatMessage(ChatRole.Assistant, defaultText));
    }

    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        if (serviceType == typeof(ChatClientMetadata)) return Metadata;
        return null;
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
