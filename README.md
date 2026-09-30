# DotNetAI — 10 Comprehensive AI & Agentic AI Use Cases for .NET 9 Developers

[![.NET 9](https://img.shields.io/badge/.NET-9.0-blue.svg)](https://dotnet.microsoft.com/)
[![C# 13](https://img.shields.io/badge/C%23-13.0-purple.svg)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Microsoft.Extensions.AI](https://img.shields.io/badge/Microsoft.Extensions.AI-10.10.0-green.svg)](https://devblogs.microsoft.com/dotnet/introducing-microsoft-extensions-ai-preview/)
[![Tests Passing](https://img.shields.io/badge/tests-53%2F53%20passed-brightgreen.svg)]()
[![Ollama](https://img.shields.io/badge/Ollama-Local%20LLM-orange.svg)](https://ollama.com/)

An enterprise-grade, production-ready educational repository designed for .NET software engineers to master modern **Generative AI**, **Agentic AI Patterns**, and the unified **`Microsoft.Extensions.AI`** ecosystem.

Every single use case runs 100% locally on your machine with **Ollama** (`llama3.2`, `phi3.5`, `nomic-embed-text`) or entirely offline via built-in deterministic **Mock Clients**.

---

## 🌟 Solution Architecture

```
DotNetAI/
├── src/
│   ├── Common/
│   │   └── DotNetAI.Core/                 # Core abstractions, Ollama clients, Mock test framework, vector math
│   ├── 01-PromptEngineering/              # System prompts, role conditioning, temperature tuning, few-shot
│   ├── 02-StructuredOutputs/              # Schema enforcement, self-healing JSON extractor, domain mapping
│   ├── 03-FunctionCalling/                # AIFunction tools, enterprise ERP/CRM integration, automated tool loop
│   ├── 04-RAG-VectorSearch/               # Recursive chunking, cosine vector similarity, grounded RAG generator
│   ├── 05-Agentic-ReAct/                  # Autonomous Reasoning + Acting loop with Thought/Action/Observation
│   ├── 06-MultiAgent-Collaboration/       # Multi-agent orchestrator with Architect, Coder, Auditor, and PM agents
│   ├── 07-PlanAndSolve-Planner/           # Hierarchical planner: high-level decomposition + sequential execution
│   ├── 08-ConversationalMemory/           # Sliding window buffer, LLM conversation summarizer, entity key-value store
│   ├── 09-SafetyGuardrails/               # PII masking (SSN/CC/ApiKey), prompt injection detection, output sanitizer
│   ├── 10-AIEvaluation-Judge/             # LLM-as-a-Judge evaluating Faithfulness, Relevance, and Coherence
│   └── DotNetAI.Runner/                   # Interactive CLI console runner with menu and batch flags
├── tests/
│   ├── DotNetAI.Core.Tests/               # Unit tests for core primitives, token estimators, vector math (18 tests)
│   └── DotNetAI.UseCases.Tests/           # Integration tests across all 10 use cases and multiple scenarios (35 tests)
├── docs/                                  # Individual in-depth use case guides and glossaries
├── USER_MANUAL.md                         # Complete operator and developer manual
├── TUTORIAL.md                            # Comprehensive tutorial covering Agentic AI terms & theory
└── DotNetAI.slnx                          # Modern .NET solution file
```

---

## 🚀 The 10 AI Use Cases at a Glance

| # | Use Case | Core AI & Agentic Concepts | Key .NET Classes |
|---|---|---|---|
| **01** | **Prompt Engineering** | System prompts, persona conditioning, dynamic templates, temperature tuning, token budgeting | `PromptTemplateEngine`, `SupportChatService` |
| **02** | **Structured Outputs** | JSON Schema constraints, self-healing validation retry loop, strongly-typed deserialization | `StructuredDataExtractorService`, `InvoiceDocument` |
| **03** | **Function Calling** | `AIFunctionFactory`, autonomous multi-turn tool calling, telemetry & query tools | `ToolCallingAgent`, `EnterpriseToolRegistry` |
| **04** | **RAG & Vector Search** | Document chunking, vector embeddings, Cosine similarity search, grounded context generation | `RAGPipeline`, `InMemoryVectorStore`, `DocumentChunker` |
| **05** | **Agentic ReAct** | Autonomous ReAct (Reasoning + Acting) loop, diagnostic inspection tools, max iteration bounds | `ReActAgent`, `SystemMetricsTool`, `ReActStep` |
| **06** | **Multi-Agent Collaboration** | Specialized agent personas (Architect, Developer, Security, PM), orchestrated pipelines | `MultiAgentOrchestrator`, `ArchitectAgent`, `CoderAgent` |
| **07** | **Plan-and-Solve Planner** | Hierarchical goal decomposition, sequential step execution, final deliverable synthesis | `HierarchicalPlanner`, `PlanStep`, `PlanExecutionReport` |
| **08** | **Conversational Memory** | Sliding window buffer, conversational summarizer, entity state store | `ConversationalMemoryManager`, `SlidingWindowMemoryStore` |
| **09** | **Safety Guardrails** | Defense-in-depth, PII scrubbing (Regex + Sanitizer), prompt injection detection | `GuardrailPipeline`, `PIIRedactionService`, `InjectionDetector` |
| **10** | **AI Evaluation Judge** | LLM-as-a-Judge, multi-criteria rubric scoring (Faithfulness, Relevance, Coherence) | `LLMJudgeService`, `EvaluationResult` |

---

## ⚡ Quickstart

### Prerequisites
1. **.NET 9 SDK** (`dotnet --version` >= `9.0.100`)
2. *(Optional for live local LLM)* **Ollama**: [Download Ollama](https://ollama.com/)
   ```bash
   ollama pull llama3.2
   ollama pull nomic-embed-text
   ```

### 1. Clone & Build
```powershell
cd c:\Users\PLT3KOR\Documents\AI\DotNetAI
dotnet build DotNetAI.slnx
```

### 2. Run All Automated Tests (53 Tests)
```powershell
dotnet test DotNetAI.slnx
```

### 3. Launch the Interactive Runner
```powershell
dotnet run --project src/DotNetAI.Runner/DotNetAI.Runner.csproj
```

### 4. Execute All Use Cases in Batch (Offline / Fast)
```powershell
dotnet run --project src/DotNetAI.Runner/DotNetAI.Runner.csproj -- --all --mock
```

---

## 📚 Documentation Links
- [📖 Complete User Manual](./USER_MANUAL.md) — Step-by-step usage, configuration, and operational guidance.
- [🎓 Deep-Dive AI & Agentic Tutorial](./TUTORIAL.md) — Exhaustive guide covering all AI terms, agent patterns, math, and code architecture.
- [📑 AI Glossary & Concepts](./docs/agentic-ai-concepts-glossary.md) — Comprehensive reference of AI terminology.
- [🦙 Ollama Local Setup Guide](./docs/ollama-setup-guide.md) — How to configure Ollama models and endpoints for local inference.

---

## 📄 License
MIT License. Free to use, adapt, and build enterprise .NET AI applications upon.
