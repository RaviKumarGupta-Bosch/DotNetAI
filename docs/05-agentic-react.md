# 05 - Agentic ReAct (Reasoning + Acting) Autonomous Loop

## 🎯 Overview
The **ReAct** (Reasoning + Acting) architecture is the cornerstone of modern agentic workflows. Instead of acting blindly or reasoning in a vacuum, a ReAct agent continuously cycles through:
1. **Thought**: Analyzing current state and reasoning about what to do next.
2. **Action**: Invoking a specific tool with arguments.
3. **Observation**: Receiving tool execution feedback from the environment.
4. **Final Answer**: Terminating when the problem is solved.

This use case demonstrates an autonomous ReAct loop in C# capable of multi-step diagnostics, tool execution, and self-directed problem resolution.

---

## 🧠 Key Concepts & AI Theory

### 1. The ReAct Prompt Format
The agent is primed with explicit instructions to structure its output into distinct sections:
```
Answer the following questions as best you can. You have access to the following tools:
- QueryDatabase(sqlQuery: string)
- RestartPod(podName: string)
- GetClusterLogs(namespace: string)

Use the following format:
Thought: you should always think about what to do
Action: the action to take, should be one of [QueryDatabase, RestartPod, GetClusterLogs]
Action Input: the input to the action
Observation: the result of the action
... (this Thought/Action/Action Input/Observation can repeat N times)
Thought: I now know the final answer
Final Answer: the final answer to the original input question
```

### 2. Autonomous Loop Mechanics
```
                      ┌──────────────────────┐
                      │   User Incident      │
                      └──────────────────────┘
                                 │
                                 ▼
                     ┌────────────────────────┐
                 ┌──▶│  LLM Generates Thought │
                 │   └────────────────────────┘
                 │               │
                 │               ▼
                 │   ┌────────────────────────┐
                 │   │  Action: Tool Invocation│
                 │   └────────────────────────┘
                 │               │
                 │               ▼
                 │   ┌────────────────────────┐
                 │   │ Local C# Tool Execution│
                 │   └────────────────────────┘
                 │               │
                 │               ▼
                 │   ┌────────────────────────┐
                 └───│ Observation Appended   │
                     └────────────────────────┘
                                 │ (When Goal Met)
                                 ▼
                     ┌────────────────────────┐
                     │      Final Answer      │
                     └────────────────────────┘
```

---

## 💻 C# Implementation Walkthrough

### 1. ReAct Models: `ReActTrace.cs`
Located at `src/05-Agentic-ReAct/Models/ReActModels.cs`:
```csharp
public record ReActStep(
    int StepNumber,
    string Thought,
    string? ActionName,
    string? ActionInput,
    string? Observation
);

public record ReActExecutionResult(
    string FinalAnswer,
    List<ReActStep> Steps,
    bool Success,
    int TotalIterations
);
```

### 2. The Loop Engine: `ReActAgentEngine.cs`
Located at `src/05-Agentic-ReAct/Services/ReActAgentEngine.cs`:
- Bounded by `maxIterations` (e.g., 5-8) to prevent infinite loops.
- Parses output regex for `Thought:`, `Action:`, `Action Input:`, and `Final Answer:`.
- Invokes registered tool delegates dynamically.
- Appends observations and continues reasoning.

---

## 🧪 Running & Validating

### Run via .NET CLI:
```powershell
dotnet run --project src/05-Agentic-ReAct -- --mock
```

### Run Unit Tests:
```powershell
dotnet test tests/DotNetAI.UseCases.Tests --filter "FullyQualifiedName~AgenticReActTests"
```
