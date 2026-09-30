using System.Diagnostics;
using DotNetAI.Core.Interfaces;
using DotNetAI.StructuredOutputs.Models;
using DotNetAI.StructuredOutputs.Services;
using Microsoft.Extensions.AI;

namespace DotNetAI.StructuredOutputs;

/// <summary>
/// Use Case 02: Structured Outputs & JSON Schema Extraction with Automated Self-Correction.
/// </summary>
public class StructuredOutputsUseCase : IAIUseCase
{
    public int Id => 2;
    public string Name => "Structured Outputs & Data Extraction";
    public string Description => "Extracts strongly-typed domain records (invoices, legal contracts) from unstructured OCR/text using schema enforcement, strict JSON parsing, domain validation, and iterative self-correction.";
    public IReadOnlyList<string> AIConcepts => new[]
    {
        "Structured Outputs & JSON Mode",
        "C# POCO/Record Schema Mapping",
        "Deterministic JSON Parsing (System.Text.Json)",
        "Domain Rule Validation (Math & Field Integrity)",
        "Self-Correction & Automated Error Feedback Loop"
    };

    private readonly StructuredDataExtractorService _extractor = new();

    public async Task<UseCaseExecutionResult> RunAsync(
        IChatClient chatClient, 
        IEmbeddingGenerator<string, Embedding<float>>? embeddingGenerator = null, 
        CancellationToken cancellationToken = default)
    {
        var sw = Stopwatch.StartNew();
        var result = new UseCaseExecutionResult();
        result.Logs.Add("[UseCase 02] Starting Structured Outputs extraction demo...");

        var rawInvoiceText = """
        INVOICE - CONTOSO CLOUD SERVICES
        Bill To: Acme Corp, 456 Innovation Way, Seattle WA
        Invoice #: INV-2026-9041
        Date: September 15, 2026
        Due Date: October 15, 2026
        Currency: USD
        Payment Terms: Net 30

        Items:
        1. Azure Managed Kubernetes Cluster (AKS) Standard Tier - Qty: 2 @ $450.00 each -> Total: $900.00
        2. Premium SSD Managed Disks 1TB - Qty: 4 @ $120.00 each -> Total: $480.00
        3. Cloud Network Egress Data Transfer - Qty: 1 @ $70.00 -> Total: $70.00

        Subtotal: $1450.00
        Tax (10%): $145.00
        Total Due: $1595.00
        """;

        result.Logs.Add("--- Extracting Invoice Document to C# Domain Record ---");

        var invoiceResult = await _extractor.ExtractAsync<InvoiceDocument>(
            chatClient,
            rawInvoiceText,
            customValidator: invoice =>
            {
                var errors = new List<string>();
                if (string.IsNullOrWhiteSpace(invoice.InvoiceNumber))
                    errors.Add("InvoiceNumber must not be empty");
                if (invoice.TotalAmount <= 0)
                    errors.Add("TotalAmount must be greater than 0");
                return errors;
            },
            maxRetries: 2,
            cancellationToken
        );

        if (invoiceResult.IsSuccess && invoiceResult.Data != null)
        {
            result.Logs.Add($"Successfully parsed Invoice: {invoiceResult.Data.InvoiceNumber} from {invoiceResult.Data.VendorName}");
            result.Logs.Add($"Total Amount: {invoiceResult.Data.Currency} {invoiceResult.Data.TotalAmount:N2} across {invoiceResult.Data.LineItems.Count} line items.");
            result.Outputs["InvoiceData"] = invoiceResult.Data;
        }
        else
        {
            result.Logs.Add($"Extraction warning/errors: {string.Join(", ", invoiceResult.ValidationErrors)}");
            result.Outputs["InvoiceErrors"] = invoiceResult.ValidationErrors;
        }

        var rawContractText = """
        MASTER SERVICES AGREEMENT (MSA)
        This Agreement is entered into on October 1, 2026 by and between Nexus Software Inc. ('Provider') and Global Logistics Ltd ('Client').
        Governing Law: State of Delaware.
        Term: 24 months, expiring October 1, 2028.
        Liability: The aggregate liability of either party shall not exceed $1,000,000 or the total fees paid in the preceding 12 months.
        Termination: Either party may terminate with 60 days written notice.
        Key Obligations: Provider will deliver 99.95% API uptime SLA; Client will pay invoices within 30 days.
        """;

        result.Logs.Add("--- Extracting Legal Contract Extraction ---");
        var contractResult = await _extractor.ExtractAsync<ContractExtraction>(
            chatClient,
            rawContractText,
            customValidator: contract =>
            {
                var errors = new List<string>();
                if (contract.TerminationNoticeDays <= 0)
                    errors.Add("TerminationNoticeDays must be positive integer.");
                return errors;
            },
            maxRetries: 2,
            cancellationToken
        );

        if (contractResult.IsSuccess && contractResult.Data != null)
        {
            result.Logs.Add($"Successfully extracted Contract: '{contractResult.Data.ContractTitle}' (Governing Law: {contractResult.Data.GoverningLaw})");
            result.Outputs["ContractData"] = contractResult.Data;
        }

        sw.Stop();
        result.Duration = sw.Elapsed;
        result.Summary = "Structured output extraction completed with strongly-typed validation and domain record mapping.";
        return result;
    }
}
