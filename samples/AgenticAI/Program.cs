using DotNetAI.AgenticAI.Samples;
using DotNetAI.AgenticAI.Samples.Models;
using DotNetAI.AgenticAI.Samples.Services;
using DotNetAI.Core.Config;
using DotNetAI.Core.Factories;
using Microsoft.Extensions.AI;

var samples = AgenticSampleCatalog.Create();
var mockMode = args.Contains("--mock", StringComparer.OrdinalIgnoreCase);
var runAll = args.Contains("--all", StringComparer.OrdinalIgnoreCase);

if (args.Contains("--list", StringComparer.OrdinalIgnoreCase))
{
    PrintSamples(samples);
    return;
}

var requestedSample = args.FirstOrDefault(argument => !argument.StartsWith("--", StringComparison.Ordinal));
IReadOnlyList<AgenticSample> selectedSamples;
if (runAll)
{
    selectedSamples = samples;
}
else if (requestedSample is null)
{
    PrintSamples(samples);
    Console.Write("Choose a sample number or slug: ");
    requestedSample = Console.ReadLine()?.Trim();
    var selected = FindSample(samples, requestedSample);
    if (selected is null)
    {
        Console.Error.WriteLine("No matching sample was selected.");
        return;
    }

    selectedSamples = [selected];
}
else
{
    var selected = FindSample(samples, requestedSample);
    if (selected is null)
    {
        Console.Error.WriteLine($"Unknown sample '{requestedSample}'. Use --list to see available samples.");
        return;
    }

    selectedSamples = [selected];
}

var options = ConfigurationLoader.LoadOllamaOptions();
if (!mockMode && !await AIClientFactory.IsOllamaReachableAsync(options.Endpoint))
{
    Console.Error.WriteLine($"Ollama is not reachable at {options.Endpoint}. Re-run with --mock for the scripted offline walkthrough.");
    return;
}

IChatClient? liveClient = mockMode
    ? null
    : AIClientFactory.CreateChatClient(new OllamaOptions
    {
        Endpoint = options.Endpoint,
        ChatModel = options.ChatModel,
        TimeoutSeconds = options.TimeoutSeconds,
        EnableFallbackToMock = false
    });

var agent = new AgenticToolAgent();
foreach (var sample in selectedSamples)
{
    Console.WriteLine($"\n[{sample.Id}] {sample.Name}");
    Console.WriteLine(sample.Description);
    Console.WriteLine($"Goal: {sample.Goal}\n");

    var client = mockMode ? AgenticSampleCatalog.CreateMockClient(sample) : liveClient!;
    var result = await agent.RunAsync(client, sample);

    Console.WriteLine($"Tools called: {(result.ToolCalls.Count == 0 ? "none" : string.Join(", ", result.ToolCalls))}");
    Console.WriteLine($"Turns: {result.Turns}; final answer: {result.ReachedFinalAnswer}");
    Console.WriteLine(result.FinalAnswer);
}

static AgenticSample? FindSample(IEnumerable<AgenticSample> samples, string? selection) =>
    samples.FirstOrDefault(sample =>
        string.Equals(sample.Id, selection, StringComparison.OrdinalIgnoreCase)
        || string.Equals(sample.Name.Replace(' ', '-'), selection, StringComparison.OrdinalIgnoreCase));

static void PrintSamples(IEnumerable<AgenticSample> samples)
{
    Console.WriteLine("Agentic AI for .NET: practical workflow samples");
    foreach (var sample in samples)
    {
        Console.WriteLine($"  {sample.Id}  {sample.Name,-36} {sample.Name.Replace(' ', '-')}");
    }
    Console.WriteLine("\nUse --mock for an offline walkthrough, --all to run every sample, or --list to print this menu.");
}