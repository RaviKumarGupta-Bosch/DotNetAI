# 07 - Plan-and-Solve Hierarchical Decomposers

## 🎯 Overview
Standard agents often act "greedily"—taking immediate actions without anticipating subsequent obstacles.

The **Plan-and-Solve** pattern (Wang et al., 2023) separates execution into two explicit phases:
1. **Strategic Planning Phase**: Deconstructs a high-level goal into an ordered sequence of discrete sub-tasks.
2. **Execution Phase**: Sequentially solves each step while maintaining intermediate context, concluding with a consolidated synthesis.

---

## 🧠 Key Concepts & AI Theory

### 1. Two-Phase Architecture
```
[High-Level User Goal]
          │
          ▼ (Phase 1: Strategic Planner)
[Structured JSON Execution Plan]
  ├─ Step 1: Discover database schema
  ├─ Step 2: Formulate optimized index strategy
  └─ Step 3: Generate migration script
          │
          ▼ (Phase 2: Step-by-Step Solver)
[Execute Step 1] ──▶ Intermediate Output 1
          │
          ▼
[Execute Step 2 (with Output 1 Context)] ──▶ Intermediate Output 2
          │
          ▼
[Execute Step 3 (with Outputs 1 & 2 Context)] ──▶ Intermediate Output 3
          │
          ▼ (Final Synthesis)
[Comprehensive Final Deliverable]
```

---

## 💻 C# Implementation Walkthrough

### 1. Plan Models: `PlanModels.cs`
Located at `src/07-PlanAndSolve-Planner/Models/PlanModels.cs`:
```csharp
public record PlanStep(
    int StepNumber,
    string StepTitle,
    string Instructions,
    string ExpectedOutput
);

public record ExecutionPlan(
    string Goal,
    List<PlanStep> Steps
);
```

### 2. Plan-and-Solve Engine: `PlanAndSolveService.cs`
Located at `src/07-PlanAndSolve-Planner/Services/PlanAndSolveService.cs`:
- Generates a structured `ExecutionPlan` JSON using `ChatResponseFormat.Json`.
- Iterates over each `PlanStep`, prompt-chaining previous intermediate deliverables.
- Synthesizes the overall solution.

---

## 🧪 Running & Validating

### Run via .NET CLI:
```powershell
dotnet run --project src/07-PlanAndSolve-Planner -- --mock
```

### Run Unit Tests:
```powershell
dotnet test tests/DotNetAI.UseCases.Tests --filter "FullyQualifiedName~PlanAndSolveTests"
```
