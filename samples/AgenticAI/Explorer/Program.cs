using System.Diagnostics;
using DotNetAI.AgenticAI.Explorer;
using DotNetAI.AgenticAI.Samples;
using DotNetAI.AgenticAI.Samples.Services;
using DotNetAI.Core.Config;
using DotNetAI.Core.Factories;
using Microsoft.Extensions.AI;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
var ollamaOptions = ConfigurationLoader.LoadOllamaOptions();
var agent = new AgenticToolAgent();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/samples", () => AgenticSampleCatalog.Create().Select(sample => new
{
    sample.Id,
    sample.Name,
    sample.Description,
    sample.Goal,
    Tools = sample.Tools.Select(tool => tool.Name)
}));

app.MapGet("/api/status", async (CancellationToken cancellationToken) => new
{
    MockAvailable = true,
    OllamaAvailable = await AIClientFactory.IsOllamaReachableAsync(ollamaOptions.Endpoint, cancellationToken),
    Endpoint = ollamaOptions.Endpoint,
    Model = ollamaOptions.ChatModel
});

app.MapPost("/api/run", async (RunRequest request, HttpContext context, ILogger<Program> logger) =>
{
    var sample = AgenticSampleCatalog.Create().FirstOrDefault(candidate =>
        string.Equals(candidate.Id, request.SampleId, StringComparison.OrdinalIgnoreCase));
    if (sample is null)
    {
        return Results.BadRequest(new { Error = "Choose a sample from the catalog." });
    }

    var mode = request.Mode.Trim().ToLowerInvariant();
    if (mode is not ("mock" or "ollama"))
    {
        return Results.BadRequest(new { Error = "Mode must be 'mock' or 'ollama'." });
    }

    var maxTurns = request.MaxTurns ?? 5;
    if (maxTurns is < 1 or > 12)
    {
        return Results.BadRequest(new { Error = "MaxTurns must be between 1 and 12." });
    }

    IChatClient chatClient;
    string model;
    if (mode == "mock")
    {
        chatClient = AgenticSampleCatalog.CreateMockClient(sample);
        model = "Scripted mock";
    }
    else
    {
        if (!await AIClientFactory.IsOllamaReachableAsync(ollamaOptions.Endpoint, context.RequestAborted))
        {
            return Results.Json(new { Error = $"Ollama is not reachable at {ollamaOptions.Endpoint}." }, statusCode: 503);
        }

        chatClient = AIClientFactory.CreateChatClient(new OllamaOptions
        {
            Endpoint = ollamaOptions.Endpoint,
            ChatModel = ollamaOptions.ChatModel,
            TimeoutSeconds = ollamaOptions.TimeoutSeconds,
            EnableFallbackToMock = false
        });
        model = ollamaOptions.ChatModel;
    }

    var stopwatch = Stopwatch.StartNew();
    try
    {
        var result = await agent.RunAsync(chatClient, sample, maxTurns, context.RequestAborted);
        stopwatch.Stop();
        var checks = RunValidation.Validate(result, sample, maxTurns);

        return Results.Ok(new ExplorerRunResponse(
            sample.Id,
            sample.Name,
            mode,
            model,
            result.FinalAnswer,
            result.ToolCalls,
            result.Turns,
            maxTurns,
            checks.All(check => check.Passed),
            checks,
            stopwatch.ElapsedMilliseconds));
    }
    catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
    {
        return Results.StatusCode(499);
    }
    catch (Exception exception)
    {
        logger.LogError(exception, "Agent sample {SampleId} failed in {Mode} mode", sample.Id, mode);
        return Results.Problem("The agent run failed. Check the explorer server log for details.", statusCode: 500);
    }
    finally
    {
        chatClient.Dispose();
    }
});

app.MapFallbackToFile("index.html");
app.Run();