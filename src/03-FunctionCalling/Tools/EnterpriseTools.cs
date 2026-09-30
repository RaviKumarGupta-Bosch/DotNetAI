using System.ComponentModel;
using Microsoft.Extensions.AI;

namespace DotNetAI.FunctionCalling.Tools;

public class InventoryTools
{
    private static readonly Dictionary<string, (string Name, int Stock, decimal Price, string Warehouse)> InventoryDb = new(StringComparer.OrdinalIgnoreCase)
    {
        ["SKU-SERVER-01"] = ("Dell PowerEdge R750 Server", 14, 4500.00m, "Austin-TX-WH1"),
        ["SKU-ROUTER-99"] = ("Cisco Catalyst 9300 Switch", 42, 2800.00m, "Chicago-IL-WH2"),
        ["SKU-CABLE-100"] = ("Cat6A Shielded Ethernet Cable 100m", 320, 85.50m, "Atlanta-GA-WH3"),
        ["SKU-GPU-H100"] = ("NVIDIA H100 80GB SXM5 Accelerator", 3, 32000.00m, "SantaClara-CA-WH1")
    };

    [Description("Retrieves real-time warehouse inventory, unit price, stock availability, and location for a specified product SKU.")]
    public string CheckInventory([Description("The product SKU identifier, e.g., SKU-SERVER-01 or SKU-GPU-H100")] string sku)
    {
        if (InventoryDb.TryGetValue(sku, out var item))
        {
            return $$"""
            {
              "sku": "{{sku}}",
              "productName": "{{item.Name}}",
              "availableStock": {{item.Stock}},
              "unitPriceUsd": {{item.Price}},
              "warehouseLocation": "{{item.Warehouse}}",
              "status": "{{(item.Stock > 0 ? "IN_STOCK" : "OUT_OF_STOCK")}}"
            }
            """;
        }

        return $$"""{"sku": "{{sku}}", "status": "NOT_FOUND", "message": "No inventory record found for SKU {{sku}}"}""";
    }
}

public class LogisticsTools
{
    [Description("Calculates estimated shipping cost and delivery SLA between two postal zip codes.")]
    public string CalculateShippingRate(
        [Description("Origin US Zip code")] string originZip,
        [Description("Destination US Zip code")] string destinationZip,
        [Description("Package weight in kilograms")] double weightKg,
        [Description("Shipping tier: Standard, Express, or Overnight")] string serviceLevel)
    {
        decimal baseRate = serviceLevel.ToLowerInvariant() switch
        {
            "overnight" => 85.00m + (decimal)weightKg * 12.50m,
            "express" => 45.00m + (decimal)weightKg * 6.50m,
            _ => 15.00m + (decimal)weightKg * 2.20m
        };

        int estimatedDays = serviceLevel.ToLowerInvariant() switch
        {
            "overnight" => 1,
            "express" => 2,
            _ => 5
        };

        return $$"""
        {
          "origin": "{{originZip}}",
          "destination": "{{destinationZip}}",
          "weightKg": {{weightKg}},
          "serviceLevel": "{{serviceLevel}}",
          "estimatedCostUsd": {{baseRate:F2}},
          "transitDays": {{estimatedDays}},
          "guaranteed": {{(serviceLevel.Equals("overnight", StringComparison.OrdinalIgnoreCase) ? "true" : "false")}}
        }
        """;
    }
}

public class DevOpsTools
{
    [Description("Fetches real-time infrastructure performance metrics for a specified database cluster.")]
    public string GetDatabaseClusterMetrics([Description("Database cluster name (e.g. prod-db-eastus, analytics-db-westus)")] string clusterName)
    {
        return $$"""
        {
          "clusterName": "{{clusterName}}",
          "cpuUtilizationPercent": 78.4,
          "memoryUtilizationPercent": 64.2,
          "activeConnections": 342,
          "maxConnections": 500,
          "ioReadsPerSec": 4500,
          "ioWritesPerSec": 1280,
          "replicationLagMs": 14.2,
          "healthStatus": "HEALTHY_DEGRADED_LOAD"
        }
        """;
    }
}
