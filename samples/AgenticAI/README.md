# Agentic AI for .NET: Real-World Samples

Ten bounded agent workflows for common .NET engineering and operations tasks. The project uses `Microsoft.Extensions.AI` function tools and the repository's Ollama client configuration.

## Browser Explorer

Run the interactive explorer from the repository root:

```powershell
dotnet run --project samples/AgenticAI/Explorer/DotNetAI.AgenticAI.Explorer.csproj -- --urls http://localhost:5187
```

Open `http://localhost:5187`, select a scenario and choose Mock or Local Ollama. Run history keeps results from both modes for side-by-side review. Each run checks for a final answer, registered tool calls, and completion within the selected turn cap. Mock mode works offline; Local Ollama availability and model are shown in the header.

## Run

From the repository root with .NET 9 SDK installed:

```powershell
dotnet run --project samples/AgenticAI/DotNetAI.AgenticAI.Samples.csproj -- --list
dotnet run --project samples/AgenticAI/DotNetAI.AgenticAI.Samples.csproj -- --mock --all
dotnet run --project samples/AgenticAI/DotNetAI.AgenticAI.Samples.csproj -- --mock 03
dotnet run --project samples/AgenticAI/DotNetAI.AgenticAI.Samples.csproj -- 03
```

Live mode uses the `Ollama` endpoint and chat model configured for the repository, or `OLLAMA_ENDPOINT` and `OLLAMA_CHAT_MODEL` environment variables. Pull a tool-calling-capable model into Ollama before running without `--mock`. The mock mode scripts one function call per sample and runs offline.

## Samples

| ID | Workflow | Agent skills demonstrated |
|---|---|---|
| 01 | Support ticket triage | Evidence gathering, urgency classification, draft-only response |
| 02 | Production incident investigation | Correlating telemetry and logs, escalation handoff |
| 03 | Pull request review assistant | Change and CI review, evidence-backed findings |
| 04 | Internal documentation research | Tool-assisted retrieval, policy-aware answers |
| 05 | Invoice intake and matching | Extraction, purchase-order reconciliation, approval routing |
| 06 | Release readiness review | Combining CI, test, security, and change evidence |
| 07 | Dependency vulnerability triage | Advisory research, compatible update recommendations |
| 08 | Database migration planning | Risk assessment, staged rollout, rollback planning |
| 09 | Accessibility remediation planning | Finding prioritization, manual verification checklist |
| 10 | Customer feedback synthesis | Theme analysis, known-issue correlation, product follow-up |

## Adapt A Sample

Each scenario is in `AgenticSampleCatalog`. Replace its fixture-backed methods in `Tools/EngineeringTools.cs` with adapters for your APIs, repositories, databases, or telemetry. Keep credentials outside source control, validate tool arguments, apply least-privilege authorization, and add approval gates before introducing tools that write data or trigger external actions.

`AgenticToolAgent` constrains the tool list per sample, records invoked tool names, and enforces a maximum turn count. The sample tools return illustrative fixture data only. They do not connect to real ticket systems, cloud resources, payment platforms, repositories, or databases.