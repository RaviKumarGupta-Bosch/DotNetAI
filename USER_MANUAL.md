# DotNetAI — User Manual & Operations Guide

Welcome to the **DotNetAI** user manual. This guide explains how to configure, execute, test, and troubleshoot the 10 AI use case implementations in your local development environment.

---

## 📋 Table of Contents
1. [System Requirements](#1-system-requirements)
2. [Project Configuration & Settings](#2-project-configuration--settings)
3. [Running Local Ollama vs. Offline Mock](#3-running-local-ollama-vs-offline-mock)
4. [Using the Interactive CLI Runner](#4-using-the-interactive-cli-runner)
5. [Running Unit and Integration Tests](#5-running-unit-and-integration-tests)
6. [Walkthrough of the 10 Use Cases](#6-walkthrough-of-the-10-use-cases)
7. [Troubleshooting & FAQs](#7-troubleshooting--faqs)

---

## 1. System Requirements

- **Operating System:** Windows 10/11, macOS (Apple Silicon or Intel), or Linux (Ubuntu 22.04+).
- **.NET SDK:** .NET 9.0 SDK or higher.
- **Hardware (for local Ollama LLM):**
  - Minimum: 8 GB RAM, 4 CPU cores.
  - Recommended: 16 GB+ RAM, GPU with 6 GB+ VRAM (NVIDIA RTX / Apple M-series).
- **Hardware (for Offline Mock):** Runs instantly on any machine with 0 GPU or network requirements.

---

## 2. Project Configuration & Settings

Configuration is managed via `appsettings.json` in `src/DotNetAI.Runner/appsettings.json` (or environment variables):

```json
{
  "Ollama": {
    "Endpoint": "http://localhost:11434",
    "ChatModel": "llama3.2",
    "EmbeddingModel": "nomic-embed-text",
    "DefaultTemperature": 0.7,
    "TimeoutSeconds": 120,
    "EnableFallbackToMock": true
  }
}
```

### Environment Variable Overrides
You can override any setting using standard .NET environment variables:
- `Ollama__Endpoint`: URL of the Ollama server (e.g. `http://192.168.1.50:11434` for remote LAN servers).
- `Ollama__ChatModel`: Model tag (e.g. `phi3.5`, `mistral`, `llama3.1:8b`).
- `Ollama__EmbeddingModel`: Embedding model (e.g. `all-minilm`, `bge-large`).
- `Ollama__EnableFallbackToMock`: `true` or `false`.

---

## 3. Running Local Ollama vs. Offline Mock

DotNetAI supports two execution modes:

### Mode A: Real Local LLM (Ollama)
1. Install Ollama from [ollama.com](https://ollama.com/).
2. Pull the required models:
   ```bash
   ollama pull llama3.2
   ollama pull nomic-embed-text
   ```
3. Start the Ollama daemon:
   ```bash
   ollama serve
   ```
4. Verify accessibility:
   ```powershell
   curl http://localhost:11434/api/tags
   ```

### Mode B: Offline Mock Mode (Zero-Dependency)
- Ideal for fast CI/CD pipelines, automated testing, or environments without GPU/Ollama installed.
- DotNetAI contains high-fidelity deterministic mocks (`MockChatClient`, `MockEmbeddingGenerator`) that emulate LLM reasoning, schema responses, tool invocations, and vector embeddings without network calls.

---

## 4. Using the Interactive CLI Runner

The `DotNetAI.Runner` console application provides an interactive terminal UI for running individual use cases or executing test batches.

### Launching Interactive Mode
```powershell
dotnet run --project src/DotNetAI.Runner/DotNetAI.Runner.csproj
```

**Interactive Menu Options:**
- Enter `1` through `10`: Run that specific AI use case.
- Enter `A`: Run all 10 use cases sequentially.
- Enter `M`: Toggle between **Local Ollama** and **Offline Mock** mode on the fly.
- Enter `Q`: Quit the program.

### Command-Line Execution Flags
For CI/CD scripts or automated verification:
```powershell
# Run all 10 use cases in batch with mock mode:
dotnet run --project src/DotNetAI.Runner/DotNetAI.Runner.csproj -- --all --mock

# Run all 10 use cases in batch with real local Ollama:
dotnet run --project src/DotNetAI.Runner/DotNetAI.Runner.csproj -- --all
```

---

## 5. Running Unit and Integration Tests

The solution includes 53 comprehensive unit and scenario integration tests covering all 10 use cases and core algorithms.

### Run All Tests
```powershell
dotnet test DotNetAI.slnx
```

### Run Specific Test Projects
```powershell
# Core algorithms, token estimators, vector math (18 tests):
dotnet test tests/DotNetAI.Core.Tests/DotNetAI.Core.Tests.csproj

# Use case scenarios & pipeline tests (35 tests):
dotnet test tests/DotNetAI.UseCases.Tests/DotNetAI.UseCases.Tests.csproj
```

### Detailed Test Output
```powershell
dotnet test DotNetAI.slnx --logger "console;verbosity=detailed"
```

---

## 6. Walkthrough of the 10 Use Cases

### Use Case 01: Prompt Engineering & Local LLM Tuning
- **What it does:** Demonstrates system prompts, persona conditioning (Technical Senior Architect vs. Empathetic Support), dynamic parameter templating, temperature modulation (0.1 for deterministic code vs. 0.8 for creative brainstorming), and token budgeting.
- **Project:** `src/01-PromptEngineering`

### Use Case 02: Structured Outputs & Data Extraction
- **What it does:** Extracts unstructured, noisy text (such as vendor invoices) into strongly-typed C# record models (`InvoiceDocument`). Features self-correcting validation loops that inspect deserialization errors and re-prompt the LLM to fix malformed JSON.
- **Project:** `src/02-StructuredOutputs`

### Use Case 03: Function Calling & Tool Execution
- **What it does:** Uses `Microsoft.Extensions.AI`'s `AIFunctionFactory` to expose C# methods (such as order status queries, refund processing, and system telemetry) as callable tools to the model. The agent executes multi-turn tool loops until user requests are satisfied.
- **Project:** `src/03-FunctionCalling`

### Use Case 04: RAG & Vector Semantic Search
- **What it does:** Implements end-to-end Retrieval-Augmented Generation: splits markdown/text documents using recursive chunking with configurable overlap, converts text chunks into vector embeddings, performs Cosine similarity top-$k$ nearest neighbor search, and grounds the LLM answer strictly in retrieved context.
- **Project:** `src/04-RAG-VectorSearch`

### Use Case 05: Agentic AI - ReAct Pattern
- **What it does:** Implements the **Reasoning + Acting** pattern. Given an IT incident (e.g., database high latency), the agent autonomously iterates through `Thought -> Action -> Observation` loops using diagnostic tools to isolate root causes and produce remediation plans.
- **Project:** `src/05-Agentic-ReAct`

### Use Case 06: Multi-Agent Collaboration
- **What it does:** Simulates a specialized engineering squad composed of four distinct AI agents: **Software Architect**, **Backend Developer**, **Security Auditor**, and **Product Delivery Lead**. Agents communicate through structured messages to produce end-to-end software specifications and audits.
- **Project:** `src/06-MultiAgent-Collaboration`

### Use Case 07: Plan-and-Solve (Hierarchical Planner)
- **What it does:** Breaks complex, ambiguous user objectives (e.g., "Implement Zero-Trust Security for E-Commerce API") into discrete, sequential, executable plan steps. A step executor runs each phase while maintaining accumulated context, and a lead synthesizer compiles the final deliverable.
- **Project:** `src/07-PlanAndSolve-Planner`

### Use Case 08: Conversational Memory & State Tracking
- **What it does:** Manages long-running conversational sessions using hybrid memory: a sliding window buffer for immediate turns, an LLM-powered background summarizer for older dialogue, and a deterministic key-value entity tracker for user preferences and metadata.
- **Project:** `src/08-ConversationalMemory`

### Use Case 09: AI Safety Guardrails & Defense
- **What it does:** Implements defense-in-depth safety filters: scrubs PII (Social Security Numbers, Credit Cards, API Keys, Emails) before sending prompts to the LLM, detects prompt injection and jailbreak attempts (DAN mode, override instructions), and sanitizes LLM outputs.
- **Project:** `src/09-SafetyGuardrails`

### Use Case 10: AI Evaluation & LLM-as-a-Judge
- **What it does:** Implements an automated LLM-as-a-Judge evaluation framework. Evaluates candidate LLM responses against a rubric across three standard dimensions: **Faithfulness** (factual consistency with context), **Relevance** (answering user intent), and **Coherence** (clarity and structure).
- **Project:** `src/10-AIEvaluation-Judge`

---

## 7. Troubleshooting & FAQs

### Q1: The runner takes long on the first step when Ollama is running.
- **Cause:** Local LLM inference on CPU can take 10–30 seconds per prompt depending on model size and hardware.
- **Fix:** Use smaller models like `llama3.2:1b` or `phi3.5:mini`, or switch to offline mock mode (`--mock` or toggle `[M]` in the menu).

### Q2: Ollama returns connection refused (`11434`).
- **Fix:** Ensure Ollama is started by running `ollama serve` in a separate terminal or checking if the Ollama background tray app is running.

### Q3: How do I point DotNetAI to a remote Ollama server?
- Update `appsettings.json` `"Endpoint": "http://<REMOTE_IP>:11434"` or set environment variable `$env:Ollama__Endpoint="http://<REMOTE_IP>:11434"`.
