using System.ComponentModel;
using System.Text.Json;

namespace DotNetAI.AgenticReAct.Tools;

public interface IDiagnosticTool
{
    string Name { get; }
    string Description { get; }
    Task<string> ExecuteAsync(string input, CancellationToken ct = default);
}

public class SystemMetricsTool : IDiagnosticTool
{
    public string Name => "get_system_metrics";
    public string Description => "Fetches CPU, RAM, Disk, and Network IO telemetry for a given hostname or cluster (e.g. 'web-node-01').";

    public Task<string> ExecuteAsync(string input, CancellationToken ct = default)
    {
        var node = input.Trim().Trim('"');
        var data = node.ToLower() switch
        {
            "web-node-01" => new { Host = node, CpuUsage = "98.4%", MemoryUsed = "15.8GB / 16.0GB (98.7%)", DiskFree = "45GB", Status = "DEGRADED" },
            "web-node-02" => new { Host = node, CpuUsage = "24.1%", MemoryUsed = "4.2GB / 16.0GB (26.2%)", DiskFree = "62GB", Status = "HEALTHY" },
            _ => new { Host = node, CpuUsage = "12.0%", MemoryUsed = "3.1GB / 16.0GB", DiskFree = "80GB", Status = "HEALTHY" }
        };
        return Task.FromResult(JsonSerializer.Serialize(data));
    }
}

public class LogAnalyzerTool : IDiagnosticTool
{
    public string Name => "search_error_logs";
    public string Description => "Searches the last 1000 lines of application logs for fatal errors and stack traces for a given service or host.";

    public Task<string> ExecuteAsync(string input, CancellationToken ct = default)
    {
        var target = input.Trim().Trim('"');
        var logs = new[]
        {
            "[2025-02-18 10:14:02 UTC] [ERROR] [BillingService] System.OutOfMemoryException: Exception of type 'System.OutOfMemoryException' was thrown in CacheManager.AllocateBuffer()",
            "[2025-02-18 10:14:05 UTC] [FATAL] [BillingService] ThreadPool starvation detected. Active threads: 1024, Queue length: 4890",
            "[2025-02-18 10:14:10 UTC] [WARN] [HealthCheck] Service /health endpoint unresponsive (timed out after 5000ms)"
        };
        return Task.FromResult(JsonSerializer.Serialize(new { Target = target, LogEntries = logs, Count = logs.Length }));
    }
}

public class ProcessActionTool : IDiagnosticTool
{
    public string Name => "restart_service";
    public string Description => "Restarts an unruly microservice process or clears cached memory buffer safely on the target host.";

    public Task<string> ExecuteAsync(string input, CancellationToken ct = default)
    {
        var svc = input.Trim().Trim('"');
        return Task.FromResult(JsonSerializer.Serialize(new
        {
            Action = "RestartService",
            Target = svc,
            Success = true,
            Message = $"Service '{svc}' restarted successfully. Memory buffer cleared and reclaimed 11.4GB RAM.",
            Timestamp = DateTime.UtcNow
        }));
    }
}
