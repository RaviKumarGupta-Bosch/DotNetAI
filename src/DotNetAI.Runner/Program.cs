using System.Diagnostics;
using DotNetAI.AIEvaluation;
using DotNetAI.AgenticReAct;
using DotNetAI.ConversationalMemory;
using DotNetAI.Core.Config;
using DotNetAI.Core.Factories;
using DotNetAI.Core.Interfaces;
using DotNetAI.FunctionCalling;
using DotNetAI.MultiAgent;
using DotNetAI.PlanAndSolve;
using DotNetAI.PromptEngineering;
using DotNetAI.RAG;
using DotNetAI.SafetyGuardrails;
using DotNetAI.StructuredOutputs;
using Microsoft.Extensions.AI;

namespace DotNetAI.Runner;

public class Program
{
    private static readonly List<IAIUseCase> AllUseCases = new()
    {
        new PromptEngineeringUseCase(),
        new StructuredOutputsUseCase(),
        new FunctionCallingUseCase(),
        new RAGVectorSearchUseCase(),
        new AgenticReActUseCase(),
        new MultiAgentCollaborationUseCase(),
        new PlanAndSolveUseCase(),
        new ConversationalMemoryUseCase(),
        new SafetyGuardrailsUseCase(),
        new AIEvaluationJudgeUseCase()
    };

    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        PrintBanner();

        var config = ConfigurationLoader.LoadOllamaOptions();
        bool useMock = false;

        // Check command-line args for non-interactive or testing runs
        if (args.Contains("--mock", StringComparer.OrdinalIgnoreCase))
        {
            useMock = true;
        }

        if (args.Contains("--all", StringComparer.OrdinalIgnoreCase))
        {
            var (chat, emb) = CreateClients(config, useMock);
            await RunAllUseCasesDirectlyAsync(chat, emb);
            return;
        }

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("================================================================================");
            Console.WriteLine("  .NET 9 + Local Ollama / Mock AI Use Cases Explorer");
            Console.WriteLine($"  Mode: {(useMock ? "[OFFLINE MOCK CLIENT]" : $"[LOCAL OLLAMA -> {config.Endpoint} ({config.ChatModel})]")}");
            Console.WriteLine("================================================================================");
            Console.ResetColor();

            Console.WriteLine("\nAvailable Use Cases:");
            for (int i = 0; i < AllUseCases.Count; i++)
            {
                var uc = AllUseCases[i];
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"  [{i + 1:D2}] ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"{uc.Name}");
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine($"       Concepts: {string.Join(", ", uc.AIConcepts.Take(2))}...");
                Console.ResetColor();
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\nOptions:");
            Console.WriteLine("  [A]  Run ALL 10 Use Cases Sequentially");
            Console.WriteLine($"  [M]  Toggle Mode (Current: {(useMock ? "MOCK" : "LOCAL OLLAMA")})");
            Console.WriteLine("  [Q]  Quit");
            Console.ResetColor();

            Console.Write("\nEnter choice (1-10, A, M, Q): ");
            var input = Console.ReadLine()?.Trim();

            if (string.IsNullOrEmpty(input)) continue;

            if (input.Equals("Q", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Exiting DotNetAI Runner. Happy coding!");
                break;
            }

            if (input.Equals("M", StringComparison.OrdinalIgnoreCase))
            {
                useMock = !useMock;
                continue;
            }

            var (chatClient, embGen) = CreateClients(config, useMock);

            if (input.Equals("A", StringComparison.OrdinalIgnoreCase))
            {
                await RunAllUseCasesAsync(chatClient, embGen);
                continue;
            }

            if (int.TryParse(input, out int choice) && choice >= 1 && choice <= AllUseCases.Count)
            {
                var useCase = AllUseCases[choice - 1];
                await RunSingleUseCaseAsync(useCase, chatClient, embGen);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid selection. Please choose 1-10, A, M, or Q.");
                Console.ResetColor();
            }
        }
    }

    private static (IChatClient ChatClient, IEmbeddingGenerator<string, Embedding<float>>? EmbeddingGen) CreateClients(OllamaOptions config, bool useMock)
    {
        if (useMock)
        {
            return (AIClientFactory.CreateMockChatClient(), AIClientFactory.CreateMockEmbeddingGenerator());
        }

        try
        {
            var chat = AIClientFactory.CreateChatClient(config, forceMock: false);
            var emb = AIClientFactory.CreateEmbeddingGenerator(config, forceMock: false);
            return (chat, emb);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"Warning: Failed to initialize Ollama ({ex.Message}). Falling back to offline mock client.");
            Console.ResetColor();
            return (AIClientFactory.CreateMockChatClient(), AIClientFactory.CreateMockEmbeddingGenerator());
        }
    }

    public static async Task RunSingleUseCaseAsync(IAIUseCase useCase, IChatClient chatClient, IEmbeddingGenerator<string, Embedding<float>>? embGen)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n================================================================================");
        Console.WriteLine($"  RUNNING USE CASE {useCase.Id:D2}: {useCase.Name.ToUpperInvariant()}");
        Console.WriteLine($"================================================================================");
        Console.ResetColor();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"Description: {useCase.Description}\n");
        Console.WriteLine("Key AI Concepts Covered:");
        foreach (var c in useCase.AIConcepts)
        {
            Console.WriteLine($"  • {c}");
        }
        Console.WriteLine("--------------------------------------------------------------------------------\n");
        Console.ResetColor();

        var sw = Stopwatch.StartNew();
        try
        {
            var result = await useCase.RunAsync(chatClient, embGen);
            sw.Stop();

            Console.ForegroundColor = ConsoleColor.White;
            foreach (var log in result.Logs)
            {
                Console.WriteLine(log);
            }
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n--------------------------------------------------------------------------------");
            Console.WriteLine($"✅ COMPLETED in {result.Duration.TotalSeconds:F2}s");
            Console.WriteLine($"Summary: {result.Summary}");
            Console.WriteLine("--------------------------------------------------------------------------------");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            sw.Stop();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n❌ ERROR executing Use Case {useCase.Id}: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
            Console.ResetColor();
        }

        Console.WriteLine("\nPress ENTER to return to menu...");
        Console.ReadLine();
    }

    private static async Task RunAllUseCasesAsync(IChatClient chatClient, IEmbeddingGenerator<string, Embedding<float>>? embGen)
    {
        await RunAllUseCasesDirectlyAsync(chatClient, embGen);
        Console.WriteLine("\nPress ENTER to return to menu...");
        Console.ReadLine();
    }

    private static async Task RunAllUseCasesDirectlyAsync(IChatClient chatClient, IEmbeddingGenerator<string, Embedding<float>>? embGen)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("\n================================================================================");
        Console.WriteLine("  EXECUTING ALL 10 AI USE CASES IN SEQUENCE");
        Console.WriteLine("================================================================================");
        Console.ResetColor();

        int passed = 0;
        var totalSw = Stopwatch.StartNew();

        for (int i = 0; i < AllUseCases.Count; i++)
        {
            var uc = AllUseCases[i];
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n[{i + 1}/{AllUseCases.Count}] Running UseCase {uc.Id:D2}: {uc.Name}...");
            Console.ResetColor();

            try
            {
                var res = await uc.RunAsync(chatClient, embGen);
                passed++;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"   ✅ Pass ({res.Duration.TotalSeconds:F2}s): {res.Summary}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"   ❌ Fail: {ex.Message}");
                Console.ResetColor();
            }
        }

        totalSw.Stop();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n================================================================================");
        Console.WriteLine($"  ALL RUN COMPLETED: {passed}/{AllUseCases.Count} Succeeded in {totalSw.Elapsed.TotalSeconds:F2}s");
        Console.WriteLine("================================================================================");
        Console.ResetColor();
    }

    private static void PrintBanner()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("""
        ==============================================================================
          ██████╗  ██████╗ ████████╗███╗   ██╗███████╗████████╗ █████╗ ██╗
          ██╔══██╗██╔═══██╗╚══██╔══╝████╗  ██║██╔════╝╚══██╔══╝██╔══██╗██║
          ██║  ██║██║   ██║   ██║   ██╔██╗ ██║█████╗     ██║   ███████║██║
          ██║  ██║██║   ██║   ██║   ██║╚██╗██║██╔══╝     ██║   ██╔══██║██║
          ██████╔╝╚██████╔╝   ██║   ██║ ╚████║███████╗   ██║   ██║  ██║██║
          ╚═════╝  ╚═════╝    ╚═╝   ╚═╝  ╚═══╝╚══════╝   ╚═╝   ╚═╝  ╚═╝╚═╝
          10 Comprehensive AI Use Cases for .NET Developers (C# 13 / .NET 9)
        ==============================================================================
        """);
        Console.ResetColor();
    }
}

