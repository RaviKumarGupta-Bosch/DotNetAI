# Modern AI & Agentic AI Tutorial for .NET 9 Developers

Welcome to the definitive tutorial on developing modern **Generative AI** and **Agentic AI** systems in .NET 9 using C# 13 and `Microsoft.Extensions.AI`.

This guide walks through core AI concepts, theoretical foundations, mathematical formulas, architectural patterns, and real-world C# implementations.

---

## 📑 Table of Contents
1. [The Paradigm Shift: From Static Prompts to Agentic Systems](#1-the-paradigm-shift-from-static-prompts-to-agentic-systems)
2. [The Unified .NET AI Ecosystem (`Microsoft.Extensions.AI`)](#2-the-unified-net-ai-ecosystem-microsoftextensionsai)
3. [Core LLM Concepts: Tokens, Embeddings & Vector Math](#3-core-llm-concepts-tokens-embeddings--vector-math)
4. [Prompt Engineering, Personas & Temperature Tuning](#4-prompt-engineering-personas--temperature-tuning)
5. [Structured Outputs & Self-Correcting Extraction Loops](#5-structured-outputs--self-correcting-extraction-loops)
6. [Tool Use & Function Calling Architecture](#6-tool-use--function-calling-architecture)
7. [Retrieval-Augmented Generation (RAG) & Vector Search](#7-retrieval-augmented-generation-rag--vector-search)
8. [Agentic Pattern 1: The ReAct (Reasoning + Acting) Loop](#8-agentic-pattern-1-the-react-reasoning--acting-loop)
9. [Agentic Pattern 2: Multi-Agent Orchestration & Squads](#9-agentic-pattern-2-multi-agent-orchestration--squads)
10. [Agentic Pattern 3: Plan-and-Solve Hierarchical Decomposers](#10-agentic-pattern-3-plan-and-solve-hierarchical-decomposers)
11. [Conversational Memory & State Management Strategies](#11-conversational-memory--state-management-strategies)
12. [AI Safety, Guardrails & Defense-in-Depth](#12-ai-safety-guardrails--defense-in-depth)
13. [AI Evaluation & LLM-as-a-Judge Quality Systems](#13-ai-evaluation--llm-as-a-judge-quality-systems)
14. [Local AI Execution with Ollama & Offline Testing](#14-local-ai-execution-with-ollama--offline-testing)

---

## 1. The Paradigm Shift: From Static Prompts to Agentic Systems

Traditional AI interactions follow a simple **one-shot request-response** model:
```
User Prompt ──▶ LLM ──▶ Text Response
```
While useful for basic summarization, this static model suffers from critical limitations:
- **Knowledge cutoff**: The model cannot access data outside its training set.
- **Hallucination**: The model invents plausible-sounding falsehoods when lacking context.
- **Inability to act**: The model cannot query live databases, execute code, or call enterprise APIs.
- **No self-reflection**: The model outputs whatever first comes to mind without verifying correctness.

### What is Agentic AI?
An **AI Agent** is an autonomous software entity that perceives its environment, makes reasoning decisions, executes actions via tools, observes outcomes, and iterates until an overarching goal is accomplished.

```
       ┌────────────────────────────────────────────────────────┐
       │                       AI AGENT                         │
       │                                                        │
       │   ┌───────────┐       ┌───────────┐      ┌─────────┐   │
       │   │  Perceive │ ───▶  │   Reason  │ ───▶ │   Act   │   │
       │   └───────────┘       └───────────┘      └─────────┘   │
       │         ▲                   │                 │        │
       │         │                   ▼                 ▼        │
       │   ┌───────────┐       ┌───────────┐      ┌─────────┐   │
       │   │  Memory   │       │ Reflection│      │  Tools  │   │
       │   └───────────┘       └───────────┘      └─────────┘   │
       └────────────────────────────────────────────────────────┘
```

---

## 2. The Unified .NET AI Ecosystem (`Microsoft.Extensions.AI`)

Historically, .NET developers had to maintain separate SDKs for Azure OpenAI, Anthropic, Semantic Kernel, and local Ollama instances.

With **`Microsoft.Extensions.AI`**, Microsoft established standard primitives for all AI interactions in .NET:
- **`IChatClient`**: Unified interface for chat completions, multi-turn dialogue, streaming, and tool calling.
- **`IEmbeddingGenerator<TInput, TEmbedding>`**: Unified interface for transforming text into dense vector embeddings.
- **`ChatMessage` & `ChatOptions`**: Standard message representations (`ChatRole.System`, `ChatRole.User`, `ChatRole.Assistant`, `ChatRole.Tool`).
- **`AIFunction` & `AIFunctionFactory`**: Strongly-typed tool registration directly from C# methods and delegates.

```csharp
// Example: Creating a local Ollama client via Microsoft.Extensions.AI
IChatClient client = new OllamaChatClient(new Uri("http://localhost:11434"), "llama3.2");

var response = await client.GetResponseAsync(
    new ChatMessage[]
    {
        new(ChatRole.System, "You are an expert .NET architect."),
        new(ChatRole.User, "Explain the ReAct agentic pattern.")
    },
    new ChatOptions { Temperature = 0.2f }
);

Console.WriteLine(response.Text);
```

---

## 3. Core LLM Concepts: Tokens, Embeddings & Vector Math

### 3.1 Tokens & Tokenization
LLMs do not process raw strings or characters; they process **Tokens** (sub-word fragments).
- On average, in English text: **1 token ≈ 4 characters** or **0.75 words**.
- A standard 4,000-word document is approximately 5,300 tokens.
- Token budgets matter because LLMs have a **Context Window** limit (e.g., 8,192 tokens for standard LLaMA 3.2, 128k for GPT-4o).

In `DotNetAI.Core`, `TokenEstimator.EstimateTokenCount(text)` provides fast $O(1)$ token estimation:
$$\text{Tokens} \approx \left\lceil \frac{\text{Character Count}}{4} \right\rceil$$

### 3.2 Vector Embeddings
An **Embedding** is a high-dimensional vector of real numbers (e.g., 384, 768, or 1,536 floats) that captures the semantic meaning of text.
- Words or sentences with similar meanings occupy nearby coordinates in vector space.
- Example: `"Database query timeout"` and `"SQL server execution deadline expired"` will have very close embedding vectors, even though they share few words.

### 3.3 Cosine Similarity
To measure how closely related two embeddings $A$ and $B$ are, we compute the **Cosine Similarity**:
$$\text{CosineSimilarity}(\mathbf{A}, \mathbf{B}) = \frac{\mathbf{A} \cdot \mathbf{B}}{\|\mathbf{A}\| \|\mathbf{B}\|} = \frac{\sum_{i=1}^{n} A_i B_i}{\sqrt{\sum_{i=1}^{n} A_i^2} \sqrt{\sum_{i=1}^{n} B_i^2}}$$

In `DotNetAI.Core.Utils.VectorMath`:
```csharp
public static float CosineSimilarity(ReadOnlySpan<float> vecA, ReadOnlySpan<float> vecB)
{
    float dot = 0f, magA = 0f, magB = 0f;
    for (int i = 0; i < vecA.Length; i++)
    {
        dot += vecA[i] * vecB[i];
        magA += vecA[i] * vecA[i];
        magB += vecB[i] * vecB[i];
    }
    float denominator = (float)(Math.Sqrt(magA) * Math.Sqrt(magB));
    return denominator < 1e-7f ? 0f : dot / denominator;
}
```

---

## 4. Prompt Engineering, Personas & Temperature Tuning

Prompt engineering is the systematic design of inputs to guide LLM behavior.

### Key Components of an Effective Prompt:
1. **System Prompt / Persona**: Establishes the agent's role, behavioral constraints, and tone.
2. **Context & Instructions**: Details the specific task, background facts, and guidelines.
3. **Few-Shot Examples**: Demonstrates input-output pairs to steer output format.
4. **Output Constraints**: Specifies exact formatting rules (e.g., JSON, markdown table).

### Temperature Tuning:
- **Low Temperature (0.0 – 0.2)**: Deterministic, focused, analytical. Ideal for code generation, mathematical analysis, and structured extraction.
- **Medium Temperature (0.5 – 0.7)**: Balanced coherence and diversity. Ideal for general assistant chat and technical writing.
- **High Temperature (0.8 – 1.0+)**: Highly creative, variable, explorative. Ideal for brainstorming, fiction, and divergent idea generation.

---

## 5. Structured Outputs & Self-Correcting Extraction Loops

In enterprise systems, AI responses must integrate directly into databases, APIs, and business workflows. Raw text is fragile; we need **JSON schemas** and strong typing.

### Self-Correcting Extraction Pattern
When an LLM produces invalid JSON, instead of crashing, our `StructuredDataExtractorService` catches the parsing exception, appends the error details, and re-prompts the model:

```
┌─────────────────┐       ┌─────────────────┐       ┌─────────────────┐
│ Unstructured    │ ───▶  │ Initial LLM     │ ───▶  │ JSON Schema     │
│ Text Input      │       │ Extraction      │       │ Validation      │
└─────────────────┘       └─────────────────┘       └─────────────────┘
                                                             │
                                                      Valid? ├─── No ───┐
                                                             │          │
                                                            Yes         ▼
                                                             │   ┌───────────────┐
                                                             │   │ Self-Correct  │
                                                             │   │ Re-prompt with│
                                                             │   │ Error Trace   │
                                                             │   └───────────────┘
                                                             ▼          │
                                                    ┌────────────────┐  │
                                                    │ Typed C# Model │◀─┘
                                                    └────────────────┘
```

---

## 6. Tool Use & Function Calling Architecture

**Function Calling** allows the LLM to act as a controller: when it detects that answering a question requires external data or action, it emits a structured tool call.

### How it works in `Microsoft.Extensions.AI`:
1. Define standard C# methods:
   ```csharp
   [Description("Retrieves telemetry metrics for an Azure microservice.")]
   public static string GetServiceMetrics(string serviceName) => "...";
   ```
2. Wrap as `AIFunction`:
   ```csharp
   AIFunction metricTool = AIFunctionFactory.Create(GetServiceMetrics);
   ```
3. Pass tools in `ChatOptions`:
   ```csharp
   var options = new ChatOptions { Tools = [metricTool] };
   ```
4. The client executes multi-turn loops: the model requests tool execution $\rightarrow$ the runner executes the C# method $\rightarrow$ returns the result as `ChatRole.Tool` $\rightarrow$ the model delivers the final answer.

---

## 7. Retrieval-Augmented Generation (RAG) & Vector Search

RAG connects private enterprise knowledge bases to LLMs without fine-tuning.

```
       [Raw Knowledge Documents]
                   │
                   ▼ (1. Recursive Chunking: 300 tokens, 50 token overlap)
       [Text Chunks: C_1, C_2, ... C_n]
                   │
                   ▼ (2. Embedding Generation)
       [Vectors: V_1, V_2, ... V_n] ──▶ Stored in InMemoryVectorStore
                   │
       [User Query: "How to configure JWT?"]
                   │
                   ▼ (3. Query Vectorization & Cosine Similarity)
       [Top-K Nearest Relevant Chunks]
                   │
                   ▼ (4. Grounded Prompt Formulation)
       "Using ONLY the provided facts, answer the question..."
                   │
                   ▼ (5. LLM Synthesis)
       [Grounded, Fact-Checked Answer with Citations]
```

---

## 8. Agentic Pattern 1: The ReAct (Reasoning + Acting) Loop

Introduced by Yao et al. (2022), **ReAct** interleaves explicit reasoning traces (*Thoughts*) with action executions (*Actions*) and environment feedback (*Observations*).

### The ReAct Cycle:
1. **Thought**: The agent reasons about the current state and decides what information is missing.
2. **Action**: The agent selects a tool and passes arguments.
3. **Observation**: The environment returns the tool execution output.
4. **Iteration**: The cycle repeats until the agent outputs **`Final Answer`**.

```
Problem: Database latency high in East US.

Thought 1: I should first check the current CPU and memory utilization of the database instance.
Action 1: GetCpuUtilization(resourceId: "db-prod-eastus")
Observation 1: CPU is at 98.4%, active connections: 1,450.

Thought 2: CPU is saturated. Let me inspect the top slow-running SQL queries.
Action 2: GetSlowQueries(resourceId: "db-prod-eastus", limit: 3)
Observation 2: Query `SELECT * FROM Orders WHERE Status = 'Pending'` lacks an index on Status.

Thought 3: The root cause is a full table scan on the Orders table due to a missing index.
Final Answer: The incident is caused by unindexed queries on Orders.Status causing 98.4% CPU load. Recommended fix: CREATE INDEX idx_orders_status ON Orders(Status).
```

---

## 9. Agentic Pattern 2: Multi-Agent Orchestration & Squads

Complex software projects exceed what a single prompt or agent can handle. In **Multi-Agent Collaboration**, specialized agent personas collaborate across defined workflows.

```
┌─────────────────────────┐
│     User Objective      │
└─────────────────────────┘
             │
             ▼
┌─────────────────────────┐
│ 🏛️ Software Architect   │ ──▶ Produces System Architecture & Component Diagram
└─────────────────────────┘
             │
             ▼
┌─────────────────────────┐
│ 💻 Backend Developer    │ ──▶ Implements C# 13 Code & Domain Services
└─────────────────────────┘
             │
             ▼
┌─────────────────────────┐
│ 🛡️ Security Auditor     │ ──▶ Conducts OWASP Risk Analysis & Threat Modeling
└─────────────────────────┘
             │
             ▼
┌─────────────────────────┐
│ 📋 Delivery Lead (PM)   │ ──▶ Synthesizes Production Deployment Plan
└─────────────────────────┘
```

---

## 10. Agentic Pattern 3: Plan-and-Solve Hierarchical Decomposers

The **Plan-and-Solve** pattern (Wang et al., 2023) addresses the "greedy decision-making" drawback of simple step-by-step agents by enforcing a two-phase architecture:
1. **Strategic Planning Phase**: Decomposes the overarching goal into a structured DAG of sequential milestones.
2. **Execution Phase**: Sequentially solves each sub-task while accumulating prior deliverables into execution context.

---

## 11. Conversational Memory & State Management Strategies

Managing conversational context requires balancing context window limits with recall accuracy:
- **Sliding Window Buffer**: Keeps the most recent $K$ dialogue turns verbatim.
- **Conversational Summarizer**: Uses an LLM to distill older turns into a concise running summary.
- **Entity State Tracking**: Extracts structured key-value pairs (e.g., `UserName = Alex`, `TargetCloud = Azure`) into a persistent dictionary.

---

## 12. AI Safety, Guardrails & Defense-in-Depth

Enterprise AI systems must be resilient against adversarial inputs and privacy leaks:
- **Pre-execution PII Masking**: Regex & token scanners replace SSNs, Credit Cards, and API keys with redact tokens (`[REDACTED_SSN]`) before prompts reach the model.
- **Adversarial Prompt Injection Detection**: Heuristics & classifiers detect jailbreak attacks (e.g., "Ignore previous instructions", "DAN mode").
- **Output Moderation**: Scans generated answers for toxicity, sensitive disclosures, or off-topic outputs.

---

## 13. AI Evaluation & LLM-as-a-Judge Quality Systems

Automated evaluation uses a superior LLM to grade candidate responses using standardized rubrics:
- **Faithfulness / Groundedness**: Is every claim in the answer directly supported by the context?
- **Answer Relevance**: Did the model directly address the user's specific question?
- **Coherence**: Is the response logically structured, concise, and clear?

---

## 14. Local AI Execution with Ollama & Offline Testing

With DotNetAI, all 10 use cases run locally on your workstation without third-party cloud subscriptions:
- **Local Ollama**: Fast, private local inference powered by llama.cpp.
- **`MockChatClient` & `MockEmbeddingGenerator`**: Built-in test doubles enable sub-second CI/CD test execution with 100% deterministic predictability.

---

*Continue to the individual guides in the [`docs/`](./docs/) directory for detailed walkthroughs of each use case.*
