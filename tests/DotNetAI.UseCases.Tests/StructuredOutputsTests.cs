using DotNetAI.Core.Testing;
using DotNetAI.StructuredOutputs.Models;
using DotNetAI.StructuredOutputs.Services;
using Xunit;

namespace DotNetAI.UseCases.Tests;

public class StructuredOutputsTests
{
    [Fact]
    public void GenerateSchemaDescription_InvoiceDocument_ContainsRequiredFields()
    {
        string schema = JsonSchemaExtractor.GenerateSchemaDescription<InvoiceDocument>();

        Assert.Contains("invoiceNumber", schema);
        Assert.Contains("vendorName", schema);
        Assert.Contains("totalAmount", schema);
        Assert.Contains("lineItems", schema);
    }

    [Fact]
    public void GenerateSchemaDescription_ContractExtraction_ContainsRequiredFields()
    {
        string schema = JsonSchemaExtractor.GenerateSchemaDescription<ContractExtraction>();

        Assert.Contains("contractTitle", schema);
        Assert.Contains("parties", schema);
        Assert.Contains("effectiveDate", schema);
        Assert.Contains("governingLaw", schema);
    }

    [Fact]
    public async Task ExtractStructuredData_ValidJson_ReturnsParsedObject()
    {
        string jsonResponse = """
        {
          "invoiceNumber": "INV-2025-001",
          "vendorName": "CloudScale Systems",
          "customerName": "Acme Holdings",
          "invoiceDate": "2025-01-15T00:00:00Z",
          "dueDate": "2025-02-15T00:00:00Z",
          "currency": "USD",
          "subtotal": 5000.0,
          "taxAmount": 450.0,
          "totalAmount": 5450.0,
          "lineItems": [
            { "description": "Kubernetes Cluster Management", "quantity": 1, "unitPrice": 5000.0, "total": 5000.0 }
          ],
          "paymentTerms": "Net 30"
        }
        """;

        var mock = new MockChatClient(jsonResponse);
        var extractor = new StructuredDataExtractorService();

        var result = await extractor.ExtractAsync<InvoiceDocument>(mock, "Unstructured invoice text...");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("INV-2025-001", result.Data.InvoiceNumber);
        Assert.Equal(5450.0m, result.Data.TotalAmount);
        Assert.Single(result.Data.LineItems);
    }

    [Fact]
    public async Task ExtractStructuredData_InvalidJsonWithCorrection_SucceedsOnRetry()
    {
        int callCount = 0;
        var mock = new MockChatClient(prompt =>
        {
            callCount++;
            if (callCount == 1)
            {
                return "Here is the result: { invalid_json";
            }
            return """
            {
              "invoiceNumber": "INV-CORRECTED-99",
              "vendorName": "Acme",
              "customerName": "Beta",
              "invoiceDate": "2025-01-01T00:00:00Z",
              "dueDate": "2025-02-01T00:00:00Z",
              "currency": "USD",
              "subtotal": 100.0,
              "taxAmount": 10.0,
              "totalAmount": 110.0,
              "lineItems": [],
              "paymentTerms": "Immediate"
            }
            """;
        });

        var extractor = new StructuredDataExtractorService();
        var result = await extractor.ExtractAsync<InvoiceDocument>(mock, "Raw text", maxRetries: 2);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("INV-CORRECTED-99", result.Data.InvoiceNumber);
        Assert.Equal(2, result.AttemptCount);
    }
}
