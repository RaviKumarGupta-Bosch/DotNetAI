# 06 - Multi-Agent Collaboration & Squad Orchestration

## 🎯 Overview
Complex enterprise engineering tasks—such as architecting, implementing, auditing, and releasing a new cloud service—cannot be reliably achieved by a single generic prompt.

**Multi-Agent Collaboration** decomposes complex workflows into a team of specialized AI agents, each with a focused persona, distinct system instructions, and specific quality responsibilities.

---

## 🧠 Key Concepts & AI Theory

### The Software Engineering Squad Architecture
```
┌────────────────────────────────────────────────────────┐
│                   Product Objective                    │
│ "Build a resilient Azure Service Bus order processor"  │
└────────────────────────────────────────────────────────┘
                            │
                            ▼
┌────────────────────────────────────────────────────────┐
│ 🏛️ Software Architect Agent                            │
│ - Analyzes requirements                                │
│ - Selects patterns (Outbox, Retry, Dead-lettering)     │
│ - Produces architecture specification                  │
└────────────────────────────────────────────────────────┘
                            │
                            ▼
┌────────────────────────────────────────────────────────┐
│ 💻 Backend Developer Agent                             │
│ - Consumes architecture specification                  │
│ - Generates production-ready C# 13 code                │
│ - Implements error handling and DI registration        │
└────────────────────────────────────────────────────────┘
                            │
                            ▼
┌────────────────────────────────────────────────────────┐
│ 🛡️ Security Auditor Agent                              │
│ - Conducts threat modeling (OWASP Top 10)              │
│ - Audits authentication, secret handling, sanitization │
│ - Produces security risk report                        │
└────────────────────────────────────────────────────────┘
                            │
                            ▼
┌────────────────────────────────────────────────────────┐
│ 📋 Delivery Lead / PM Agent                            │
│ - Consolidates all deliverables                        │
│ - Produces final deployment plan & executive summary   │
└────────────────────────────────────────────────────────┘
```

---

## 💻 C# Implementation Walkthrough

### 1. Specialized Agent Configuration: `AgentPersona.cs`
Located at `src/06-MultiAgent-Collaboration/Models/MultiAgentModels.cs`:
```csharp
public record AgentPersona(
    string Name,
    string RoleTitle,
    string SystemInstructions,
    float Temperature
);
```

### 2. Multi-Agent Orchestrator: `MultiAgentOrchestratorService.cs`
Located at `src/06-MultiAgent-Collaboration/Services/MultiAgentOrchestratorService.cs`:
```csharp
public async Task<CollaborationResult> RunSprintWorkflowAsync(string projectRequirement)
{
    var history = new List<AgentTurnResult>();

    // Step 1: Architect
    var archSpec = await ExecuteAgentTurnAsync(_architect, projectRequirement);
    history.Add(new AgentTurnResult(_architect.RoleTitle, archSpec));

    // Step 2: Coder (Contextualized with Architecture)
    var codeInput = $"Requirement: {projectRequirement}\nArchitecture Spec:\n{archSpec}";
    var codeResult = await ExecuteAgentTurnAsync(_coder, codeInput);
    history.Add(new AgentTurnResult(_coder.RoleTitle, codeResult));

    // Step 3: Security Auditor (Contextualized with Code)
    var auditInput = $"Requirement: {projectRequirement}\nCode to Audit:\n{codeResult}";
    var auditResult = await ExecuteAgentTurnAsync(_securityAuditor, auditInput);
    history.Add(new AgentTurnResult(_securityAuditor.RoleTitle, auditResult));

    // Step 4: Delivery Lead (Consolidates all artifacts)
    var deliveryInput = $"Architecture:\n{archSpec}\n\nCode:\n{codeResult}\n\nSecurity:\n{auditResult}";
    var finalReport = await ExecuteAgentTurnAsync(_deliveryLead, deliveryInput);
    history.Add(new AgentTurnResult(_deliveryLead.RoleTitle, finalReport));

    return new CollaborationResult(projectRequirement, history, finalReport);
}
```

---

## 🧪 Running & Validating

### Run via .NET CLI:
```powershell
dotnet run --project src/06-MultiAgent-Collaboration -- --mock
```

### Run Unit Tests:
```powershell
dotnet test tests/DotNetAI.UseCases.Tests --filter "FullyQualifiedName~MultiAgentCollaborationTests"
```
