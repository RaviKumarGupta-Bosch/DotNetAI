using DotNetAI.Core.Testing;
using DotNetAI.PlanAndSolve.Services;
using Xunit;

namespace DotNetAI.UseCases.Tests;

public class PlanAndSolveTests
{
    [Fact]
    public async Task HierarchicalPlanner_GeneratePlan_ReturnsStructuredSteps()
    {
        string planJson = """
        [
          {"StepNumber": 1, "Title": "Threat Assessment", "Description": "Identify OWASP risks", "ExpectedOutput": "Risk Matrix"},
          {"StepNumber": 2, "Title": "Implementation", "Description": "Add rate limiter", "ExpectedOutput": "Middleware code"}
        ]
        """;

        var mock = new MockChatClient(planJson);
        var planner = new HierarchicalPlanner();

        var steps = await planner.GeneratePlanAsync(mock, "Secure public API endpoints");

        Assert.Equal(2, steps.Count);
        Assert.Equal("Threat Assessment", steps[0].Title);
        Assert.Equal("Implementation", steps[1].Title);
    }

    [Fact]
    public async Task HierarchicalPlanner_ExecutePlan_ProducesFullReport()
    {
        var mock = new MockChatClient(prompt =>
        {
            if (prompt.Contains("Solutions Delivery Lead", StringComparison.OrdinalIgnoreCase) || prompt.Contains("Consolidate the completed plan", StringComparison.OrdinalIgnoreCase))
            {
                return "Consolidated Deliverable: Successfully engineered DB schema and repository.";
            }
            if (prompt.Contains("Strategic Task Planner", StringComparison.OrdinalIgnoreCase))
            {
                return """
                [
                  {"StepNumber": 1, "Title": "Design Schema", "Description": "Design DB tables", "ExpectedOutput": "DDL Script"},
                  {"StepNumber": 2, "Title": "Write Repository", "Description": "Implement EF Core repo", "ExpectedOutput": "Repository class"}
                ]
                """;
            }
            if (prompt.Contains("Design Schema", StringComparison.OrdinalIgnoreCase))
                return "CREATE TABLE Users (Id INT PRIMARY KEY);";
            if (prompt.Contains("Write Repository", StringComparison.OrdinalIgnoreCase))
                return "public class UserRepository : IUserRepository { }";
            return "Consolidated Deliverable: Successfully engineered DB schema and repository.";
        });

        var planner = new HierarchicalPlanner();
        var report = await planner.ExecutePlanAsync(mock, "Create User Microservice");

        Assert.NotNull(report);
        Assert.Equal(2, report.ExecutedSteps.Count);
        Assert.Contains("Consolidated Deliverable", report.FinalConsolidatedOutput);
    }
}
